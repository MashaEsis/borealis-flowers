using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using borealis_flowers.api.Features.Wallet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Auth;

public static class AuthHandler
{
    public static async Task<IResult> RegisterAsync(
        RegisterRequest dto,
        DataContext db,
        JwtTokenService jwt)
    {
        string email = dto.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.Name))
            return Results.BadRequest("Укажите email, пароль и имя.");

        if (await db.Customers.AnyAsync(c => c.Email != null && c.Email.ToLower() == email))
            return Results.Conflict("Пользователь с таким email уже есть.");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstVisit = DateTime.UtcNow,
            LastVisit = DateTime.UtcNow,
        };

        await db.Customers.AddAsync(customer);
        await db.SaveChangesAsync();

        string token = jwt.CreateToken(customer);
        return Results.Ok(new AuthResponse
        {
            Token = token,
            User = await UserMeDtoExtensions.FromCustomerAsync(customer, db),
        });
    }

    public static async Task<IResult> LoginAsync(
        LoginRequest dto,
        DataContext db,
        JwtTokenService jwt)
    {
        string email = dto.Email.Trim().ToLowerInvariant();
        Customer? customer =
            await db.Customers.FirstOrDefaultAsync(c =>
                c.Email != null && c.Email.ToLower() == email);

        if (customer?.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash))
            return Results.Unauthorized();

        customer.LastVisit = DateTime.UtcNow;
        await db.Customers
            .Where(c => c.Id == customer.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LastVisit, DateTime.UtcNow));

        string token = jwt.CreateToken(customer);
        return Results.Ok(new AuthResponse
        {
            Token = token,
            User = await UserMeDtoExtensions.FromCustomerAsync(customer, db),
        });
    }

    public static async Task<IResult> MeAsync(ClaimsPrincipal user, DataContext db)
    {
        string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (sub is null || !Guid.TryParse(sub, out Guid id))
            return Results.Unauthorized();

        Customer? customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return customer is null
            ? Results.NotFound()
            : Results.Ok(await UserMeDtoExtensions.FromCustomerAsync(customer, db));
    }

    public static async Task<IResult> UpdateProfileAsync(
        UpdateProfileRequest dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (sub is null || !Guid.TryParse(sub, out Guid id))
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Results.BadRequest("Укажите имя.");

        Customer? customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
            return Results.NotFound();

        string? email = string.IsNullOrWhiteSpace(dto.Email)
            ? null
            : dto.Email.Trim().ToLowerInvariant();

        if (email is not null &&
            await db.Customers.AnyAsync(c => c.Id != id && c.Email != null && c.Email.ToLower() == email))
            return Results.Conflict("Пользователь с таким email уже есть.");

        customer.Name = dto.Name.Trim();
        customer.Email = email;
        customer.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        customer.Birthday = dto.Birthday?.Date;
        customer.LastVisit = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(await UserMeDtoExtensions.FromCustomerAsync(customer, db));
    }
}

public static class UserMeDtoExtensions
{
    public static async Task<UserMeDto> FromCustomerAsync(Customer c, DataContext db)
    {
        (int progress, bool nextDiscount) = await WalletService.GetLoyaltyAsync(db, c.Id);
        return new UserMeDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Birthday = c.Birthday,
            Role = JwtTokenService.ResolveRole(c),
            IsAdmin = c.IsAdmin,
            IsSpecialist = c.IsSpecialist,
            SpecialistId = c.SpecialistId,
            WalletBalance = c.WalletBalance,
            LoyaltyProgress = progress,
            LoyaltyTarget = WalletService.LoyaltyEvery,
            NextOrderDiscount = nextDiscount,
        };
    }
}

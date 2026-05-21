using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
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
            User = UserMeDtoExtensions.FromCustomer(customer),
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
        await db.SaveChangesAsync();

        string token = jwt.CreateToken(customer);
        return Results.Ok(new AuthResponse
        {
            Token = token,
            User = UserMeDtoExtensions.FromCustomer(customer),
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
            : Results.Ok(UserMeDtoExtensions.FromCustomer(customer));
    }
}

public static class UserMeDtoExtensions
{
    public static UserMeDto FromCustomer(Customer c) =>
        new()
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Role = JwtTokenService.ResolveRole(c),
            IsAdmin = c.IsAdmin,
            IsSpecialist = c.IsSpecialist,
            SpecialistId = c.SpecialistId,
        };
}

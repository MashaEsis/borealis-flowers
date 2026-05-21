using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.FloristApplications;

public static class FloristApplicationsHandler
{
    public sealed class FloristApplicationDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerEmail { get; set; }
        public string FullName { get; set; } = "";
        public string Experience { get; set; } = "";
        public string PortfolioNotes { get; set; } = "";
        public string Motivation { get; set; } = "";
        public FloristApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminComment { get; set; }
    }

    public sealed class CreateFloristApplicationDto
    {
        public string FullName { get; set; } = "";
        public string Experience { get; set; } = "";
        public string PortfolioNotes { get; set; } = "";
        public string Motivation { get; set; } = "";
    }

    public static async Task<IResult> CreateAsync(
        CreateFloristApplicationDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? cid = TryCustomerId(user);
        if (cid is null)
            return Results.Unauthorized();

        Customer? customer = await db.Customers.FindAsync(cid.Value);
        if (customer is null)
            return Results.NotFound();

        if (customer.IsSpecialist || customer.IsAdmin)
            return Results.Conflict("Уже есть права флориста или администратора.");

        bool pending = await db.FloristApplications.AnyAsync(a =>
            a.CustomerId == cid && a.Status == FloristApplicationStatus.Pending);
        if (pending)
            return Results.Conflict("Заявка уже на рассмотрении.");

        var app = new FloristApplication
        {
            Id = Guid.NewGuid(),
            CustomerId = cid.Value,
            FullName = dto.FullName.Trim(),
            Experience = dto.Experience.Trim(),
            PortfolioNotes = dto.PortfolioNotes.Trim(),
            Motivation = dto.Motivation.Trim(),
            Status = FloristApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        await db.FloristApplications.AddAsync(app);
        await db.SaveChangesAsync();
        return Results.Created($"/florist-applications/{app.Id}", app.Id);
    }

    public static async Task<IResult> ListPendingAsync(ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        List<FloristApplication> rows = await db.FloristApplications.AsNoTracking()
            .Include(a => a.Customer)
            .Where(a => a.Status == FloristApplicationStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var list = rows.Select(a => new FloristApplicationDto
        {
            Id = a.Id,
            CustomerId = a.CustomerId,
            CustomerEmail = a.Customer?.Email,
            FullName = a.FullName,
            Experience = a.Experience,
            PortfolioNotes = a.PortfolioNotes,
            Motivation = a.Motivation,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            AdminComment = a.AdminComment,
        }).ToList();

        return Results.Ok(list);
    }

    public static async Task<IResult> ApproveAsync(Guid id, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        FloristApplication? app = await db.FloristApplications
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app is null)
            return Results.NotFound();
        if (app.Status != FloristApplicationStatus.Pending)
            return Results.BadRequest("Заявка уже обработана.");

        Guid specId = Guid.NewGuid();
        Guid specializationId =
            await db.Specialization.Where(s => s.IsActive).Select(s => s.Id).FirstAsync();

        var specialist = new Specialist
        {
            Id = specId,
            FullName = app.FullName,
            SpecializationId = specializationId,
            ImgUrl = $"https://picsum.photos/seed/{specId:N}/480/600",
            IsActive = true,
            City = "",
        };

        await db.Specialists.AddAsync(specialist);

        Customer? cust = app.Customer;
        if (cust is not null)
        {
            cust.IsSpecialist = true;
            cust.SpecialistId = specId;
            cust.Name = app.FullName;
        }

        app.Status = FloristApplicationStatus.Approved;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> DeclineAsync(
        Guid id,
        HttpContext http,
        ClaimsPrincipal user,
        DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        string? comment = http.Request.Query["comment"].FirstOrDefault();

        FloristApplication? app = await db.FloristApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (app is null)
            return Results.NotFound();
        if (app.Status != FloristApplicationStatus.Pending)
            return Results.BadRequest();

        app.Status = FloristApplicationStatus.Declined;
        app.AdminComment = comment;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    static bool IsAdmin(ClaimsPrincipal user) =>
        user.HasClaim(ClaimTypes.Role, "Admin");

    static Guid? TryCustomerId(ClaimsPrincipal user)
    {
        string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub is not null && Guid.TryParse(sub, out Guid id) ? id : null;
    }
}

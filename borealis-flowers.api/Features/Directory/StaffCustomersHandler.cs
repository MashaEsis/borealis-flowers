using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Directory;

public static class StaffCustomersHandler
{
    public sealed class CustomerBriefDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSpecialist { get; set; }
    }

    public sealed class FloristStaffDto
    {
        public Guid CustomerId { get; set; }
        public Guid? SpecialistId { get; set; }
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? SpecialistName { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
    }

    public static async Task<IResult> ListAsync(ClaimsPrincipal user, DataContext db)
    {
        string? role = user.FindFirstValue(ClaimTypes.Role);
        if (role != "Florist" && role != "Admin")
            return Results.Forbid();

        IQueryable<Customer> query = db.Customers.AsNoTracking();

        if (role == "Florist")
            query = query.Where(c => !c.IsSpecialist && !c.IsAdmin);

        var list = await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerBriefDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                IsAdmin = c.IsAdmin,
                IsSpecialist = c.IsSpecialist,
            })
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> ListUsersAsync(ClaimsPrincipal user, DataContext db)
    {
        if (user.FindFirstValue(ClaimTypes.Role) != "Admin")
            return Results.Forbid();

        var list = await db.Customers.AsNoTracking()
            .Where(c => c.IsAdmin || !c.IsSpecialist)
            .OrderByDescending(c => c.IsAdmin)
            .ThenBy(c => c.Name)
            .Select(c => new CustomerBriefDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                IsAdmin = c.IsAdmin,
                IsSpecialist = false,
            })
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> ListFloristsAsync(ClaimsPrincipal user, DataContext db)
    {
        if (user.FindFirstValue(ClaimTypes.Role) != "Admin")
            return Results.Forbid();

        var list = await (
                from c in db.Customers.AsNoTracking()
                where c.IsSpecialist && !c.IsAdmin
                join s in db.Specialists.AsNoTracking() on c.SpecialistId equals s.Id into sj
                from s in sj.DefaultIfEmpty()
                orderby c.Name
                select new FloristStaffDto
                {
                    CustomerId = c.Id,
                    SpecialistId = c.SpecialistId,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    SpecialistName = s != null ? s.FullName : null,
                    City = s != null ? s.City : null,
                    IsActive = s != null && s.IsActive,
                })
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> DemoteFloristAsync(Guid customerId, ClaimsPrincipal user, DataContext db)
    {
        if (user.FindFirstValue(ClaimTypes.Role) != "Admin")
            return Results.Forbid();

        Customer? customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer is null)
            return Results.NotFound("Пользователь не найден.");

        if (!customer.IsSpecialist)
            return Results.BadRequest("Пользователь не является флористом.");

        if (customer.IsAdmin)
            return Results.BadRequest("Нельзя уволить администратора через этот раздел.");

        if (customer.SpecialistId is Guid specialistId)
        {
            Specialist? specialist = await db.Specialists.FindAsync(specialistId);
            if (specialist is not null)
                specialist.IsActive = false;
        }

        customer.IsSpecialist = false;
        customer.SpecialistId = null;
        customer.LastVisit = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Флорист переведён в клиенты. История заказов сохранена." });
    }
}

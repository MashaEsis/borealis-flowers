using System.Security.Claims;
using borealis_flowers.api.Data;
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
    }

    public static async Task<IResult> ListAsync(ClaimsPrincipal user, DataContext db)
    {
        string? role = user.FindFirstValue(ClaimTypes.Role);
        if (role != "Florist" && role != "Admin")
            return Results.Forbid();

        var list = await db.Customers.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CustomerBriefDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
            })
            .ToListAsync();

        return Results.Ok(list);
    }
}

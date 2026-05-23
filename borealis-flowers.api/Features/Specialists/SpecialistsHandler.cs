using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Specialists;

public static class SpecialistsHandler
{
    public static Func<DataContext, Task<List<SpecialistDto>>> GetSpecialists()
    {
        return async (DataContext db) =>
        {
            List<Specialist> specialists = await db.Specialists
                .AsNoTracking()
                .Where(s => s.IsActive)
                .Include(x => x.Specialization)
                .ToListAsync();

            var ids = specialists.Select(s => s.Id).ToList();
            var allWorks = await db.SpecialistPortfolioWorks.AsNoTracking()
                .Where(w => ids.Contains(w.SpecialistId))
                .OrderBy(w => w.SpecialistId)
                .ThenBy(w => w.SortOrder)
                .ThenBy(w => w.CreatedAt)
                .Select(w => new { w.SpecialistId, w.ImageUrl })
                .ToListAsync();

            var previewMap = allWorks
                .GroupBy(w => w.SpecialistId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ImageUrl).Take(4).ToList());

            return specialists.Select(s => new SpecialistDto
            {
                Id = s.Id,
                FullName = s.FullName,
                ImgUrl = s.ImgUrl,
                Address = s.Address ?? string.Empty,
                City = s.City ?? string.Empty,
                Specialization = s.Specialization?.Name ?? string.Empty,
                StyleDescription = s.StyleDescription ?? string.Empty,
                PortfolioPreview = previewMap.TryGetValue(s.Id, out List<string>? imgs) ? imgs : [],
            }).ToList();
        };
    }

    public static Func<SpecialistUpdateVM, DataContext, ClaimsPrincipal, Task<IResult>> UpdateSpecialist()
    {
        return async ([FromBody] SpecialistUpdateVM specialist, DataContext db, ClaimsPrincipal user) =>
        {
            string? role = user.FindFirstValue(ClaimTypes.Role);
            if (role is not ("Admin" or "Florist"))
                return Results.Forbid();

            Specialist? existingSpecialist = await db.Specialists.FindAsync(specialist.Id);
            if (existingSpecialist == null)
                return Results.NotFound($"Specialist with ID {specialist.Id} not found");

            if (role == "Florist")
            {
                string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (sub is null || !Guid.TryParse(sub, out Guid customerId))
                    return Results.Unauthorized();

                bool ownsProfile = await db.Customers.AnyAsync(c =>
                    c.Id == customerId && c.SpecialistId == specialist.Id && c.IsSpecialist);
                if (!ownsProfile)
                    return Results.Forbid();
            }

            string trimmedName = specialist.FullName.Trim();
            existingSpecialist.FullName = trimmedName;
            existingSpecialist.ImgUrl = specialist.ImgUrl;
            existingSpecialist.SpecializationId = specialist.SpecializationId;
            existingSpecialist.Address = specialist.Address;
            existingSpecialist.Latitude = specialist.Latitude;
            existingSpecialist.Longitude = specialist.Longitude;

            await db.Customers
                .Where(c => c.SpecialistId == specialist.Id && c.IsSpecialist && !c.IsAdmin)
                .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Name, trimmedName));

            await db.SaveChangesAsync();

            return Results.Ok();
        };
    }
    public static Func<string, DataContext, Task<IEnumerable<VisitInfo>>> GetLastShedules()
    {
        return async (string customerId, DataContext db) =>
        {
            if (!Guid.TryParse(customerId, out var customerGuid))
                return Enumerable.Empty<VisitInfo>();

            var raw = await (from ts in db.Timeslots
                             join ds in db.DateSchedules on ts.DateScheduleId equals ds.Id
                             join s in db.Specialists on ds.SpecialistId equals s.Id
                             join sp in db.Specialization on s.SpecializationId equals sp.Id into spLeft
                             from sp in spLeft.DefaultIfEmpty()
                             where ts.CustomerId == customerGuid
                             orderby ds.Date descending
                             select new
                             {
                                 ts.Id,
                                 ds.Date,
                                 ts.Time,
                                 SpecialistId = s.Id,
                                 s.FullName,
                                 s.ImgUrl,
                                 s.Address,
                                 s.City,
                                 SpecializationName = sp != null ? sp.Name : null
                             })
                .ToListAsync();

            return raw.Select(r => new VisitInfo
            {
                TimeSlotId = r.Id,
                VisitDate = r.Date.Date.AddHours(r.Time),
                Specialist = new SpecialistDto
                {
                    Id = r.SpecialistId,
                    FullName = r.FullName,
                    ImgUrl = r.ImgUrl,
                    Address = r.Address,
                    Specialization = r.SpecializationName,
                    City = r.City,
                }
            });
        };
    }

    public static Func<DataContext, Task<IEnumerable<string>>> GetCities()
    {
        return async (DataContext db) =>
        {
            return await db.Specialists
                .Select(x => x.Address)
                .ToListAsync();
        };
    }
}

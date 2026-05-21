using borealis_flowers.api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Specialists;

public static class SpecialistsHandler
{
    public static Func<DataContext, Task<List<SpecialistDto>>> GetSpecialists()
    {
        return async (DataContext db) => await db.Specialists
            .AsNoTracking()
            .Include(x => x.Specialization)
            .Select(s => new SpecialistDto
            {
                Id = s.Id,
                FullName = s.FullName,
                ImgUrl = s.ImgUrl,
                Address = s.Address ?? string.Empty,
                City = s.City ?? string.Empty,
                Specialization = s.Specialization.Name,
            }).ToListAsync();
    }

    public static Func<SpecialistUpdateVM, DataContext, Task<IResult>> UpdateSpecialist()
    {
        return async ([FromBody] SpecialistUpdateVM specialist, DataContext db) =>
        {
            var existingSpecialist = await db.Specialists.FindAsync(specialist.Id);

            if (existingSpecialist == null)
                return Results.NotFound($"Specialist with ID {specialist.Id} not found");

            existingSpecialist.FullName = specialist.FullName;
            existingSpecialist.ImgUrl = specialist.ImgUrl;
            existingSpecialist.SpecializationId = specialist.SpecializationId;
            existingSpecialist.Address = specialist.Address;
            existingSpecialist.Latitude = specialist.Latitude;
            existingSpecialist.Longitude = specialist.Longitude;

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

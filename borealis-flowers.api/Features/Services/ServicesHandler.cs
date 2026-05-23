using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Services;

public static class ServicesHandler
{
    public static Func<DataContext, Task<List<ServiceDto>>> GetServices()
    {
        return async (DataContext db) => await db.Services.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                SpecializationId = s.SpecializationId,
                SpecializationName = s.Specialization != null ? s.Specialization.Name : "Florist",
                EstimatedTime = s.EstimatedTime,
                Name = s.Name,
                Description = s.Description,
                FlowerComposition = s.FlowerComposition,
                ImageUrl = s.ImageUrl,
                Price = s.Price,
                SpecialistId = s.SpecialistId,
                SpecialistName = s.Specialist != null ? s.Specialist.FullName : null,
            }).ToListAsync();
    }

    public static async Task<IResult> GetServiceById(Guid id, DataContext db)
    {
        ServiceDto? row = await db.Services.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                SpecializationId = s.SpecializationId,
                SpecializationName = s.Specialization != null ? s.Specialization.Name : "Florist",
                EstimatedTime = s.EstimatedTime,
                Name = s.Name,
                Description = s.Description,
                FlowerComposition = s.FlowerComposition,
                ImageUrl = s.ImageUrl,
                Price = s.Price,
                SpecialistId = s.SpecialistId,
                SpecialistName = s.Specialist != null ? s.Specialist.FullName : null,
            })
            .FirstOrDefaultAsync();

        return row is null ? Results.NotFound() : Results.Ok(row);
    }
}

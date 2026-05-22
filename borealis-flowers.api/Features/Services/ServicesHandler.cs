using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Services;

public static class ServicesHandler
{
    public static Func<DataContext, Task<List<ServiceDto>>> GetServices()
    {
        return async (DataContext db) => await db.Services.Include(x => x.Specialization)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                SpecializationId = s.SpecializationId,
                SpecializationName = s.Specialization!.Name,
                EstimatedTime = s.EstimatedTime,
                Name = s.Name,
                Description = s.Description,
                FlowerComposition = s.FlowerComposition,
                ImageUrl = s.ImageUrl,
                Price = s.Price,
            }).ToListAsync();
    }

    public static async Task<IResult> GetServiceById(Guid id, DataContext db)
    {
        ServiceDto? row = await db.Services.AsNoTracking()
            .Include(x => x.Specialization)
            .Where(s => s.Id == id)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                SpecializationId = s.SpecializationId,
                SpecializationName = s.Specialization!.Name,
                EstimatedTime = s.EstimatedTime,
                Name = s.Name,
                Description = s.Description,
                FlowerComposition = s.FlowerComposition,
                ImageUrl = s.ImageUrl,
                Price = s.Price,
            })
            .FirstOrDefaultAsync();

        return row is null ? Results.NotFound() : Results.Ok(row);
    }
}

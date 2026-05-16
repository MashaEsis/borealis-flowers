using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Services;

public static class ServicesHandler
{

    public static Func<DataContext, Task<List<ServiceDto>>> GetServices()
    {
        return async (DataContext db) => await db.Services.Include(x => x.Specialization)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                SpecializationId = s.SpecializationId,
                SpecializationName = s.Specialization.Name,
                EstimatedTime = s.EstimatedTime,
                Name = s.Name,
                Price = s.Price,
            }).ToListAsync();
    }
}

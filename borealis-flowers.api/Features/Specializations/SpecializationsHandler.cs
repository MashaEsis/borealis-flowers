using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Specializations;

public static class SpecializationsHandler
{
    public static Func<DataContext, Task<List<SpecializationDto>>> GetSpecializations()
    {
        return async (DataContext db) => await db.Specialization
            .Select(s => new SpecializationDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            }).ToListAsync();
    }
}

using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Проставляет флориста для букетов без привязки (по кругу среди активных).
/// </summary>
public static class BouquetFloristSeeder
{
    public static async Task ApplyAsync(DataContext db)
    {
        List<Guid> florists = await db.Specialists.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => s.Id)
            .ToListAsync();

        if (florists.Count == 0)
            return;

        List<Service> unassigned = await db.Services
            .Where(s => s.SpecialistId == null)
            .OrderBy(s => s.Name)
            .ToListAsync();

        if (unassigned.Count == 0)
            return;

        for (int i = 0; i < unassigned.Count; i++)
            unassigned[i].SpecialistId = florists[i % florists.Count];

        await db.SaveChangesAsync();
    }
}

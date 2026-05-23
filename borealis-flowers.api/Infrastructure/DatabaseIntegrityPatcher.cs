using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Одноразовые исправления целостности данных (идемпотентно при каждом старте).
/// </summary>
public static class DatabaseIntegrityPatcher
{
    public const string DefaultFloristPassword = "florist123";

    static readonly Guid MissingMariaSpecialistId = Guid.Parse("60672BC1-64D3-466A-8A9B-BAA332BEFBDC");

    static readonly Guid KeepMudMaxCustomerId = Guid.Parse("2D11986C-15B6-41CE-81FA-D31ABE276459");

    const string DuplicateMudMaxEmail = "mud.max@mail.ru";

    public static async Task ApplyAsync(DataContext db)
    {
        await FixOrphanServiceSpecializationsAsync(db);
        await EnsureMariaSpecialistAsync(db);
        await SetFloristPasswordsAsync(db);
        await RemoveDuplicateMudMaxAsync(db);
        await RemoveOrphanFloristApplicationsAsync(db);
    }

    /// <summary>
    /// Букеты с удалённой специализацией не попадают в каталог (inner join в EF).
    /// </summary>
    static async Task FixOrphanServiceSpecializationsAsync(DataContext db)
    {
        Guid? defaultSpecId = await db.Specialization.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();

        if (defaultSpecId is not Guid specId)
            return;

        List<Guid> validIds = await db.Specialization.AsNoTracking()
            .Select(s => s.Id)
            .ToListAsync();

        await db.Services
            .Where(s => !validIds.Contains(s.SpecializationId))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.SpecializationId, specId));
    }

    static async Task EnsureMariaSpecialistAsync(DataContext db)
    {
        if (await db.Specialists.AnyAsync(s => s.Id == MissingMariaSpecialistId))
            return;

        Guid specializationId = await db.Specialization
            .Where(s => s.IsActive)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        if (specializationId == Guid.Empty)
            return;

        Customer? maria = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.SpecialistId == MissingMariaSpecialistId);

        await db.Specialists.AddAsync(new Specialist
        {
            Id = MissingMariaSpecialistId,
            FullName = maria?.Name ?? "Maria Esis",
            SpecializationId = specializationId,
            ImgUrl = $"https://picsum.photos/seed/{MissingMariaSpecialistId:N}/480/600",
            City = "",
            IsActive = true,
        });
        await db.SaveChangesAsync();
    }

    static async Task SetFloristPasswordsAsync(DataContext db)
    {
        bool needs = await db.Customers.AnyAsync(c =>
            c.IsSpecialist && !c.IsAdmin &&
            (c.PasswordHash == null || c.PasswordHash == ""));

        if (!needs)
            return;

        string hash = BCrypt.Net.BCrypt.HashPassword(DefaultFloristPassword);
        await db.Customers
            .Where(c => c.IsSpecialist && !c.IsAdmin &&
                        (c.PasswordHash == null || c.PasswordHash == ""))
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.PasswordHash, hash));
    }

    static async Task RemoveDuplicateMudMaxAsync(DataContext db)
    {
        Guid? duplicateId = await db.Customers.AsNoTracking()
            .Where(c => c.Email != null && c.Email.ToLower() == DuplicateMudMaxEmail)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();

        if (duplicateId is not Guid duplicateCustomerId)
            return;

        await db.Requests
            .Where(r => r.CustomerId == duplicateCustomerId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.CustomerId, KeepMudMaxCustomerId));

        await db.Timeslots
            .Where(t => t.CustomerId == duplicateCustomerId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.CustomerId, KeepMudMaxCustomerId));

        await db.FloristApplications
            .Where(a => a.CustomerId == duplicateCustomerId)
            .ExecuteDeleteAsync();

        await db.Customers
            .Where(c => c.Id == duplicateCustomerId)
            .ExecuteDeleteAsync();
    }

    static async Task RemoveOrphanFloristApplicationsAsync(DataContext db)
    {
        List<Guid> customerIds = await db.Customers.Select(c => c.Id).ToListAsync();

        await db.FloristApplications
            .Where(a => !customerIds.Contains(a.CustomerId))
            .ExecuteDeleteAsync();
    }
}

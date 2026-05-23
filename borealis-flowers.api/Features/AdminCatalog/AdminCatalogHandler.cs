using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using IO = System.IO;

namespace borealis_flowers.api.Features.AdminCatalog;

public static class AdminCatalogHandler
{
    public static bool IsAdmin(ClaimsPrincipal user) =>
        user.HasClaim(ClaimTypes.Role, "Admin");

    public sealed class ServiceEditDto
    {
        public Guid SpecializationId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? FlowerComposition { get; set; }
        public string? ImageUrl { get; set; }
        public double Price { get; set; }
        public int? EstimatedTime { get; set; }
        public Guid? SpecialistId { get; set; }
    }

    public sealed class SpecializationEditDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public sealed class ServicePriceEditDto
    {
        public double Price { get; set; }
    }

    public sealed class TimeslotAdminEditDto
    {
        public bool Available { get; set; }
        public Guid? CustomerId { get; set; }
    }

    public sealed class TimeslotAdminRowDto
    {
        public Guid Id { get; set; }
        public bool Available { get; set; }
        public Guid? CustomerId { get; set; }
        public int Time { get; set; }
        public Guid DateScheduleId { get; set; }
        public DateTime Date { get; set; }
        public Guid SpecialistId { get; set; }
        public string SpecialistName { get; set; } = "";
    }

    public static async Task<IResult> ListServices(ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        var list = await db.Services.AsNoTracking()
            .Include(s => s.Specialization)
            .Include(s => s.Specialist)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.FlowerComposition,
                s.ImageUrl,
                s.Price,
                s.EstimatedTime,
                s.SpecializationId,
                s.SpecialistId,
                SpecializationName = s.Specialization.Name,
                SpecialistName = s.Specialist != null ? s.Specialist.FullName : null,
            })
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> CreateService(ServiceEditDto dto, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        if (!await db.Specialization.AnyAsync(sp => sp.Id == dto.SpecializationId))
            return Results.BadRequest("Специализация не найдена.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Results.BadRequest("Укажите название букета.");

        if (dto.SpecialistId is not Guid floristId ||
            !await db.Specialists.AnyAsync(s => s.Id == floristId && s.IsActive))
            return Results.BadRequest("Выберите активного флориста для букета.");

        var entity = new Service
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            FlowerComposition = string.IsNullOrWhiteSpace(dto.FlowerComposition) ? null : dto.FlowerComposition.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            Price = dto.Price,
            EstimatedTime = dto.EstimatedTime,
            SpecializationId = dto.SpecializationId,
            SpecialistId = floristId,
        };
        await db.Services.AddAsync(entity);
        await db.SaveChangesAsync();
        return Results.Created($"/admin/catalog/services/{entity.Id}", entity.Id);
    }

    public static async Task<IResult> UpdateService(Guid id, ServiceEditDto dto, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        Service? s = await db.Services.FindAsync(id);
        if (s is null)
            return Results.NotFound();
        if (!await db.Specialization.AnyAsync(sp => sp.Id == dto.SpecializationId))
            return Results.BadRequest("Специализация не найдена.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Results.BadRequest("Укажите название букета.");

        if (dto.SpecialistId is not Guid floristId ||
            !await db.Specialists.AnyAsync(s => s.Id == floristId && s.IsActive))
            return Results.BadRequest("Выберите активного флориста для букета.");

        s.Name = dto.Name.Trim();
        s.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        s.FlowerComposition = string.IsNullOrWhiteSpace(dto.FlowerComposition) ? null : dto.FlowerComposition.Trim();
        s.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        s.Price = dto.Price;
        s.EstimatedTime = dto.EstimatedTime;
        s.SpecializationId = dto.SpecializationId;
        s.SpecialistId = floristId;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> DeleteService(Guid id, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        Service? s = await db.Services.FindAsync(id);
        if (s is null)
            return Results.NotFound();

        List<ServicePrice> prices = await db.ServicePrice.Where(p => p.ServiceId == id).ToListAsync();
        db.ServicePrice.RemoveRange(prices);
        db.Services.Remove(s);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> ListSpecializations(ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        var list = await db.Specialization.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Description, x.IsActive })
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> CreateSpecialization(SpecializationEditDto dto, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        var entity = new Specialization
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            IsActive = dto.IsActive,
        };
        await db.Specialization.AddAsync(entity);
        await db.SaveChangesAsync();
        return Results.Ok(entity.Id);
    }

    public static async Task<IResult> UpdateSpecialization(Guid id, SpecializationEditDto dto, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        Specialization? s = await db.Specialization.FindAsync(id);
        if (s is null)
            return Results.NotFound();

        s.Name = dto.Name.Trim();
        s.Description = dto.Description.Trim();
        s.IsActive = dto.IsActive;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> ListServicePrices(ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        var list = await db.ServicePrice.AsNoTracking()
            .Include(p => p.Service)
            .Include(p => p.Specialist)
            .OrderBy(p => p.Service!.Name)
            .Select(p => new
            {
                p.Id,
                p.ServiceId,
                ServiceName = p.Service!.Name,
                p.SpecialistId,
                SpecialistName = p.Specialist!.FullName,
                p.Price,
            })
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> UpdateServicePrice(Guid id, ServicePriceEditDto dto, ClaimsPrincipal user, DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        ServicePrice? p = await db.ServicePrice.FindAsync(id);
        if (p is null)
            return Results.NotFound();

        p.Price = dto.Price;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> ListTimeslots(ClaimsPrincipal user, DataContext db, DateTime? minDate = null)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        DateTime min = (minDate ?? DateTime.UtcNow.Date).Date;

        var list = await (
                from t in db.Timeslots.AsNoTracking()
                join ds in db.DateSchedules.AsNoTracking() on t.DateScheduleId equals ds.Id
                join s in db.Specialists.AsNoTracking() on ds.SpecialistId equals s.Id
                where ds.Date.Date >= min
                orderby ds.Date, t.Time
                select new TimeslotAdminRowDto
                {
                    Id = t.Id,
                    Available = t.Available,
                    CustomerId = t.CustomerId,
                    Time = t.Time,
                    DateScheduleId = ds.Id,
                    Date = ds.Date,
                    SpecialistId = s.Id,
                    SpecialistName = s.FullName,
                })
            .Take(500)
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> UpdateTimeslot(
        Guid id,
        TimeslotAdminEditDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        Timeslot? t = await db.Timeslots.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null)
            return Results.NotFound();

        t.Available = dto.Available;
        t.CustomerId = dto.CustomerId;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> UploadBouquetImage(
        IFormFile file,
        ClaimsPrincipal user,
        IWebHostEnvironment env)
    {
        if (!IsAdmin(user))
            return Results.Forbid();

        if (file.Length == 0)
            return Results.BadRequest("Файл пустой.");

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jfif" or ".jpg" or ".jpeg" or ".png" or ".webp"))
            return Results.BadRequest("Поддерживаются JPG, PNG, WEBP, JFIF.");

        string? solutionRoot = IO.Directory.GetParent(env.ContentRootPath)?.FullName;
        if (solutionRoot is null)
            return Results.Problem("Не найден каталог решения.");

        string targetDir = IO.Path.Combine(solutionRoot, "borealis-flowers.ui", "wwwroot", "images", "bouquets");
        IO.Directory.CreateDirectory(targetDir);

        string safeStem = IO.Path.GetFileNameWithoutExtension(file.FileName)
            .Replace(" ", "-", StringComparison.Ordinal);
        string fileName = safeStem + ext;
        string targetPath = IO.Path.Combine(targetDir, fileName);

        await using (IO.FileStream stream = IO.File.Create(targetPath))
        {
            await file.CopyToAsync(stream);
        }

        return Results.Ok(new { url = $"/images/bouquets/{fileName}" });
    }
}

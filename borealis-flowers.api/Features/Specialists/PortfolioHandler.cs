using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using IO = System.IO;

namespace borealis_flowers.api.Features.Specialists;

public static class PortfolioHandler
{
    public sealed class PortfolioWorkDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
        public string? Title { get; set; }
        public int SortOrder { get; set; }
    }

    public sealed class PortfolioDetailDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string ImgUrl { get; set; } = "";
        public string City { get; set; } = "";
        public string Specialization { get; set; } = "";
        public string StyleDescription { get; set; } = "";
        public List<PortfolioWorkDto> Works { get; set; } = [];
    }

    public sealed class UpdatePortfolioDto
    {
        public string? FullName { get; set; }
        public string? City { get; set; }
        public string? StyleDescription { get; set; }
        public string? ImgUrl { get; set; }
    }

    public sealed class AddPortfolioWorkDto
    {
        public string ImageUrl { get; set; } = "";
        public string? Title { get; set; }
    }

    public static async Task<IResult> GetPublicAsync(Guid id, DataContext db)
    {
        Specialist? specialist = await db.Specialists.AsNoTracking()
            .Include(s => s.Specialization)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (specialist is null)
            return Results.NotFound();

        List<PortfolioWorkDto> works = await LoadWorksAsync(db, id);
        return Results.Ok(ToDetail(specialist, works));
    }

    public static async Task<IResult> GetMineAsync(ClaimsPrincipal user, DataContext db)
    {
        Guid? specialistId = await TryOwnedSpecialistIdAsync(user, db);
        if (specialistId is null)
            return Results.Forbid();

        Specialist? specialist = await db.Specialists.AsNoTracking()
            .Include(s => s.Specialization)
            .FirstOrDefaultAsync(s => s.Id == specialistId);

        if (specialist is null)
            return Results.NotFound();

        List<PortfolioWorkDto> works = await LoadWorksAsync(db, specialistId.Value);
        return Results.Ok(ToDetail(specialist, works));
    }

    public static async Task<IResult> UpdateMineAsync(
        UpdatePortfolioDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? specialistId = await TryOwnedSpecialistIdAsync(user, db);
        if (specialistId is null)
            return Results.Forbid();

        Specialist? specialist = await db.Specialists.FindAsync(specialistId.Value);
        if (specialist is null)
            return Results.NotFound();

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            string name = dto.FullName.Trim();
            specialist.FullName = name;
            await db.Customers
                .Where(c => c.SpecialistId == specialistId && c.IsSpecialist && !c.IsAdmin)
                .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Name, name));
        }

        if (dto.City is not null)
            specialist.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();

        if (dto.StyleDescription is not null)
            specialist.StyleDescription = string.IsNullOrWhiteSpace(dto.StyleDescription)
                ? null
                : dto.StyleDescription.Trim();

        if (dto.ImgUrl is not null)
            specialist.ImgUrl = dto.ImgUrl.Trim();

        await db.SaveChangesAsync();

        List<PortfolioWorkDto> works = await LoadWorksAsync(db, specialistId.Value);
        return Results.Ok(ToDetail(specialist, works));
    }

    public static async Task<IResult> AddWorkAsync(
        AddPortfolioWorkDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? specialistId = await TryOwnedSpecialistIdAsync(user, db);
        if (specialistId is null)
            return Results.Forbid();

        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
            return Results.BadRequest("Укажите URL фото.");

        int maxOrder = await db.SpecialistPortfolioWorks
            .Where(w => w.SpecialistId == specialistId)
            .Select(w => (int?)w.SortOrder)
            .MaxAsync() ?? -1;

        var work = new SpecialistPortfolioWork
        {
            Id = Guid.NewGuid(),
            SpecialistId = specialistId.Value,
            ImageUrl = dto.ImageUrl.Trim(),
            Title = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim(),
            SortOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow,
        };

        await db.SpecialistPortfolioWorks.AddAsync(work);
        await db.SaveChangesAsync();

        return Results.Ok(new PortfolioWorkDto
        {
            Id = work.Id,
            ImageUrl = work.ImageUrl,
            Title = work.Title,
            SortOrder = work.SortOrder,
        });
    }

    public static async Task<IResult> DeleteWorkAsync(
        Guid workId,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? specialistId = await TryOwnedSpecialistIdAsync(user, db);
        if (specialistId is null)
            return Results.Forbid();

        SpecialistPortfolioWork? work = await db.SpecialistPortfolioWorks
            .FirstOrDefaultAsync(w => w.Id == workId && w.SpecialistId == specialistId);

        if (work is null)
            return Results.NotFound();

        db.SpecialistPortfolioWorks.Remove(work);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    public static async Task<IResult> UploadPhotoAsync(
        IFormFile file,
        ClaimsPrincipal user,
        DataContext db,
        IWebHostEnvironment env)
    {
        Guid? specialistId = await TryOwnedSpecialistIdAsync(user, db);
        if (specialistId is null)
            return Results.Forbid();

        if (file.Length == 0)
            return Results.BadRequest("Файл пустой.");

        string ext = IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jfif" or ".jpg" or ".jpeg" or ".png" or ".webp"))
            return Results.BadRequest("Поддерживаются JPG, PNG, WEBP, JFIF.");

        string? solutionRoot = IO.Directory.GetParent(env.ContentRootPath)?.FullName;
        if (solutionRoot is null)
            return Results.Problem("Не найден каталог решения.");

        string targetDir = IO.Path.Combine(
            solutionRoot,
            "borealis-flowers.ui",
            "wwwroot",
            "images",
            "florists",
            specialistId.Value.ToString("D"));

        IO.Directory.CreateDirectory(targetDir);

        string safeStem = IO.Path.GetFileNameWithoutExtension(file.FileName)
            .Replace(" ", "-", StringComparison.Ordinal);
        string fileName = $"{Guid.NewGuid():N}-{safeStem}{ext}";
        string targetPath = IO.Path.Combine(targetDir, fileName);

        await using (IO.FileStream stream = IO.File.Create(targetPath))
        {
            await file.CopyToAsync(stream);
        }

        string url = $"/images/florists/{specialistId.Value:D}/{fileName}";
        return Results.Ok(new { url });
    }

    static async Task<List<PortfolioWorkDto>> LoadWorksAsync(DataContext db, Guid specialistId) =>
        await db.SpecialistPortfolioWorks.AsNoTracking()
            .Where(w => w.SpecialistId == specialistId)
            .OrderBy(w => w.SortOrder)
            .ThenBy(w => w.CreatedAt)
            .Select(w => new PortfolioWorkDto
            {
                Id = w.Id,
                ImageUrl = w.ImageUrl,
                Title = w.Title,
                SortOrder = w.SortOrder,
            })
            .ToListAsync();

    static PortfolioDetailDto ToDetail(Specialist specialist, List<PortfolioWorkDto> works) =>
        new()
        {
            Id = specialist.Id,
            FullName = specialist.FullName,
            ImgUrl = specialist.ImgUrl,
            City = specialist.City ?? "",
            Specialization = specialist.Specialization?.Name ?? "",
            StyleDescription = specialist.StyleDescription ?? "",
            Works = works,
        };

    static async Task<Guid?> TryOwnedSpecialistIdAsync(ClaimsPrincipal user, DataContext db)
    {
        string? role = user.FindFirstValue(ClaimTypes.Role);
        if (role is not ("Florist" or "Admin"))
            return null;

        string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (sub is null || !Guid.TryParse(sub, out Guid customerId))
            return null;

        Customer? customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        return customer?.SpecialistId;
    }
}

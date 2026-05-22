using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Infrastructure;

/// <summary>
/// Копирует фото букетов из flowers/katalog в wwwroot UI и проставляет ImageUrl в Services.
/// </summary>
public static class BouquetCatalogSeeder
{
    static readonly string[] Extensions = [".jfif", ".jpg", ".jpeg", ".png", ".webp"];

    public static async Task ApplyAsync(DataContext db, string apiContentRootPath)
    {
        string? solutionRoot = Directory.GetParent(apiContentRootPath)?.FullName;
        if (solutionRoot is null)
            return;

        string sourceDir = Path.Combine(solutionRoot, "flowers", "katalog");
        string targetDir = Path.Combine(solutionRoot, "borealis-flowers.ui", "wwwroot", "images", "bouquets");

        if (!Directory.Exists(sourceDir))
            return;

        Directory.CreateDirectory(targetDir);

        var imagesByStem = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string ext = Path.GetExtension(file);
            if (!Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                continue;

            string stem = Path.GetFileNameWithoutExtension(file);
            string targetName = stem + ext.ToLowerInvariant();
            string targetPath = Path.Combine(targetDir, targetName);
            File.Copy(file, targetPath, overwrite: true);
            imagesByStem[stem] = $"/images/bouquets/{targetName}";
        }

        if (imagesByStem.Count == 0)
            return;

        List<Service> services = await db.Services.ToListAsync();
        bool changed = false;

        foreach (Service service in services)
        {
            if (!imagesByStem.TryGetValue(service.Name.Trim(), out string? url))
                continue;

            if (string.Equals(service.ImageUrl, url, StringComparison.OrdinalIgnoreCase))
                continue;

            service.ImageUrl = url;
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync();
    }
}

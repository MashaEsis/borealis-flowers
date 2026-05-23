using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Home;

public static class HomeHandler
{
    public sealed class HomeHighlightsDto
    {
        public TopFloristDto? TopFlorist { get; set; }
        public PopularBouquetDto? PopularBouquet { get; set; }
    }

    public sealed class TopFloristDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string ImgUrl { get; set; } = "";
        public int CompletedOrders { get; set; }
    }

    public sealed class PopularBouquetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
        public int OrderCount { get; set; }
    }

    public static async Task<IResult> GetHighlightsAsync(DataContext db)
    {
        DateTime monthStart = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var floristStats = await db.Requests.AsNoTracking()
            .Where(r =>
                r.OrderKind == OrderKind.Bouquet &&
                r.OrderStatus == OrderStatus.Completed &&
                r.CompletedAtUtc >= monthStart &&
                r.SpecialistId != null)
            .GroupBy(r => r.SpecialistId!.Value)
            .Select(g => new { SpecialistId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        TopFloristDto? topFlorist = null;
        if (floristStats is not null)
        {
            Specialist? specialist = await db.Specialists.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == floristStats.SpecialistId);
            if (specialist is not null)
            {
                topFlorist = new TopFloristDto
                {
                    Id = specialist.Id,
                    FullName = specialist.FullName,
                    ImgUrl = string.IsNullOrWhiteSpace(specialist.ImgUrl)
                        ? $"https://picsum.photos/seed/florist-{specialist.Id:N}/480/560"
                        : specialist.ImgUrl,
                    CompletedOrders = floristStats.Count,
                };
            }
        }

        var bouquetStats = await db.Requests.AsNoTracking()
            .Where(r =>
                r.OrderKind == OrderKind.Bouquet &&
                r.ServiceId != null &&
                r.CreatedAt >= monthStart)
            .GroupBy(r => r.ServiceId!.Value)
            .Select(g => new { ServiceId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        PopularBouquetDto? popular = null;
        if (bouquetStats is not null)
        {
            Service? service = await db.Services.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == bouquetStats.ServiceId);
            if (service is not null)
            {
                popular = new PopularBouquetDto
                {
                    Id = service.Id,
                    Name = service.Name,
                    Price = service.Price,
                    ImageUrl = service.ImageUrl,
                    OrderCount = bouquetStats.Count,
                };
            }
        }

        if (popular is null)
        {
            Service? fallback = await db.Services.AsNoTracking()
                .OrderBy(s => s.Name)
                .FirstOrDefaultAsync();
            if (fallback is not null)
            {
                popular = new PopularBouquetDto
                {
                    Id = fallback.Id,
                    Name = fallback.Name,
                    Price = fallback.Price,
                    ImageUrl = fallback.ImageUrl,
                    OrderCount = 0,
                };
            }
        }

        return Results.Ok(new HomeHighlightsDto
        {
            TopFlorist = topFlorist,
            PopularBouquet = popular,
        });
    }
}

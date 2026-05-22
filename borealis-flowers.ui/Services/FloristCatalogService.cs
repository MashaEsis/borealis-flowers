using System.Net.Http.Json;
using System.Text.Json.Serialization;
using borealis_flowers.ui.Models;

namespace borealis_flowers.ui.Services;

public sealed class FloristCatalogService
{
    readonly List<FloristVm> _cache = [];

    public IReadOnlyList<FloristVm> Cached => _cache;

    /// <summary>
    /// Загружает флористов с API borealis-flowers.api или локальный демо-список.
    /// </summary>
    public async Task LoadFloristsAsync(HttpClient httpClient, BackendSettings backend)
    {
        _cache.Clear();
        Uri? backendUri =
            Uri.TryCreate(string.IsNullOrWhiteSpace(backend.BackendApiUrl)
                ? null
                : backend.BackendApiUrl.Trim(), UriKind.Absolute, out var uri)
                ? uri
                : null;

        async Task FallbackDemoAsync()
        {
            Guid id1 = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7");
            Guid id2 = Guid.Parse("88639ec4-d834-4788-bce4-05cfce258cce");

            foreach (FloristVm f in new[]
                     {
                         new FloristVm
                         {
                             Id = id1, FullName = "Александра Серова", ImgUrl =
                                 "https://images.unsplash.com/photo-1455659817273-f96807779a38?w=400&auto=format&fit=crop&q=70",
                             City = "Казань",
                             Specialization = "Студийные композиции",
                             StyleDescription = "Нежный минимализм, сезонные текстуры и спокойная палитра."
                         },
                         new FloristVm
                         {
                             Id = id2, FullName = "Марк Тюльпанов", ImgUrl =
                                 "https://images.unsplash.com/photo-1466692476869-aefc1fc43f73?w=400&auto=format&fit=crop&q=70",
                             City = "Казань",
                             Specialization = "Мероприятия под ключ",
                             StyleDescription = "Крупные инсталляции, свадебный декор и корпоративное оформление."
                         }
                     })
                _cache.Add(f);
            await Task.CompletedTask;
        }

        if (backendUri is null)
        {
            await FallbackDemoAsync();
            return;
        }

        using HttpClient scoped = new() { BaseAddress = backendUri };

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            var apiList = await scoped.GetFromJsonAsync<List<SpecialistApiDto>>(
                "specialists",
                cts.Token);

            if (apiList is null || apiList.Count == 0)
            {
                await FallbackDemoAsync();
                return;
            }

            foreach (SpecialistApiDto row in apiList)
            {
                _cache.Add(new FloristVm
                {
                    Id = row.Id,
                    FullName = row.FullName,
                    ImgUrl = string.IsNullOrWhiteSpace(row.ImgUrl)
                        ? PortfolioUrl(row.Id, "primary")
                        : row.ImgUrl.Trim(),
                    City = row.City ?? "",
                    Specialization = row.Specialization ?? "",
                    StyleDescription = DescribeStyle(row.Specialization),
                });
            }
        }
        catch
        {
            await FallbackDemoAsync();
        }
    }

    public static IEnumerable<string> BuildPortfolioGallery(Guid floristId) =>
        BuildPortfolioPreview(floristId, 6);

    public static IEnumerable<string> BuildPortfolioPreview(Guid floristId, int count = 4) =>
        PortfolioSeeds.Take(count).Select(seed => PortfolioUrl(floristId, seed));

    static readonly string[] PortfolioSeeds =
        ["peony-b", "orchid-z", "wedding-alt", "table-set", "arch-fl", "hand-tie-q"];

    static string PortfolioUrl(Guid floristId, string variation)
    {
        string nf = floristId.ToString("N");
        string prefix = nf.Length >= 8 ? nf[..8] : nf;
        return $"https://picsum.photos/seed/{prefix}-{variation}/480/560";
    }

    static string DescribeStyle(string? specialization) =>
        specialization switch
        {
            "Hair" or "Nail" or "Skincare" or "Makeup" =>
                "Авторские букеты в сдержанной эстетике: воздух, линия и сезонные оттенки.",
            null or "" =>
                "Индивидуальный подход к композиции и подбору цветов под ваше событие.",
            _ => $"Стиль: {specialization}. Аккуратные композиции без визуального шума.",
        };

    private sealed record SpecialistApiDto
    {
        [JsonPropertyName("id")] public Guid Id { get; init; }

        [JsonPropertyName("fullName")] public string FullName { get; init; } = "";

        [JsonPropertyName("imgUrl")] public string ImgUrl { get; init; } = "";

        [JsonPropertyName("city")] public string? City { get; init; }

        [JsonPropertyName("specialization")] public string? Specialization { get; init; }
    };
}

namespace borealis_flowers.ui.Models;

public sealed class HomeHighlightsDto
{
    public TopFloristHighlightDto? TopFlorist { get; set; }

    public PopularBouquetHighlightDto? PopularBouquet { get; set; }
}

public sealed class TopFloristHighlightDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = "";

    public string ImgUrl { get; set; } = "";

    public int CompletedOrders { get; set; }
}

public sealed class PopularBouquetHighlightDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public int OrderCount { get; set; }
}

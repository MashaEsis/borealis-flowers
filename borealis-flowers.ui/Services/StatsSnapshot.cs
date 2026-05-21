namespace borealis_flowers.ui.Services;

public sealed record StatsSnapshot(
    int BouquetsCompleted,
    int EventsCompleted,
    int ActiveOrders,
    decimal TotalRevenue,
    int FlowerSkuCount,
    IReadOnlyDictionary<string, int> FloristWorkload,
    decimal AverageOrderValueCompleted);

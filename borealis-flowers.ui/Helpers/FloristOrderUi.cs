namespace borealis_flowers.ui.Helpers;

public static class FloristOrderUi
{
    public static readonly (int Status, string Label)[] BouquetSteps =
    [
        (0, "Новый"),
        (1, "В обработке"),
        (4, "Принят"),
        (5, "Готов"),
        (8, "У курьера"),
    ];

    public static bool IsBouquetLocked(int status) =>
        status is 7 or 8 or 6;

    public static bool CanFloristChange(int current, int target) =>
        (current, target) switch
        {
            (0, 1) => true,
            (0, 7) => true,
            (1, 4) => true,
            (1, 7) => true,
            (4, 5) => true,
            (5, 8) => true,
            _ when current == target => false,
            _ => false,
        };

    public static IEnumerable<(int Status, string Label)> NextActions(int current)
    {
        if (IsBouquetLocked(current))
            yield break;

        foreach ((int status, string label) in current switch
                 {
                     0 => new[] { (1, "В обработку"), (7, "Отменить") },
                     1 => new[] { (4, "Принять заказ"), (7, "Отменить") },
                     4 => new[] { (5, "Готов") },
                     5 => new[] { (8, "Передать курьеру") },
                     _ => Array.Empty<(int, string)>(),
                 })
            yield return (status, label);
    }

    public static bool IsStepReached(int current, int step) =>
        current switch
        {
            7 => step == 0,
            6 => true,
            8 => step is 0 or 1 or 4 or 5 or 8,
            5 => step is 0 or 1 or 4 or 5,
            4 => step is 0 or 1 or 4,
            1 => step is 0 or 1,
            _ => step == 0,
        };
}

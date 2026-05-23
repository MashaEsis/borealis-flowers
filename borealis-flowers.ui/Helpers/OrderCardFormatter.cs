using borealis_flowers.ui.Models;

namespace borealis_flowers.ui.Helpers;

public static class OrderCardFormatter
{
    public static bool IsBouquet(OrderRowDto o) => o.OrderKind == 0;

    public static string Title(OrderRowDto o) =>
        IsBouquet(o)
            ? (string.IsNullOrWhiteSpace(o.ServiceTitleSnapshot) ? "Букет" : o.ServiceTitleSnapshot)
            : EventTypeName(o.EventType);

    public static string FormatWhen(DateTime utc) =>
        utc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    public static string ShortLine(OrderRowDto o)
    {
        if (IsBouquet(o))
        {
            string? addr = ShortText(o.DeliveryAddress, 72);
            if (!string.IsNullOrWhiteSpace(addr))
                return $"Доставка: {addr}";
            return "Доставка по адресу уточняется";
        }

        if (o.EventStartsAtUtc is DateTime when)
            return $"Дата: {FormatWhen(when)}";

        return string.IsNullOrWhiteSpace(o.Venue) ? "Место уточняется" : ShortText(o.Venue, 72)!;
    }

    public static string? PriceLine(OrderRowDto o)
    {
        if (o.ChargedAmount is double charged)
            return $"{charged:N0} ₽";

        if (o.QuoteTotal is double quote)
            return $"{quote:N0} ₽";

        if (IsBouquet(o) && !string.IsNullOrWhiteSpace(o.Description))
        {
            int idx = o.Description.IndexOf('₽', StringComparison.Ordinal);
            if (idx > 0)
            {
                int dash = o.Description.LastIndexOf('—', idx);
                if (dash >= 0 && idx - dash <= 12)
                    return o.Description[(dash + 1)..idx].Trim() + " ₽";
            }
        }

        return null;
    }

    public static string EventTypeName(int? eventType) =>
        eventType switch
        {
            0 => "Свадьба",
            1 => "Корпоратив",
            2 => "День рождения",
            3 => "Юбилей",
            4 => "Другое",
            _ => "Мероприятие",
        };

    public static IEnumerable<(int Status, string Label, bool Done)> StatusTimeline(OrderRowDto o)
    {
        if (IsBouquet(o))
        {
            (int, string)[] steps =
            [
                (0, "Новый"),
                (1, "В обработке"),
                (4, "Принят"),
                (5, "Готов"),
                (8, "Передано курьеру"),
                (6, "Получено"),
            ];

            if (o.OrderStatus == 7)
            {
                yield return (7, "Отменён", true);
                yield break;
            }

            foreach ((int status, string label) in steps)
                yield return (status, label, IsStatusReached(o.OrderStatus, status));
        }
        else
        {
            (int, string)[] steps =
            [
                (0, "Новая заявка"),
                (2, "Согласование"),
                (4, "Подтверждено"),
                (1, "В работе"),
                (5, "Готово"),
                (6, "Завершено"),
            ];

            if (o.OrderStatus == 7)
            {
                yield return (7, "Отменено", true);
                yield break;
            }

            foreach ((int status, string label) in steps)
                yield return (status, label, IsStatusReached(o.OrderStatus, status));
        }
    }

    static bool IsStatusReached(int current, int step) =>
        current switch
        {
            6 => true,
            7 => false,
            8 => step is 0 or 1 or 4 or 5 or 8,
            5 => step is 0 or 1 or 4 or 5,
            4 => step is 0 or 1 or 4,
            1 => step is 0 or 1,
            2 or 3 => step is 0 or 2 or 3,
            _ => step == 0,
        };

    static string? ShortText(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        string t = text.Trim();
        if (t.Length <= max)
            return t;

        return t[..max].TrimEnd() + "…";
    }
}

using borealis_flowers.ui.Models;

namespace borealis_flowers.ui.Helpers;

public static class OrderUiFormatter
{
    public static string OccasionRussian(EventOccasionKind kind) =>
        kind switch
        {
            EventOccasionKind.Wedding => "Свадьба",
            EventOccasionKind.Corporate => "Корпоратив",
            EventOccasionKind.Birthday => "День рождения",
            EventOccasionKind.Anniversary => "Годовщина",
            EventOccasionKind.Jubilee => "Юбилей",
            _ => "Мероприятие",
        };

    public static string PhaseRussian(FloristOrderPhase phase, OrderKind kind) =>
        phase switch
        {
            FloristOrderPhase.New => kind == OrderKind.Bouquet
                ? "Новый букет"
                : "Новое мероприятие",

            FloristOrderPhase.AwaitingMaterials => "Ожидает материалов",

            FloristOrderPhase.InProgress => "В обработке",

            FloristOrderPhase.ReadySent => "Готов и отправлен",

            FloristOrderPhase.EnRoute =>
                kind == OrderKind.Event ? "Выезд к клиенту" : "Не применимо",

            FloristOrderPhase.AwaitingCompletionConfirm => "Ожидание совместного подтверждения",
            FloristOrderPhase.Completed => "Выполнен",
            _ => phase.ToString(),
        };

    public static string TripleProgress(StudioOrderDto order) =>
        $"Флорист {(order.FloristCompletionAcknowledged ? "✓" : "…")} · Клиент {(order.ClientCompletionConfirmed ? "✓" : "…")} · Админ {(order.AdminCompletionConfirmed ? "✓" : "…")}";
}

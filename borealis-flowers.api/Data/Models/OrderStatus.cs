namespace borealis_flowers.api.Data.Models;

/// <summary>Статус заявки / заказа (колонка SQLite &quot;State&quot;).</summary>
public enum OrderStatus
{
    New = 0,
    InProgress = 1,
    /// <summary>Согласование материалов (мероприятия).</summary>
    MaterialNegotiation = 2,
    AwaitingApproval = 3,
    Approved = 4,
    Ready = 5,
    Completed = 6,
    Rejected = 7,
    /// <summary>Букет передан курьеру (отсчёт 20 мин до авто-получения).</summary>
    HandedToCourier = 8,
}

namespace borealis_flowers.ui.Helpers;

public static class OrderStateUi
{
    public static string Russian(int state, int orderKind = 0) =>
        state switch
        {
            0 => "Новый",
            1 => orderKind == 1 ? "В работе" : "В обработке",
            2 => "Согласование",
            3 => "Ожидает одобрения",
            4 => orderKind == 1 ? "Подтверждён" : "Принят",
            5 => "Готов",
            6 => "Получено",
            7 => "Отменён",
            8 => "У курьера",
            _ => "—",
        };

    public static string CssClass(int state) =>
        state switch
        {
            0 => "status-new",
            1 => "status-progress",
            2 or 3 => "status-negotiation",
            4 => "status-approved",
            5 => "status-ready",
            6 => "status-done",
            7 => "status-cancelled",
            8 => "status-courier",
            _ => "status-default",
        };

    public static bool ClientCanCancel(int orderKind, int orderStatus) =>
        orderKind == 0 && orderStatus is 0 or 1;

    public static bool ClientCanConfirmReceived(int orderStatus) =>
        orderStatus == 8;
}

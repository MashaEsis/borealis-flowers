namespace borealis_flowers.ui.Helpers;

public static class OrderStateUi
{
    public static string Russian(int state, int orderKind = 0) =>
        state switch
        {
            0 => "Новый",
            1 => orderKind == 1 ? "В работе" : "В обработке",
            2 => "Согласование материалов",
            3 => "Ожидает одобрения",
            4 => orderKind == 1 ? "Подтверждён" : "Принят",
            5 => "Готов",
            6 => "Получено",
            7 => "Отменён",
            8 => "Передано курьеру",
            _ => "—",
        };

    public static bool ClientCanCancel(int orderKind, int orderStatus) =>
        orderKind == 0 && orderStatus is 0 or 1;

    public static bool ClientCanConfirmReceived(int orderStatus) =>
        orderStatus == 8;
}

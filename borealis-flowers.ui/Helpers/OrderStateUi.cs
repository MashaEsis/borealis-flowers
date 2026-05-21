namespace borealis_flowers.ui.Helpers;

public static class OrderStateUi
{
    /// <inheritdoc cref="OrderStateUi" />
    public static string Russian(int state) =>
        state switch
        {
            0 => "Новый",
            1 => "В обработке",
            2 => "Согласование материалов",
            3 => "Ожидает одобрения",
            4 => "Подтверждён",
            5 => "Готов к выдаче",
            6 => "Завершён",
            7 => "Отклонён",
            _ => "—",
        };
}

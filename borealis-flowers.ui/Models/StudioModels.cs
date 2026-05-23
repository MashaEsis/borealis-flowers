namespace borealis_flowers.ui.Models;

public enum StudioRole
{
    Visitor,
    Client,
    Florist,
    Admin
}

public enum OrderKind
{
    Bouquet,
    Event
}

public enum EventOccasionKind
{
    Wedding,
    Anniversary,
    Corporate,
    Birthday,
    Jubilee
}

public enum FloristOrderPhase
{
    /// <summary>Новый заказ или без заявки на склад.</summary>
    New,

    /// <summary>Не хватает цветов — заявка отправлена администратору.</summary>
    AwaitingMaterials,

    /// <summary>Цветы есть, сборка начата.</summary>
    InProgress,

    /// <summary>Букет готов и отправлен (или готов к отгрузке).</summary>
    ReadySent,

    /// <summary>Только мероприятия: выезд к клиенту.</summary>
    EnRoute,

    /// <summary>После «Завершить» от флориста — ждём подтверждений клиента и админа.</summary>
    AwaitingCompletionConfirm,

    /// <summary>Заказ архивирован (выполнен).</summary>
    Completed
}

public sealed class FloristVm
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = "";
    public string ImgUrl { get; init; } = "";
    public string City { get; init; } = "";
    public string Specialization { get; init; } = "";
    public string StyleDescription { get; init; } = "";
    public IReadOnlyList<string> PortfolioPreview { get; init; } = [];
}

public sealed class WarehouseItemDto
{
    public string FlowerName { get; set; } = "";
    public int Quantity { get; set; }
}

/// <summary>Заявка флориста на пополнение под конкретный заказ.</summary>
public sealed class SupplyRequestDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid FloristId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Approved { get; set; }
    public List<FlowerDemandLine> Lines { get; set; } = [];
}

public sealed class FlowerDemandLine
{
    public string FlowerName { get; init; } = "";
    public int Quantity { get; init; }
}

/// <summary>Заказ букета или мероприятия (клиент/гость).</summary>
public sealed class StudioOrderDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public OrderKind Kind { get; set; }
    public EventOccasionKind? OccasionKind { get; set; }
    public Guid FloristId { get; set; }
    public string FloristName { get; set; } = "";

    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Email { get; set; }

    public string WishDescription { get; set; } = "";
    public DateOnly? EventDate { get; set; }

    public FloristOrderPhase Phase { get; set; } = FloristOrderPhase.New;
    public decimal PriceQuoted { get; set; }

    /// <summary>Финальное подтверждение от флориста (после выезда/работ).</summary>
    public bool FloristCompletionAcknowledged { get; set; }

    public bool ClientCompletionConfirmed { get; set; }
    public bool AdminCompletionConfirmed { get; set; }

    /// <summary>Идём по цепочке подтверждений только для мероприятий.</summary>
    public bool RequiresTripleCompletion => Kind == OrderKind.Event;

    public Guid? PlacedByCustomerGuid { get; set; }

    /// <summary>Гость без регистрации.</summary>
    public bool IsGuestOrder => PlacedByCustomerGuid is null;

    /// <summary>Для списков и статистики.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

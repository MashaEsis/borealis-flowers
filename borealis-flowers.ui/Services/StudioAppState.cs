using System.Text.Json;
using System.Text.Json.Serialization;
using borealis_flowers.ui.Models;

namespace borealis_flowers.ui.Services;

public sealed class StudioAppState
{
    readonly List<StudioOrderDto> _orders = [];
    readonly List<WarehouseItemDto> _warehouse = [];
    readonly List<SupplyRequestDto> _supply = [];

    public event Action? Changed;

    JsonSerializerOptions PersistOptions => CreatePersistOptions();

    static JsonSerializerOptions CreatePersistOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        return options;
    }

    static bool IsArchived(FloristOrderPhase phase) =>
        phase is FloristOrderPhase.Completed;

    public IReadOnlyList<StudioOrderDto> OrdersByRecencyDescending() =>
        _orders.OrderByDescending(o => o.CreatedAtUtc).ToList();

    public IReadOnlyList<WarehouseItemDto> WarehouseSorted() =>
        _warehouse.OrderBy(w => w.FlowerName).ToList();

    public IReadOnlyList<SupplyRequestDto> SupplyRequestsSorted() =>
        _supply.OrderByDescending(s => s.CreatedAt).ToList();

    public IEnumerable<StudioOrderDto> OrdersForFlorist(Guid floristId) =>
        _orders.Where(o => o.FloristId == floristId);

    public IEnumerable<StudioOrderDto> WorkflowOrdersFor(Guid floristId) =>
        OrdersForFlorist(floristId)
            .Where(o => !IsArchived(o.Phase));

    public IEnumerable<StudioOrderDto> ClientTriplePending(Guid clientGuid) =>
        _orders.Where(o =>
            o.RequiresTripleCompletion &&
            o.PlacedByCustomerGuid == clientGuid &&
            o.Phase == FloristOrderPhase.AwaitingCompletionConfirm &&
            !o.ClientCompletionConfirmed);

    public IEnumerable<StudioOrderDto> AdminTriplePending() =>
        _orders.Where(o =>
            o.RequiresTripleCompletion &&
            o.Phase == FloristOrderPhase.AwaitingCompletionConfirm &&
            !o.AdminCompletionConfirmed);

    public IEnumerable<StudioOrderDto> FloristTriplePending(Guid floristId) =>
        _orders.Where(o =>
            o.FloristId == floristId &&
            o.RequiresTripleCompletion &&
            o.Phase == FloristOrderPhase.AwaitingCompletionConfirm &&
            !o.FloristCompletionAcknowledged);

    /// <inheritdoc />
    public ResultMessage ValidateFlorist(StudioOrderDto? order, Guid actingFlorist) =>
        order switch
        {
            null => new ResultMessage(false, "Заказ не найден."),
            StudioOrderDto o when o.FloristId != actingFlorist =>
                new ResultMessage(false, "Этот заказ назначен другому флористу."),
            _ => new ResultMessage(true, ""),
        };

    public StatsSnapshot Statistics()
    {
        List<StudioOrderDto> archived = _orders.Where(o => IsArchived(o.Phase)).ToList();

        IEnumerable<StudioOrderDto> active = _orders.Where(o => !IsArchived(o.Phase));

        Dictionary<string, int> workload =
            active.GroupBy(o => string.IsNullOrWhiteSpace(o.FloristName) ? $"id:{o.FloristId:N}" : o.FloristName!)
                .ToDictionary(g => g.Key, g => g.Count());

        decimal avgCompleted = archived.Count == 0
            ? decimal.Zero
            : archived.Where(o => o.PriceQuoted > 0).Average(o => o.PriceQuoted);

        return new StatsSnapshot(
            archived.Count(o => o.Kind == OrderKind.Bouquet),
            archived.Count(o => o.Kind == OrderKind.Event),
            active.Count(o => o.Phase is not FloristOrderPhase.AwaitingMaterials),
            archived.Sum(o => o.PriceQuoted),
            _warehouse.Select(w => w.FlowerName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            workload,
            avgCompleted);
    }

    public async Task HydrateAsync(BrowserStorageService storage)
    {
        string? raw = await storage.GetRawAsync(BrowserStorageService.StateKey);

        PersistedStudioBundle bundle = DeserializeBundle(raw);

        _orders.Clear();
        _warehouse.Clear();
        _supply.Clear();

        foreach (WarehouseItemDto w in bundle.Warehouse)
            UpsertFlower(w.FlowerName, w.Quantity, replaceQty: true);

        foreach (StudioOrderDto o in bundle.Orders.OrderByDescending(x => x.CreatedAtUtc))
            _orders.Add(o);

        _supply.AddRange(bundle.SupplyRequests);

        if (_warehouse.Count == 0)
            SeedWarehouseDefaults();

        foreach (WarehouseItemDto w in _warehouse)
            w.FlowerName = NormalizeFlowerName(w.FlowerName);

        Notify();
    }

    PersistedStudioBundle DeserializeBundle(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new PersistedStudioBundle();

        try
        {
            return JsonSerializer.Deserialize<PersistedStudioBundle>(
                       raw,
                       DeserializeOptions())
                   ?? new PersistedStudioBundle();
        }
        catch
        {
            return new PersistedStudioBundle();
        }
    }

    static JsonSerializerOptions DeserializeOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return options;
    }

    public Task PersistAsync(BrowserStorageService storage)
    {
        PersistedStudioBundle bundle = new()
        {
            Orders = _orders.ToList(),
            Warehouse = WarehouseSorted().ToList(),
            SupplyRequests = _supply.ToList(),
        };

        return storage
            .SetRawAsync(BrowserStorageService.StateKey, JsonSerializer.Serialize(bundle, PersistOptions))
            .AsTask();
    }

    public async Task SaveAsync(BrowserStorageService storage)
    {
        await PersistAsync(storage);
        Notify();
    }

    void Notify() => Changed?.Invoke();

    void SeedWarehouseDefaults()
    {
        foreach ((string Flower, int Qty) tuple in SeedRows())
            UpsertFlower(tuple.Flower, tuple.Qty, replaceQty: false);
    }

    static IEnumerable<(string Flower, int Qty)> SeedRows() =>
        [("роза эквадорская", 120), ("пион Coral Charm", 65), ("фрезия", 80), ("эвкалипт", 200)];

    /// <inheritdoc />
    public void ManualStockAdjust(string flowerName, int qtyDeltaOrReplace)
    {
        UpsertFlower(flowerName, qtyDeltaOrReplace, replaceQty: qtyDeltaOrReplace <= 9999);
        // выше упрощаем — для пополнения вручную: админ задаёт финальную цифру (replace)
        // для delta вызовем отдельный метод ниже если нужно
    }

    public void WarehouseSetQuantityExact(string flowerName, int qty)
    {
        string name = NormalizeFlowerName(flowerName);
        WarehouseItemDto? existing = Match(name);
        if (existing is null)
        {
            if (qty <= 0) return;

            _warehouse.Add(new WarehouseItemDto { FlowerName = name, Quantity = qty });
            return;
        }

        if (qty <= 0)
        {
            _warehouse.Remove(existing);

            return;
        }

        existing.Quantity = qty;
        existing.FlowerName = name;
    }

    public void UpsertFlower(string flowerName, int qty, bool replaceQty)
    {
        string normalized = NormalizeFlowerName(flowerName);
        WarehouseItemDto? existing = Match(normalized);

        if (existing is null)
        {
            if ((replaceQty ? qty : qty) <= 0) return;

            _warehouse.Add(new WarehouseItemDto
            {
                FlowerName = normalized,
                Quantity = Math.Max(qty, 0),
            });

            return;
        }

        if (replaceQty)
            existing.Quantity = Math.Max(0, qty);
        else
            existing.Quantity = Math.Max(0, existing.Quantity + qty);

        existing.FlowerName = normalized;

        if (existing.Quantity == 0)
            _warehouse.Remove(existing);
    }

    WarehouseItemDto? Match(string normalized) =>
        _warehouse.FirstOrDefault(w =>
            NormalizeFlowerKey(w.FlowerName) == NormalizeFlowerKey(normalized));

    static string NormalizeFlowerName(string name) =>
        string.IsNullOrWhiteSpace(name) ? "позиция" : name.Trim();

    static string NormalizeFlowerKey(string name) =>
        NormalizeFlowerName(name).ToLowerInvariant();

    public StudioOrderDto AddOrder(StudioOrderDto order)
    {
        StudioOrderDto copy = Clone(order);
        copy.Id = Guid.NewGuid();
        copy.CreatedAtUtc = DateTime.UtcNow;
        copy.Phase = FloristOrderPhase.New;
        copy.FloristCompletionAcknowledged = false;
        copy.ClientCompletionConfirmed = false;
        copy.AdminCompletionConfirmed = false;

        copy.PriceQuoted = EstimateQuote(copy);

        _orders.Add(copy);

        Notify();
        return copy;
    }

    static decimal EstimateQuote(StudioOrderDto dto) =>
        dto.Kind switch
        {
            OrderKind.Bouquet =>
                decimal.Round(4_599m + (97m * (dto.WishDescription?.Length ?? 0)), 2),

            OrderKind.Event => decimal.Round(24_799m + dto.OccasionKind switch
            {
                EventOccasionKind.Wedding => 12_900m,

                EventOccasionKind.Corporate => 13_499m,

                EventOccasionKind.Birthday or EventOccasionKind.Anniversary or EventOccasionKind.Jubilee =>
                    3_699m,

                _ => 0m,
            }, 2),

            _ => decimal.Round(4_599m, 2),
        };

    StudioOrderDto Clone(StudioOrderDto o) =>
        new()
        {
            Kind = o.Kind,
            OccasionKind = o.OccasionKind,
            FloristId = o.FloristId,
            FloristName = o.FloristName,
            CustomerName = o.CustomerName,
            Phone = o.Phone,
            Address = o.Address,
            Email = o.Email,
            WishDescription = o.WishDescription,
            EventDate = o.EventDate,
            Phase = o.Phase,
            PriceQuoted = o.PriceQuoted,
            FloristCompletionAcknowledged = o.FloristCompletionAcknowledged,

            ClientCompletionConfirmed = o.ClientCompletionConfirmed,

            AdminCompletionConfirmed = o.AdminCompletionConfirmed,

            PlacedByCustomerGuid = o.PlacedByCustomerGuid,
        };

    /// <inheritdoc />
    public SupplyRequestDto CreateSupplyOrder(Guid floristId, Guid orderId,
        IReadOnlyList<FlowerDemandLine> lines)
    {
        SupplyRequestDto dto = new()
        {
            Id = Guid.NewGuid(),
            FloristId = floristId,
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            Approved = false,
            Lines = lines.ToList(),
        };

        _supply.Add(dto);
        FlipPhase(orderId, FloristOrderPhase.AwaitingMaterials);
        return dto;
    }

    public void ApproveSupply(Guid requestId)
    {
        SupplyRequestDto? target = _supply.FirstOrDefault(s => s.Id == requestId && !s.Approved);
        if (target is null) return;

        target.Approved = true;
        foreach (FlowerDemandLine line in target.Lines)
            UpsertFlower(line.FlowerName, line.Quantity, replaceQty: false);

        FlipPhase(target.OrderId, FloristOrderPhase.InProgress);
    }

    StudioOrderDto? FindOrder(Guid orderId) => _orders.FirstOrDefault(o => o.Id == orderId);

    public ResultMessage FloristTakeInWork(Guid orderId, Guid actingFlorist)
    {
        StudioOrderDto? order = FindOrder(orderId);

        ResultMessage flor = ValidateFlorist(order, actingFlorist);
        if (!flor.Success)
            return flor;

        switch (order!.Phase)
        {
            case FloristOrderPhase.New:
                break;

            case FloristOrderPhase.InProgress or FloristOrderPhase.ReadySent:
                Notify();
                return new ResultMessage(true, "Этот заказ уже активен.");

            case FloristOrderPhase.AwaitingMaterials:
                return new ResultMessage(false, "Необходимо согласование материалов с администратором.");

            default:
                return new ResultMessage(false, "Не подходит статус заказа.");
        }

        order.Phase = FloristOrderPhase.InProgress;

        Notify();
        return new ResultMessage(true, "Сборка активна.");
    }

    /// <inheritdoc />
    public ResultMessage BouquetReadyForDelivery(Guid orderId, Guid actingFlorist)
    {
        StudioOrderDto? order = FindOrder(orderId);
        ResultMessage flor = ValidateFlorist(order, actingFlorist);

        if (!flor.Success)
            return flor;

        if (order!.Kind != OrderKind.Bouquet)
            return new ResultMessage(false, "Только для букета.");

        if (order.Phase == FloristOrderPhase.AwaitingMaterials)
            return new ResultMessage(false, "Сначала закройте вопрос с материалами.");

        if (order.Phase != FloristOrderPhase.InProgress &&
            order.Phase != FloristOrderPhase.ReadySent)
            return new ResultMessage(false, "Сначала активируйте этап «В обработке».");

        order.Phase = FloristOrderPhase.ReadySent;

        Notify();
        return new ResultMessage(true, "Статус: готов и отправлен.");
    }

    public ResultMessage BouquetClose(Guid orderId, Guid actingFlorist)
    {
        StudioOrderDto? order = FindOrder(orderId);
        ResultMessage flor = ValidateFlorist(order, actingFlorist);
        if (!flor.Success)
            return flor;

        if (order!.Kind != OrderKind.Bouquet ||
            order.Phase != FloristOrderPhase.ReadySent)
            return new ResultMessage(false, "Подтвердите стадию «Готов и отправлен» перед закрытием.");

        order.Phase = FloristOrderPhase.Completed;

        Notify();
        return new ResultMessage(true, "Букет отправлен клиенту — заказ выполнен.");
    }

    public ResultMessage EventDepartToClient(Guid orderId, Guid actingFlorist)
    {
        StudioOrderDto? order = FindOrder(orderId);
        ResultMessage flor = ValidateFlorist(order, actingFlorist);

        if (!flor.Success)
            return flor;

        if (order!.Kind != OrderKind.Event)
            return new ResultMessage(false, "Только заказы для мероприятий.");

        if (order.Phase != FloristOrderPhase.InProgress)
            return new ResultMessage(false,
                "Сборка должна быть в статусе «В обработке».");

        order.Phase = FloristOrderPhase.EnRoute;

        Notify();
        return new ResultMessage(true, "Отправились на площадку.");
    }

    /// <inheritdoc />
    public ResultMessage BeginTripleCompletionReview(Guid orderId, Guid actingFlorist)
    {
        StudioOrderDto? order = FindOrder(orderId);

        ResultMessage flor = ValidateFlorist(order, actingFlorist);

        if (!flor.Success)
            return flor;

        if (order!.Kind != OrderKind.Event || !order.RequiresTripleCompletion)
            return new ResultMessage(false, "Только для событий с мастер-пакетом.");

        if (order.Phase != FloristOrderPhase.EnRoute)
            return new ResultMessage(false, "Подтвердите этап «Выехали к клиенту» перед завершением.");

        order.Phase = FloristOrderPhase.AwaitingCompletionConfirm;

        order.FloristCompletionAcknowledged = false;
        order.ClientCompletionConfirmed = false;
        order.AdminCompletionConfirmed = false;

        Notify();
        return new ResultMessage(true, "Клиент и администратор увидели запрос на подтверждение выполнения.");
    }

    /// <inheritdoc />
    public ResultMessage FloristSealTriple(Guid orderId, Guid actingFlorist)
    {
        StudioOrderDto? order = FindOrder(orderId);
        ResultMessage flor = ValidateFlorist(order, actingFlorist);
        if (!flor.Success)
            return flor;

        if (!order!.RequiresTripleCompletion ||
            order.Phase != FloristOrderPhase.AwaitingCompletionConfirm)
            return new ResultMessage(false, "Сначала активируйте этап согласований выполнения.");

        order.FloristCompletionAcknowledged = true;

        TrySealTripleCompletion(order.Id);

        Notify();
        return new ResultMessage(true, "Флорист записал финальное согласие.");
    }

    void TrySealTripleCompletion(Guid orderId)
    {
        StudioOrderDto? dto = FindOrder(orderId);

        if (dto is not { RequiresTripleCompletion: true } ||
            dto.Phase != FloristOrderPhase.AwaitingCompletionConfirm)
            return;

        if (!(dto.FloristCompletionAcknowledged &&
              dto.ClientCompletionConfirmed &&
              dto.AdminCompletionConfirmed))
            return;

        dto.Phase = FloristOrderPhase.Completed;

        dto.FloristCompletionAcknowledged = false;
        dto.ClientCompletionConfirmed = false;
        dto.AdminCompletionConfirmed = false;

        Notify();
    }


    void FlipPhase(Guid orderId, FloristOrderPhase phase)
    {
        StudioOrderDto? dto = _orders.FirstOrDefault(o => o.Id == orderId);

        if (dto is null) return;

        dto.Phase = phase;

        Notify();
    }

    public void ClientFinalize(Guid orderId, Guid actingClientGuid)
    {
        StudioOrderDto? dto =
            _orders.FirstOrDefault(o => o.Id == orderId &&
                                        o.RequiresTripleCompletion &&
                                        o.PlacedByCustomerGuid == actingClientGuid);

        if (dto is null) return;

        dto.ClientCompletionConfirmed = true;

        TrySealTripleCompletion(orderId);

        Notify();
    }

    public void AdminSealTriple(Guid orderId)
    {
        StudioOrderDto? dto =
            _orders.FirstOrDefault(o => o.Id == orderId && o.RequiresTripleCompletion);

        if (dto is null || dto.Phase != FloristOrderPhase.AwaitingCompletionConfirm)
            return;

        dto.AdminCompletionConfirmed = true;

        TrySealTripleCompletion(orderId);

        Notify();
    }

    /// <inheritdoc />
    public SupplyRequestDto? NextPendingSupply(Guid? floristGuid) =>
        _supply.Where(s => !s.Approved && floristGuid.HasValue &&
                          s.FloristId == floristGuid.Value).MaxBy(o => o.CreatedAt);
}

public readonly record struct ResultMessage(bool Success, string Message);

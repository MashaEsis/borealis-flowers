using System.Linq.Expressions;
using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Orders;

public static class OrdersHandler
{
    public sealed class PlaceBouquetDto
    {
        public Guid ServiceId { get; set; }
        public string? Comment { get; set; }
    }

    /// <summary>Заказ мероприятия (форма заявки).</summary>
    public sealed class PlaceEventPlanDto
    {
        public EventTypeKind EventType { get; set; }
        public DateTime EventStartsAtUtc { get; set; }
        public string Venue { get; set; } = "";
        public Guid SpecialistId { get; set; }
        public string WishNotes { get; set; } = "";
        public double? Budget { get; set; }
    }

    public sealed class PlaceEventDto
    {
        public Guid TimeslotId { get; set; }
        public string Description { get; set; } = "";
    }

    public sealed class OrderListDto
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid? SpecialistId { get; set; }
        public int OrderStatus { get; set; }
        public int OrderKind { get; set; }
        public Guid? ServiceId { get; set; }
        public string? ServiceTitleSnapshot { get; set; }
        public int? EventType { get; set; }
        public DateTime? EventStartsAtUtc { get; set; }
        public string? Venue { get; set; }
        public double? Budget { get; set; }
        public string? WishNotes { get; set; }
        public string? FloristMaterials { get; set; }
        public string? FloristInventory { get; set; }
        public double? QuoteTotal { get; set; }
        public DateTime? DepartureAtUtc { get; set; }
        public string? AdminComment { get; set; }
        public string? FloristComment { get; set; }
        public DateTime? ClientConfirmedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
    }

    public sealed class UpdateOrderStateDto
    {
        public int OrderStatus { get; set; }
        public string? Resolution { get; set; }
        public string? FloristMaterials { get; set; }
        public string? FloristInventory { get; set; }
        public double? QuoteTotal { get; set; }
        public DateTime? DepartureAtUtc { get; set; }
        public string? AdminComment { get; set; }
        public string? FloristComment { get; set; }
    }

    public static async Task<IResult> PlaceBouquetAsync(
        PlaceBouquetDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? cid = TryCustomerId(user);
        if (cid is null || !IsClientRole(user))
            return Results.Forbid();

        Service? service = await db.Services.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);
        if (service is null)
            return Results.NotFound("Услуга не найдена.");

        List<Guid> florists = await db.Specialists.AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => s.Id)
            .ToListAsync();
        if (florists.Count == 0)
            return Results.BadRequest("Нет активных флористов в системе.");

        Guid specialistId = florists[Random.Shared.Next(florists.Count)];
        string note = string.IsNullOrWhiteSpace(dto.Comment) ? "" : $" · {dto.Comment.Trim()}";
        var request = new Request
        {
            Id = Guid.NewGuid(),
            CustomerId = cid,
            SpecialistId = specialistId,
            OrderKind = OrderKind.Bouquet,
            ServiceId = service.Id,
            ServiceTitleSnapshot = service.Name,
            OrderStatus = OrderStatus.New,
            Description = $"[Букет] {service.Name} — {service.Price:N0} ₽{note}",
            CreatedAt = DateTime.UtcNow,
        };
        await db.Requests.AddAsync(request);
        await db.SaveChangesAsync();
        return Results.Ok(request.Id);
    }

    public static async Task<IResult> PlaceEventPlanAsync(
        PlaceEventPlanDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? cid = TryCustomerId(user);
        if (cid is null || !IsClientRole(user))
            return Results.Forbid();

        if (string.IsNullOrWhiteSpace(dto.Venue))
            return Results.BadRequest("Укажите место проведения.");

        if (!await db.Specialists.AnyAsync(s => s.Id == dto.SpecialistId && s.IsActive))
            return Results.BadRequest("Флорист не найден или не активен.");

        string typeRu = dto.EventType switch
        {
            EventTypeKind.Wedding => "Свадьба",
            EventTypeKind.Corporate => "Корпоратив",
            EventTypeKind.Birthday => "День рождения",
            EventTypeKind.Anniversary => "Юбилей",
            _ => "Другое",
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            CustomerId = cid,
            SpecialistId = dto.SpecialistId,
            OrderKind = OrderKind.Event,
            OrderStatus = OrderStatus.New,
            EventType = dto.EventType,
            EventStartsAtUtc = dto.EventStartsAtUtc,
            Venue = dto.Venue.Trim(),
            Budget = dto.Budget,
            WishNotes = dto.WishNotes.Trim(),
            Description =
                $"[Мероприятие] {typeRu} · {dto.EventStartsAtUtc:u} · {dto.Venue.Trim()}",
            CreatedAt = DateTime.UtcNow,
        };
        await db.Requests.AddAsync(request);
        await db.SaveChangesAsync();
        return Results.Ok(request.Id);
    }

    public static async Task<IResult> PlaceEventAsync(
        PlaceEventDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Guid? cid = TryCustomerId(user);
        if (cid is null || !IsClientRole(user))
            return Results.Forbid();

        Timeslot? ts = await db.Timeslots
            .Include(t => t.DateSchedule)
            .FirstOrDefaultAsync(t => t.Id == dto.TimeslotId);

        if (ts is null)
            return Results.NotFound("Слот не найден.");
        if (!ts.Available)
            return Results.Conflict("Слот уже занят.");

        Guid specialistId = ts.DateSchedule.SpecialistId;
        ts.CustomerId = cid;
        ts.Available = false;

        var request = new Request
        {
            Id = Guid.NewGuid(),
            CustomerId = cid,
            SpecialistId = specialistId,
            OrderKind = OrderKind.Event,
            OrderStatus = OrderStatus.New,
            Description = $"[Мероприятие·слот] {dto.Description.Trim()} · слот {dto.TimeslotId}",
            WishNotes = dto.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        await db.Requests.AddAsync(request);
        await db.SaveChangesAsync();
        return Results.Ok(request.Id);
    }

    public static async Task<IResult> MyOrdersAsync(
        ClaimsPrincipal user,
        DataContext db,
        bool historyOnly = false)
    {
        Guid? cid = TryCustomerId(user);
        if (cid is null)
            return Results.Unauthorized();

        IQueryable<Request> q = db.Requests.AsNoTracking()
            .Where(r => r.CustomerId == cid);
        if (historyOnly)
            q = q.Where(r => r.OrderStatus == OrderStatus.Completed);
        else
            q = q.Where(r => r.OrderStatus != OrderStatus.Completed);

        var list = await q
            .OrderByDescending(r => r.CompletedAtUtc ?? r.UpdatedAt ?? r.CreatedAt)
            .Select(MapOrder())
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> FloristOrdersAsync(ClaimsPrincipal user, DataContext db)
    {
        Guid? specialistId = TrySpecialistId(user);
        if (specialistId is null)
            return Results.Forbid();

        var list = await db.Requests.AsNoTracking()
            .Include(r => r.Customer)
            .Where(r => r.SpecialistId == specialistId && r.OrderStatus != OrderStatus.Completed)
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapOrder())
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> FloristHistoryAsync(ClaimsPrincipal user, DataContext db)
    {
        Guid? specialistId = TrySpecialistId(user);
        if (specialistId is null)
            return Results.Forbid();

        var list = await db.Requests.AsNoTracking()
            .Include(r => r.Customer)
            .Where(r => r.SpecialistId == specialistId && r.OrderStatus == OrderStatus.Completed)
            .OrderByDescending(r => r.CompletedAtUtc)
            .Select(MapOrder())
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> AllOrdersAdminAsync(ClaimsPrincipal user, DataContext db)
    {
        if (!user.HasClaim(ClaimTypes.Role, "Admin"))
            return Results.Forbid();

        var list = await db.Requests.AsNoTracking()
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapOrder())
            .ToListAsync();

        return Results.Ok(list);
    }

    public static async Task<IResult> UpdateOrderStateAsync(
        Guid id,
        UpdateOrderStateDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        Request? r = await db.Requests.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null)
            return Results.NotFound();

        if (!Enum.IsDefined(typeof(OrderStatus), dto.OrderStatus))
            return Results.BadRequest("Неизвестный статус.");

        var next = (OrderStatus)dto.OrderStatus;
        bool admin = user.HasClaim(ClaimTypes.Role, "Admin");
        Guid? sid = TrySpecialistId(user);
        Guid? cid = TryCustomerId(user);

        if (!admin)
        {
            if (sid is not null && r.SpecialistId == sid)
            {
                if (!CanFloristTransition(r, next, dto))
                    return Results.BadRequest("Недопустимый переход для флориста.");
            }
            else if (cid is not null && r.CustomerId == cid)
            {
                if (!(r.OrderStatus == OrderStatus.Ready && next == OrderStatus.Completed))
                    return Results.BadRequest("Клиент может только подтвердить получение (из «Готов» в «Завершён»).");
            }
            else
                return Results.Forbid();
        }

        ApplyDto(r, dto, next);
        r.UpdatedAt = DateTime.UtcNow;
        if (next == OrderStatus.Completed)
        {
            r.CompletedAtUtc = DateTime.UtcNow;
            if (r.ClientConfirmedAtUtc is null)
                r.ClientConfirmedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Results.Ok();
    }

    static bool CanFloristTransition(Request r, OrderStatus next, UpdateOrderStateDto dto)
    {
        if (r.OrderKind == OrderKind.Bouquet)
        {
            return (r.OrderStatus, next) switch
            {
                (OrderStatus.New, OrderStatus.InProgress) => true,
                (OrderStatus.New, OrderStatus.Rejected) => true,
                (OrderStatus.InProgress, OrderStatus.Ready) => true,
                (OrderStatus.InProgress, OrderStatus.Rejected) => true,
                _ => false,
            };
        }

        // Event
        return (r.OrderStatus, next) switch
        {
            (OrderStatus.New, OrderStatus.MaterialNegotiation) => true,
            (OrderStatus.New, OrderStatus.Rejected) => true,
            (OrderStatus.MaterialNegotiation, OrderStatus.AwaitingApproval) =>
                dto.QuoteTotal is not null &&
                !string.IsNullOrWhiteSpace(dto.FloristMaterials),
            (OrderStatus.Approved, OrderStatus.InProgress) => true,
            (OrderStatus.InProgress, OrderStatus.Ready) => true,
            (OrderStatus.InProgress, OrderStatus.Rejected) => true,
            _ => false,
        };
    }

    static void ApplyDto(Request r, UpdateOrderStateDto dto, OrderStatus next)
    {
        r.OrderStatus = next;
        if (dto.Resolution is not null)
            r.Resolution = dto.Resolution;
        if (dto.FloristMaterials is not null)
            r.FloristMaterials = dto.FloristMaterials;
        if (dto.FloristInventory is not null)
            r.FloristInventory = dto.FloristInventory;
        if (dto.QuoteTotal is not null)
            r.QuoteTotal = dto.QuoteTotal;
        if (dto.DepartureAtUtc is not null)
            r.DepartureAtUtc = dto.DepartureAtUtc;
        if (dto.AdminComment is not null)
            r.AdminComment = dto.AdminComment;
        if (dto.FloristComment is not null)
            r.FloristComment = dto.FloristComment;
    }

    public static async Task<IResult> AdminDecisionAsync(
        Guid id,
        UpdateOrderStateDto dto,
        ClaimsPrincipal user,
        DataContext db)
    {
        if (!user.HasClaim(ClaimTypes.Role, "Admin"))
            return Results.Forbid();

        Request? r = await db.Requests.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null)
            return Results.NotFound();

        if (!Enum.IsDefined(typeof(OrderStatus), dto.OrderStatus))
            return Results.BadRequest();

        var next = (OrderStatus)dto.OrderStatus;
        if (r.OrderKind != OrderKind.Event ||
            r.OrderStatus != OrderStatus.AwaitingApproval ||
            (next != OrderStatus.Approved && next != OrderStatus.Rejected))
            return Results.BadRequest("Доступно только для мероприятия «Ожидает одобрения».");

        r.OrderStatus = next;
        r.AdminComment = dto.AdminComment ?? r.AdminComment;
        r.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    static Expression<Func<Request, OrderListDto>> MapOrder() =>
        r => new OrderListDto
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer != null ? r.Customer.Name : null,
            SpecialistId = r.SpecialistId,
            OrderStatus = (int)r.OrderStatus,
            OrderKind = (int)r.OrderKind,
            ServiceId = r.ServiceId,
            ServiceTitleSnapshot = r.ServiceTitleSnapshot,
            EventType = r.EventType.HasValue ? (int?)r.EventType.Value : null,
            EventStartsAtUtc = r.EventStartsAtUtc,
            Venue = r.Venue,
            Budget = r.Budget,
            WishNotes = r.WishNotes,
            FloristMaterials = r.FloristMaterials,
            FloristInventory = r.FloristInventory,
            QuoteTotal = r.QuoteTotal,
            DepartureAtUtc = r.DepartureAtUtc,
            AdminComment = r.AdminComment,
            FloristComment = r.FloristComment,
            ClientConfirmedAtUtc = r.ClientConfirmedAtUtc,
            CompletedAtUtc = r.CompletedAtUtc,
            CreatedAt = r.CreatedAt,
            Description = r.Description,
        };

    static Guid? TryCustomerId(ClaimsPrincipal user)
    {
        string? sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub is not null && Guid.TryParse(sub, out Guid id) ? id : null;
    }

    static Guid? TrySpecialistId(ClaimsPrincipal user)
    {
        string? s = user.FindFirstValue("specialistId");
        if (string.IsNullOrEmpty(s))
            return null;
        return Guid.TryParse(s, out Guid id) ? id : null;
    }

    static bool IsClientRole(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role) == "Client";
}

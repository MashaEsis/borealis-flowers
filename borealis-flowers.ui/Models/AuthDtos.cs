using System.Text.Json.Serialization;

namespace borealis_flowers.ui.Models;

public sealed class RegisterRequestDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
}

public sealed class LoginRequestDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class AuthResponseDto
{
    public string Token { get; set; } = "";

    public UserMeDto User { get; set; } = null!;
}

public sealed class UserMeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public string Role { get; set; } = "";

    public bool IsAdmin { get; set; }

    public bool IsSpecialist { get; set; }

    [JsonPropertyName("specialistId")]
    public Guid? SpecialistId { get; set; }
}

public sealed class OrderRowDto
{
    public Guid Id { get; set; }

    public Guid? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public Guid? SpecialistId { get; set; }

    /// <summary>OrderStatus (API).</summary>
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

    [JsonIgnore]
    public int State => OrderStatus;

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }
}

public sealed class PlaceBouquetRequestDto
{
    public Guid ServiceId { get; set; }

    public string? Comment { get; set; }
}

public sealed class PlaceEventRequestDto
{
    public Guid TimeslotId { get; set; }

    public string Description { get; set; } = "";
}

public sealed class PlaceEventPlanRequestDto
{
    public int EventType { get; set; }

    public DateTime EventStartsAtUtc { get; set; }

    public string Venue { get; set; } = "";

    public Guid SpecialistId { get; set; }

    public string WishNotes { get; set; } = "";

    public double? Budget { get; set; }
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

public sealed class FloristApplicationCreateDto
{
    public string FullName { get; set; } = "";
    public string Experience { get; set; } = "";
    public string PortfolioNotes { get; set; } = "";
    public string Motivation { get; set; } = "";
}

public sealed class FloristApplicationDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public string FullName { get; set; } = "";
    public string Experience { get; set; } = "";
    public string PortfolioNotes { get; set; } = "";
    public string Motivation { get; set; } = "";
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AdminComment { get; set; }
}

public sealed class AvailableTimeslotDto
{
    public Guid TimeslotId { get; set; }
    public Guid DateScheduleId { get; set; }
    public DateTime Date { get; set; }
    public int Time { get; set; }
    public Guid SpecialistId { get; set; }
    public string SpecialistName { get; set; } = "";
}

public sealed class CustomerBriefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public sealed class SpecializationRowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class ServiceRowDto
{
    public Guid Id { get; set; }
    public Guid SpecializationId { get; set; }
    public string SpecializationName { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public double Price { get; set; }
    public int? EstimatedTime { get; set; }
}

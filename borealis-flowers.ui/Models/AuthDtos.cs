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

    public string? Phone { get; set; }

    public DateTime? Birthday { get; set; }

    public string Role { get; set; } = "";

    public bool IsAdmin { get; set; }

    public bool IsSpecialist { get; set; }

    [JsonPropertyName("specialistId")]
    public Guid? SpecialistId { get; set; }

    public double WalletBalance { get; set; }

    public int LoyaltyProgress { get; set; }

    public int LoyaltyTarget { get; set; } = 5;

    public bool NextOrderDiscount { get; set; }
}

public sealed class UpdateProfileRequestDto
{
    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? Birthday { get; set; }
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

    public string? CardMessage { get; set; }

    public string? DeliveryAddress { get; set; }

    public double? DeliveryLatitude { get; set; }

    public double? DeliveryLongitude { get; set; }

    public string? CustomerPhoneSnapshot { get; set; }

    public double? ChargedAmount { get; set; }

    public int DiscountPercent { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAtUtc { get; set; }
}

public sealed class PlaceBouquetRequestDto
{
    public Guid ServiceId { get; set; }

    public string? Comment { get; set; }

    public string? CardMessage { get; set; }

    public string DeliveryAddress { get; set; } = "";

    public double DeliveryLatitude { get; set; }

    public double DeliveryLongitude { get; set; }

    public string? Phone { get; set; }
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
    public bool IsAdmin { get; set; }
    public bool IsSpecialist { get; set; }
}

public sealed class FloristStaffDto
{
    public Guid CustomerId { get; set; }
    public Guid? SpecialistId { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? SpecialistName { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
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
    public string? FlowerComposition { get; set; }
    public string? ImageUrl { get; set; }
    public double Price { get; set; }
    public int? EstimatedTime { get; set; }
    public Guid? SpecialistId { get; set; }
    public string? SpecialistName { get; set; }
}

public sealed class ServiceEditDto
{
    public Guid SpecializationId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? FlowerComposition { get; set; }
    public string? ImageUrl { get; set; }
    public double Price { get; set; }
    public int? EstimatedTime { get; set; }
    public Guid? SpecialistId { get; set; }
}

public sealed class PortfolioWorkDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = "";
    public string? Title { get; set; }
    public int SortOrder { get; set; }
}

public sealed class PortfolioDetailDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string ImgUrl { get; set; } = "";
    public string City { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string StyleDescription { get; set; } = "";
    public List<PortfolioWorkDto> Works { get; set; } = [];
}

public sealed class UpdatePortfolioDto
{
    public string? FullName { get; set; }
    public string? City { get; set; }
    public string? StyleDescription { get; set; }
    public string? ImgUrl { get; set; }
}

public sealed class AddPortfolioWorkDto
{
    public string ImageUrl { get; set; } = "";
    public string? Title { get; set; }
}

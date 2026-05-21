using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;

public class Request
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid? SpecialistId { get; set; }

    [ForeignKey("SpecialistId")]
    public Specialist? Specialist { get; set; }

    /// <summary>История: колонка в БД названа State.</summary>
    [Column("State")]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }
    public string? Resolution { get; set; }

    public bool ResolutionSent { get; set; }

    public OrderKind OrderKind { get; set; } = OrderKind.Bouquet;

    public Guid? ServiceId { get; set; }

    [ForeignKey("ServiceId")]
    public Service? Service { get; set; }

    public string? ServiceTitleSnapshot { get; set; }

    public EventTypeKind? EventType { get; set; }

    public DateTime? EventStartsAtUtc { get; set; }

    public string? Venue { get; set; }

    public double? Budget { get; set; }

    /// <summary>Пожелания клиента по мероприятию.</summary>
    public string? WishNotes { get; set; }

    public string? FloristMaterials { get; set; }
    public string? FloristInventory { get; set; }
    public double? QuoteTotal { get; set; }
    public DateTime? DepartureAtUtc { get; set; }

    public string? AdminComment { get; set; }
    public string? FloristComment { get; set; }

    public DateTime? ClientConfirmedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

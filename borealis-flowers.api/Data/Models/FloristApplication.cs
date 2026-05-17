using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;

public sealed class FloristApplication
{
    [Key]
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; } = "";

    public string Experience { get; set; } = "";

    /// <summary>Ссылка или текст про портфолио.</summary>
    public string PortfolioNotes { get; set; } = "";

    public string Motivation { get; set; } = "";

    public FloristApplicationStatus Status { get; set; } = FloristApplicationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? AdminComment { get; set; }
}

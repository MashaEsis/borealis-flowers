using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;

public class PaymentCard
{
    [Key]
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    /// <summary>Название карты, например «Visa».</summary>
    public string Label { get; set; } = "";

    /// <summary>Последние 4 цифры.</summary>
    public string LastFour { get; set; } = "";

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

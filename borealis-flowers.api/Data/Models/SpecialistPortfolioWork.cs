using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;

public class SpecialistPortfolioWork
{
    [Key]
    public Guid Id { get; set; }

    public Guid SpecialistId { get; set; }

    [ForeignKey(nameof(SpecialistId))]
    public Specialist? Specialist { get; set; }

    public string ImageUrl { get; set; } = "";

    public string? Title { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

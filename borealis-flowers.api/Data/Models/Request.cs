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

    public State State { get; set; } = State.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }
    public string? Resolution { get; set; }

    public bool ResolutionSent { get; set; } = false;
}

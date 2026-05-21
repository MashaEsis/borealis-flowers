using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;
public class ServicePrice
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid ServiceId { get; set; }

    [ForeignKey("ServiceId")]
    public Service Service { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid SpecialistId { get; set; }

    [ForeignKey("SpecialistId")]
    public Specialist Specialist { get; set; }

    public double Price { get; set; }
}

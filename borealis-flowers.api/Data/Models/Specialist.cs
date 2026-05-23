using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;
public class Specialist
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid SpecializationId { get; set; }

    [ForeignKey("SpecializationId")]
    public Specialization Specialization { get; set; }

    public string FullName { get; set; }
    public string ImgUrl { get; set; }

    public List<DateSchedule> DateSchedules { get; set; }
    public override string ToString() => FullName;
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; } = false;

    /// <summary>Описание стиля / о себе для портфолио.</summary>
    public string? StyleDescription { get; set; }

    public List<SpecialistPortfolioWork> PortfolioWorks { get; set; } = [];

}

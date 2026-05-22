using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;
public class Service
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    public Guid SpecializationId { get; set; }
    public Specialization Specialization { get; set; }

    public string Name { get; set; }
    public string? Description { get; set; }
    /// <summary>Состав цветов (для карточки каталога).</summary>
    public string? FlowerComposition { get; set; }
    /// <summary>URL фото букета.</summary>
    public string? ImageUrl { get; set; }
    public double Price { get; set; }
    public int? EstimatedTime { get; set; }

    public override string ToString() => Name;
}

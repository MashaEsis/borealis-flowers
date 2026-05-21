using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;
public class Timeslot
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    public int Time { get; set; }
    public bool Available { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid DateScheduleId { get; set; }

    [ForeignKey("DateScheduleId")]
    public DateSchedule DateSchedule { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }
}

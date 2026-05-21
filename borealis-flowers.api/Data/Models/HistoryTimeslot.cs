using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;
public class HistoryTimeslot
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Display(AutoGenerateField = false)]
    public Guid TimeslotId { get; set; }

    [ForeignKey("TimeslotId")]
    public Timeslot Timeslot { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ExternalUserId { get; set; }
    public bool FeedbackRequested { get; set; } = false;
    public Status Status { get; set; }
}


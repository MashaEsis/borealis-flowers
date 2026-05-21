using borealis_flowers.api.Data.Models;

namespace borealis_flowers.api.Features.HistoryTimeslots;

public class HistoryTimeslotDto
{
    public Guid TimeslotId { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ExternalUserId { get; set; }
    public bool FeedbackRequest { get; set; }
}

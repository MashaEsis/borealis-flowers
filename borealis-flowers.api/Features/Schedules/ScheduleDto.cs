using borealis_flowers.api.Data.Models;
using borealis_flowers.api.Features.Specialists;

public class ScheduleCreate
{
    public Guid SpecialistId { get; set; }
    public DateTime StartDate { get; set; }
}

public class WorkingDaysCreate
{
    public Guid SpecialistId { get; set; }
    public IEnumerable<DateSchedule> WorkingDates { get; set; }
}

public class WorkingDaysUpdate : WorkingDaysCreate {}

public class TimeslotUpdate
{
    public Guid Id { get; set; }
    public Guid SheduleId { get; set; }
    public Guid? CustomerId { get; set; }
    public int Time { get; set; }
    public bool Available { get; set; } = true;
}

public class VisitInfo
{
    public Guid TimeSlotId { get; set; }
    public DateTime VisitDate { get; set; }
    public SpecialistDto Specialist { get; set; }
}

public class WorkingDaysDelete
{
    public Guid SpecialistId { get; set; }
    public List<DateTime> Dates { get; set; }

}

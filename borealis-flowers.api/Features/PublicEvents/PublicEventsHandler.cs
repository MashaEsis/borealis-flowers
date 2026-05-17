using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.PublicEvents;

public static class PublicEventsHandler
{
    public sealed class AvailableSlotDto
    {
        public Guid TimeslotId { get; set; }
        public Guid DateScheduleId { get; set; }
        public DateTime Date { get; set; }
        public int Time { get; set; }
        public Guid SpecialistId { get; set; }
        public string SpecialistName { get; set; } = "";
    }

    public static async Task<IResult> ListAvailableTimeslots(DataContext db)
    {
        DateTime today = DateTime.UtcNow.Date;

        var list = await (
                from t in db.Timeslots.AsNoTracking()
                join ds in db.DateSchedules.AsNoTracking() on t.DateScheduleId equals ds.Id
                join s in db.Specialists.AsNoTracking() on ds.SpecialistId equals s.Id
                where t.Available && ds.Date.Date >= today
                orderby ds.Date, t.Time
                select new AvailableSlotDto
                {
                    TimeslotId = t.Id,
                    DateScheduleId = ds.Id,
                    Date = ds.Date,
                    Time = t.Time,
                    SpecialistId = s.Id,
                    SpecialistName = s.FullName,
                })
            .Take(400)
            .ToListAsync();

        return Results.Ok(list);
    }
}

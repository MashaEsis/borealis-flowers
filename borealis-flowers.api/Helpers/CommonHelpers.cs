using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;

namespace borealis_flowers.api.Helpers;

public static class CommonHelpers
{
    public static IEnumerable<Timeslot> GetDefaultDayTimeslots()
    {
        for (int i = 8; i < 20; i++)
            yield return new Timeslot { Time = i, Available = true };
    }

    public static IEnumerable<Timeslot> GetDefaultTimeslotsForDaySchedule(Guid dateScheduleId)
    {
        for (int i = 8; i < 20; i++)
            yield return new Timeslot { Time = i, Available = true, DateScheduleId = dateScheduleId };
    }

    public static async Task AddTimeslotsForWorkingDays(IEnumerable<DateSchedule> workingDates, DataContext db)
    {
        var timeslotsForAllWorkingDays = new List<Timeslot>();
        foreach (var wd in workingDates)
        {
            var dateScheduleId = wd.Id;
            timeslotsForAllWorkingDays.AddRange(GetDefaultTimeslotsForDaySchedule(dateScheduleId));
        }
        await db.Timeslots.AddRangeAsync(timeslotsForAllWorkingDays);
    }

}

using System.Globalization;
using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using borealis_flowers.api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Schedules;

public static class SchedulesHandler
{
    public static Func<Guid, DataContext, Task<IEnumerable<object>>> GetSchedulesBySpecialist()
    {
        return async ([FromQuery] Guid masterId, DataContext db) =>
        {
            var result = await db.DateSchedules.Where(x => x.SpecialistId.Equals(masterId)).ToListAsync();

            var query = result.GroupBy(x => new { x.Date.Year, x.Date.Month });

            // here we group all days available but for admin we interested in 
            // months and years
            return query.Select(x => new
            {
                Date = new DateTime(x.Key.Year, x.Key.Month, 1)
            });
        };
    }
    public static Func<string, string, DataContext, Task<Guid>> GetScheduleDateId()
    {
        return async (masterId,date,db ) =>
        {
            if (!DateTime.TryParseExact(
                    date,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                return Guid.Empty;
            }
            var schedule = await db.DateSchedules
                .FirstOrDefaultAsync(
                    x =>
                        x.Date.Date == parsedDate.Date 
                        && x.SpecialistId == Guid.Parse(masterId)
                        );

            return schedule?.Id ?? Guid.Empty;
        };
    }

    public static Func<ScheduleCreate, DataContext, Task> CreateSchedule()
    {
        return async ([FromBody] ScheduleCreate value, DataContext db) =>
        {

            var workingDays = value.StartDate.GenerateOnlyWorkingDaysForThisMonthStartingFromDay();
            foreach (var wd in workingDays)
            {
                wd.SpecialistId = value.SpecialistId;
            }
            await db.DateSchedules.AddRangeAsync(workingDays);

            // lines 53-59 were wrapped with static method call 'await CommonHelpers.AddTimeslotsForWorkingDays(workingDates, db);'
            var timeslotsForAllWorkingDays = new List<Timeslot>();
            foreach (var wd in workingDays)
            {
                var dateScheduleId = wd.Id;
                timeslotsForAllWorkingDays.AddRange(CommonHelpers.GetDefaultTimeslotsForDaySchedule(dateScheduleId));
            }
            await db.Timeslots.AddRangeAsync(timeslotsForAllWorkingDays);

            await db.SaveChangesAsync();
        };
    }
    public static Func<WorkingDaysDelete, DataContext, Task<IResult>> DeleteWorkingDays()
    {
        return async ([FromBody] WorkingDaysDelete request, DataContext db) =>
        {
            if (request.Dates == null || !request.Dates.Any())
                return Results.BadRequest("No dates provided");

            var days = await db.DateSchedules
                .Where(x => x.SpecialistId == request.SpecialistId &&
                            request.Dates.Contains(x.Date.Date))
                .ToListAsync();

            if (!days.Any())
                return Results.Ok(true);

            var ids = days.Select(x => x.Id).ToList();

            var relatedTimeslots = db.Timeslots
                .Where(x => ids.Contains(x.DateScheduleId));

            await relatedTimeslots.ExecuteDeleteAsync();
            db.DateSchedules.RemoveRange(days);

            await db.SaveChangesAsync();

            return Results.Ok(true);
        };
    }

    public static Func<string, Guid, DataContext, Task> DeleteSchedule()
    {
        return async (string scheduleId, Guid masterId, DataContext db) =>
        {
            var splitted = scheduleId.Split("-");
            var year = Convert.ToInt32(splitted[0]);
            var month = Convert.ToInt32(splitted[1]);
            var records = db.DateSchedules
                .Where(x =>
                    x.SpecialistId == masterId &&
                    x.Date.Year == year &&
                    x.Date.Month == month);

            var recordsIds = await records.Select(x => x.Id).ToListAsync();
            var relatedTimeslots = db.Timeslots.Where(x =>
                recordsIds.Contains(x.DateScheduleId));
            await relatedTimeslots.ExecuteDeleteAsync();
            await records.ExecuteDeleteAsync();

            await db.SaveChangesAsync();
        };
    }
}

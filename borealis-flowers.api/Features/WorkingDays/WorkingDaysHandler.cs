using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using borealis_flowers.api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.WorkingDays;

public static class WorkingDaysHandler
{
    public static Func<Guid, int, int, DataContext, Task<List<DateSchedule>>> GetWorkingDays()
    {
        return async ([FromQuery] Guid masterId, [FromQuery] int month, [FromQuery] int year, DataContext db) =>
        {
            var result = await db.DateSchedules
                .Where(x =>
                    x.SpecialistId.Equals(masterId) &&
                    x.Date.Month == month &&
                    x.Date.Year == year)
                .ToListAsync();

            return result; //!invoked every time we switch from TimeslotSelector to WorkingDaySelector
        };
    }

    public static Func<WorkingDaysCreate, DataContext, Task> CreateWorkingDays()
    {
        return async ([FromBody] WorkingDaysCreate items, DataContext db) =>
        {
            await db.DateSchedules.AddRangeAsync(items.WorkingDates);

            var timeslotsForAllWorkingDays = new List<Timeslot>();
            foreach (var wd in items.WorkingDates)
            {
                var dateScheduleId = wd.Id;
                timeslotsForAllWorkingDays.AddRange(CommonHelpers.GetDefaultTimeslotsForDaySchedule(dateScheduleId));
            }
            await db.Timeslots.AddRangeAsync(timeslotsForAllWorkingDays);

            await db.SaveChangesAsync();
        };
    }

    public static Func<WorkingDaysUpdate, DataContext, Task> UpdateWorkingDaysAvailability()
    {
        return async ([FromBody] WorkingDaysUpdate items, DataContext db) =>
        {
            var idsToBeUpdated = items.WorkingDates
                .Select(x => x.Id)
                .ToList();

            var schedulesToUpdate = await db.DateSchedules
                .Where(x => x.SpecialistId == items.SpecialistId && idsToBeUpdated.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.IsAvailable, w => !w.IsAvailable));
        };
    }

    public static Func<WorkingDaysUpdate, DataContext, Task> UpdateWorkingDays()
    {
        return async ([FromBody] WorkingDaysUpdate items, DataContext db) =>
        {
            var idsToBeDeleted = items.WorkingDates
                .Where(x => x.IsWorkingDay == false)
                .Select(x => x.Id)
                .ToList();

            var schedulesToDelete = db.DateSchedules
                .Where(x => x.SpecialistId == items.SpecialistId && idsToBeDeleted.Contains(x.Id));

            var timeslotsToDelete = db.Timeslots
                .Where(x => idsToBeDeleted.Contains(x.DateScheduleId));

            await timeslotsToDelete.ExecuteDeleteAsync();
            await schedulesToDelete.ExecuteDeleteAsync();

            await db.SaveChangesAsync();

            // Add new working days
            var workingDaysToBeAdded = items.WorkingDates
                .Where(x => x.IsWorkingDay == true)
                .ToList();

            await db.DateSchedules.AddRangeAsync(workingDaysToBeAdded);
            await CommonHelpers.AddTimeslotsForWorkingDays(workingDaysToBeAdded, db);

            await db.SaveChangesAsync();
        };
    }

    public static Func<WorkingDaysUpdate, DataContext, Task> SoftUpdateWorkingDays()
    {
        return async ([FromBody] WorkingDaysUpdate items, DataContext db) =>
        {
            var listOfIds = items.WorkingDates.Select(x => x.Id).ToList();
            var schedulesToUpdate = db.DateSchedules
                .Where(x => x.SpecialistId == items.SpecialistId && listOfIds.Contains(x.Id));
            foreach (var schedule in schedulesToUpdate)
            {
                schedule.IsWorkingDay = items.WorkingDates.First(x => x.Id == schedule.Id).IsWorkingDay;
            }
            await db.SaveChangesAsync();
        };
    }

    public static Func<Guid, int, int, int, DataContext, Task<List<DateSchedule>>> GetWorkingDaysForMonthRange()
    {
        return async ([FromQuery] Guid masterId, [FromQuery] int startMonth, [FromQuery] int finalMonth, [FromQuery] int year, DataContext db) =>
        {
            var monthRange = Enumerable.Range(startMonth, finalMonth - startMonth + 1);
            var result = await db.DateSchedules
                .Where(x =>
                    x.SpecialistId.Equals(masterId) &&
                    monthRange.Contains(x.Date.Month) &&
                    x.Date.Year == year
                ).ToListAsync();

            return result;
        };
    }
}

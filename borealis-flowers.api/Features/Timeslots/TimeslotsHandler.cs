using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Timeslots;

public static class TimeslotsHandler
{
    public static Func<Guid, DataContext, Task<IEnumerable<Timeslot>>> GetTimeslotsByWorkingDay()
    {
        return async (Guid workingDayId, DataContext db) =>
        {
            var result = await db.Timeslots
                .Where(x => x.DateScheduleId == workingDayId)
                .ToListAsync();

            return result.OrderBy(x => x.Time);
        };
    }
    public static Func<List<Guid>, DataContext, Task<bool>> HasReservedTimeSlots()
    {
        return async (List<Guid> workingDayIds, DataContext db) =>
        {
            return await db.Timeslots
                .AnyAsync(x =>
                    workingDayIds.Contains(x.DateScheduleId) &&
                    x.Customer != null);
        };
    }

    public static Func<List<TimeslotUpdate>, DataContext, Task> UpdateTimeslots()
    {
        return async (List<TimeslotUpdate> updatedTimeslots, DataContext db) =>
        {
            var foundTimeslots = await db.Timeslots
                .Where(x => updatedTimeslots.Select(x => x.Id).Contains(x.Id))
                .ToListAsync();
            foundTimeslots.ForEach(x =>
            {
                x.Available = updatedTimeslots.FirstOrDefault(_ => _.Id == x.Id).Available;
                x.CustomerId = updatedTimeslots.FirstOrDefault(_ => _.Id == x.Id).CustomerId;
            });

            db.Timeslots.UpdateRange(foundTimeslots);

            await db.SaveChangesAsync();
        };
    }

    public static Func<Guid, DataContext, Task> ReleaseTimeslot()
    {
        return async ([FromRoute] Guid id, DataContext db) =>
        {
            var timeslot = await db.Timeslots.FirstOrDefaultAsync(t => t.Id.Equals(id));
            timeslot.Available = true;
            timeslot.CustomerId = null;
            await db.SaveChangesAsync();
        };
    }
}

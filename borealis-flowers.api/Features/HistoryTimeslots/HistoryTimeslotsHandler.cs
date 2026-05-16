using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;

namespace borealis_flowers.api.Features.HistoryTimeslots;

public static class HistoryTimeslotsHandler
{
    public static Func<HistoryTimeslotDto, DataContext, Task<IResult>> AddTimeslotsHistory()
    {
        return async (HistoryTimeslotDto request, DataContext db) =>
        {
            try
            {
                var history = new HistoryTimeslot
                {
                    TimeslotId = request.TimeslotId,
                    Status = request.Status,
                    ExternalUserId = request.ExternalUserId
                };

                db.HistoryTimeslots.Add(history);
                await db.SaveChangesAsync();

                return Results.Created($"/history-timeslots/{history.Id}", history);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "An error occurred while adding timeslot history",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        };
    }
}

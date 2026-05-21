using borealis_flowers.api.Data;

namespace borealis_flowers.api.Features.Schedules;

public static class SchedulesModule
{
    public static void SchedulesEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/schedule", SchedulesHandler.GetSchedulesBySpecialist()).WithTags("Schedules");
        
        endpoints.MapGet("/schedule/master/{masterId}", async (string masterId, string date, DataContext db) =>
        {
            var handler = SchedulesHandler.GetScheduleDateId();
            return await handler(masterId, date, db);
        }).WithTags("Schedules");
        
        endpoints.MapPost("/schedule", SchedulesHandler.CreateSchedule()).WithTags("Schedules");

        endpoints.MapDelete("/schedule/{scheduleId}/master/{masterId}", SchedulesHandler.DeleteSchedule()).WithTags("Schedules");

        endpoints.MapDelete("/workingdays", SchedulesHandler.DeleteWorkingDays()).WithTags("Schedules");
    }
}

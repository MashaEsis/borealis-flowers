namespace borealis_flowers.api.Features.HistoryTimeslots;

public static class HistoryTimeslotsModule
{
    public static void TimeslotsHistoryEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/history-timeslots",
                HistoryTimeslotsHandler.AddTimeslotsHistory())
            .WithTags("HistoryTimeslots");
    }
}

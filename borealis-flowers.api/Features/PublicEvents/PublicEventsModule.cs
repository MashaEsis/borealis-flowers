namespace borealis_flowers.api.Features.PublicEvents;

public static class PublicEventsModule
{
    public static void PublicEventsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/events/available-timeslots",
                PublicEventsHandler.ListAvailableTimeslots)
            .WithTags("Events");
    }
}

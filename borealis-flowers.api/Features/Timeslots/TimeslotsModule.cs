namespace borealis_flowers.api.Features.Timeslots;

public static class TimeslotsModule
{
    public static void TimeslotsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/timeslots/workingday/{workingDayId}", TimeslotsHandler.GetTimeslotsByWorkingDay()).WithTags("Timeslots");
        endpoints.MapPut("/timeslots", TimeslotsHandler.UpdateTimeslots()).WithTags("Timeslots");
        endpoints.MapPut("/timeslots/release/{id}", TimeslotsHandler.ReleaseTimeslot()).WithTags("Timeslots");
        endpoints.MapPost(
            "/timeslots/workingslots/check",
            TimeslotsHandler.HasReservedTimeSlots())
            .WithTags("Timeslots");
    }
}

using borealis_flowers.api.Features.Schedules;

namespace borealis_flowers.api.Features.Specialists;

public static class SpecialistsModule
{
    public static void SpecialistsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/specialists", SpecialistsHandler.GetSpecialists()).WithTags("Specialists");
        endpoints.MapPut("/specialists", SpecialistsHandler.UpdateSpecialist()).WithTags("Specialists");
        endpoints.MapGet("/specialists/lastvisits/{customerID}", SpecialistsHandler.GetLastShedules()).WithTags("Specialists");
        endpoints.MapGet("/specialists/adress", SpecialistsHandler.GetCities()).WithTags("Specialists");

    }
}

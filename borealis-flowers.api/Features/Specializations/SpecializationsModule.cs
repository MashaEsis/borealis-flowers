namespace borealis_flowers.api.Features.Specializations;

public static class SpecializationsModule
{
    public static void SpecializationsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/specializations", SpecializationsHandler.GetSpecializations()).WithTags("Specializations");
    }
}

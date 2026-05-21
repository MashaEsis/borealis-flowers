namespace borealis_flowers.api.Features.Services;

public static class ServicesModule
{
    public static void ServicesEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/services", ServicesHandler.GetServices()).WithTags("Services");
        endpoints.MapGet("/services/{id:guid}", ServicesHandler.GetServiceById).WithTags("Services");
    }
}

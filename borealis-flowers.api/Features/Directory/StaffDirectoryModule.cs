namespace borealis_flowers.api.Features.Directory;

public static class StaffDirectoryModule
{
    public static void StaffDirectoryEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/staff/customers", StaffCustomersHandler.ListAsync)
            .WithTags("Directory")
            .RequireAuthorization(p => p.RequireRole("Florist", "Admin"));
    }
}

namespace borealis_flowers.api.Features.Directory;

public static class StaffDirectoryModule
{
    public static void StaffDirectoryEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/staff/customers", StaffCustomersHandler.ListAsync)
            .WithTags("Directory")
            .RequireAuthorization(p => p.RequireRole("Florist", "Admin"));

        endpoints.MapGet("/staff/users", StaffCustomersHandler.ListUsersAsync)
            .WithTags("Directory")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        endpoints.MapGet("/staff/florists", StaffCustomersHandler.ListFloristsAsync)
            .WithTags("Directory")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        endpoints.MapPost("/staff/florists/{customerId:guid}/demote", StaffCustomersHandler.DemoteFloristAsync)
            .WithTags("Directory")
            .RequireAuthorization(p => p.RequireRole("Admin"))
            .DisableAntiforgery();
    }
}

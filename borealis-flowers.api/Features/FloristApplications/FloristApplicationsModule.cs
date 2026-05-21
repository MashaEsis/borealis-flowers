namespace borealis_flowers.api.Features.FloristApplications;

public static class FloristApplicationsModule
{
    public static void FloristApplicationsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder g = endpoints.MapGroup("/florist-applications").WithTags("FloristApplications");

        g.MapPost("/", FloristApplicationsHandler.CreateAsync)
            .RequireAuthorization()
            .DisableAntiforgery();

        g.MapGet("/pending", FloristApplicationsHandler.ListPendingAsync)
            .RequireAuthorization(p => p.RequireRole("Admin"));

        g.MapPost("/{id:guid}/approve", FloristApplicationsHandler.ApproveAsync)
            .RequireAuthorization(p => p.RequireRole("Admin"))
            .DisableAntiforgery();

        g.MapPost("/{id:guid}/decline", FloristApplicationsHandler.DeclineAsync)
            .RequireAuthorization(p => p.RequireRole("Admin"))
            .DisableAntiforgery();
    }
}

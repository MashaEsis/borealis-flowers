namespace borealis_flowers.api.Features.AdminCatalog;

public static class AdminCatalogModule
{
    public static void AdminCatalogEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder g = endpoints.MapGroup("/admin/catalog").WithTags("AdminCatalog")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        g.MapGet("/services", AdminCatalogHandler.ListServices).RequireAuthorization();
        g.MapPost("/services", AdminCatalogHandler.CreateService).RequireAuthorization().DisableAntiforgery();
        g.MapPut("/services/{id:guid}", AdminCatalogHandler.UpdateService).RequireAuthorization().DisableAntiforgery();
        g.MapDelete("/services/{id:guid}", AdminCatalogHandler.DeleteService).RequireAuthorization().DisableAntiforgery();

        g.MapGet("/specializations", AdminCatalogHandler.ListSpecializations).RequireAuthorization();
        g.MapPost("/specializations", AdminCatalogHandler.CreateSpecialization).RequireAuthorization().DisableAntiforgery();
        g.MapPut("/specializations/{id:guid}", AdminCatalogHandler.UpdateSpecialization).RequireAuthorization()
            .DisableAntiforgery();

        g.MapGet("/service-prices", AdminCatalogHandler.ListServicePrices).RequireAuthorization();
        g.MapPut("/service-prices/{id:guid}", AdminCatalogHandler.UpdateServicePrice).RequireAuthorization()
            .DisableAntiforgery();

        g.MapGet("/timeslots", AdminCatalogHandler.ListTimeslots).RequireAuthorization();
        g.MapPut("/timeslots/{id:guid}", AdminCatalogHandler.UpdateTimeslot).RequireAuthorization()
            .DisableAntiforgery();
    }
}

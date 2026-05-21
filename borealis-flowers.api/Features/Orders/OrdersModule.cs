namespace borealis_flowers.api.Features.Orders;

using System.Security.Claims;
using borealis_flowers.api.Data;

public static class OrdersModule
{
    public static void OrdersEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder o = endpoints.MapGroup("/orders").WithTags("Orders");

        o.MapPost("/bouquet", OrdersHandler.PlaceBouquetAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
        o.MapPost("/event", OrdersHandler.PlaceEventAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
        o.MapPost("/event-plan", OrdersHandler.PlaceEventPlanAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
        o.MapGet("/mine", OrdersHandler.MyOrdersAsync).RequireAuthorization();
        o.MapGet("/mine/history", (ClaimsPrincipal user, DataContext db) =>
            OrdersHandler.MyOrdersAsync(user, db, true)).RequireAuthorization();
        o.MapGet("/florist", OrdersHandler.FloristOrdersAsync).RequireAuthorization();
        o.MapGet("/florist/history", OrdersHandler.FloristHistoryAsync).RequireAuthorization();
        o.MapGet("/admin/all", OrdersHandler.AllOrdersAdminAsync)
            .RequireAuthorization(p => p.RequireRole("Admin"));
        o.MapPut("/{id:guid}/state", OrdersHandler.UpdateOrderStateAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
        o.MapPut("/{id:guid}/admin-decision", OrdersHandler.AdminDecisionAsync)
            .RequireAuthorization(p => p.RequireRole("Admin"))
            .DisableAntiforgery();
    }
}

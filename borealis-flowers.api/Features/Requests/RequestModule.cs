namespace borealis_flowers.api.Features.Requests;

public static class RequestModule
{
    public static void RequestsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/requests", RequestsHandler.CreateRequest()).WithTags("Requests");
        endpoints.MapGet("/requests", RequestsHandler.GetAllRequests()).WithTags("Requests");
        endpoints.MapGet("/requests/by-state/{state}", RequestsHandler.GetRequestsByState()).WithTags("Requests");
        endpoints.MapPut("/requests/{id}", RequestsHandler.UpdateRequest()).WithTags("Requests");
    }
}

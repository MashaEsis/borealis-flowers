namespace borealis_flowers.api.Features.Home;

public static class HomeModule
{
    public static void HomeEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/home/highlights", HomeHandler.GetHighlightsAsync)
            .WithTags("Home");
    }
}

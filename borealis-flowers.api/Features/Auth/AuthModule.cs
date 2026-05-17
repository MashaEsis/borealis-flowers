namespace borealis_flowers.api.Features.Auth;

public static class AuthModule
{
    public static void AuthEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder g = endpoints.MapGroup("/auth").WithTags("Auth");
        g.MapPost("/register", AuthHandler.RegisterAsync).DisableAntiforgery();
        g.MapPost("/login", AuthHandler.LoginAsync).DisableAntiforgery();
        g.MapGet("/me", AuthHandler.MeAsync).RequireAuthorization();
    }
}

namespace borealis_flowers.api.Features.Wallet;

public static class WalletModule
{
    public static void WalletEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder g = endpoints.MapGroup("/wallet").WithTags("Wallet").RequireAuthorization();
        g.MapGet("/", WalletHandler.GetOverviewAsync);
        g.MapPost("/cards", WalletHandler.AddCardAsync).DisableAntiforgery();
        g.MapDelete("/cards/{id:guid}", WalletHandler.RemoveCardAsync);
        g.MapPost("/topup", WalletHandler.TopUpAsync).DisableAntiforgery();
    }
}

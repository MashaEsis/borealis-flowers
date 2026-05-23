using borealis_flowers.api.Features.Schedules;

namespace borealis_flowers.api.Features.Specialists;

public static class SpecialistsModule
{
    public static void SpecialistsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/specialists", SpecialistsHandler.GetSpecialists()).WithTags("Specialists");
        endpoints.MapPut("/specialists", SpecialistsHandler.UpdateSpecialist())
            .RequireAuthorization()
            .WithTags("Specialists");

        endpoints.MapGet("/specialists/{id:guid}/portfolio", PortfolioHandler.GetPublicAsync).WithTags("Specialists");
        endpoints.MapGet("/specialists/me/portfolio", PortfolioHandler.GetMineAsync).RequireAuthorization().WithTags("Specialists");
        endpoints.MapPut("/specialists/me/portfolio", PortfolioHandler.UpdateMineAsync).RequireAuthorization().DisableAntiforgery().WithTags("Specialists");
        endpoints.MapPost("/specialists/me/portfolio/works", PortfolioHandler.AddWorkAsync).RequireAuthorization().DisableAntiforgery().WithTags("Specialists");
        endpoints.MapDelete("/specialists/me/portfolio/works/{workId:guid}", PortfolioHandler.DeleteWorkAsync).RequireAuthorization().WithTags("Specialists");
        endpoints.MapPost("/specialists/me/portfolio/upload", PortfolioHandler.UploadPhotoAsync).RequireAuthorization().DisableAntiforgery().WithTags("Specialists");
        endpoints.MapGet("/specialists/lastvisits/{customerID}", SpecialistsHandler.GetLastShedules()).WithTags("Specialists");
        endpoints.MapGet("/specialists/adress", SpecialistsHandler.GetCities()).WithTags("Specialists");

    }
}

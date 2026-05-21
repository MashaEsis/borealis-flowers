using System.Globalization;
using borealis_flowers.api.Data;

namespace borealis_flowers.api.Features.Statistics;

public static class StatisticsModule
{
    public static void StatisticsEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/statistics", async (MonthStatisticsRequest req, IFireApiConnector fireDb) =>
        {
            try
            {
                if (ValidateParamsForReservationInfo(req) == false)
                {
                    var msg = $"Model is not valid: monthName = {req.MonthName}, monthId = {req.Id}, date = {req.Date}";
                    return Results.BadRequest(msg);
                }
                return Results.Ok(await fireDb.GetReservationsForDay(req.MonthName, req.Id, req.Date));
            }
            catch (Exception e)
            {
                return Results.Problem($"Error: {e.Message}");
            }
        }).WithTags("Statistics");

        endpoints.MapPost("/statistics/month", async (WholeMonthStatisticsRequest req, IFireApiConnector fireDb) =>
        {
            try
            {
                if (ValidateParamsForReservationInfo(req) == false)
                {
                    var msg = $"Model is not valid: monthName = {req.MonthName}, year = {req.Year}";
                    return Results.BadRequest(msg);
                }
                return Results.Ok(await fireDb.GetReservationsForWholeMonthDay(req.MonthName, req.Year));
            }
            catch (Exception e)
            {
                return Results.Problem($"Error: {e.Message}");
            }
        }).WithTags("Statistics");
    }

    public static bool ValidateParamsForReservationInfo(MonthStatisticsRequest request)
    {
        if (request.MonthName == null
            || !DateTime.TryParseExact(
                request.MonthName,
                "MMMM",
                CultureInfo.CreateSpecificCulture("ru"), DateTimeStyles.None,
                out var resultMonthParse)
            || request.Id == null
            || request.Id.Length < 1
            || request.Date == null
            || !DateTime.TryParse(request.Date, out var resultDateParse)
           )
        {
            return false;
        }

        return true;
    }
    public static bool ValidateParamsForReservationInfo(WholeMonthStatisticsRequest req)
    {
        return !(string.IsNullOrEmpty(req.MonthName)
                || !DateTime.TryParseExact(
                    req.MonthName,
                    "MMMM",
                    CultureInfo.CreateSpecificCulture("ru"), DateTimeStyles.None,
                    out var resultMonthParse)
                || req.Year < 2020 || req.Year > DateTime.Today.Year);

    }
}

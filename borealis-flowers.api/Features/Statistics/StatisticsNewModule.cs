using System.Globalization;
using borealis_flowers.api.Data;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Statistics;

public static class StatisticsNewModule
{
    public static void StatisticsNewEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/statistics", async (MonthStatisticsRequest req, DataContext db) =>
        {
            try
            {
                if (ValidateParamsForDayStatistics(req) == false)
                {
                    var msg = $"Model is not valid: date = {req.Date}, specialistId = {req.SpecialistId}";
                    return Results.BadRequest(msg);
                }

                var date = DateTime.Parse(req.Date!);
                var dateStart = date.Date;
                var dateEnd = dateStart.AddDays(1);

                var query = db.Timeslots
                    .AsNoTracking()
                    .Include(t => t.DateSchedule)
                    .Include(t => t.Customer)
                    .Where(t => t.DateSchedule.Date >= dateStart
                                && t.DateSchedule.Date < dateEnd
                                && t.CustomerId != null
                                && !t.Available);

                if (Guid.TryParse(req.SpecialistId, out var specialistId))
                {
                    query = query.Where(t => t.DateSchedule.SpecialistId == specialistId);
                }

                var timeslots = await query.ToListAsync();

                var result = timeslots.Select(t => new ReservationInfo
                {
                    Id = t.Id.ToString(),
                    Time = $"{t.Time}:00",
                    ClientName = t.Customer?.Name ?? string.Empty,
                    ClientPhone = t.Customer?.Phone ?? string.Empty,
                    Year = t.DateSchedule.Date.Year,
                    Month = t.DateSchedule.Date.Month,
                    Day = t.DateSchedule.Date.Day,
                    Hours = t.Time,
                    Minutes = 0,
                    DateTime = new DateTime(t.DateSchedule.Date.Year, t.DateSchedule.Date.Month, t.DateSchedule.Date.Day, t.Time, 0, 0)
                }).OrderBy(r => r.DateTime);

                return Results.Ok(result);
            }
            catch (Exception e)
            {
                return Results.Problem($"Error: {e.Message}");
            }
        }).WithTags("Statistics");

        endpoints.MapPost("/statistics/month", async (WholeMonthStatisticsRequest req, DataContext db) =>
        {
            try
            {
                if (ValidateParamsForMonthStatistics(req) == false)
                {
                    var msg = $"Model is not valid: monthName = {req.MonthName}, year = {req.Year}";
                    return Results.BadRequest(msg);
                }

                // Parse the Russian month name to get month number
                var parsedDate = DateTime.ParseExact(
                    req.MonthName,
                    "MMMM",
                    CultureInfo.CreateSpecificCulture("ru"));
                var monthNumber = parsedDate.Month;

                var monthStart = new DateTime(req.Year, monthNumber, 1);
                var monthEnd = monthStart.AddMonths(1);

                var query = db.Timeslots
                    .AsNoTracking()
                    .Include(t => t.DateSchedule)
                    .Include(t => t.Customer)
                    .Where(t => t.DateSchedule.Date >= monthStart
                                && t.DateSchedule.Date < monthEnd
                                && t.CustomerId != null
                                && !t.Available);

                if (!string.IsNullOrEmpty(req.SpecialistId) && Guid.TryParse(req.SpecialistId, out var specialistId))
                {
                    query = query.Where(t => t.DateSchedule.SpecialistId == specialistId);
                }

                var timeslots = await query.ToListAsync();

                var result = timeslots.Select(t => new ReservationInfo
                {
                    Id = t.Id.ToString(),
                    Time = $"{t.Time}:00",
                    ClientName = t.Customer?.Name ?? string.Empty,
                    ClientPhone = t.Customer?.Phone ?? string.Empty,
                    Year = t.DateSchedule.Date.Year,
                    Month = t.DateSchedule.Date.Month,
                    Day = t.DateSchedule.Date.Day,
                    Hours = t.Time,
                    Minutes = 0,
                    DateTime = new DateTime(t.DateSchedule.Date.Year, t.DateSchedule.Date.Month, t.DateSchedule.Date.Day, t.Time, 0, 0)
                }).OrderBy(r => r.DateTime);

                return Results.Ok(result);
            }
            catch (Exception e)
            {
                return Results.Problem($"Error: {e.Message}");
            }
        }).WithTags("Statistics");
    }

    public static bool ValidateParamsForDayStatistics(MonthStatisticsRequest request)
    {
        return request.Date != null && DateTime.TryParse(request.Date, out _);
    }

    public static bool ValidateParamsForMonthStatistics(WholeMonthStatisticsRequest req)
    {
        return !(string.IsNullOrEmpty(req.MonthName)
                 || !DateTime.TryParseExact(
                     req.MonthName,
                     "MMMM",
                     CultureInfo.CreateSpecificCulture("ru"), DateTimeStyles.None,
                     out _)
                 || req.Year < 2020 || req.Year > DateTime.Today.Year);
    }
}


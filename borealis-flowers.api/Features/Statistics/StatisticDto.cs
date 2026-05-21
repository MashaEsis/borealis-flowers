namespace borealis_flowers.api.Features.Statistics;

public class StatisticsDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}

public class WholeMonthStatisticsRequest
{
    public string? SpecialistId { get; set; }
    public string MonthName { get; set; }
    public int Year { get; set; }
}

public class MonthStatisticsRequest
{
    public string? SpecialistId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? MonthName { get; set; }
    public string? Id { get; set; }
    public string? Date { get; set; }
}

public class ClientInfo
{
    public string ClientName { get; set; }
    public string ClientPhone { get; set; }
}

public class ReservationInfo : ClientInfo
{
    public string Id { get; set; }
    public string Time { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public DateTime DateTime { get; set; }

    public void InitializeDateTime()
    {
        DateTime = new DateTime(Year, Month, Day, Hours, Minutes, 0);
    }
}

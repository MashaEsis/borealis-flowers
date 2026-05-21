using borealis_flowers.api.Features.Statistics;
using borealis_flowers.api.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace borealis_flowers.api.Data;
public interface IFireApiConnector
{
    Task<IEnumerable<ReservationInfo>> GetReservationsForDay(string monthName, string monthId, string date);
    Task<IEnumerable<ReservationInfo>> GetReservationsForWholeMonthDay(string monthName, int year);
}
public class CoreFireApiConnector : IFireApiConnector
{
    private readonly string _connectionString;
    private FirebaseClient _firebase;

    public CoreFireApiConnector(IOptions<FirebaseSettings> fireConnection)
    {
        _connectionString = fireConnection.Value.ConnectionString;
        _firebase = new FirebaseClient(_connectionString);
    }

    public async Task<IEnumerable<ReservationInfo>> GetReservationsForDay(string monthName, string monthId, string date)
    {
        IReadOnlyCollection<FirebaseObject<object>> result = await _firebase
            .Child("reservation")
            .Child(monthName) // e.g. май
            .Child(monthId)
            .Child("Days")
            .Child(date) //2025-05-12
            .OnceAsync<object>();

        if (result == null)
        {
            throw new Exception("Result is not defined!");
        }
        var resultSet = new List<ReservationInfo>();

        foreach (var item in result)
        {
            var clientAndTime = JsonConvert.DeserializeObject<Dictionary<string, ClientInfo>>(item.Object.ToString()).FirstOrDefault();
            resultSet.Add(new ReservationInfo
            {
                Id = item.Key,
                Time = clientAndTime.Key,
                ClientName = clientAndTime.Value.ClientName,
                ClientPhone = clientAndTime.Value.ClientPhone
            });
        }
        return resultSet;
    }

    public async Task<IEnumerable<ReservationInfo>> GetReservationsForWholeMonthDay(string monthName, int year)
    {
        var monthIds = await _firebase
            .Child("reservation")
            .Child(monthName)
            .OnceAsync<object>();

        var resultSet = new List<ReservationInfo>();
        foreach (var monthIdObj in monthIds)
        {
            var days = await _firebase
                .Child("reservation")
                .Child(monthName)
                .Child(monthIdObj.Key)
                .Child("Days")
                .OnceAsync<object>();

            foreach (var dayObj in days)
            {
                // dayObj.Key is the date string, e.g. "2020-05-05"
                if (!dayObj.Key.StartsWith(year.ToString()))
                    continue;

                // Parse date
                var dateParts = dayObj.Key.Split('-');
                int parsedYear = int.Parse(dateParts[0]);
                int parsedMonth = int.Parse(dateParts[1]);
                int parsedDay = int.Parse(dateParts[2]);

                // Each dayObj.Object is a nested reservationId -> time -> clientInfo
                var reservations = JsonConvert.DeserializeObject<JObject>(dayObj.Object.ToString());
                foreach (var reservationId in reservations)
                {
                    var times = reservationId.Value as JObject;
                    if (times == null) continue;
                    foreach (var timeEntry in times)
                    {
                        var clientInfo = timeEntry.Value.ToObject<ClientInfo>();
                        // Parse time (e.g., "10:00")
                        var timeParts = timeEntry.Key.Split(':');
                        int parsedHour = int.Parse(timeParts[0]);
                        int parsedMinute = timeParts.Length > 1 ? int.Parse(timeParts[1]) : 0;
                        var reservation = new ReservationInfo
                        {
                            Id = reservationId.Key,
                            Time = timeEntry.Key,
                            ClientName = clientInfo.ClientName,
                            ClientPhone = clientInfo.ClientPhone,
                            Year = parsedYear,
                            Month = parsedMonth,
                            Day = parsedDay,
                            Hours = parsedHour,
                            Minutes = parsedMinute
                        };
                        reservation.InitializeDateTime();
                        resultSet.Add(reservation);
                    }
                }
            }
        }
        return resultSet;
    }

    public async Task<IEnumerable<ReservationInfo>> GetReservationsForWholeMonth(string monthName, string monthId, int month)
    {
        IReadOnlyCollection<FirebaseObject<object>> result = await _firebase
            .Child("reservation")
            .Child(monthName) // e.g. май
            .Child("Days")
            .OnceAsync<object>();

        if (result == null)
        {
            throw new Exception("Result is not defined!");
        }
        var resultSet = new List<ReservationInfo>();

        foreach (var item in result)
        {
            var clientAndTime = JsonConvert.DeserializeObject<Dictionary<string, ClientInfo>>(item.Object.ToString()).FirstOrDefault();
            resultSet.Add(new ReservationInfo
            {
                Id = item.Key,
                Time = clientAndTime.Key,
                ClientName = clientAndTime.Value.ClientName,
                ClientPhone = clientAndTime.Value.ClientPhone
            });
        }

        return resultSet;
    }
}

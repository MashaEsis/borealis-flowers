using borealis_flowers.api.Helpers;

namespace borealis_flowers.api.Models
{
    public class VisitData
    {
        public DateTime Date { get; set; }
    }
    public class Availability
    {
        public Guid Id { get; set; }
        public Guid SpecialistId { get; set; }
        public DateOnly Month { get; set; }
        public int Day { get; set; }
        public TimeOnly Hour { get; set; }
        //public int Hour { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class ScheduleDay
    {
        public DateTime Day { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class TimeSchedule
    {
        public Guid Id { get; set; }
        public Guid DateScheduleId { get; set; }

        public int Hour { get; set; }
        public bool IsAvailable { get; set; }
    }

    // not sure for what it
    public class Schedule
    {
        private string _monthNameEn;

        public string MonthNameEn
        {
            get
            {
                if (string.IsNullOrEmpty(_monthNameEn))
                {
                    return WorkingDates.First().GetMonthName();
                }
                else
                {
                    return _monthNameEn;
                }
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _monthNameEn = value;
                }
            }
        }

        public int Month => WorkingDates.First().Month;
        public int Year => WorkingDates.First().Year;
        public List<DateTime> WorkingDates { get; set; }
    }
}

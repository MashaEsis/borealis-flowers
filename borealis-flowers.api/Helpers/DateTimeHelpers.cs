using borealis_flowers.api.Models;
using System.Globalization;
using borealis_flowers.api.Data.Models;

namespace borealis_flowers.api.Helpers
{
    public static class DateTimeHelpers
    {
        public static DateOnly AsDateOnly(this DateTime value)
        {
            return DateOnly.FromDateTime(value);
        }
        public static string GetMonthNameByMonthNumber(this int number)
        {
            var monthName = CultureInfo.CreateSpecificCulture("ru").DateTimeFormat.GetMonthName(number);
            return monthName.Capitalize();
        }

        public static string Capitalize(this string input)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
        }

        public static string ToIsoDateString(this DateTime self)
        {
            return $"{self.Year:D4}-{self.Month:D2}-{self.Day:D2}";
        }

        public static string ToIsoDateString(this DateTime? self)
        {
            if (self == null)
                return null;

            return $"{self.Value.Year:D4}-{self.Value.Month:D2}-{self.Value.Day:D2}";
        }

        public static string GetMonthName(this DateTime self)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(self.Month);
        }

        public static IEnumerable<DateTime> GetWorkingDaysInMonth(this DateTime? self)
        {
            if (self == null)
                return Enumerable.Empty<DateTime>();

            int days = DateTime.DaysInMonth(self.Value.Year, self.Value.Month);
            List<DateTime> dates = new List<DateTime>();
            for (int i = 1; i <= days; i++)
            {
                dates.Add(new DateTime(self.Value.Year, self.Value.Month, i));
            }

            var weekDays = dates.Where(d => d.DayOfWeek > DayOfWeek.Sunday & d.DayOfWeek < DayOfWeek.Saturday);

            return weekDays;
        }

        public static IEnumerable<DateTime> GetWorkingDaysIn2By2Schedule(this DateTime startingDate)
        {
            List<DateTime> dates = new List<DateTime>();

            int daysInMonth = DateTime.DaysInMonth(startingDate.Year, startingDate.Month);

            for (int i = startingDate.Day; i <= daysInMonth; i = i + 4)
            {
                dates.Add(new DateTime(startingDate.Year, startingDate.Month, i));

                if ((i + 1) <= daysInMonth)
                {
                    dates.Add(new DateTime(startingDate.Year, startingDate.Month, i + 1));
                }
            }

            return dates;
        }
        public static IEnumerable<DateSchedule> GenerateOnlyWorkingDaysForThisMonthStartingFromDay5By2(this DateTime startingDate)
        {
            int daysInMonth = DateTime.DaysInMonth(startingDate.Year, startingDate.Month);
            List<DateSchedule> dates = new List<DateSchedule>(daysInMonth);
            int dayInWeek = ((int)startingDate.DayOfWeek + 6) % 7 + 1;

            for (int i = startingDate.Day; i <= daysInMonth; i++, dayInWeek++)
            {
                if (IsWorkingDay(dayInWeek))
                {
                    dates.Add(new DateSchedule
                    {
                        Date = new DateTime(
                            startingDate.Year,
                            startingDate.Month,
                            i),
                        IsWorkingDay = true
                    });
                }
                if (dayInWeek > 7)
                {
                    dayInWeek = 1;
                }
            }
            return dates;
        }
        private static bool IsWorkingDay(int dayInWeek)
        {
            return dayInWeek % 7 == 6 || dayInWeek % 7 == 0;
        }
        public static IEnumerable<DateTime> GetHeadOfMonth(this DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var prevMonth = date.AddMonths(-1);

            int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            var tailCounter = dayOfWeek - DayOfWeek.Sunday;

            List<DateTime> dates = new List<DateTime>(tailCounter);

            for (int i = daysInPrevMonth - tailCounter + 1; i <= daysInPrevMonth; i++)
            {
                dates.Add(new DateTime(prevMonth.Year, prevMonth.Month, i));
            }

            return dates;
        }

        public static IEnumerable<DateTime> GetTailOfMonth(this DateTime date)
        {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            var lastDayInMonth = new DateTime(date.Year, date.Month, daysInMonth).DayOfWeek;

            var nextMonth = date.AddMonths(1);

            var headCounter = DayOfWeek.Saturday - lastDayInMonth;

            List<DateTime> dates = new List<DateTime>(headCounter);

            for (int i = 1; i <= headCounter; i++)
            {
                dates.Add(new DateTime(nextMonth.Year, nextMonth.Month, i));
            }

            return dates;
        }

        //public static IEnumerable<ScheduleDay> GetDatesToRenderSchedule(this DateTime startingDate)
        //{
        //    List<ScheduleDay> dates = new List<ScheduleDay>();

        //    int daysInMonth = DateTime.DaysInMonth(startingDate.Year, startingDate.Month);

        //    for (int i = startingDate.Day; i <= daysInMonth; i = i + 4)
        //    {
        //        dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i), IsAvailable = true });

        //        // add other day if it's possible
        //        if ((i + 1) <= daysInMonth)
        //        {
        //            dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i + 1), IsAvailable = true });
        //        }

        //        // fill in non-working days
        //        if ((i + 2) <= daysInMonth)
        //        {
        //            dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i + 2), IsAvailable = false });
        //        }
        //        if ((i + 3) <= daysInMonth)
        //        {
        //            dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i + 3), IsAvailable = false });
        //        }
        //    }

        //    return dates;
        //}

        /// <summary>
        /// Should return whole set of dates in month but with mark working dates with boolean flag
        /// </summary>
        /// <param name="startingDate"></param>
        /// <returns></returns>
        public static IEnumerable<ScheduleDay> GetScheduleForMonthStartingFromDay(this DateTime startingDate)
        {
            int daysInMonth = DateTime.DaysInMonth(startingDate.Year, startingDate.Month);

            // add NON-working dates to schedule in the beginning
            List<ScheduleDay> dates = new List<ScheduleDay>(daysInMonth);
            for (int i = 1; i < startingDate.Day; i++)
            {
                dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i), IsAvailable = false });
            }

            for (int i = startingDate.Day; i <= daysInMonth; i = i + 4)
            {
                dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i), IsAvailable = true });

                // add other day if it's possible
                if ((i + 1) <= daysInMonth)
                {
                    dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i + 1), IsAvailable = true });
                }

                // fill in non-working days
                if ((i + 2) <= daysInMonth)
                {
                    dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i + 2), IsAvailable = false });
                }
                if ((i + 3) <= daysInMonth)
                {
                    dates.Add(new ScheduleDay { Day = new DateTime(startingDate.Year, startingDate.Month, i + 3), IsAvailable = false });
                }
            }

            return dates;
        }

        public static IEnumerable<DateSchedule> GenerateOnlyWorkingDaysForThisMonthStartingFromDay(this DateTime startingDate)
        {
            int daysInMonth = DateTime.DaysInMonth(startingDate.Year, startingDate.Month);

            // add NON-working dates to schedule in the beginning
            List<DateSchedule> dates = new List<DateSchedule>(daysInMonth);

            for (int i = startingDate.Day; i <= daysInMonth; i = i + 4)
            {
                dates.Add(new DateSchedule
                {
                    Date = new DateTime(startingDate.Year, startingDate.Month, i),
                    IsWorkingDay = true
                });

                // add other day if it's possible
                if ((i + 1) <= daysInMonth)
                {
                    dates.Add(new DateSchedule
                    {
                        Date = new DateTime(startingDate.Year, startingDate.Month, i + 1),
                        IsWorkingDay = true
                    });
                }
            }
            return dates;
        }
        public static DateTime StartOfMonth(this DateTime self, CultureInfo culture)
        {
            var month = culture.Calendar.GetMonth(self);
            var year = culture.Calendar.GetYear(self);
            return culture.Calendar.ToDateTime(year, month, 1, 0, 0, 0, 0);
        }

        public static DateTime EndOfMonth(this DateTime self, CultureInfo culture)
        {
            var month = culture.Calendar.GetMonth(self);
            var year = culture.Calendar.GetYear(self);
            var days = culture.Calendar.GetDaysInMonth(year, month);
            return culture.Calendar.ToDateTime(year, month, days, 0, 0, 0, 0);
        }

        public static DateTime StartOfWeek(this DateTime self, DayOfWeek firstDayOfWeek)
        {
            var diff = (7 + (self.DayOfWeek - firstDayOfWeek)) % 7;
            if (self.Year == 1 && self.Month == 1 && (self.Day - diff) < 1)
            {
                return self.Date;
            }
            return self.AddDays(-1 * diff).Date;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JellyfinNotify.Plugin.Extensions
{
    public static class DateExtensions
    {
        public static DateOnly? ToDateOnly(this DateTime? datetime)
        {
            if (datetime == null)
            {
                return null;
            }

            var localDateTime = datetime.Value.ToLocalTime();
            return new DateOnly(localDateTime.Year, localDateTime.Month, localDateTime.Day);
        }

        // Is Tomorrow? - DayCount 1
        public static bool IsInDays(this DateOnly date, int dayCount)
        {
            var today = DateOnly.FromDateTime(DateTime.Now.ToLocalTime());

            return today.AddDays(dayCount) == date;
        }

        public static int DaysFromToday(this DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.Now.ToLocalTime());

            return date.DayNumber - today.DayNumber;
        }
    }
}

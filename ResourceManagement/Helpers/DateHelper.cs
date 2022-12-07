using System;
using System.Collections.Generic;

namespace ResourceManagement.Helpers
{
    public static class DateHelper
    {
        public static string GetDateInRequiredFormat(this string actualdate)
        {
            if (string.IsNullOrWhiteSpace(actualdate))
                return string.Empty;
            string dateString = actualdate;
            DateTime dateTime = DateTime.Parse(dateString);
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static List<string> WeekdaysList()
        {
            var weekDays = new List<string>();
            weekDays.Add(Constants.Monday);
            weekDays.Add(Constants.Tuesday);
            weekDays.Add(Constants.Wednesday);
            weekDays.Add(Constants.Thursday);
            weekDays.Add(Constants.Friday);
            weekDays.Add(Constants.Saturday);
            weekDays.Add(Constants.Sunday);

            return weekDays;
        }
    }
}
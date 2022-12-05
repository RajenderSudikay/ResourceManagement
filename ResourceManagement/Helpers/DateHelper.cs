using System;

namespace ResourceManagement.Helpers
{
    public static class DateHelper
    {
        private static string GetDateInRequiredFormat(this string actualdate)
        {
            if (string.IsNullOrWhiteSpace(actualdate))
                return string.Empty;
            string dateString = actualdate;
            DateTime dateTime = DateTime.Parse(dateString);
            return dateTime.ToString("yyyy-MM-dd");
        }
    }
}
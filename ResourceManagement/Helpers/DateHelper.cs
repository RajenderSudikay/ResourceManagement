using System;
using System.Collections.Generic;
using System.Web.Mvc;

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

        public static List<SelectListItem> MonthList()
        {
            var monthList = new List<SelectListItem>();
            var monthsList = new List<DateTime>();
            monthsList.Add(DateTime.Now);
            monthsList.Add(DateTime.Now.AddMonths(-1));
            monthsList.Add(DateTime.Now.AddMonths(-2));
            monthsList.Add(DateTime.Now.AddMonths(-3));
            monthsList.Add(DateTime.Now.AddMonths(-4));
            monthsList.Add(DateTime.Now.AddMonths(-5));

            foreach (var month in monthsList)
            {
                monthList.Add(new System.Web.Mvc.SelectListItem()
                {
                    Text = month.ToString("MMM") + "-" + month.Year,
                    Value = month.ToString("MMM") + "-" + month.Year
                });
            }

            return monthList;
        }

        public static string ConvertDateToServerSystemFormat(DateTime? date)
        {
            if (date.ToString().Contains("-"))
            {
                var actualDate = date.ToString().Replace("00:00:00", "").Replace("12:00:00 AM", "").Replace('/', '-').Trim().Split('-');
                return actualDate[0].Trim() + "-" + actualDate[1].Trim() + "-" + actualDate[2].Trim();
            }

            if (date.ToString().Contains("/"))
            {
                var actualDate = date.ToString().Replace("00:00:00", "").Replace("12:00:00 AM", "").Replace('/', '-').Trim().Split('-');
                return actualDate[1].Trim() + "-" + actualDate[0].Trim() + "-" + actualDate[2].Trim();
            }

            return "";
        }


        public static string DateShortFormats(string shortName, string longName)
        {
            var dates = new Dictionary<string, string>();
            dates.Add("Jan", "January");
            dates.Add("Feb", "February");
            dates.Add("Mar", "March");
            dates.Add("Apr", "April");
            dates.Add("May", "May");
            dates.Add("Jun", "June");
            dates.Add("Jul", "July");
            dates.Add("Aug", "August");
            dates.Add("Sep", "September");
            dates.Add("Oct", "October");
            dates.Add("Nov", "November");
            dates.Add("Dec", "December");

            return !string.IsNullOrEmpty(shortName) ? dates[shortName] : dates[longName];
        }
    }
}
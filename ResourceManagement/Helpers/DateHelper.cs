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
            var monthList = new List<System.Web.Mvc.SelectListItem>();
            var monthsList = new List<DateTime>();
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
    }
}
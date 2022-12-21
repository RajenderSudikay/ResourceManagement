using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourceManagement.Models
{
    public class TimesheetReportModel
    {
        public class TimeSheetViewModel
        {
            public string EmpId { get; set; }
            public string WeekStartDate { get; set; }
            public string WeekEndDate { get; set; }
            public string WeekNumber { get; set; }
        }


        public class TimeSheetEmailReport
        {
            public List<ambctaskcapture> reports { get; set; }
            public emplogin empData { get; set; }

            public string MondayHours { get; set; }
            public string MondayOverTime { get; set; }
            public string MondayHoliday { get; set; }

            public string TuesdayHours { get; set; }
            public string TuesdayOverTime { get; set; }
            public string TuesdayHoliday { get; set; }

            public string WednesdayHours { get; set; }
            public string WednesdayOverTime { get; set; }
            public string WednesdayHoliday { get; set; }


            public string ThursdayHours { get; set; }
            public string ThursdayOverTime { get; set; }
            public string ThursdayHoliday { get; set; }

            public string FridayHours { get; set; }
            public string FridayOverTime { get; set; }
            public string FridayHoliday { get; set; }

            public string SaturdayHours { get; set; }
            public string SaturdayOverTime { get; set; }
            public string SundayHours { get; set; }
            public string SundayOverTime { get; set; }
            public string TotalHoursSpent { get; set; }

        }

        public class TimeSheetAjaxReportModel
        {
            public string WeekStartDate { get; set; }

            public string WeekEndDate { get; set; }

            public string WeekNumber { get; set; }

            public string EmpId { get; set; }

            public List<string> Employees { get; set; }

            public string ClientName { get; set; }

            public string Type { get; set; }
        }

        public class TimeSheetReportViewModel
        {
            public AMBC_Active_Emp_view EmployeeInfo { get; set; }
            public List<ambctaskcapture> timeSheetInfo { get; set; }


        }

        public class SourceFile
        {
            public string Name { get; set; }
            public string Extension { get; set; }
            public Byte[] FileBytes { get; set; }
        }


    }
}
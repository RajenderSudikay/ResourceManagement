﻿using System;
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

        public class TimeSheetReport
        {
            public List<ambctaskcapture> reports { get; set; }
            public emplogin empData { get; set; }

            public string MondayHours { get; set; }
            public string TuesdayHours { get; set; }

            public string WednesdayHours { get; set; }

            public string ThursdayHours { get; set; }
            public string FriidayHours { get; set; }

        }
    }
}
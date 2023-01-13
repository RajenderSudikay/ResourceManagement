using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourceManagement.Models
{
    public class LeaveOrHolidayModel
    {
        public class AjaxLeaveOrHolidayModel
        {
            public string EmpId { get; set; }

            public string EmpRegion { get; set; }
            public string WeekStartDate { get; set; }
            public string WeekEndDate { get; set; }
            public string WeekNumber { get; set; }
        }

        public class ReportLeaveOrHolidayInfo
        {
            public string LeaveDate { get; set; }

            public string LeaveType { get; set; }          
        }
    }
}
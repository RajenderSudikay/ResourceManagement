using System.Collections.Generic;

namespace ResourceManagement.Models
{
    public class RMA_EmployeeModel
    {
        public AMBC_Active_Emp_view AMBC_Active_Emp_view { get; set; }
        public emp_project projectInfo { get; set; }
        public string leaveOrHolidayInfo { get; set; }
        public tbld_ambclogininformation signInOutInfo { get; set; }
    }

    public class RMA_TimeSheetAjaxModel
    {
        public string Date { get; set; }
        public string Category { get; set; }
        public string IncidentNumber { get; set; }
        public string IncidentDescription { get; set; }
        public string Requester { get; set; }
        public string Urgency { get; set; }
        public string Status { get; set; }
        public string ClosedDate { get; set; }
        public string TimeSpent { get; set; }
        public string Comments { get; set; }
    }

    public class RMA_TimeSheetWeekData
    {
        public string EmpId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class RMA_LeaveOrHolidayInfo
    {
        public string LeaveOrHolidayDate { get; set; }
        public string Reason { get; set; }
    }

    public class TimeSheetViewModel
    {
        public string EmpId { get; set; }
        public string WeekStartDate { get; set; }
        public string WeekEndDate { get; set; }
        public string WeekNumber { get; set; }
    }
}
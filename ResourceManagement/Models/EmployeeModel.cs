namespace ResourceManagement.Models
{
    public class RMA_EmployeeModel
    {
        public emp_info empInfo { get; set; }
        public emp_project projectInfo { get; set; }
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
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceManagement.Models
{
    public class TimeSheet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int tslogno { get; set; }
        public string employeeid { get; set; }
        public string employeename { get; set; }
        public string empdesignation { get; set; }
        public string empmanager { get; set; }
        public string empcontactnumber { get; set; }
        public string clientname { get; set; }
        public string projectname { get; set; }
        public string projstatus { get; set; }
        public string taskdate { get; set; }
        public string category { get; set; }
        public string incidentnumber { get; set; }
        public string taskdetails { get; set; }
        public string requester { get; set; }
        public string callpriority { get; set; }
        public string callstatus { get; set; }
        public string closeddate { get; set; }
        public string timespent { get; set; }
        public string comments { get; set; }
    }

    public class EmployeeModel
    {
        public string empID { get; set; }
        public string empName { get; set; }
        public string empDesignation { get; set; }
        public string empManager { get; set; }
        public string empPhone { get; set; }
        public string empClientName { get; set; }
        public string empProjectName { get; set; }
        public string empProjectStatus { get; set; }
    }


    public class TimeSheetAjaxModel
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
namespace ResourceManagement.Models
{
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


    public class TimeSheetModel
    {
        public EmployeeModel empData { get; set; }
        public string empName { get; set; }
        public string empDesignation { get; set; }
        public string empManager { get; set; }
        public string empPhone { get; set; }
        public string empClientName { get; set; }
        public string empProjectName { get; set; }
        public string empProjectStatus { get; set; }
    }
}
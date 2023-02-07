using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Web;
using System.Web.Mvc;

namespace ResourceManagement.Models
{
    public class StatusReportModel
    {
        //To change label title value  
        [DisplayName("Employee Name")]
        public string EmployeeName { get; set; }

        [DisplayName("Employee ID")]
        public string EmployeeID { get; set; }

        //To change label title value  
        [DisplayName("Month")]
        public string Month { get; set; }
        public string TemplateType { get; set; }
        public string ProjectID { get; set; }
        public string ClientName { get; set; }

        //To change label title value  
        [DisplayName("Upload Report File")]
        public string ExcelPath { get; set; }
        public HttpPostedFileBase ExcelFile { get; set; }
        public List<SelectListItem> EmployeeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MonthList { get; set; } = new List<SelectListItem>();
        public string FieldsIndexJson { get; set; }
        public string ValuesMappingJson { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SelectedColumnIndex { get; set; }
        public string SelectedColumnName { get; set; }
    }
    public class FieldsIndex
    {
        public string FieldName { get; set; }
        public string Index { get; set; }
    }

    public class StatusReport_Template1Model
    {
        public List<monthlyreports_Template1> Template1Reports { get; set; } = new List<monthlyreports_Template1>();
    }
    public class StatusReport_Template2Model
    {
        public List<monthlyreports_Template2> Template2Reports { get; set; } = new List<monthlyreports_Template2>();
    }
    public class StatusReport_Template3Model
    {
        public List<monthlyreports_Template3> Template3Reports { get; set; } = new List<monthlyreports_Template3>();
    }
    public class RMA_StatusReportModel
    {
        public RMA_EmployeeModel RMA_EmployeeModel { get; set; } = new RMA_EmployeeModel();
        public StatusReportModel StatusReportInfo { get; set; } = new StatusReportModel();
        public JsonResponseModel Response { get; set; } = new JsonResponseModel();
    }

    public class RMA_StatusReportViewModel
    {
        public RMA_StatusReportModel RMA_StatusReportModel { get; set; } = new RMA_StatusReportModel();

        public monthlyreports_Template1 Template1Report { get; set; } = new monthlyreports_Template1();
    }


    public class StatusReportChartModel
    {       

        [JsonPropertyName("EmployeeName")]
        public string EmployeeName { get; set; } = string.Empty;

        [JsonPropertyName("EmployeeID")]
        public string EmployeeID { get; set; } = string.Empty;

        [JsonPropertyName("Month")]
        public string Month { get; set; } = string.Empty;

        [JsonPropertyName("MonthNumber")]
        public int MonthNumber { get; set; } = 0;        

        [JsonPropertyName("Year")]
        public int Year { get; set; } = 0;

        [JsonPropertyName("ProjectID")]
        public string ProjectID { get; set; } = string.Empty;

        [JsonPropertyName("ClientName")]
        public string ClientName { get; set; } = string.Empty;

    }
}
﻿using System;
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
        public List<string> EmployeeID { get; set; } = new List<string>();

        [JsonPropertyName("Month")]
        public string Month { get; set; } = string.Empty;

        [JsonPropertyName("MonthNumber")]
        public int MonthNumber { get; set; } = 0;

        [JsonPropertyName("Year")]
        public int Year { get; set; } = 0;

        [JsonPropertyName("ProjectID")]
        public int ProjectID { get; set; } = 0;

        [JsonPropertyName("ClientName")]
        public string ClientName { get; set; } = string.Empty;

        [JsonPropertyName("ReportType")]
        public string ReportType { get; set; } = string.Empty;

        [JsonPropertyName("TemplateType")]
        public string TemplateType { get; set; } = string.Empty;

        [JsonPropertyName("TemplateNumber")]
        public string TemplateNumber { get; set; } = string.Empty;
    }

    public class MonthWiseReportModel
    {
        public string Month { get; set; } = string.Empty;
        public Nullable<int> MonthNumber { get; set; } = 0;
        public Nullable<int> Year { get; set; } = 0;

    }


    public class GraphChartModel
    {
        public AMBC_Active_Emp_view AMBC_Active_Emp_view { get; set; }
        public SelectedReportMonthModel SelectedReportMonth { get; set; }
        public string EmployeeImage { get; set; }

    }

    public class SelectedReportMonthModel
    {
        public string MonthName { get; set; }
        public string MonthEndDate { get; set; }
        public string year { get; set; }
        public string Suffix { get; set; }
        public string ShortFormat { get; set; }
        public string ReportStartMonth { get; set; }

    }

    public class RMA_UploadedStatusReportViewModel
    {
        public bool IsExcelReport { get; set; } = false;
        public StatusReportChartModel AjaxModel { get; set; } = new StatusReportChartModel();
        public List<RMA_UploadedStatusReportModel> ViewModel { get; set; } = new List<RMA_UploadedStatusReportModel>();

    }

    public class RMA_UploadedStatusReportModel
    {
        public AMBC_Active_Emp_view AMBC_Active_Emp_view { get; set; } = new AMBC_Active_Emp_view();
        public string EmployeeImage { get; set; }
        public List<monthlyreports_Template1> Template1Reports { get; set; } = new List<monthlyreports_Template1>();
        public List<monthlyreports_Template2> Template2Reports { get; set; } = new List<monthlyreports_Template2>();
        public List<monthlyreports_Template3> Template3Reports { get; set; } = new List<monthlyreports_Template3>();
    }

}

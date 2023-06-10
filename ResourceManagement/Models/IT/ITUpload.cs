using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ResourceManagement.Models.IT
{
    public class ITUpload
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
        public string ToolName { get; set; }
        public string Uploadedby { get; set; }
        public bool IsAuditReport { get; set; }
        public string ProjectCategory { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        //To change label title value  
        [DisplayName("Upload Report File")]
        public string ExcelPath { get; set; }

        public HttpPostedFileBase ExcelFile { get; set; }

        public List<SelectListItem> EmployeeList { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> MonthList { get; set; } = new List<SelectListItem>();

        public string SuccessMessage { get; set; }

        public string FailureMessage { get; set; }
    }
}
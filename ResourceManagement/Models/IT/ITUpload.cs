using System.Collections.Generic;
using System.ComponentModel;
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

        [DisplayName("Employee Email")]
        public string Emailaddress { get; set; }

        //To change label title value  
        [DisplayName("Month")]
        public string UploadedMonth { get; set; }
        public string AssetID { get; set; }
        public string ReportType { get; set; }

        //To change label title value  
        [DisplayName("Upload Report File")]
        public string ExcelPath { get; set; }
        public HttpPostedFileBase ExcelFile { get; set; }
        public List<SelectListItem> EmployeeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MonthList { get; set; } = new List<SelectListItem>();
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string Uploadedbyempid { get; set; }
        public string Uploadedempname { get; set; }
        public string Remarks { get; set; }
        public string UploadedFileName { get; set; }
        public string Activities { get; set; }

        public JsonResponseModel jsonResponse { get; set; } = new JsonResponseModel();
    }

    public class ITModel
    {
        public RMA_EmployeeModel RMA_EmployeeModel { get; set; } = new RMA_EmployeeModel();
        public List<AMBC_Active_Emp_view> ITAdminUsers { get; set; } = new List<AMBC_Active_Emp_view>();
        public List<SelectListItem> MonthsList { get; set; }
        public List<AmbcNewITAssetMgmt> AmbcNewITAssetMgmt { get; set; }
        public List<AmbcNewITAssetMgmt> EmpSpecificAssets { get; set; } = new List<AmbcNewITAssetMgmt>();
    }

    public class ITDownloadReportModel
    {
        public string FileName { get; set; }
        public string ReportMonth { get; set; }
    }

    public class ITScheduleMaintenanceModel
    {
        public string EmpName { get; set; }

        public string EmpID { get; set; }

        public string EmpEmail { get; set; }

        public string UploadedByName { get; set; }

        public string UploadedByID { get; set; }

        public string UploadedByEmail { get; set; }

        public string TO { get; set; } = string.Empty;

        public string CC { get; set; } = string.Empty;

        public string BCC { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string EmailBody { get; set; } = string.Empty;

    }

    public class ITMaintenanceActivityModel
    {
        public string ActivityName { get; set; }

        public string Status { get; set; }

        public string Remarks { get; set; }

        public string Additionalcomments { get; set; }

    }

    public class ITMaintenanceEmailAck
    {
        public List<ITMaintenanceActivityModel> ITActivities { get; set; }
        public AMBCITMonthlyMaintenance AMBCITMonthlyMaintenance { get; set; }
        public string itadminIds { get; set; }
        public List<AMBC_Active_Emp_view> SelectedEmp { get; set; }

    }

    public class ITMaintenanceAjaxModel
    {
        public string itadminIds { get; set; }
        public AMBCITMonthlyMaintenance AMBCITMonthlyMaintenance { get; set; }

    }
}
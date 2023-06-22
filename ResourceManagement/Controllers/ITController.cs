using ResourceManagement.Models;
using ResourceManagement.Models.IT;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.UI;

namespace ResourceManagement.Controllers
{
    using static Helpers.DateHelper;
    using static Helpers.MediaHelper;
    using static Helpers.MVCExtension;
    using static Helpers.EmailHelper;

    public class ITController : Controller
    {
        // GET: Employee    
        public ActionResult Index()
        {
            return null;
        }

        ////https://www.jqueryscript.net/text/Rich-Text-Editor-jQuery-RichText.html
        public ActionResult ScheduleMaintenance()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            var ITModel = new ITModel();
            ITModel.RMA_EmployeeModel = employeeModel;

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var itAdminEmplist = db.AMBC_Active_Emp_view.Where(a => a.Access_Role.Equals("itadmin") && a.Project_Status == "Active").ToList();
                if (itAdminEmplist != null && itAdminEmplist.Count() > 0)
                {
                    ITModel.ITAdminUsers = itAdminEmplist;
                }
            }

            return View(ITModel);
        }

        public JsonResult GenerateEmailBody(ITScheduleMaintenanceModel maintenanceModel)
        {
            var emailBody = RenderPartialToString(this, "schedulemaintenanceemail", maintenanceModel, ViewData, TempData);
            return Json(emailBody, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ScheduleMaintenanceEmailTrigger(ITScheduleMaintenanceModel maintenanceModel)
        {
            //var emailBody = RenderPartialToString(this, "schedulemaintenanceemail", maintenanceModel, ViewData, TempData);
            maintenanceModel.EmailBody = maintenanceModel.EmailBody.Trim();
            var emailBody = RenderPartialToString(this, "schedulemaintenanceemail", maintenanceModel, ViewData, TempData);

            if (maintenanceModel.CC != null && !maintenanceModel.CC.Contains(maintenanceModel.UploadedByEmail))
            {
                maintenanceModel.CC += "," + maintenanceModel.UploadedByEmail;
            }
            else
            {
                maintenanceModel.CC = maintenanceModel.UploadedByEmail;
            }

            Models.Email.SendEmail emailModel = new Models.Email.SendEmail()
            {
                To = maintenanceModel.TO,
                BCC = maintenanceModel.BCC,
                Subject = maintenanceModel.Subject,
                CC = maintenanceModel.CC,
                EmailBody = emailBody
            };

            var EmailResponse = SendStatusReportRemainderEmail(emailModel);

            return Json(EmailResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssetAddUpdate()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }

        public ActionResult MMReportgenerate()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            var ITModel = new ITModel();
            ITModel.RMA_EmployeeModel = employeeModel;
            ITModel.MonthsList = MonthList();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "").ToList();
                if (Assets != null && Assets.Count() > 0)
                {
                    ITModel.AmbcNewITAssetMgmt = Assets;
                }
            }

            return View(ITModel);
        }

        public ActionResult AssetAssign()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }

        public ActionResult UploadReport()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            var ITModel = new ITModel();
            ITModel.RMA_EmployeeModel = employeeModel;
            ITModel.MonthsList = MonthList();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "").ToList();
                if (Assets != null && Assets.Count() > 0)
                {
                    ITModel.AmbcNewITAssetMgmt = Assets;
                }
            }

            return View(ITModel);
        }

        public JsonResult ITReportsUploadAjax(ITUpload fileData)
        {
            var model = new ITUpload();
            try
            {
                HttpPostedFileBase file = fileData.ExcelFile;
                var systemGeneratedFileName = string.Empty;

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    var uploadedMonth = fileData.UploadedMonth;
                    var year = uploadedMonth.Split('-')[1];
                    var month = uploadedMonth.Split('-')[0];
                    systemGeneratedFileName = fileData.ReportType + "-" + fileData.EmployeeID + "-" + file.FileName;

                    using (var context = new TimeSheetEntities())
                    {
                        var contextModel = new AMBCITMonthlyMaintenance()
                        {
                            AssetID = fileData.AssetID,
                            EmployeeID = fileData.EmployeeID,
                            MaintenanceMonth = fileData.UploadedMonth,
                            //TODOD REMARKS
                            Remarks = systemGeneratedFileName,
                            EmployeeName = fileData.EmployeeName
                        };

                        context.AMBCITMonthlyMaintenances.Add(contextModel);
                        context.SaveChanges();
                    }

                    var fileLocation = ConfigurationManager.AppSettings["UploadFilePath"] + year + "\\" + month;

                    if (!Directory.Exists(fileLocation))
                    {
                        Directory.CreateDirectory(fileLocation);
                    }
                    //string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(fileLocation, systemGeneratedFileName);
                    file.SaveAs(_path);

                    model.jsonResponse.StatusCode = 200;
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                model.jsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');
                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            model.jsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    model.jsonResponse.Message = ex.Message;
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ViewReport()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            var ITModel = new ITModel();
            ITModel.RMA_EmployeeModel = employeeModel;
            ITModel.MonthsList = MonthList();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "").ToList();
                if (Assets != null && Assets.Count() > 0)
                {
                    ITModel.AmbcNewITAssetMgmt = Assets;
                }
            }

            return View(ITModel);
        }

        public JsonResult ViewITAjaxReports(ITUpload itReportModel)
        {
            var reports = new System.Collections.Generic.List<AMBCITMonthlyMaintenance>();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                reports = db.AMBCITMonthlyMaintenances.Where(x => x.EmployeeID == itReportModel.EmployeeID && x.Remarks != null && x.Remarks != "").ToList();
                var jsonReponse = JsonConvert.SerializeObject(reports);
                return Json(jsonReponse, JsonRequestBehavior.AllowGet);
            }


        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult DownloadReport(string GridHtml)
        {
            ITDownloadReportModel itReportModel = JsonConvert.DeserializeObject<ITDownloadReportModel>(GridHtml);

            var reportYear = itReportModel.ReportMonth.Split('-')[1];
            var reportmonth = itReportModel.ReportMonth.Split('-')[0];

            var filePath = ConfigurationManager.AppSettings["UploadFilePath"] + reportYear + "\\" + reportmonth + "\\" + itReportModel.FileName;
            var fileName = itReportModel.FileName;
            var mimeType = GetMimeType(fileName);

            if (System.IO.File.Exists(filePath))
            {
                return File(new FileStream(filePath, FileMode.Open), mimeType, fileName);
            }
            else
            {
                Response.Write("<script>alert('File not exists!')</script>");
                return null;
            }
        }

        public JsonResult AssetsAddUpdateAjax(AssetModelData assetInfo)
        {
            var model = assetInfo;
            try
            {
                using (var context = new TimeSheetEntities())
                {
                    var contextModel = new AmbcNewITAssetMgmt()
                    {
                        AssetHostName = assetInfo.AssetHostName,
                        AssetMacNo = assetInfo.AssetMacNo,
                        AssetModel = assetInfo.AssetModel,
                        AssetManufacturer = assetInfo.AssetManufacturer,
                        AssetSerialNo = assetInfo.AssetSerialNo,
                        AssetType = assetInfo.AssetType,
                        ChargerCapicity = assetInfo.ChargerCapicity,
                        ChargerSerialNo = assetInfo.ChargerSerialNo,
                        OperatingSystemDetail = assetInfo.OperatingSystemDetail,
                        RAM_Size = assetInfo.RAM_Size,
                        ServiceTag = assetInfo.ServiceTag,
                        USBPortStatus = assetInfo.USBPortStatus,
                        WarrentyEndDate = assetInfo.WarrentyEndDate,
                        WarrentyStartDate = assetInfo.WarrentyStartDate,
                        WarrentyStatus = assetInfo.WarrentyStatus,
                        Lastupdated = System.DateTime.Now
                    };

                    context.AmbcNewITAssetMgmts.Add(contextModel);
                    context.SaveChanges();
                    model.jsonResponse.StatusCode = 200;
                }
                var finalResponse = JsonConvert.SerializeObject(model);
                return Json(finalResponse, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                model.jsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');
                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            model.jsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    model.jsonResponse.Message = ex.Message;
                }
            }
            var finalResponseonError = JsonConvert.SerializeObject(model);
            return Json(finalResponseonError, JsonRequestBehavior.AllowGet);
        }

    }
}
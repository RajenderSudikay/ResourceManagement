using ResourceManagement.Models;
using ResourceManagement.Models.IT;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.UI;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;

using Microsoft.Ajax.Utilities;
using static ResourceManagement.Helpers.EmployeeHelper;
using static ResourceManagement.Helpers.AssetsHelper;

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

            if (!string.IsNullOrWhiteSpace(maintenanceModel.CC) && !maintenanceModel.CC.Contains(maintenanceModel.UploadedByEmail))
            {
                maintenanceModel.CC += "," + maintenanceModel.UploadedByEmail;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(maintenanceModel.CC))
                {
                    maintenanceModel.CC += maintenanceModel.UploadedByEmail;
                }
            }

            Models.Email.SendEmail emailModel = new Models.Email.SendEmail()
            {
                To = maintenanceModel.TO,
                BCC = maintenanceModel.BCC,
                Subject = maintenanceModel.Subject,
                CC = maintenanceModel.CC,
                EmailBody = emailBody,
                inputObject = maintenanceModel,
                SpecificUserName = ConfigurationManager.AppSettings["ITSMTPUserName"],
                SpecificPassword = ConfigurationManager.AppSettings["ITSMTPPassword"]

            };

            var EmailResponse = SendEmailFromHRMS(emailModel);

            return Json(EmailResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssetAddUpdate()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }

        public ActionResult VendorAddUpdate()
        {
            var model = FilDefaultITModel();
            return View(model);
        }

        public ActionResult ViewVendors()
        {
            var model = FilDefaultITModel();
            return View(model);
        }

        public ActionResult PurchaseRequest()
        {
            var model = FilDefaultITModel();
            return View(model);
        }

        public ActionResult MMReportgenerate()
        {
            ITModel ITModel = FilDefaultITModel();
            return View(ITModel);
        }

        private ITModel FilDefaultITModel()
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

                var itAdminEmplist = db.AMBC_Active_Emp_view.Where(a => a.Access_Role.Equals("itadmin") && a.Project_Status == "Active").ToList();
                if (itAdminEmplist != null && itAdminEmplist.Count() > 0)
                {
                    ITModel.ITAdminUsers = itAdminEmplist;
                }

                var locations = db.Locations.ToList();
                if (locations != null && locations.Count() > 0)
                {
                    ITModel.Locations = locations;
                }

                var assetLocations = db.AssetTypes.ToList();
                if (assetLocations != null && assetLocations.Count() > 0)
                {
                    ITModel.AssetTypes = assetLocations;
                }
            }

            return ITModel;
        }

        public JsonResult MMReportgenerateAjax(AMBCITMonthlyMaintenance monthlyMaintenanceModel, string itadminIds)
        {
            var inputModel = new ITMaintenanceEmailAck();
            var emailModel = new Models.Email.SendEmail();
            try
            {
                monthlyMaintenanceModel.CreatedDate = System.DateTime.Now;
                using (TimeSheetEntities context = new TimeSheetEntities())
                {

                    context.AMBCITMonthlyMaintenances.Add(monthlyMaintenanceModel);
                    context.SaveChanges();
                    inputModel.SelectedEmp = context.AMBC_Active_Emp_view.Where(x => x.Project_Status == "Active" && x.Employee_ID == monthlyMaintenanceModel.EmployeeID).ToList();

                    inputModel.AMBCITMonthlyMaintenance = monthlyMaintenanceModel;
                    inputModel.ITActivities = JsonConvert.DeserializeObject<List<ITMaintenanceActivityModel>>(monthlyMaintenanceModel.PerformedActivityInfo.ToString());
                    inputModel.itadminIds = itadminIds;
                }

                return ITMaintenaceAckEmailTrigger(inputModel);
            }
            catch (System.Exception ex)
            {
                emailModel.JsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');
                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            emailModel.JsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    emailModel.JsonResponse.Message = ex.Message;
                }

                emailModel.inputObject = monthlyMaintenanceModel;
            }
            return Json(JsonConvert.SerializeObject(emailModel), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ITMaintenaceAckEmailTrigger(ITMaintenanceEmailAck ITMaintenanceEmailAck)
        {
            var emailBody = RenderPartialToString(this, "MMAckEmail", ITMaintenanceEmailAck, ViewData, TempData);
            if (!string.IsNullOrWhiteSpace(ITMaintenanceEmailAck.itadminIds) && !ITMaintenanceEmailAck.itadminIds.Contains(ITMaintenanceEmailAck.AMBCITMonthlyMaintenance.UploadedByEmail))
            {
                ITMaintenanceEmailAck.itadminIds += "," + ITMaintenanceEmailAck.AMBCITMonthlyMaintenance.UploadedByEmail;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ITMaintenanceEmailAck.itadminIds))
                {
                    ITMaintenanceEmailAck.itadminIds += ITMaintenanceEmailAck.AMBCITMonthlyMaintenance.UploadedByEmail;
                }
            }


            Models.Email.SendEmail emailModel = new Models.Email.SendEmail()
            {
                To = ITMaintenanceEmailAck.AMBCITMonthlyMaintenance.Emailaddress,
                Subject = "MM Report - " + ITMaintenanceEmailAck.AMBCITMonthlyMaintenance.MaintenanceMonth.Replace("-", ", ") + " - Asset#: " + ITMaintenanceEmailAck.AMBCITMonthlyMaintenance.AssetID,
                CC = ITMaintenanceEmailAck.itadminIds,
                EmailBody = emailBody,
                inputObject = ITMaintenanceEmailAck,
                SpecificUserName = ConfigurationManager.AppSettings["ITSMTPUserName"],
                SpecificPassword = ConfigurationManager.AppSettings["ITSMTPPassword"]
            };

            var EmailResponse = SendEmailFromHRMS(emailModel);

            return Json(EmailResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssetAssign()
        {
            var defaultModel = FilDefaultITModel();
            return View(defaultModel);
        }

        public ActionResult ViewAssets()
        {
            var defaultModel = FilDefaultITModel();
            return View(defaultModel);
        }

        public JsonResult GetAssets(GetAssetModel getAssetModel)
        {
            var assetsInfo = string.Empty;
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            var ITModel = new ITModel();
            ITModel.RMA_EmployeeModel = employeeModel;

            var AssetsTotal = new List<AssetDataPoint>();
            var AssetsAssigned = new List<AssetDataPoint>();
            var AssetsNotAssigned = new List<AssetDataPoint>();
            var AssetsSoldOut = new List<AssetDataPoint>();

            try
            {
                var Assets = new List<AmbcNewITAssetMgmt>();
                using (TimeSheetEntities db = new TimeSheetEntities())
                {
                    if (getAssetModel.FilterBy == "Asset")
                    {
                        if (getAssetModel.AssetType == "All")
                        {
                            Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "").ToList();
                        }
                        else
                        {
                            Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "" && x.AssetType == getAssetModel.AssetType).ToList();
                        }

                        if (getAssetModel.Category != "All")
                        {
                            if (getAssetModel.Category == "Assigned")
                            {
                                Assets = Assets.Where(x => x.AssetAllocationStatus != "NA").ToList();
                            }
                            else
                            {
                                Assets = Assets.Where(x => x.AssetAllocationStatus == "NA").ToList();
                            }
                        }
                    }

                    if (getAssetModel.FilterBy == "Employee")
                    {
                        var assignedAssetsToEmp = db.AssetAllocationMgmts.Where(x => x.AssetSerialNo != "" && x.AssetAllocatedToEmpID == getAssetModel.EmployeeID).ToList();

                        foreach(var assignedAssetToEmp in assignedAssetsToEmp)
                        {
                            var asset = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "" && x.AssetType == assignedAssetToEmp.AssetType && x.AssetSerialNo == assignedAssetToEmp.AssetSerialNo).ToList();

                            if(asset != null)
                            {
                                Assets.AddRange(asset);
                            }
                        }
                    }


                    if (Assets != null && Assets.Count() > 0)
                    {
                        var uniqueAssets = Assets.Where(x => x.AssetType != "").DistinctBy(x => x.AssetType);

                        foreach (var uniqueAsset in uniqueAssets)
                        {
                            var totalAssets = Assets.Where(x => x.AssetType == uniqueAsset.AssetType);

                            if (totalAssets != null && totalAssets.Count() > 0)
                            {
                                AssetsTotal.Add(new AssetDataPoint()
                                {
                                    Priority = "Total",
                                    label = uniqueAsset.AssetType,
                                    y = totalAssets != null && totalAssets.Count() > 0 ? totalAssets.Count() : 0
                                });

                                var assignedAssets = totalAssets.Where(x => x.AssetAllocationStatus != "NA");

                                AssetsAssigned.Add(new AssetDataPoint()
                                {
                                    Priority = "Assigned",
                                    label = uniqueAsset.AssetType,
                                    y = assignedAssets != null && assignedAssets.Count() > 0 ? assignedAssets.Count() : 0
                                });

                                var notAssignedAssets = totalAssets.Where(x => x.AssetAllocationStatus == "NA");

                                AssetsNotAssigned.Add(new AssetDataPoint()
                                {
                                    Priority = "Not Assigned",
                                    label = uniqueAsset.AssetType,
                                    y = notAssignedAssets != null && notAssignedAssets.Count() > 0 ? notAssignedAssets.Count() : 0
                                });

                                var soldOutAssets = totalAssets.Where(x => x.AssetAllocationStatus == "NA");

                                AssetsSoldOut.Add(new AssetDataPoint()
                                {
                                    Priority = "Sold Out",
                                    label = uniqueAsset.AssetType,
                                    y = notAssignedAssets != null && notAssignedAssets.Count() > 0 ? notAssignedAssets.Count() : 0
                                });

                            }
                        }

                        ITModel.AmbcNewITAssetMgmt = Assets;

                        ITModel.AssetsTotalDataPoints = JsonConvert.SerializeObject(AssetsTotal);
                        ITModel.AssetsAssignedDataPoints = JsonConvert.SerializeObject(AssetsAssigned);
                        ITModel.AssetsNotAssignedDataPoints = JsonConvert.SerializeObject(AssetsNotAssigned);
                        ITModel.AssetsSoldOutDataPoints = JsonConvert.SerializeObject(AssetsSoldOut);

                        assetsInfo = RenderPartialToString(this, "ViewAssetsPartial", ITModel, ViewData, TempData);
                    }
                }

                return Json(assetsInfo);
            }
            catch (Exception ex)
            {
                return Json(null);
            }
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


        public ActionResult MMreportAckFromEmail(int uniqueID)
        {

            var reportAckModel = new ITMaintenanceEmailAck();
            using (var context = new TimeSheetEntities())
            {
                var requireMMReport = context.AMBCITMonthlyMaintenances.Where(report => report.UniqNo == uniqueID).FirstOrDefault();

                if (requireMMReport != null)
                {
                    reportAckModel.ITActivities = JsonConvert.DeserializeObject<List<ITMaintenanceActivityModel>>(requireMMReport.PerformedActivityInfo.ToString());
                    reportAckModel.AMBCITMonthlyMaintenance = requireMMReport;
                }

                reportAckModel.SelectedEmp = context.AMBC_Active_Emp_view.Where(x => x.Project_Status == "Active" && x.Employee_ID == requireMMReport.EmployeeID).ToList();

                requireMMReport.EmployeeAck = true;
                context.SaveChanges();
            }

            return View(reportAckModel);
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
                        var contextModel = new AMBCITReportUpload()
                        {
                            EmpAssetID = fileData.AssetID,
                            EmployeeID = fileData.EmployeeID,
                            EmployeeMailAddress = fileData.Emailaddress,
                            EmployeeName = fileData.EmployeeName,
                            ReportMonth = fileData.UploadedMonth,
                            UploadedByID = fileData.Uploadedbyempid,
                            UploadedByName = fileData.Uploadedempname,
                            UploadedByEmail = fileData.UploadedByEmpEmail,
                            UploadReportPathDetail = systemGeneratedFileName,
                            ReportType = fileData.ReportType,
                            UploadRemarks = fileData.Remarks,
                            CreatedDate = System.DateTime.Now,
                            ambcrptuniqkey = fileData.ManualGenaratedUniqueNum
                        };

                        context.AMBCITReportUploads.Add(contextModel);
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
            var reportViewModel = new System.Collections.Generic.List<ITDownloadReportViewModel>();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                if (itReportModel.Action == "Delete")
                {
                    if (itReportModel.ReportType == "MM Report")
                    {
                        var deleteRecord = db.AMBCITMonthlyMaintenances.Where(x => x.EmployeeID == itReportModel.EmployeeID && x.MaintenanceMonth == itReportModel.UploadedMonth && x.AssetID == itReportModel.AssetID && x.UniqNo == itReportModel.UniqueNo).FirstOrDefault();

                        if (deleteRecord != null)
                        {
                            db.AMBCITMonthlyMaintenances.Remove(deleteRecord);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var deleteRecord = db.AMBCITReportUploads.Where(x => x.EmployeeName == itReportModel.EmployeeName && x.ReportMonth == itReportModel.UploadedMonth && x.EmpAssetID == itReportModel.AssetID).FirstOrDefault();

                        if (deleteRecord != null)
                        {
                            db.AMBCITReportUploads.Remove(deleteRecord);
                            db.SaveChanges();
                        }
                    }

                }

                var MMReports = db.AMBCITMonthlyMaintenances.Where(x => x.EmployeeID == itReportModel.EmployeeID && x.MaintenanceMonth == itReportModel.UploadedMonth && x.AssetID == itReportModel.AssetID).ToList();

                if (MMReports != null && MMReports.Count > 0)
                {
                    foreach (var mmReport in MMReports)
                    {
                        reportViewModel.Add(new ITDownloadReportViewModel()
                        {
                            AssetID = mmReport.AssetID,
                            EmpID = mmReport.EmployeeID,
                            EmpName = mmReport.EmployeeName,
                            FileName = "MMR-" + mmReport.EmployeeName + "-" + mmReport.MaintenanceMonth,
                            ReportType = "MM Report",
                            ReportMonth = mmReport.MaintenanceMonth,
                            UniqueNumber = mmReport.UniqNo.ToString(),
                            Ack = mmReport.EmployeeAck == true ? "Yes" : "No",
                            ReportDate = ConvertDateToServerSystemFormat(mmReport.CreatedDate)
                        });
                    }
                }

                var ITOtherReports = db.AMBCITReportUploads.Where(x => x.EmployeeName == itReportModel.EmployeeName && x.ReportMonth == itReportModel.UploadedMonth && x.EmpAssetID == itReportModel.AssetID).ToList();

                if (ITOtherReports != null && ITOtherReports.Count > 0)
                {
                    foreach (var ITOtherReport in ITOtherReports)
                    {
                        reportViewModel.Add(new ITDownloadReportViewModel()
                        {
                            AssetID = ITOtherReport.EmpAssetID,
                            EmpName = ITOtherReport.EmployeeName,
                            FileName = ITOtherReport.UploadReportPathDetail,
                            ReportType = ITOtherReport.ReportType,
                            ReportMonth = ITOtherReport.ReportMonth,
                            Ack = "NA",
                            UniqueNumber = "0",
                            ReportDate = ConvertDateToServerSystemFormat(ITOtherReport.CreatedDate)
                        });
                    }
                }
                var jsonReponse = JsonConvert.SerializeObject(reportViewModel);
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

            if (itReportModel.ReportType == "MM Report")
            {
                var reportModel = new ITMaintenanceEmailAck();
                using (var context = new TimeSheetEntities())
                {
                    var requireMMReport = context.AMBCITMonthlyMaintenances.Where(report => report.EmployeeID == itReportModel.EmpID && report.UniqNo == itReportModel.UniqueNumber && report.AssetID == itReportModel.AssetID).FirstOrDefault();

                    if (requireMMReport != null)
                    {
                        reportModel.ITActivities = JsonConvert.DeserializeObject<List<ITMaintenanceActivityModel>>(requireMMReport.PerformedActivityInfo.ToString());
                        reportModel.AMBCITMonthlyMaintenance = requireMMReport;
                    }

                    reportModel.SelectedEmp = context.AMBC_Active_Emp_view.Where(x => x.Project_Status == "Active" && x.Employee_ID == itReportModel.EmpID).ToList();
                    var reportContent = RenderPartialToString(this, "MMreport", reportModel, ViewData, TempData);


                    //https://www.c-sharpcorner.com/article/convert-html-string-to-pdf-via-itextsharp-library-and-downlo/
                    StringReader reportData = new StringReader(reportContent.ToString());

                    Document pdfDoc = new Document(PageSize.A4, 15f, 15f, 30f, 0f);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfDoc.Open();

                        htmlparser.Parse(reportData);
                        pdfDoc.Close();

                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();

                        // Clears all content output from the buffer stream
                        Response.Clear();
                        // Gets or sets the HTTP MIME type of the output stream.
                        Response.ContentType = "application/pdf";
                        // Adds an HTTP header to the output stream
                        var fileName = itReportModel.FileName + ".pdf";
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + "");

                        //Gets or sets a value indicating whether to buffer output and send it after
                        // the complete response is finished processing.
                        Response.Buffer = true;
                        // Sets the Cache-Control header to one of the values of System.Web.HttpCacheability.
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        // Writes a string of binary characters to the HTTP output stream. it write the generated bytes .
                        Response.BinaryWrite(bytes);

                        // Sends all currently buffered output to the client, stops execution of the
                        // page, and raises the System.Web.HttpApplication.EndRequest event.
                        Response.End();
                        // Closes the socket connection to a client. it is a necessary step as you must close the response after doing work.its best approach.
                        Response.Close();
                    }

                }
            }
            else
            {
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

            return null;

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
                        AssetAllocationStatus = assetInfo.AllocationStatus,
                        AssetHostName = assetInfo.AssetHostName,
                        AssetMacNo = assetInfo.AssetMacNo,
                        AssetModel = assetInfo.AssetModel,
                        AssetManufacturer = assetInfo.AssetManufacturer,
                        AssetSerialNo = assetInfo.AssetSerialNo,
                        AssetType = assetInfo.AssetType,
                        ChargerCapicity = assetInfo.ChargerDetails,
                        ChargerSerialNo = assetInfo.ChargerSerialNo,
                        OperatingSystemDetail = assetInfo.OperatingSystemDetail,
                        RAM_Size = assetInfo.RAM_Size,
                        ServiceTag = assetInfo.ServiceTag,
                        USBPortStatus = assetInfo.USBPortStatus,
                        WarrentyEndDate = assetInfo.WarrentyEndDate,
                        WarrentyStartDate = assetInfo.WarrentyStartDate,
                        WarrentyStatus = assetInfo.WarrentyStatus,
                        Lastupdated = System.DateTime.Now,
                        AccessControl = assetInfo.AccessControl,

                        //THIS FILED TO BE UPDATED IN TABLE
                        AssetUploadedByEmpEmailAddress = assetInfo.CreatedByEmail,
                        AssetUploadedByEmpid = assetInfo.CreatedByID,
                        AssetUploadedByEmpName = assetInfo.CreatedByName,

                        AssetDescription = assetInfo.Description,
                        AssetPurchaseLocation = assetInfo.PurchaseLocation,
                        AssetPurchaseVendor = assetInfo.PurchaseVendor,
                        AssetRemarks = assetInfo.Remarks,
                        DisplaySize = assetInfo.DisplaySize,
                        HeadsetDetail = assetInfo.Headsets,
                        MouseDetail = assetInfo.MouseDetails,
                        ROM_Size = assetInfo.ROM_Size,
                        SoldOutDate = assetInfo.SoldOutDate,
                        SoldoutPrice = assetInfo.SoldoutPrice,
                        SoldOutStatus = assetInfo.SoldOutStatus,
                        SoldOutVendor = assetInfo.SoldOutVendor,
                        AssetEntryCreatedDate = DateTime.Now,
                        AssetEntryModifiedDate = DateTime.Now
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

        public JsonResult GetAssetsByEmpID(GetAssetModelByEmp GetAssetModelByEmp)
        {
            var model = new ITModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var Assets = db.AssetAllocationMgmts.Where(x => x.AssetAllocatedToEmpID == GetAssetModelByEmp.EmpID).ToList();
                if (Assets != null && Assets.Count() > 0)
                {
                    model.EmpSpecificAssets = Assets;
                }
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //THIS IS FOR ALLOWCATING ASSETS TO EMPLOYEES
        public JsonResult GetAssetsByAssetType(GetAssetModelByAsset GetAssetModel)
        {
            var model = new ITModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "" && x.AssetType == GetAssetModel.AssetType && x.AssetAllocationStatus == "NA").ToList();
                if (Assets != null && Assets.Count() > 0)
                {
                    model.AssetsByAssetType = Assets;
                }
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AssignAsset(AssignAssetModel AssetAssignModel)
        {
            var model = new ITModel();
            using (TimeSheetEntities context = new TimeSheetEntities())
            {
                var assignedEmp = GetEmpByEmpID(AssetAssignModel.EmployeeID);
                if (assignedEmp == null)
                {
                    return null;
                }

                var selectedAsset = GetAssetByAssetID(AssetAssignModel.AssetID);

                if (selectedAsset == null)
                {
                    return null;
                }


                var inputModel = new AssetAllocationMgmt();
                if (AssetAssignModel.AssetTxn == "Add")
                {
                    inputModel.AssetAllocationCreatedDate = DateTime.Now;
                }
                else
                {
                    inputModel.AssetAllocationModifiedDate = DateTime.Now;
                }

                inputModel.AssetAllocatedToEmpName = assignedEmp.Employee_Name;
                inputModel.AssetAllocatedToEmailAddress = assignedEmp.AMBC_Mail_Address;
                inputModel.AssetAllocatedToEmpID = assignedEmp.Employee_ID;
                inputModel.AssetAllocatedToMobileNo = assignedEmp.Contact_Number;
                inputModel.AssetAllocatedByEmail = AssetAssignModel.UploadedByEmpEmail;
                inputModel.AssetAllocatedByEmpID = AssetAssignModel.UploadedByEmpID;
                inputModel.AssetAllocatedByEmpName = AssetAssignModel.UploadedByEmpName;
                inputModel.AssetAllocatedByMobile = AssetAssignModel.UploadedByMobile;


                inputModel.AssetHostName = selectedAsset.AssetHostName;
                inputModel.AssetMacNo = selectedAsset.AssetMacNo;
                inputModel.AssetSerialNo = selectedAsset.AssetSerialNo;
                inputModel.AssetType = selectedAsset.AssetType;
                inputModel.AssetTxn = AssetAssignModel.AssetTxn;
                inputModel.AssetAllocationRemarks = AssetAssignModel.Remarks;

                context.AssetAllocationMgmts.Add(inputModel);
                context.SaveChanges();

                var assetsInfo = context.AmbcNewITAssetMgmts.Where(b => b.AssetSerialNo == AssetAssignModel.AssetID).OrderBy(x => x.AssetEntryCreatedDate).FirstOrDefault();
                if (assetsInfo != null)
                {
                    assetsInfo.AssetAllocationStatus = "Assigned";
                    assetsInfo.AssetEntryModifiedDate = DateTime.Now;
                    context.SaveChanges();
                }
            }

            var emailBody = RenderPartialToString(this, "AssignAssetEmailPartial", AssetAssignModel, ViewData, TempData);

            Models.Email.SendEmail emailModel = new Models.Email.SendEmail()
            {
                //To = AssetAssignModel.EmployeeEmail,
                To = "srajender@ambconline.com",
                Subject = "Asset assigned - " + AssetAssignModel.AssetType + " (" + AssetAssignModel.AssetID + ")",
                //CC = AssetAssignModel.itadminIds,
                EmailBody = emailBody,
                SpecificUserName = ConfigurationManager.AppSettings["ITSMTPUserName"],
                SpecificPassword = ConfigurationManager.AppSettings["ITSMTPPassword"]
            };

            var EmailResponse = SendEmailFromHRMS(emailModel);
            return Json(EmailResponse, JsonRequestBehavior.AllowGet);
        }

    }
}
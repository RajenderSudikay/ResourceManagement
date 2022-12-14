using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace ResourceManagement.Controllers
{
    using Models;
    using SelectPdf;
    using System;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using static ResourceManagement.Helpers.DateHelper;
    using static ResourceManagement.Models.TimesheetReportModel;

    public class HomeController : Controller
    {

        public ActionResult Convert()
        {
            // read parameters from the webpage
            string url = "https://localhost:44375/dashboard";

            string pdf_page_size = "10";
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), pdf_page_size, true);

            string pdf_orientation = "Landscape";
            PdfPageOrientation pdfOrientation = (PdfPageOrientation)Enum.Parse(
                typeof(PdfPageOrientation), pdf_orientation, true);

            int webPageWidth = 1024;
            try
            {
                webPageWidth = System.Convert.ToInt32(1024);
            }
            catch { }

            int webPageHeight = 0;
            try
            {
                webPageHeight = System.Convert.ToInt32(700);
            }
            catch { }

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertUrl(url);

            // save pdf document
            byte[] pdf = doc.Save();

            // close pdf document
            doc.Close();

            // return resulted pdf document
            FileResult fileResult = new FileContentResult(pdf, "application/pdf");
            fileResult.FileDownloadName = "Document.pdf";
            return fileResult;
        }

        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Charts()
        {
            return View("");
        }

        public ActionResult Logout()
        {
            Session["UserModel"] = null;
            return RedirectToAction("Login");
        }

        public JsonResult SignIn()
        {
            var respone = new SignInOurResponseModel();

            try
            {
                if (Session["UserModel"] != null)
                {
                    var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

                    var todayDate = System.DateTime.Now;
                    string hostName = Dns.GetHostName(); // Retrive the Name of HOST

                    // Get the IP
                    string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();

                    var ambcEmpLoginInfo = new tbld_ambclogininformation()
                    {
                        Employee_Code = employeeModel.AMBC_Active_Emp_view.Employee_ID,
                        Employee_Name = employeeModel.AMBC_Active_Emp_view.Employee_Name,
                        Employee_Designation = employeeModel.AMBC_Active_Emp_view.Designation,
                        Employee_Shift = employeeModel.AMBC_Active_Emp_view.Shift,
                        Login_date = todayDate,
                        Signin_Time = todayDate,
                        Employee_Hostname = hostName,
                        Employee_IP = myIP,
                        Employee_LoginLocation = employeeModel.AMBC_Active_Emp_view.Location
                    };

                    using (var context = new TimeSheetEntities())
                    {
                        context.tbld_ambclogininformation.Add(ambcEmpLoginInfo);
                        context.SaveChanges();
                        respone.jsonResponse.StatusCode = 200;
                        respone.jsonResponse.Message = "TimeSheet added successfully!";
                        respone.signin = true;
                    }
                }
            }
            catch (Exception ex)
            {
                respone.jsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');

                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            respone.jsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    respone.jsonResponse.Message = ex.Message;
                }
            }

            return Json(respone, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SignOut()
        {
            var respone = new SignInOurResponseModel();

            try
            {
                if (Session["UserModel"] != null)
                {
                    var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

                    using (var context = new TimeSheetEntities())
                    {
                        var result = context.tbld_ambclogininformation.SingleOrDefault(b => b.Employee_Code == employeeModel.AMBC_Active_Emp_view.Employee_ID && b.Login_date == DateTime.Today);
                        if (result != null)
                        {
                            result.Signout_Time = System.DateTime.Now;
                            context.SaveChanges();
                            respone.jsonResponse.StatusCode = 200;
                            respone.jsonResponse.Message = "TimeSheet added successfully!";
                        }
                        else
                        {
                            respone.jsonResponse.StatusCode = 400;
                            respone.jsonResponse.Message = "No results found to update!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respone.jsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');

                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            respone.jsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    respone.jsonResponse.Message = ex.Message;
                }
            }

            return Json(respone, JsonRequestBehavior.AllowGet);
        }

        private tbld_ambclogininformation GetSignInDetails(RMA_EmployeeModel employeeModel)
        {
            var todayDate = DateTime.Today;
            using (var context = new TimeSheetEntities())
            {
                var result = context.tbld_ambclogininformation.SingleOrDefault(b => b.Employee_Code == employeeModel.AMBC_Active_Emp_view.Employee_ID && b.Login_date == todayDate);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }



        public ActionResult TimesheetWeeklyChart(List<WeekReportModel> weekreportmodel)
        {
            List<DataPoint> dataPoints = new List<DataPoint>();

            var weekdays = WeekdaysList();
            double hoursSpent = 0;

            if (weekreportmodel != null && weekreportmodel.Any() && weekdays != null && weekdays.Any())
            {
                foreach (var weekday in weekdays)
                {
                    double currentDayHoursSpent = 0;
                    var daySpecificEntries = weekreportmodel.Where(x => x.weekday == weekday).Count() > 0 ? weekreportmodel.Where(x => x.weekday == weekday).ToList() : null;
                    if (daySpecificEntries != null)
                    {
                        foreach (var daySpecificEntrie in daySpecificEntries)
                        {
                            hoursSpent += daySpecificEntrie.hoursspent;
                            currentDayHoursSpent += daySpecificEntrie.hoursspent;
                        }
                    }
                    dataPoints.Add(new DataPoint(weekday, currentDayHoursSpent));
                }
            }
            else
            {
                foreach (var weekday in weekdays)
                {
                    dataPoints.Add(new DataPoint(weekday, 0));
                }
            }


            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            ViewBag.TotalHoursSpent = hoursSpent;


            return PartialView();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(emplogin loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var employeeModel = new RMA_EmployeeModel();
                    using (TimeSheetEntities db = new TimeSheetEntities())
                    {
                        var loginObj = db.emplogins.Where(a => a.att_username.Equals(loginModel.att_username) && a.att_password.Equals(loginModel.att_password) && a.emp_status).FirstOrDefault();
                        if (loginObj != null)
                        {
                            var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(loginModel.att_username)).FirstOrDefault();
                            if (employeeInfo != null)
                            {
                                employeeModel.AMBC_Active_Emp_view = employeeInfo;
                            }

                            var empSignInOutInfo = db.tbld_ambclogininformation.Where(a => a.Employee_Code.Equals(loginObj.employee_id) && a.Login_date == DateTime.Today).FirstOrDefault();
                            if (empSignInOutInfo != null)
                            {
                                employeeModel.signInOutInfo = empSignInOutInfo;
                            }

                            Session["UserModel"] = employeeModel;

                            return RedirectToAction("Dashboard");
                        }
                    }
                }
                return View(loginModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        private string GetDateInRequiredFormat(string actualdate)
        {
            string dateString = actualdate;
            DateTime dateTime = DateTime.Parse(dateString);
            return dateTime.ToString("yyyy-MM-dd");
        }

        public string GetLeaveandHolidayInfofromDb(RMA_EmployeeModel empModel)
        {
            var leaveOrHolidayData = new List<RMA_LeaveOrHolidayInfo>();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var startDate = DateTime.Today.AddMonths(-2);
                var endDate = DateTime.Today;

                var conleaves = db.con_leaveupdate.Where(a => a.employee_id.Equals(empModel.AMBC_Active_Emp_view.Employee_ID) && a.leavedate >= startDate && a.leavedate <= endDate).ToList();
                if (conleaves != null && conleaves.Count > 0)
                {
                    foreach (var conleave in conleaves)
                    {
                        leaveOrHolidayData.Add(new RMA_LeaveOrHolidayInfo()
                        {
                            LeaveOrHolidayDate = GetDateInRequiredFormat(conleave.leavedate.ToString()),
                            Reason = conleave.leave_reason
                        }); ;
                    }
                }

                var ambcHolidays = db.tblambcholidays.Where(b => b.holiday_date >= startDate && b.holiday_date <= endDate && b.region == empModel.AMBC_Active_Emp_view.Location).ToList();
                if (ambcHolidays != null && ambcHolidays.Count > 0)
                {
                    foreach (var ambcLeave in ambcHolidays)
                    {
                        leaveOrHolidayData.Add(new RMA_LeaveOrHolidayInfo()
                        {
                            LeaveOrHolidayDate = GetDateInRequiredFormat(ambcLeave.holiday_date.ToString()),
                            Reason = ambcLeave.holiday_name
                        });
                    }
                }
            }

            if (leaveOrHolidayData != null && leaveOrHolidayData.Count > 0)
            {
                return JsonSerializer.Serialize(leaveOrHolidayData);
            }

            return string.Empty;
        }

        public ActionResult Dashboard()
        {
            if (Session["UserModel"] != null)
            {
                var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

                if (employeeModel != null && employeeModel.AMBC_Active_Emp_view != null && !string.IsNullOrWhiteSpace(employeeModel.AMBC_Active_Emp_view.Employee_ID))
                {
                    employeeModel.signInOutInfo = GetSignInDetails(employeeModel);
                    ViewBag.Message = "Dashboard page.";
                    return View(employeeModel);
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Login");
        }

        public ActionResult TimeSheet()
        {
            ViewBag.Message = "Timesheet page.";
            if (Session["UserModel"] != null)
            {
                var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

                if (employeeModel != null && employeeModel.AMBC_Active_Emp_view != null && !string.IsNullOrWhiteSpace(employeeModel.AMBC_Active_Emp_view.Employee_ID))
                {
                    employeeModel.leaveOrHolidayInfo = GetLeaveandHolidayInfofromDb(employeeModel);
                    return View(employeeModel);
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Login");

            //return View();
        }

        public JsonResult AddTimeSheet(List<ambctaskcapture> timesheetmodel)
        {
            var response = AddOrUpdateTimeSheetstoDB(timesheetmodel);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public JsonResponseModel AddOrUpdateTimeSheetstoDB(List<ambctaskcapture> timesheetmodel)
        {
            var respone = new JsonResponseModel();
            try
            {
                var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
                using (var context = new TimeSheetEntities())
                {
                    if (timesheetmodel != null)
                    {
                        context.ambctaskcaptures.AddRange(timesheetmodel);
                        context.SaveChanges();
                        respone.StatusCode = 200;
                        respone.Message = "TimeSheet added successfully!";
                    }
                    else
                    {
                        respone.StatusCode = 400;
                        respone.Message = "Bad request. Please enter task details!";
                    }
                }
            }
            catch (System.Exception ex)
            {
                respone.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');

                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            respone.Message = actuallError;
                        }
                    }
                }
                else
                {
                    respone.Message = ex.Message;
                }
            }
            return respone;
        }


        public List<ambctaskcapture> GetEmployeeTimeSheets(TimeSheetViewModel timeSheetViewModel)
        {
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                return db.ambctaskcaptures.Where(a => a.employeeid.Equals(timeSheetViewModel.EmpId) && a.taskdate >= System.Convert.ToDateTime(timeSheetViewModel.WeekStartDate) && a.taskdate <= System.Convert.ToDateTime(timeSheetViewModel.WeekEndDate)).ToList();
            }
        }


        public ActionResult TimeSheetEmailReport(TimeSheetViewModel timeSheetViewModel)
        {
            var timeSheetReport = new TimeSheetReport();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var loginObj = db.emplogins.Where(a => a.att_username.Equals("C4046") && a.att_password.Equals("abc@123") && a.emp_status).FirstOrDefault();
                if (loginObj != null)
                {

                    timeSheetReport.reports = db.ambctaskcaptures.Where(a => a.employeeid.Equals(loginObj.employee_id)).ToList();
                    timeSheetReport.empData = loginObj;

                    // && a.taskdate >= System.Convert.ToDateTime(timeSheetViewModel.WeekStartDate) && a.taskdate <= System.Convert.ToDateTime(timeSheetViewModel.WeekEndDate)

                }

                timeSheetReport.MondayHours = "10";
                timeSheetReport.TuesdayHours = "6";
                timeSheetReport.WednesdayHours = "8";
                timeSheetReport.ThursdayHours = "12";
                timeSheetReport.FriidayHours = "4";
            }

            return View(timeSheetReport);
        }


        public ActionResult TimeSheetReports()
        {
            //var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            var employeeModel = new RMA_EmployeeModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var loginObj = db.emplogins.Where(a => a.att_username.Equals("C4046") && a.att_password.Equals("abc@123") && a.emp_status).FirstOrDefault();
                if (loginObj != null)
                {
                    var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals("C4046")).FirstOrDefault();
                    if (employeeInfo != null)
                    {
                        employeeModel.AMBC_Active_Emp_view = employeeInfo;
                    }

                    var empSignInOutInfo = db.tbld_ambclogininformation.Where(a => a.Employee_Code.Equals(loginObj.employee_id) && a.Login_date == DateTime.Today).FirstOrDefault();
                    if (empSignInOutInfo != null)
                    {
                        employeeModel.signInOutInfo = empSignInOutInfo;
                    }
                }
            }

            return View(employeeModel);
        }

        public ActionResult TimeSheetReportsPartial(TimeSheetAjaxReportModel timeSheetAjaxReportModel)
        {
            var employeeReports = new RMA_EmployeeModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                employeeReports.TimeSheetReports = new List<TimeSheetReportViewModel>();

                if (timeSheetAjaxReportModel.Employees != null && timeSheetAjaxReportModel.Employees.Count > 0)
                {
                    foreach (var employee in timeSheetAjaxReportModel.Employees)
                    {
                        var reportModel = new TimeSheetReportViewModel();

                        var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(employee)).FirstOrDefault();
                        if (employeeInfo != null)
                        {
                            reportModel.EmployeeInfo = employeeInfo;
                        }

                        int weekNumber = System.Convert.ToInt32(timeSheetAjaxReportModel.WeekNumber);

                        var empTimeSheetInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employee) && a.weekno == weekNumber).ToList();
                        if (empTimeSheetInfo != null)
                        {
                            reportModel.timeSheetInfo = empTimeSheetInfo;
                        }

                        employeeReports.TimeSheetReports.Add(reportModel);
                    }
                }
            }

            return PartialView(employeeReports);
        }

        public JsonResult GetEmployees()
        {
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var employeesDetails = db.AMBC_Active_Emp_view.ToList();
                var employeeJson = JsonConvert.SerializeObject(employeesDetails);
                return Json(employeeJson);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Export(string GridHtml)
        {
            var ccc = GridHtml;
            //return File(Encoding.ASCII.GetBytes(GridHtml), "application/vnd.ms-excel", "Grid.xls");

            TimeSheetAjaxReportModel timeSheetAjaxReportModel = JsonConvert.DeserializeObject<TimeSheetAjaxReportModel>(GridHtml);

            List<SourceFile> sourceFiles = new List<SourceFile>();

            //TimeSheetAjaxReportModel timeSheetAjaxReportModel = new TimeSheetAjaxReportModel();

            //timeSheetAjaxReportModel.Employees.Add("C4046");
            //timeSheetAjaxReportModel.WeekNumber = "49";
            //timeSheetAjaxReportModel.WeekStartDate = "1bc";
            //timeSheetAjaxReportModel.WeekEndDate = "111";

            if (timeSheetAjaxReportModel != null && timeSheetAjaxReportModel.Employees != null && timeSheetAjaxReportModel.Employees.Count > 0)
            {
                foreach (var employee in timeSheetAjaxReportModel.Employees)
                {
                    string urlAddress = "https://localhost:44375/TimeSheetDownloadReportsPartial?employeeId=" + employee + "&weeknum=" + timeSheetAjaxReportModel.WeekNumber + "";

                    string htmlContent = new System.Net.WebClient().DownloadString(urlAddress);

                    byte[] byteArray = Encoding.ASCII.GetBytes(htmlContent);

                    sourceFiles.Add(new SourceFile()
                    {
                        FileBytes = byteArray,
                        Extension = timeSheetAjaxReportModel.Type,
                        Name = employee + "-TimeSheet-" + timeSheetAjaxReportModel.WeekStartDate + "to" + timeSheetAjaxReportModel.WeekEndDate + "," + "2022"
                    });
                }
            }



            byte[] fileBytes = null;

            // create a working memory stream
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                // create a zip
                using (System.IO.Compression.ZipArchive zip = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    // interate through the source files
                    foreach (SourceFile f in sourceFiles)
                    {
                        // add the item name to the zip
                        System.IO.Compression.ZipArchiveEntry zipItem = zip.CreateEntry(f.Name + "." + f.Extension);
                        // add the item bytes to the zip entry by opening the original file and copying the bytes
                        using (System.IO.MemoryStream originalFileMemoryStream = new System.IO.MemoryStream(f.FileBytes))
                        {
                            using (System.IO.Stream entryStream = zipItem.Open())
                            {
                                originalFileMemoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
                fileBytes = memoryStream.ToArray();
            }

            // download the constructed zip
            Response.AddHeader("Content-Disposition", "attachment; filename=download.zip");
            return File(fileBytes, "application/zip");



            //return File(Encoding.ASCII.GetBytes(htmlContent), "application/vnd.ms-excel", "attachment;filename=ExportedHtml.xls");

            //return View("~/Views/Shared/Error.cshtml");
        }


        public ActionResult TimeSheetDownloadReportsPartial(string employeeId, string weekNum)
        {
            var employeeReports = new RMA_EmployeeModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                employeeReports.TimeSheetReports = new List<TimeSheetReportViewModel>();

                var reportModel = new TimeSheetReportViewModel();

                var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(employeeId)).FirstOrDefault();
                if (employeeInfo != null)
                {
                    reportModel.EmployeeInfo = employeeInfo;
                }

                int weekNumber = System.Convert.ToInt32(weekNum);

                var empTimeSheetInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber).ToList();
                if (empTimeSheetInfo != null)
                {
                    reportModel.timeSheetInfo = empTimeSheetInfo;
                }

                employeeReports.TimeSheetReports.Add(reportModel);

            }

            return PartialView(employeeReports);
        }
    }
}
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
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Text.Json;
    using System.Web.UI;
    using static ResourceManagement.Helpers.DateHelper;
    using static ResourceManagement.Models.LeaveOrHolidayModel;
    using static ResourceManagement.Models.TimesheetReportModel;

    public class HomeController : Controller
    {

        public PdfDocument Convert(string pageURl)
        {
            // read parameters from the webpage
            string url = pageURl;

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
                webPageHeight = System.Convert.ToInt32(1024);
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

            //// save pdf document
            //byte[] pdf = doc.Save();

            //// close pdf document
            //doc.Close();

            return doc;

            //// return resulted pdf document
            //FileResult fileResult = new FileContentResult(pdf, "application/pdf");
            //fileResult.FileDownloadName = "Document.pdf";
            //return fileResult;
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
                        Employee_LoginLocation = employeeModel.AMBC_Active_Emp_view.Location,
                        Concat_loginstring = employeeModel.AMBC_Active_Emp_view.Employee_ID + "," + todayDate.ToString("yyyy-MM-dd") + "," + todayDate.ToString()
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
            List<DataPoint> dataPointsWorkingHours = new List<DataPoint>();
            List<DataPoint> dataPointsOverTime = new List<DataPoint>();

            var weekdays = WeekdaysList();
            double hoursSpent = 0;

            if (weekreportmodel != null && weekreportmodel.Any() && weekdays != null && weekdays.Any())
            {
                foreach (var weekday in weekdays)
                {
                    double currentDayHoursSpent = 0;
                    double currentDayOverTime = 0;
                    var daySpecificEntries = weekreportmodel.Where(x => x.weekday == weekday).Count() > 0 ? weekreportmodel.Where(x => x.weekday == weekday).ToList() : null;
                    if (daySpecificEntries != null)
                    {
                        foreach (var daySpecificEntrie in daySpecificEntries)
                        {
                            hoursSpent += daySpecificEntrie.hoursspent + daySpecificEntrie.overtime;
                            currentDayHoursSpent += daySpecificEntrie.hoursspent;
                            currentDayOverTime += daySpecificEntrie.overtime;
                        }

                        //By default time spent hours assigining as 8 hour only
                        //Remaining Time Spent hours considered as Over Time hours for the day
                        var totalOverTimeHoursSpent = currentDayHoursSpent > 8 ? currentDayHoursSpent - 8 : 0;

                        currentDayHoursSpent = currentDayHoursSpent >= 8 ? 8 : currentDayHoursSpent;
                        currentDayOverTime = currentDayOverTime + totalOverTimeHoursSpent;
                    }

                    var weekdayExistsInweekreportModel = weekreportmodel.Where(x => x.weekday == weekday).FirstOrDefault();

                    var isHolidayOrLeaveDay = true;

                    //Hours Spent
                    if (currentDayHoursSpent > 0)
                    {
                        isHolidayOrLeaveDay = false;
                        dataPointsWorkingHours.Add(new DataPoint(weekday, currentDayHoursSpent, "rgb(81, 205, 160)", ""));
                    }
                    else
                    {

                        if (daySpecificEntries != null)
                        {
                            dataPointsWorkingHours.Add(new DataPoint(weekday + "(Leave)", 8, "rgb(220, 20, 60)", "Leave"));
                        }
                        else
                        {
                            dataPointsWorkingHours.Add(new DataPoint(weekday, 0, "", ""));
                        }
                    }

                    //Over Time
                    if (currentDayOverTime > 0)
                    {
                        dataPointsOverTime.Add(new DataPoint(weekday, currentDayOverTime, "rgb(109, 120, 173)", ""));
                    }
                    else
                    {
                        if (daySpecificEntries != null && isHolidayOrLeaveDay)
                        {
                            dataPointsOverTime.Add(new DataPoint(weekday + "(Leave)", 0, "", ""));
                        }
                        else
                        {
                            dataPointsOverTime.Add(new DataPoint(weekday, 0, "", ""));
                        }
                    }

                }
            }
            else
            {
                foreach (var weekday in weekdays)
                {
                    dataPointsWorkingHours.Add(new DataPoint(weekday, 0, "", ""));
                    dataPointsOverTime.Add(new DataPoint(weekday, 0, "", ""));
                }
            }


            ViewBag.DataPointsWorkingHours = JsonConvert.SerializeObject(dataPointsWorkingHours);
            ViewBag.DataPointsOverTime = JsonConvert.SerializeObject(dataPointsOverTime);
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
                            var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(loginModel.att_username)).ToList();
                            if (employeeInfo != null && employeeInfo.Count() > 0)
                            {
                                employeeModel.AMBC_Active_Emp_view = employeeInfo[0];
                                employeeModel.projectInfo = employeeInfo;
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

        public JsonResult GetLeaveandHolidayInfofromDb(AjaxLeaveOrHolidayModel timeSheetAjaxLeaveOrHolidayModel)
        {
            var leaveHolidaySignInData = new RMA_LeaveHolidaySignInModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var startDate = DateTime.Parse(timeSheetAjaxLeaveOrHolidayModel.WeekStartDate);
                var endDate = DateTime.Parse(timeSheetAjaxLeaveOrHolidayModel.WeekEndDate);

                List<DateTime> datesBetweenStartAndEnd = GetWeekdays(startDate, endDate);

                var conSignInDetails = db.tbld_ambclogininformation.Where(a => a.Employee_Code.Equals(timeSheetAjaxLeaveOrHolidayModel.EmpId) && a.Login_date >= startDate && a.Login_date <= endDate).ToList();
                if (conSignInDetails != null && conSignInDetails.Count > 0)
                {
                    foreach (var conSignIn in conSignInDetails)
                    {
                        leaveHolidaySignInData.SignInInfo.Add(new RMA_SignInInfo()
                        {
                            SignInDate = GetDateInRequiredFormat(conSignIn.Signin_Time.ToString()),
                            Reason = "Checked In"
                        });
                    }
                }

                var ambcHolidays = db.tblambcholidays.Where(b => b.holiday_date >= startDate && b.holiday_date <= endDate && b.region == timeSheetAjaxLeaveOrHolidayModel.EmpRegion).ToList();
                //var employeeWorkedOnHolidays = db.tblambcholidaylogs.Where(b => b.holiday_date >= startDate && b.holiday_date <= endDate && b.employee_id == timeSheetAjaxLeaveOrHolidayModel.EmpId).ToList();

                if (ambcHolidays != null && ambcHolidays.Count > 0)
                {
                    //Holidays mapping
                    foreach (var ambcLeave in ambcHolidays)
                    {
                        var isEmployeeWOrkedonHoliday = conSignInDetails.Where(holiday => holiday.Login_date == ambcLeave.holiday_date).FirstOrDefault();

                        if (isEmployeeWOrkedonHoliday == null)
                        {
                            leaveHolidaySignInData.LeaveHolidayInfo.Add(new RMA_LeaveOrHolidayInfo()
                            {
                                LeaveOrHolidayDate = GetDateInRequiredFormat(ambcLeave.holiday_date.ToString()),
                                Reason = ambcLeave.holiday_name,
                                DefaultLeaveOrHolidayDate = ambcLeave.holiday_date
                            });
                        }

                    }

                }


                //If no sign in considering those dates as Holiday or Leave
                foreach (var selecteddate in datesBetweenStartAndEnd)
                {
                    var isEmployeeSignedIn = conSignInDetails.Where(holiday => holiday.Login_date == selecteddate).FirstOrDefault();

                    if (isEmployeeSignedIn == null)
                    {
                        var isMissedSignInDateHoliday = leaveHolidaySignInData.LeaveHolidayInfo.Where(holiday => holiday.DefaultLeaveOrHolidayDate == selecteddate).FirstOrDefault();

                        if (isMissedSignInDateHoliday == null)
                        {
                            leaveHolidaySignInData.LeaveHolidayInfo.Add(new RMA_LeaveOrHolidayInfo()
                            {
                                LeaveOrHolidayDate = GetDateInRequiredFormat(selecteddate.ToString()),
                                Reason = "No Check-In"
                            });
                        }
                    }

                }
            }

            if ((leaveHolidaySignInData.SignInInfo != null && leaveHolidaySignInData.SignInInfo.Count > 0) || (leaveHolidaySignInData.LeaveHolidayInfo != null && leaveHolidaySignInData.LeaveHolidayInfo.Count > 0))
            {
                return Json(JsonSerializer.Serialize(leaveHolidaySignInData), JsonRequestBehavior.AllowGet);
            }

            return Json(null);
        }

        private static List<DateTime> GetWeekdays(DateTime startDate, DateTime endDate)
        {
            var datesBetweenStartAndEnd = new List<DateTime>();

            for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
            {
                datesBetweenStartAndEnd.Add(dt);
            }

            return datesBetweenStartAndEnd;
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
                TempData.Remove("TimeSheetModeldata");
                using (var context = new TimeSheetEntities())
                {
                    if (timesheetmodel != null)
                    {
                        context.ambctaskcaptures.AddRange(timesheetmodel);
                        context.SaveChanges();
                        respone.StatusCode = 200;
                        respone.Message = "TimeSheet added successfully!";
                        TempData["TimeSheetModeldata"] = timesheetmodel;
                        TimeSheetReportEmail(employeeModel);
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


        public JsonResult ISEmployeeSubmittedTimeSheetInSelectedWeek(TimeSheetAjaxReportModel timeSheetAjaxReportModel)
        {
            var timeSheetViewreportModel = new RMA_TimeSheetReportsFromDB();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                if (timeSheetAjaxReportModel != null)
                {

                    int weekNumber = System.Convert.ToInt32(timeSheetAjaxReportModel.WeekNumber);
                    var empTimeSheetInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(timeSheetAjaxReportModel.EmpId) && a.weekno == weekNumber && a.clientname == timeSheetAjaxReportModel.ClientName).ToList();

                    if (empTimeSheetInfo != null && empTimeSheetInfo.Count > 0)
                    {
                        timeSheetViewreportModel.Viewreports = empTimeSheetInfo;
                        timeSheetViewreportModel.StatusCode = 200;
                        timeSheetViewreportModel.Message = "TimeSheet Report exists for the selected week";
                    }
                    else
                    {
                        timeSheetViewreportModel.StatusCode = 404;
                        timeSheetViewreportModel.Message = "TimeSheet Report not exists for the selected week";
                    }
                }
            }

            return Json(JsonConvert.SerializeObject(timeSheetViewreportModel));
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

            TimeSheetAjaxReportModel timeSheetAjaxReportModel = JsonConvert.DeserializeObject<TimeSheetAjaxReportModel>(GridHtml);

            List<SourceFile> sourceFiles = new List<SourceFile>();

            if (timeSheetAjaxReportModel != null && timeSheetAjaxReportModel.Employees != null && timeSheetAjaxReportModel.Employees.Count > 0)
            {
                foreach (var employee in timeSheetAjaxReportModel.Employees)
                {

                    if (timeSheetAjaxReportModel.Type == ".xls")
                    {
                        string urlAddress = ConfigurationManager.AppSettings["SiteURL"] + "/TimeSheetDownloadReportsPartial?employeeId=" + employee + "&weeknum=" + timeSheetAjaxReportModel.WeekNumber + "";

                        string htmlContent = new System.Net.WebClient().DownloadString(urlAddress);

                        byte[] byteArray = Encoding.ASCII.GetBytes(htmlContent);

                        sourceFiles.Add(new SourceFile()
                        {
                            FileBytes = byteArray,
                            Extension = timeSheetAjaxReportModel.Type,
                            Name = employee + "-TimeSheet-" + timeSheetAjaxReportModel.WeekStartDate + "to" + timeSheetAjaxReportModel.WeekEndDate + "," + "2022"
                        });
                    }
                    else
                    {
                        PdfDocument pdfData = Convert(ConfigurationManager.AppSettings["SiteURL"] + "/TimeSheetDownloadReportsPartial?employeeId=" + employee + "&weeknum=" + timeSheetAjaxReportModel.WeekNumber + "");

                        byte[] pdfArray = pdfData.Save();

                        //// close pdf document
                        pdfData.Close();

                        sourceFiles.Add(new SourceFile()
                        {
                            FileBytes = pdfArray,
                            Extension = timeSheetAjaxReportModel.Type,
                            Name = employee + "-TimeSheet-" + timeSheetAjaxReportModel.WeekStartDate + "to" + timeSheetAjaxReportModel.WeekEndDate + "," + "2022"
                        });
                    }


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


        public static string RenderPartialToString(Controller controller, string partialViewName, object model, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            ViewEngineResult result = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName);

            if (result.View != null)
            {
                controller.ViewData.Model = model;
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    using (HtmlTextWriter output = new HtmlTextWriter(sw))
                    {
                        ViewContext viewContext = new ViewContext(controller.ControllerContext, result.View, viewData, tempData, output);
                        result.View.Render(viewContext, output);
                    }
                }

                return sb.ToString();
            }
            return String.Empty;
        }


        public string TimeSheetEmailReport()
        {
            var timeSheetReport = new TimeSheetEmailReport();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                timeSheetReport.reports = TempData["TimeSheetModeldata"] as List<ambctaskcapture>;

                if (timeSheetReport.reports != null && timeSheetReport.reports.Count > 0)
                {
                    timeSheetReport.reports = timeSheetReport.reports.OrderBy(date => date.taskdate).ToList();

                    List<DateTime> datesBetweenStartAndEnd = GetWeekdays(timeSheetReport.reports[0].weekstartdate.Value, timeSheetReport.reports[0].weekenddate.Value);

                    var totalHoursSpent = 0;

                    foreach (var date in datesBetweenStartAndEnd)
                    {
                        string dayName = date.ToString("dddd");

                        var requiredDate = GetDateInRequiredFormat(date.ToString());

                        var dayHoursSpent = 0;
                        var dayOverTime = 0;

                        var hoursworkedInparticulatDate = timeSheetReport.reports.Where(report => report.taskdate == System.Convert.ToDateTime(requiredDate)).ToList();

                        foreach (var TimeWorked in hoursworkedInparticulatDate)
                        {
                            dayHoursSpent += TimeWorked.timespent.Value;
                            if (TimeWorked.overtime != null)
                            {
                                dayOverTime += TimeWorked.overtime.Value;
                            }
                        }

                        totalHoursSpent += dayHoursSpent;
                        totalHoursSpent += dayOverTime;

                        switch (dayName)
                        {
                            case "Monday":
                                timeSheetReport.MondayHoliday = dayHoursSpent == 0 ? "8" : "0";
                                timeSheetReport.MondayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.MondayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            case "Tuesday":
                                timeSheetReport.TuesdayHoliday = dayHoursSpent == 0 ? "8" : "0";
                                timeSheetReport.TuesdayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.TuesdayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            case "Wednesday":
                                timeSheetReport.WednesdayHoliday = dayHoursSpent == 0 ? "8" : "0";
                                timeSheetReport.WednesdayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.WednesdayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            case "Thursday":
                                timeSheetReport.ThursdayHoliday = dayHoursSpent == 0 ? "8" : "0";
                                timeSheetReport.ThursdayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.ThursdayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            case "Friday":
                                timeSheetReport.FridayHoliday = dayHoursSpent == 0 ? "8" : "0";
                                timeSheetReport.FridayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.FridayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            case "Saturday":
                                timeSheetReport.SaturdayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.SaturdayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            case "Sunday":
                                timeSheetReport.SundayHours = dayHoursSpent >= 8 ? "8" : System.Convert.ToString(dayHoursSpent);
                                timeSheetReport.SundayOverTime = dayHoursSpent >= 8 ? System.Convert.ToString(dayOverTime + (dayHoursSpent - System.Convert.ToInt32(8))) : "0";
                                break;

                            default:
                                Console.WriteLine("No match found");
                                break;
                        }
                    }

                    timeSheetReport.TotalHoursSpent = System.Convert.ToString(totalHoursSpent);
                    int overTime = System.Convert.ToInt32(timeSheetReport.TotalHoursSpent) - 40;
                    return RenderPartialToString(this, "TimeSheetEmailReport", timeSheetReport, ViewData, TempData);
                }
            }

            return string.Empty;
        }


        public void TimeSheetReportEmail(RMA_EmployeeModel empModel)
        {
            string htmlContent = TimeSheetEmailReport();
            TempData.Remove("TimeSheetModeldata");
            using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], empModel.AMBC_Active_Emp_view.AMBC_Mail_Address))
            {
                mm.Subject = empModel.AMBC_Active_Emp_view.Employee_Name + " Timesheet Submission Details " + empModel.AMBC_Active_Emp_view.AMBC_Mail_Address;
                mm.Body = htmlContent;

                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["SMTPHost"];
                smtp.EnableSsl = true;
                NetworkCredential credentials = new NetworkCredential();
                credentials.UserName = ConfigurationManager.AppSettings["SMTPUserName"];
                credentials.Password = ConfigurationManager.AppSettings["SMTPPassword"];
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = credentials;
                smtp.Port = System.Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                smtp.Send(mm);
            }

        }

        public ActionResult TimeSheetRemainder(TimeSheetAjaxReportModel timeSheetAjaxReportModel)
        {
            var timeSheetRemainder = new RMA_TimeSheetRemainder();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                if (timeSheetAjaxReportModel != null)
                {

                    int weekNumber = System.Convert.ToInt32(timeSheetAjaxReportModel.WeekNumber);

                    var projectSpecificEmployees = db.AMBC_Active_Emp_view.Where(emp => emp.Client == timeSheetAjaxReportModel.ClientName).ToList();

                    var submittedEmpList = db.ambctaskcaptures.Where(a => a.weekno == weekNumber && a.clientname == timeSheetAjaxReportModel.ClientName).ToList();

                    var notSubmittedEmpList = submittedEmpList.Count() > 0 ? projectSpecificEmployees.Where(x => submittedEmpList.Where(y => y.employeeid != x.Employee_ID).FirstOrDefault() != null).ToList() : projectSpecificEmployees.ToList();

                    if (notSubmittedEmpList != null && notSubmittedEmpList.Count() > 0)
                    {
                        timeSheetRemainder.EmpListForremainder = notSubmittedEmpList;
                        timeSheetRemainder.Message = "Employees who not submitted timesheet yet!";
                        timeSheetRemainder.StatusCode = 200;
                    }
                }
            }

            return PartialView(timeSheetRemainder);
        }


        public JsonResult SendRemainderEmail(RMA_TimeSheetRemainderEmail timesheetEmpRemainder)
        {
            try
            {
                if (timesheetEmpRemainder != null && timesheetEmpRemainder.emails.Count() > 0)
                {
                    foreach (var email in timesheetEmpRemainder.emails)
                    {
                        using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], email))
                        {
                            mm.Subject = "TimeSheet is missing!";
                            mm.Body = "Hello Consultant, <br> Please submit your time sheet immediatly. + <br> <br> AMBC Technologies";

                            mm.IsBodyHtml = true;
                            SmtpClient smtp = new SmtpClient();
                            smtp.Host = ConfigurationManager.AppSettings["SMTPHost"];
                            smtp.EnableSsl = true;
                            NetworkCredential credentials = new NetworkCredential();
                            credentials.UserName = ConfigurationManager.AppSettings["SMTPUserName"];
                            credentials.Password = ConfigurationManager.AppSettings["SMTPPassword"];
                            smtp.UseDefaultCredentials = true;
                            smtp.Credentials = credentials;
                            smtp.Port = System.Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                            smtp.Send(mm);
                        }
                    }

                    var emailRemainderResponse = new JsonResponseModel();

                    emailRemainderResponse.StatusCode = 200;
                    emailRemainderResponse.Message = "Remainder Email Sent Successfully!";
                    return Json(emailRemainderResponse);

                }
            }
            catch (Exception ex)
            {

            }

            return Json(null);

        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    using Microsoft.Ajax.Utilities;
    using Models;
    using OfficeOpenXml;
    using SelectPdf;
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Text;
    using System.Text.Json;
    using System.Web;
    using System.Web.UI;
    using static ResourceManagement.Helpers.DateHelper;
    using static ResourceManagement.Models.Graph1DataPoint;
    using static ResourceManagement.Models.LeaveOrHolidayModel;
    using static ResourceManagement.Models.ProjectGraphDataPoint;
    using static ResourceManagement.Models.TimesheetReportModel;
    using DataPoint = Models.DataPoint;
    using static ResourceManagement.Models.Email.RemainderEmailBody;

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
                    var SystemInfo = SystemInformation();

                    var ambcEmpLoginInfo = new tbld_ambclogininformation()
                    {
                        Employee_Code = employeeModel.AMBC_Active_Emp_view.Employee_ID,
                        Employee_Name = employeeModel.AMBC_Active_Emp_view.Employee_Name,
                        Employee_Designation = employeeModel.AMBC_Active_Emp_view.Designation,
                        Employee_Shift = employeeModel.AMBC_Active_Emp_view.Shift,
                        Login_date = todayDate,
                        Signin_Time = todayDate,
                        Employee_Hostname = SystemInfo.SystemHostName,
                        Employee_IP = SystemInfo.SystemIP,
                        Employee_LoginLocation = employeeModel.AMBC_Active_Emp_view.Location,
                        Concat_loginstring = employeeModel.AMBC_Active_Emp_view.Employee_ID + "_" + todayDate.ToString("yyyy-MM-dd")
                    };

                    using (var context = new TimeSheetEntities())
                    {
                        var isEmpAppliedLeavePresentDay = context.con_leaveupdate.Where(a => a.employee_id.Equals(ambcEmpLoginInfo.Employee_Code) && a.leavedate == System.DateTime.Today).FirstOrDefault();

                        //HANDLE FOR HOLIDAYS and WEEKENDS AS WELL IN FUTURE

                        if (isEmpAppliedLeavePresentDay == null)
                        {
                            context.tbld_ambclogininformation.Add(ambcEmpLoginInfo);
                            context.SaveChanges();
                            respone.jsonResponse.StatusCode = 200;
                            respone.jsonResponse.Message = "Checked in Successful!";
                            respone.signin = true;
                            respone.empemailid = employeeModel.AMBC_Active_Emp_view.AMBC_Mail_Address;
                            respone.empname = employeeModel.AMBC_Active_Emp_view.Employee_Name;
                            respone.type = "SignIn";
                        }
                        else
                        {
                            respone.jsonResponse.StatusCode = 500;
                            respone.jsonResponse.Message = "You are on Leave/Holiday!";
                            respone.empemailid = employeeModel.AMBC_Active_Emp_view.AMBC_Mail_Address;
                            respone.empname = employeeModel.AMBC_Active_Emp_view.Employee_Name;
                            respone.type = "No-SignIn-Required";
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

        public JsonResult SignInAndOutEmail(RMA_SignInOutEmailModel signInAndOutEmailData)
        {
            var emailBody = "";
            var emailSubject = "";
            if (signInAndOutEmailData.type == "SignIn")
            {
                emailSubject = signInAndOutEmailData.empname + " Check-In (" + GetDateInRequiredFormatDDMMYYYY(DateTime.Today.ToString()) + ") Successful.";
                emailBody = "<div style='color:#23366f'>Hey <b>" + signInAndOutEmailData.empname + "</b>, <br><br> Thank you for check-In. <br> Have a good day! <br>  <br><br> <b>~AMBC Technologies</b></div>";
            }

            if (signInAndOutEmailData.type == "SignOut")
            {
                emailSubject = signInAndOutEmailData.empname + " Check-Out (" + GetDateInRequiredFormatDDMMYYYY(DateTime.Today.ToString()) + ") Successful.";
                emailBody = "<div style='color:#23366f'>Hey <b>" + signInAndOutEmailData.empname + "</b>, <br><br>  Thank you for check-Out. <br> Have a good day. See you tomorrow! <br>  <br><br> <b>~AMBC Technologies</b></div>";
            }
            using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], signInAndOutEmailData.empemailid))
            {
                mm.Subject = emailSubject;
                mm.Body = emailBody;

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

            return Json(null);

        }

        private static RMA_SystemDetails SystemInformation()
        {
            var systemInfo = new RMA_SystemDetails();
            systemInfo.SystemHostName = Dns.GetHostName();

            // Get the IP
            IPHostEntry ip = Dns.GetHostEntry(systemInfo.SystemHostName);
            systemInfo.SystemIP = ip.AddressList.Count() > 3 ? ip.AddressList[3].ToString() : ip.AddressList[0].ToString();

            return systemInfo;
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
                            respone.jsonResponse.Message = "Check-Out Successful!";
                            respone.empemailid = employeeModel.AMBC_Active_Emp_view.AMBC_Mail_Address;
                            respone.empname = employeeModel.AMBC_Active_Emp_view.Employee_Name;
                            respone.type = "SignOut";
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
                            var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(loginModel.att_username) && a.Project_Status == "Active").ToList();
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

        private string GetDateInRequiredFormatDDMMYYYY(string actualdate)
        {
            string dateString = actualdate;
            DateTime dateTime = DateTime.Parse(dateString);
            return dateTime.ToString("dd-MM-yyyy");
        }
        public JsonResult GetLeaveandHolidayInfofromDb(AjaxLeaveOrHolidayModel timeSheetAjaxLeaveOrHolidayModel)
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            var leaveHolidaySignInData = new RMA_LeaveHolidaySignInModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var startDate = DateTime.Parse(timeSheetAjaxLeaveOrHolidayModel.WeekStartDate);
                var endDate = DateTime.Parse(timeSheetAjaxLeaveOrHolidayModel.WeekEndDate);

                List<DateTime> datesBetweenStartAndEnd = GetWeekdays(startDate, endDate);

                var conSignInDetails = db.tbld_ambclogininformation.Where(a => a.Employee_Code.Equals(timeSheetAjaxLeaveOrHolidayModel.EmpId) && a.Login_date >= startDate && a.Login_date <= endDate).ToList();
                var employeeHalfDayLevaList = db.ambclogin_leave_view.Where(leave => leave.Employee_Code == employeeModel.AMBC_Active_Emp_view.Employee_ID && leave.Leave_Type == "Half Day Leave");
                if (conSignInDetails != null && conSignInDetails.Count > 0)
                {
                    foreach (var conSignIn in conSignInDetails)
                    {
                        var isEmployeeTakenHalfDayLeave = employeeHalfDayLevaList.Where(leave => leave.Leave_Date == conSignIn.Login_date).FirstOrDefault();

                        //If Employee Signed in but later taken leave then adding Signin date as Holiday or Leave
                        var isEmployeeTaeknUnplannedLeavePostSignIn = db.ambclogin_leave_view.Where(leave => leave.Employee_Code == employeeModel.AMBC_Active_Emp_view.Employee_ID && leave.Leave_Date == conSignIn.Login_date && leave.Leave_Type != null && leave.Leave_Type != "working" && leave.Leave_Type != "Half Day Leave").FirstOrDefault();
                        if (isEmployeeTaeknUnplannedLeavePostSignIn != null)
                        {
                            leaveHolidaySignInData.LeaveHolidayInfo.Add(new RMA_LeaveOrHolidayInfo()
                            {
                                LeaveOrHolidayDate = GetDateInRequiredFormat(conSignIn.Login_date.ToString()),
                                Reason = "Checked In, then applied leave",
                                DefaultLeaveOrHolidayDate = conSignIn.Login_date
                            });
                        }
                        else
                        {
                            leaveHolidaySignInData.SignInInfo.Add(new RMA_SignInInfo()
                            {
                                SignInDate = GetDateInRequiredFormat(conSignIn.Signin_Time.ToString()),
                                Reason = isEmployeeTakenHalfDayLeave != null ? "Half Day Leave" : "Checked In"
                            });
                        }

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
                        //Checking if the selected date is fall under Holiday
                        var isMissedSignInDateHoliday = leaveHolidaySignInData.LeaveHolidayInfo.Where(holiday => holiday.DefaultLeaveOrHolidayDate == selecteddate).FirstOrDefault();

                        if (isMissedSignInDateHoliday == null)
                        {
                            //Checking whether employee applied leave on selected date in case of no sign in /check in
                            var isAppliedLeaveOnSelctedDate = db.con_leaveupdate.Where(leave => leave.employee_id == employeeModel.AMBC_Active_Emp_view.Employee_ID && leave.leavedate == selecteddate).FirstOrDefault();

                            if (isAppliedLeaveOnSelctedDate == null)
                            {
                                leaveHolidaySignInData.LeaveHolidayInfo.Add(new RMA_LeaveOrHolidayInfo()
                                {
                                    LeaveOrHolidayDate = GetDateInRequiredFormat(selecteddate.ToString()),
                                    Reason = "No Check-In / not applied leave"
                                });
                            }
                            else
                            {
                                leaveHolidaySignInData.LeaveHolidayInfo.Add(new RMA_LeaveOrHolidayInfo()
                                {
                                    LeaveOrHolidayDate = GetDateInRequiredFormat(selecteddate.ToString()),
                                    Reason = !string.IsNullOrEmpty(isAppliedLeaveOnSelctedDate.leavecategory) ? isAppliedLeaveOnSelctedDate.leavecategory : isAppliedLeaveOnSelctedDate.leavesource
                                });
                            }

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
                    employeeModel.SystemInfo = SystemInformation();
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
                timesheetmodel.All(x => { x.submittedtime = DateTime.Now; return true; });
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
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
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
                        var employeeId = employee.Split('&')[0];
                        var emplyeeName = employee.Split('&')[1];

                        var reportModel = new TimeSheetReportViewModel();

                        var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(employeeId) && a.Client == timeSheetAjaxReportModel.ClientName).FirstOrDefault();
                        if (employeeInfo != null)
                        {
                            reportModel.EmployeeInfo = employeeInfo;
                        }

                        int weekNumber = System.Convert.ToInt32(timeSheetAjaxReportModel.WeekNumber);

                        var empTimeSheetInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber && a.clientname == timeSheetAjaxReportModel.ClientName).ToList();
                        if (empTimeSheetInfo != null)
                        {
                            reportModel.timeSheetInfo = empTimeSheetInfo;
                        }

                        reportModel.timeSheetLeaveOrHolidayInfo = new List<ReportLeaveOrHolidayInfo>();

                        var empHolidayInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber && a.timespent == 0 && a.overtime == 0).ToList();

                        if (empHolidayInfo != null)
                        {
                            foreach (var empHolidayInf in empHolidayInfo)
                            {
                                var leaveInfoModel = new ReportLeaveOrHolidayInfo()
                                {
                                    LeaveDate = empHolidayInf.taskdate.ToString(),
                                    LeaveType = empHolidayInf.comments,
                                    LeaveDateTime = empHolidayInf.taskdate

                                };

                                reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
                            }

                        }

                        var weekstartDate = System.Convert.ToDateTime(timeSheetAjaxReportModel.WeekStartDate);
                        var weekEndDate = System.Convert.ToDateTime(timeSheetAjaxReportModel.WeekEndDate);

                        var empHalfDayHolidayInfo = db.ambclogin_leave_view.Where(a => a.Employee_Code.Equals(employeeId) && a.Leave_Date >= weekstartDate && a.Leave_Date <= weekEndDate && a.Leave_Type == "Half Day Leave").ToList();

                        if (empHalfDayHolidayInfo != null)
                        {
                            foreach (var empHalfDayHolidayInf in empHalfDayHolidayInfo)
                            {
                                var leaveInfoModel = new ReportLeaveOrHolidayInfo()
                                {
                                    LeaveDate = empHalfDayHolidayInf.Leave_Date.ToString(),
                                    LeaveType = empHalfDayHolidayInf.Leave_Type,
                                    LeaveDateTime = empHalfDayHolidayInf.Leave_Date

                                };
                                reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
                            }
                        }


                        var empAppliedLeavePostTimeSheetSubmissions = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber && (a.timespent > 0 || a.overtime > 0)).ToList();

                        if (empAppliedLeavePostTimeSheetSubmissions != null)
                        {
                            foreach (var empAppliedLeavePostTimeSheetSubmission in empAppliedLeavePostTimeSheetSubmissions)
                            {
                                var isEmpAppliedLeaveonDateModel = db.con_leaveupdate.Where(leave => leave.employee_id == empAppliedLeavePostTimeSheetSubmission.employeeid && leave.leavedate == empAppliedLeavePostTimeSheetSubmission.taskdate).FirstOrDefault();

                                if (isEmpAppliedLeaveonDateModel != null)
                                {
                                    var isLeaveExistsInTheList = reportModel.timeSheetLeaveOrHolidayInfo.Where(x => x.LeaveDateTime == isEmpAppliedLeaveonDateModel.leavedate).FirstOrDefault();

                                    if (isLeaveExistsInTheList == null)
                                    {
                                        var leaveInfoModel = new ReportLeaveOrHolidayInfo()
                                        {
                                            LeaveDate = isEmpAppliedLeaveonDateModel.leavedate.ToString(),
                                            LeaveType = isEmpAppliedLeaveonDateModel.leavesource,
                                            LeaveDateTime = isEmpAppliedLeaveonDateModel.leavedate
                                        };
                                        reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
                                    }
                                }
                            }

                        }

                        //Passing Inputs to view
                        reportModel.timeSheetAjaxInputReportModel = timeSheetAjaxReportModel;

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
                var employeesDetails = db.AMBC_Active_Emp_view.Where(x => x.Project_Status == "Active").ToList();
                var employeeJson = JsonConvert.SerializeObject(employeesDetails);
                return Json(employeeJson);
            }
        }

        public JsonResult GetEmployeeByEmpID(string empID)
        {
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var employeesDetails = db.AMBC_Active_Emp_view.Where(x => x.Project_Status == "Active" && x.Employee_ID == empID).ToList();
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

            var requiredStartDateForExcelReport = string.Empty;
            var requiredEndDateForExcelreport = string.Empty;
            var requiredZIPFileName = string.Empty;

            if (timeSheetAjaxReportModel != null && timeSheetAjaxReportModel.Employees != null && timeSheetAjaxReportModel.Employees.Count > 0)
            {
                var reportEndDate = timeSheetAjaxReportModel.WeekEndDate;
                DateTime dt = System.Convert.ToDateTime(reportEndDate);
                var reportMonthName = dt.ToString("MMMM");

                reportMonthName = reportMonthName.Substring(0, 3);

                var inputStartDate = timeSheetAjaxReportModel.WeekStartDate.Split('-');
                requiredStartDateForExcelReport = inputStartDate[2];

                var inputEnddate = timeSheetAjaxReportModel.WeekEndDate.Split('-');
                requiredEndDateForExcelreport = inputEnddate[2];

                var reportYear = inputEnddate[0];

                requiredZIPFileName = timeSheetAjaxReportModel.ClientName + "-" + requiredStartDateForExcelReport + "to" + requiredEndDateForExcelreport;

                foreach (var employee in timeSheetAjaxReportModel.Employees)
                {
                    var employeeId = employee.Split('&')[0];
                    var emplyeeName = employee.Split('&')[1];

                    if (timeSheetAjaxReportModel.Type == ".xls")
                    {
                        string htmlContent = TimeSheetDownloadReportsPartial(employeeId, timeSheetAjaxReportModel.WeekNumber, timeSheetAjaxReportModel);

                        //string htmlContent = new System.Net.WebClient().DownloadString(urlAddress);

                        byte[] byteArray = Encoding.ASCII.GetBytes(htmlContent);

                        sourceFiles.Add(new SourceFile()
                        {
                            FileBytes = byteArray,
                            Extension = timeSheetAjaxReportModel.Type,
                            Name = emplyeeName + "-TimeSheet- " + requiredStartDateForExcelReport + " to " + requiredEndDateForExcelreport + ", " + reportMonthName + " " + reportYear
                        });
                    }
                    else
                    {
                        PdfDocument pdfData = Convert(ConfigurationManager.AppSettings["SiteURL"] + "/TimeSheetDownloadReportsPartial?employeeId=" + employeeId + "&weeknum=" + timeSheetAjaxReportModel.WeekNumber + "");

                        byte[] pdfArray = pdfData.Save();

                        //// close pdf document
                        pdfData.Close();

                        sourceFiles.Add(new SourceFile()
                        {
                            FileBytes = pdfArray,
                            Extension = timeSheetAjaxReportModel.Type,
                            Name = emplyeeName + "-TimeSheet- " + requiredStartDateForExcelReport + " to " + requiredEndDateForExcelreport + ", " + reportMonthName + " " + reportYear
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
            Response.AddHeader("Content-Disposition", "attachment; filename=" + requiredZIPFileName + ".zip");
            return File(fileBytes, "application/zip");
        }


        public string TimeSheetDownloadReportsPartial(string employeeId, string weekNum, TimeSheetAjaxReportModel timeSheetAjaxReportModel)
        {
            var employeeReports = new RMA_EmployeeModel();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                employeeReports.TimeSheetReports = new List<TimeSheetReportViewModel>();

                var reportModel = new TimeSheetReportViewModel();

                var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(employeeId) && a.Client == timeSheetAjaxReportModel.ClientName).FirstOrDefault();
                if (employeeInfo != null)
                {
                    reportModel.EmployeeInfo = employeeInfo;
                }

                int weekNumber = System.Convert.ToInt32(weekNum);

                var empTimeSheetInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber && a.clientname == timeSheetAjaxReportModel.ClientName).ToList();
                if (empTimeSheetInfo != null)
                {
                    reportModel.timeSheetInfo = empTimeSheetInfo;
                }

                reportModel.timeSheetLeaveOrHolidayInfo = new List<ReportLeaveOrHolidayInfo>();

                var empHolidayInfo = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber && a.timespent == 0 && a.overtime == 0).ToList();

                if (empHolidayInfo != null)
                {
                    foreach (var empHolidayInf in empHolidayInfo)
                    {
                        var leaveInfoModel = new ReportLeaveOrHolidayInfo()
                        {
                            LeaveDate = empHolidayInf.taskdate.ToString(),
                            LeaveType = empHolidayInf.comments,
                            LeaveDateTime = empHolidayInf.taskdate

                        };

                        reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
                    }

                }

                var weekstartDate = System.Convert.ToDateTime(timeSheetAjaxReportModel.WeekStartDate);
                var weekEndDate = System.Convert.ToDateTime(timeSheetAjaxReportModel.WeekEndDate);

                var empHalfDayHolidayInfo = db.ambclogin_leave_view.Where(a => a.Employee_Code.Equals(employeeId) && a.Leave_Date >= weekstartDate && a.Leave_Date <= weekEndDate && a.Leave_Type == "Half Day Leave").ToList();

                if (empHalfDayHolidayInfo != null)
                {
                    foreach (var empHalfDayHolidayInf in empHalfDayHolidayInfo)
                    {
                        var leaveInfoModel = new ReportLeaveOrHolidayInfo()
                        {
                            LeaveDate = empHalfDayHolidayInf.Leave_Date.ToString(),
                            LeaveType = empHalfDayHolidayInf.Leave_Type,
                            LeaveDateTime = empHalfDayHolidayInf.Leave_Date

                        };
                        reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
                    }
                }

                var empAppliedLeavePostTimeSheetSubmissions = db.ambctaskcaptures.Where(a => a.employeeid.Equals(employeeId) && a.weekno == weekNumber && (a.timespent > 0 || a.overtime > 0)).ToList();

                if (empAppliedLeavePostTimeSheetSubmissions != null)
                {
                    foreach (var empAppliedLeavePostTimeSheetSubmission in empAppliedLeavePostTimeSheetSubmissions)
                    {
                        var isEmpAppliedLeaveonDateModel = db.con_leaveupdate.Where(leave => leave.employee_id == empAppliedLeavePostTimeSheetSubmission.employeeid && leave.leavedate == empAppliedLeavePostTimeSheetSubmission.taskdate).FirstOrDefault();

                        if (isEmpAppliedLeaveonDateModel != null)
                        {
                            var isLeaveExistsInTheList = reportModel.timeSheetLeaveOrHolidayInfo.Where(x => x.LeaveDateTime == isEmpAppliedLeaveonDateModel.leavedate).FirstOrDefault();

                            if (isLeaveExistsInTheList == null)
                            {
                                var leaveInfoModel = new ReportLeaveOrHolidayInfo()
                                {
                                    LeaveDate = isEmpAppliedLeaveonDateModel.leavedate.ToString(),
                                    LeaveType = isEmpAppliedLeaveonDateModel.leavesource,
                                    LeaveDateTime = isEmpAppliedLeaveonDateModel.leavedate
                                };
                                reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
                            }
                        }
                    }

                }

                //Passing Inputs to view
                reportModel.timeSheetAjaxInputReportModel = timeSheetAjaxReportModel;
                employeeReports.TimeSheetReports.Add(reportModel);
            }

            return RenderPartialToString(this, "TimeSheetDownloadReportsPartial", employeeReports, ViewData, TempData);
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
                mm.Subject = empModel.AMBC_Active_Emp_view.Employee_Name + " Timesheet Submission Details ";
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

                    var notSubmittedEmpList = new List<AMBC_Active_Emp_view>();

                    foreach (var projectSpecificEmployee in projectSpecificEmployees)
                    {
                        var isEmployeeSubmittedTimeSheet = db.ambctaskcaptures.Where(a => a.weekno == weekNumber && a.clientname == timeSheetAjaxReportModel.ClientName && a.employeeid == projectSpecificEmployee.Employee_ID).FirstOrDefault();

                        if (isEmployeeSubmittedTimeSheet == null)
                        {
                            notSubmittedEmpList.Add(projectSpecificEmployee);
                        }
                    }

                    //var submittedEmpList = db.ambctaskcaptures.Where(a => a.weekno == weekNumber && a.clientname == timeSheetAjaxReportModel.ClientName).ToList();

                    //var notSubmittedEmpList = submittedEmpList.Count() > 0 ? projectSpecificEmployees.Where(x => submittedEmpList.Where(y => y.employeeid != x.Employee_ID).FirstOrDefault() != null).ToList() : projectSpecificEmployees.ToList();

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

        public JsonResult RunSchedularJob()
        {
            var response = new JsonResponseModel();
            try
            {
                RemainderJobSchedular();
                response.Message = "Job executed succesfully";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException.Message;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public void RemainderJobSchedular()
        {
            var clientNames = new List<string>();
            clientNames.Add("Littelfuse");
            clientNames.Add("Federal Signal");
            clientNames.Add("Modine");
            clientNames.Add("AMBC");

            var lastweekDay = DateTime.Today.AddDays(-7);
            int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(lastweekDay, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            DateTime Firstday = lastweekDay.AddDays(-(int)lastweekDay.DayOfWeek).AddDays(1);
            DateTime Endaday = Firstday.AddDays(4);

            var currentMonth = FirstDayOfMonth();
            var currentMonthSf = MonthShortFormat(currentMonth);
            var formattedCurrentMonth = FormatDate(currentMonth.ToString());

            var lastMonth = FirstDayOfLastMonth();
            var lastMonthSf = MonthShortFormat(lastMonth);
            var formattedLastMonth = FormatDate(lastMonth.ToString());

            var timeSheetnotSubmittedEmpList = new List<AMBC_Active_Emp_view>();
            var statusReportnotSubmittedEmpList = new List<AMBC_Active_Emp_view>();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                foreach (var client in clientNames)
                {
                    var projectSpecificEmployees = db.AMBC_Active_Emp_view.Where(emp => emp.Client == client).ToList();
                    foreach (var projectSpecificEmployee in projectSpecificEmployees)
                    {
                        var isEmployeeSubmittedTimeSheet = db.ambctaskcaptures.Where(a => a.weekno == weekNumber && a.clientname == client && a.employeeid == projectSpecificEmployee.Employee_ID).FirstOrDefault();

                        if (isEmployeeSubmittedTimeSheet == null)
                        {
                            timeSheetnotSubmittedEmpList.Add(projectSpecificEmployee);
                        }

                        var currentEmpTemplate1Repots = db.monthlyreports_Template1.Where(a => a.EmplyeeID.Equals(projectSpecificEmployee.Employee_ID) && a.Uploaded_Month == lastMonthSf && a.Client_Name == projectSpecificEmployee.Client).ToList();
                        if (currentEmpTemplate1Repots != null && currentEmpTemplate1Repots.Count() > 0)
                        {
                            continue;
                        }
                        var currentEmpTemplate2Repots = db.monthlyreports_Template2.Where(a => a.EmplyeeID.Equals(projectSpecificEmployee.Employee_ID) && a.Uploaded_Month == lastMonthSf && a.Client_Name == projectSpecificEmployee.Client).ToList();
                        if (currentEmpTemplate1Repots != null && currentEmpTemplate1Repots.Count() > 0)
                        {
                            continue;
                        }

                        statusReportnotSubmittedEmpList.Add(projectSpecificEmployee);
                    }
                }
            }

            var remainderModel = new JobRemainderModel();
            remainderModel.EndDate = FormatDate(Endaday.ToString());
            remainderModel.StartDate = FormatDate(Firstday.ToString());
            remainderModel.CurrentMonth = formattedCurrentMonth;
            remainderModel.CurrentMonthDate = currentMonth;
            remainderModel.StartDateTime = Firstday;
            remainderModel.EndDateTime = Endaday;
            remainderModel.CurrentMonthShortFormat = currentMonthSf;
            remainderModel.PreviousMonthShortFormat = lastMonthSf;

            remainderModel.RemainderType = "TimeSheet";
            SchedularJobRemainderEmail(timeSheetnotSubmittedEmpList, remainderModel);

            remainderModel.RemainderType = "StatusReport";
            SchedularJobRemainderEmail(statusReportnotSubmittedEmpList, remainderModel);
        }

        public string FormatDate(string Date)
        {
            var actualDate = Date.ToString().Replace("00:00:00", "").Replace("12:00:00 AM", "").Trim();
            if (actualDate.Contains('-'))
            {
                return actualDate.Split('-')[1] + "-" + actualDate.Split('-')[0] + "-" + actualDate.Split('-')[2];
            }
            if (actualDate.Contains('/'))
            {
                return actualDate.Split('/')[1] + "-" + actualDate.Split('/')[0] + "-" + actualDate.Split('/')[2];
            }

            return actualDate;
        }

        public void SchedularJobRemainderEmail(List<AMBC_Active_Emp_view> employees, JobRemainderModel remainderModel)
        {
            try
            {
                foreach (var emp in employees)
                {
                    if ((remainderModel.RemainderType == "StatusReport" && emp.Client == "AMBC") || emp.Employee_Name == "seema")
                    {
                        continue;
                    }

                    //if(emp.Employee_ID != "C4046")
                    //{
                    //    continue;
                    //}
                    using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], emp.AMBC_Mail_Address))
                    {
                        if (remainderModel.RemainderType == "TimeSheet")
                        {
                            mm.Subject = "REMINDER: TimeSheet for the week - " + remainderModel.StartDate + " to " + remainderModel.EndDate;

                            var renainderEmailModel = new RMA_RemainderEmailSelectedEmpModel();
                            renainderEmailModel.selectedemployeeempname = emp.Employee_Name;
                            renainderEmailModel.selectedweekstartdate = remainderModel.StartDate;
                            renainderEmailModel.selectedweekenddate = remainderModel.EndDate;
                            renainderEmailModel.EmailType = "TimeSheet";
                            //var emailBody = RenderPartialToString(this, "RemainderEmail", renainderEmailModel, ViewData, TempData);
                            var emailBody = GenerateEmailBody(renainderEmailModel);
                            mm.Body = emailBody;
                        }

                        if (remainderModel.RemainderType == "StatusReport")
                        {
                            mm.Subject = "REMINDER: Dashboard Report for the month - " + remainderModel.PreviousMonthShortFormat;
                            var remainderStatusEmailModel = new RMA_RemainderEmailSelectedEmpModel();
                            remainderStatusEmailModel.SendSingleEmailToAllEmp = false;
                            remainderStatusEmailModel.selectedemployeeempname = emp.Employee_Name;
                            remainderStatusEmailModel.RemainderMonth = remainderModel.PreviousMonthShortFormat;
                            remainderStatusEmailModel.EmailType = "StatusReport";
                            //var emailBody = RenderPartialToString(this, "StatusReportRemainderEmail", remainderStatusEmailModel, ViewData, TempData);
                            var emailBody = GenerateEmailBody(remainderStatusEmailModel);
                            mm.Body = emailBody;
                        }

                        //TODO                     
                        if (!string.IsNullOrEmpty(emp.AMBC_PM_Mail_Address))
                        {
                            mm.CC.Add(emp.AMBC_PM_Mail_Address);
                        }

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
            }
            catch (Exception ex)
            {
            }
        }

        public JsonResult SendRemainderEmail(RMA_TimeSheetRemainderEmail timesheetEmpRemainder)
        {
            try
            {
                if (timesheetEmpRemainder != null && timesheetEmpRemainder.selctedempmodel.Count() > 0)
                {
                    var weekStartDate = timesheetEmpRemainder.weekstartdate.Split('-');
                    var formattedStartDate = weekStartDate[2] + "-" + weekStartDate[1] + "-" + weekStartDate[0];

                    var weekEndDate = timesheetEmpRemainder.weekenddate.Split('-');
                    var formattedEndDate = weekEndDate[2] + "-" + weekEndDate[1] + "-" + weekEndDate[0];


                    if (timesheetEmpRemainder.SendSingleEmailToAllEmp == false)
                    {
                        foreach (var email in timesheetEmpRemainder.selctedempmodel)
                        {
                            using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], email.selectedemployeeemail))
                            {
                                mm.Subject = "REMINDER: TimeSheet for the week - " + formattedStartDate + " to " + formattedEndDate;
                                email.selectedweekstartdate = formattedStartDate;
                                email.selectedweekenddate = formattedEndDate;


                                var emailBody = RenderPartialToString(this, "RemainderEmail", email, ViewData, TempData);

                                mm.Body = emailBody;

                                if (email.selectedemploymanageremail != string.Empty)
                                {
                                    mm.CC.Add(email.selectedemploymanageremail);
                                }

                                if (timesheetEmpRemainder.LogedInEmpEmail != string.Empty)
                                {
                                    mm.CC.Add(timesheetEmpRemainder.LogedInEmpEmail);
                                }

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
                    }


                    else
                    {
                        var model = new RMA_RemainderEmailSelectedEmpModel();
                        using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], timesheetEmpRemainder.selctedempmodel[0].selectedemployeeemail))
                        {
                            mm.Subject = "REMINDER: TimeSheet for the week - " + formattedStartDate + " to " + formattedEndDate;
                            int firstSelectedEmp = 0;

                            foreach (var selectedEmp in timesheetEmpRemainder.selctedempmodel)
                            {
                                if (firstSelectedEmp > 0)
                                {
                                    mm.To.Add(selectedEmp.selectedemployeeemail);
                                }
                                firstSelectedEmp++;

                                mm.CC.Add(selectedEmp.selectedemploymanageremail);
                            }

                            model.selectedweekstartdate = formattedStartDate;
                            model.selectedweekenddate = formattedEndDate;
                            model.SendSingleEmailToAllEmp = timesheetEmpRemainder.SendSingleEmailToAllEmp;

                            var emailBody = RenderPartialToString(this, "RemainderEmail", model, ViewData, TempData);

                            mm.Body = emailBody;

                            if (timesheetEmpRemainder.LogedInEmpEmail != string.Empty)
                            {
                                mm.CC.Add(timesheetEmpRemainder.LogedInEmpEmail);
                            }

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

                }

                var emailRemainderResponse = new JsonResponseModel();

                emailRemainderResponse.StatusCode = 200;
                emailRemainderResponse.Message = "Remainder Email Sent Successfully!";
                return Json(emailRemainderResponse);

            }

            catch (Exception ex)
            {

            }

            return Json(null);

        }

        public ActionResult ApplyLeave()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }

        public ActionResult LeaveReport()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }


        public DateTime[] GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> allDates = new List<DateTime>();

            if (endDate == DateTime.MinValue)
            {
                allDates.Add(startDate);
                return allDates.ToArray();
            }

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                allDates.Add(date);
            return allDates.ToArray();
        }

        public JsonResult SubmitLeaves(RMA_LeaveModel leaveModel)
        {
            var respone = new LeaveEmailModel();
            try
            {
                if (leaveModel != null)
                {
                    using (var context = new TimeSheetEntities())
                    {
                        var contextModelList = new List<con_leaveupdate>();

                        var startDate = System.Convert.ToDateTime(leaveModel.StartDate);
                        var endDate = leaveModel.EndDate != null ? System.Convert.ToDateTime(leaveModel.EndDate) : DateTime.MinValue;

                        var leaveApplieddates = GetDatesBetween(startDate, endDate);

                        if (leaveModel.LeaveType == "Cancel Leave")
                        {
                            foreach (var leaveDate in leaveApplieddates)
                            {
                                var itemToRemove = context.con_leaveupdate.SingleOrDefault(x => x.leavedate == leaveDate && x.employee_id == leaveModel.SelectedEmpId); //returns a single item.

                                if (itemToRemove != null)
                                {
                                    context.con_leaveupdate.Remove(itemToRemove);
                                    context.SaveChanges();
                                    respone.jsonResponse.Message = "Leave Deleted Successfully!";
                                }
                            }
                        }
                        else
                        {
                            foreach (var leaveDate in leaveApplieddates)
                            {
                                string dayName = leaveDate.ToString("dddd");

                                if (dayName != "Saturday" && dayName != "Sunday")
                                {
                                    contextModelList.Add(new con_leaveupdate()
                                    {
                                        employee_id = leaveModel.SelectedEmpId,
                                        employee_name = leaveModel.SelectedEmpName,
                                        leavedate = System.Convert.ToDateTime(leaveDate.ToString("yyyy-MM-dd")),
                                        leavesource = leaveModel.LeaveType,
                                        leavecategory = leaveModel.LeaveCategory,
                                        leave_reason = leaveModel.Reason,
                                        submittedby = leaveModel.SubmittedBy,
                                        leaveuniqkey = leaveModel.SelectedEmpId + "_" + leaveDate.ToString("yyyy-MM-dd")
                                    });
                                }
                            }

                            context.con_leaveupdate.AddRange(contextModelList);
                            context.SaveChanges();
                        }
                    }

                    var leaveEmailInfo = SubmitLeavesEmailGenerate(leaveModel);

                    if (leaveEmailInfo.EmailSent)
                    {
                        respone.jsonResponse.StatusCode = 200;
                        respone.jsonResponse.Message = "Leave Apply Email Sent Successfully!";
                    }
                    else
                    {
                        respone.jsonResponse.StatusCode = 201;
                        respone.jsonResponse.Message = "Leave Apply Email Sent failed to send!";
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


        public LeaveEmailModel SubmitLeavesEmailGenerate(RMA_LeaveModel emailLeaveModel)
        {
            var respone = new LeaveEmailModel();
            try
            {
                if (emailLeaveModel != null)
                {
                    using (TimeSheetEntities db = new TimeSheetEntities())
                    {
                        var empDetails = db.AMBC_Active_Emp_view.Where(emp => emp.Employee_ID == emailLeaveModel.SelectedEmpId).FirstOrDefault();
                        var leaveSubmissionType = string.Empty;

                        emailLeaveModel.SubmissionType = emailLeaveModel.SelectedEmpId == emailLeaveModel.LogedInEmpId ? "own" : "others";

                        var subject = string.Empty;
                        var emailBody = RenderPartialToString(this, "LeaveEmail", emailLeaveModel, ViewData, TempData);

                        if (empDetails != null)
                        {
                            using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], empDetails.AMBC_Mail_Address))
                            {
                                mm.Subject = emailLeaveModel.LeaveType == "Cancel Leave" ? "Leave Cancelled Update!" : "Leave Submission Update!";
                                mm.Body = emailBody;

                                if (empDetails.AMBC_PM_Mail_Address != "")
                                {
                                    mm.CC.Add(empDetails.AMBC_PM_Mail_Address);
                                }

                                if (emailLeaveModel.LogedInEmpEmail != "" && (emailLeaveModel.LogedInEmpEmail != empDetails.AMBC_Mail_Address))
                                {
                                    mm.CC.Add(emailLeaveModel.LogedInEmpEmail);
                                }

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
                                respone.EmailSent = true;
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                respone.EmailSent = false;
            }

            return respone;
        }


        public JsonResult CheckeLeaveAppliiedStatus(List<RMA_LeaveOrHolidayInfo> leaveOrholidayModel)
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            var respone = new ApplyLeaveForMissedSidnInModel();
            if (leaveOrholidayModel != null)
            {
                using (TimeSheetEntities db = new TimeSheetEntities())
                {
                    foreach (var holidayInfo in leaveOrholidayModel)
                    {
                        if (!string.IsNullOrWhiteSpace(holidayInfo.LeaveOrHolidayDate))
                        {

                            var modeldate = System.Convert.ToDateTime(holidayInfo.LeaveOrHolidayDate);

                            var dayName = modeldate.ToString("dddd");

                            if (dayName == "Saturday" || dayName == "Sunday")
                                continue;

                            var dateFallunderHoliday = db.tblambcholidays.Where(b => b.holiday_date == modeldate && b.region == employeeModel.AMBC_Active_Emp_view.Location).FirstOrDefault();

                            if (dateFallunderHoliday != null)
                                continue;

                            var isEmpAppliedLeaveonDateModel = db.con_leaveupdate.Where(leave => leave.employee_id == employeeModel.AMBC_Active_Emp_view.Employee_ID && leave.leavedate == modeldate).FirstOrDefault();

                            if (isEmpAppliedLeaveonDateModel != null)
                                continue;

                            var actualDate = holidayInfo.LeaveOrHolidayDate.Split('-');

                            var displayDate = actualDate[2] + "-" + actualDate[1] + "-" + actualDate[0];
                            respone.MissingLeaveDates.Add(displayDate, dayName);
                            respone.jsonResponse.StatusCode = 404;
                            respone.jsonResponse.Message = "Employee Not applied leaves on missed Chck-In dates!";
                        }

                    }

                }
            }

            return Json(respone, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckinAdjust()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }


        public JsonResult SubmitAdjustments(RMA_LeaveModel adjustModel)
        {
            var respone = new LeaveEmailModel();
            try
            {
                if (adjustModel != null)
                {
                    var SystemInfo = SystemInformation();
                    using (var context = new TimeSheetEntities())
                    {
                        var employeeModel = context.AMBC_Active_Emp_view.Where(x => x.Employee_ID == adjustModel.SelectedEmpId && x.Project_Status == "Active").FirstOrDefault();

                        var signIndate = System.Convert.ToDateTime(adjustModel.StartDate);
                        var SignInTime = signIndate.AddHours(9);
                        var SignOutTime = signIndate.AddHours(18);

                        var ambcEmpLoginInfo = new tbld_ambclogininformation()
                        {
                            Employee_Code = employeeModel.Employee_ID,
                            Employee_Name = employeeModel.Employee_Name,
                            Employee_Designation = employeeModel.Designation,
                            Employee_Shift = employeeModel.Shift,
                            Login_date = signIndate,
                            Signin_Time = SignInTime,
                            Signout_Time = SignOutTime,
                            Employee_Hostname = SystemInfo.SystemHostName,
                            Employee_IP = SystemInfo.SystemIP,
                            Employee_LoginLocation = employeeModel.Location,
                            Concat_loginstring = employeeModel.Employee_ID + "_" + signIndate.ToString("yyyy-MM-dd")
                        };

                        context.tbld_ambclogininformation.Add(ambcEmpLoginInfo);
                        context.SaveChanges();
                        respone.jsonResponse.StatusCode = 200;
                        respone.jsonResponse.Message = "Checked in Successful!";
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

        [HttpGet]
        public ActionResult zohosigninupdate()
        {

            return View();
        }

        [HttpPost]
        public ActionResult zohosigninupdate(ZohoSignInModel membervalues)
        {
            var model = new ZohoSignInModel();
            try
            {
                if (membervalues != null && membervalues.ImageFile != null)
                {
                    string FileName = Path.GetFileNameWithoutExtension(membervalues.ImageFile.FileName);

                    HttpPostedFileBase file = membervalues.ImageFile;
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, System.Convert.ToInt32(file.ContentLength));

                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;

                            var adjustmentEmployeeIds = new List<string>();

                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                adjustmentEmployeeIds.Add(workSheet.Cells[rowIterator, 2].Value.ToString());
                            }

                            var SystemInfo = SystemInformation();
                            using (var context = new TimeSheetEntities())
                            {
                                var adjustmentEmpList = new List<tbld_ambclogininformation>();
                                foreach (var adjustmentEmployeeId in adjustmentEmployeeIds)
                                {
                                    var employeeModel = context.AMBC_Active_Emp_view.Where(x => x.Employee_ID == adjustmentEmployeeId && x.Project_Status == "Active").FirstOrDefault();

                                    if (employeeModel == null)
                                        continue;

                                    var signIndate = System.Convert.ToDateTime(membervalues.AdjustmentDate);
                                    var SignInTime = signIndate.AddHours(9);
                                    var SignOutTime = signIndate.AddHours(18);

                                    var ambcEmpLoginInfo = new tbld_ambclogininformation()
                                    {
                                        Employee_Code = employeeModel.Employee_ID,
                                        Employee_Name = employeeModel.Employee_Name,
                                        Employee_Designation = employeeModel.Designation,
                                        Employee_Shift = employeeModel.Shift,
                                        Login_date = signIndate,
                                        Signin_Time = SignInTime,
                                        Signout_Time = SignOutTime,
                                        Employee_Hostname = SystemInfo.SystemHostName,
                                        Employee_IP = SystemInfo.SystemIP,
                                        Employee_LoginLocation = employeeModel.Location,
                                        Concat_loginstring = employeeModel.Employee_ID + "_" + signIndate.ToString("yyyy-MM-dd")
                                    };

                                    adjustmentEmpList.Add(ambcEmpLoginInfo);
                                }

                                context.tbld_ambclogininformation.AddRange(adjustmentEmpList);
                                context.SaveChanges();
                                model.SuccessMessage = "Successfully added check-in for all the consultants who are exists in the sheet";
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                model.FailureMessage = "Error occured when adding check-in. Please contact admin.";
            }

            return View(model);
        }

        public FileResult zohosigninreport(ZohoSignInModel modelData)
        {
            var reportModel = new ZohoSignInExcelReportModel();
            using (var context = new TimeSheetEntities())
            {
                var results = context.tbld_ambclogininformation.Where(b => b.Login_date == modelData.AdjustmentDate).ToList();
                if (results != null)
                {
                    int sno = 1;
                    reportModel.Reports = new List<ZohoSignInReportModel>();
                    foreach (var result in results)
                    {
                        reportModel.Reports.Add(new ZohoSignInReportModel()
                        {
                            SNo = sno.ToString(),
                            EmployeeCode = result.Employee_Code,
                            EmplyeeName = result.Employee_Name,
                            ReportDate = result.Login_date.ToString("dd-MM-yyyy")
                        });

                        sno++;
                    }
                }
            }

            var fileName = "CheckIn Report " + modelData.AdjustmentDate.ToString("dd-MM-yyyy");

            var reportHtml = RenderPartialToString(this, "zohosigninreport", reportModel, ViewData, TempData);
            return File(Encoding.ASCII.GetBytes(reportHtml), "application/vnd.ms-excel", fileName + ".xls");
        }


        public JsonResult ClientBasedEmpDetails(RMA_ClientBasedEmpModel ClientBasedEmpModel)
        {
            var ClientBasedemployeeModel = new RMA_ClientBasedEmpJson();
            try
            {

                using (TimeSheetEntities db = new TimeSheetEntities())
                {
                    var empProjectCode = System.Convert.ToInt32(ClientBasedEmpModel.ProjectID);
                    var clientBasedEmpInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID == ClientBasedEmpModel.EmpId && a.Client == ClientBasedEmpModel.ClientName && a.Project_Code == empProjectCode).FirstOrDefault();
                    if (clientBasedEmpInfo != null)
                    {
                        ClientBasedemployeeModel.ClientBasedAMBCEmp = clientBasedEmpInfo;
                        ClientBasedemployeeModel.jsonResponse.StatusCode = 200;
                        ClientBasedemployeeModel.jsonResponse.Message = "Clinet based employee details found";
                    }
                }

                return Json(ClientBasedemployeeModel);
            }
            catch (Exception ex)
            {
                return Json(ClientBasedemployeeModel);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExportLeaveReport(string GridHtml)
        {
            RMA_LeaveModel leaveReportModel = JsonConvert.DeserializeObject<RMA_LeaveModel>(GridHtml);
            var reportHtml = GetLeaveInfo(leaveReportModel);
            var fileName = "Leave Report ";
            return File(Encoding.ASCII.GetBytes(reportHtml.Data.ToString()), "application/vnd.ms-excel", fileName + ".xls");
        }

        public JsonResult GetLeaveInfo(RMA_LeaveModel leaveReportModel)
        {
            try
            {
                var leaveModel = new LeaveInfoModel();
                var leaveHtmlData = string.Empty;
                using (TimeSheetEntities db = new TimeSheetEntities())
                {
                    var startDate = DateTime.Parse(leaveReportModel.StartDate);
                    var endDate = DateTime.Parse(leaveReportModel.EndDate);

                    if (leaveReportModel.SelectedEmpId == "All Employees")
                    {
                        leaveModel.leaveDetails = db.con_leaveupdate.Where(a => a.leavedate >= startDate && a.leavedate <= endDate).ToList();
                    }
                    else
                    {
                        leaveModel.leaveDetails = db.con_leaveupdate.Where(a => a.leavedate >= startDate && a.leavedate <= endDate && a.employee_id == leaveReportModel.SelectedEmpId).ToList();
                    }

                    if (leaveModel.leaveDetails != null && leaveModel.leaveDetails.Count > 0)
                    {
                        var leavesBasedOnEmpId = leaveModel.leaveDetails.OrderBy(x => x.employee_id).ToList();
                        leaveModel.leaveDetails = leavesBasedOnEmpId;
                        leaveModel.jsonResponse.StatusCode = 200;
                        leaveModel.jsonResponse.Message = "Leave Details Found for the selected inputs";
                    }
                    else
                    {
                        leaveModel.jsonResponse.StatusCode = 404;
                        leaveModel.jsonResponse.Message = "Leave Details not Found for the selected inputs";
                    }


                    leaveHtmlData = RenderPartialToString(this, "LeaveInfoPartial", leaveModel, ViewData, TempData);
                }

                return Json(leaveHtmlData);
            }
            catch (Exception ex)
            {
                return Json(null);
            }
        }

        //Status REPORt Upload Code
        public ActionResult StatusReportUpload()
        {
            var model = new RMA_StatusReportModel();

            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            model.RMA_EmployeeModel = employeeModel;

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var employeesDetails = db.AMBC_Active_Emp_view.Where(x => x.Project_Status == "Active").ToList();

                if (employeesDetails != null && employeesDetails.Count > 0)
                {
                    foreach (var employeesDetail in employeesDetails)
                    {
                        model.StatusReportInfo.EmployeeList.Add(new SelectListItem()
                        {
                            Text = employeesDetail.Employee_Name,
                            Value = employeesDetail.Employee_ID
                        });
                    }

                }

                var monthsList = new List<DateTime>();
                monthsList.Add(DateTime.Now.AddMonths(-1));
                monthsList.Add(DateTime.Now.AddMonths(-2));
                monthsList.Add(DateTime.Now.AddMonths(-3));
                monthsList.Add(DateTime.Now.AddMonths(-4));
                monthsList.Add(DateTime.Now.AddMonths(-5));

                foreach (var month in monthsList)
                {
                    model.StatusReportInfo.MonthList.Add(new SelectListItem()
                    {
                        Text = month.ToString("MMM") + "-" + month.Year,
                        Value = month.ToString("MMM") + "-" + month.Year
                    });
                }

            }

            return View(model);
        }


        public JsonResult StatusReportsUploadAjax(StatusReportModel fileData)
        {
            var model = new RMA_StatusReportModel();
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            model.RMA_EmployeeModel = employeeModel;

            HttpPostedFileBase file = fileData.ExcelFile;

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    var indexList = new List<FieldsIndex>();
                    indexList = JsonConvert.DeserializeObject<List<FieldsIndex>>(fileData.FieldsIndexJson);

                    switch (fileData.TemplateType)
                    {
                        case "Template1":
                            Template1Updates(fileData, file, workSheet, noOfRow, indexList, model);
                            break;

                        case "Template2":
                            //Template2Updates(fileData, file, workSheet, noOfRow, indexList, model);
                            break;

                        case "Template3":
                            fileData.IsAuditReport = true;
                            Template1Updates(fileData, file, workSheet, noOfRow, indexList, model);
                            //Template3Updates(fileData, file, workSheet, noOfRow, indexList, model);
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(fileData.ProjectList))
                {
                    Template2Updates(fileData, model);
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public static double GetBusinessDays(DateTime startD, DateTime endD)
        {
            double calcBusinessDays =
                1 + ((endD - startD).TotalDays * 5 -
                (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;

            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            return calcBusinessDays;
        }


        public static DateTime FirstDayOfMonth()
        {
            var date = DateTime.Now;
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime FirstDayOfLastMonth()
        {
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            return month.AddMonths(-1);
        }


        private static void Template1Updates(StatusReportModel fileData, HttpPostedFileBase file, ExcelWorksheet workSheet, int noOfRow, List<FieldsIndex> indexList, RMA_StatusReportModel model)
        {
            try
            {
                //Ticket_Prioriy & Ticket& Status
                var MappingValuesList = new List<FieldsIndex>();
                MappingValuesList = JsonConvert.DeserializeObject<List<FieldsIndex>>(fileData.ValuesMappingJson);

                int Ticket_NumberIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Number").FirstOrDefault().Index);
                //int Ticket_SummaryIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Summary").FirstOrDefault().Index);

                var Ticket_Created_DateIndex = indexList.Where(x => x.FieldName == "Ticket_Priority").FirstOrDefault() != null ? System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Created_Date").FirstOrDefault().Index) : 0;
                var Ticket_CategoryIndex = fileData.IsAuditReport == false ? System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Category").FirstOrDefault().Index) : 0;

                //var Ticket_RaisedbyIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Raisedby").FirstOrDefault().Index);
                var Ticket_PriorityIndex = indexList.Where(x => x.FieldName == "Ticket_Priority").FirstOrDefault() != null ? System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Priority").FirstOrDefault().Index) : 0;

                var Ticket_StatusIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Status").FirstOrDefault().Index);
                var Ticket_Closed_DateIndex = indexList.Where(x => x.FieldName == "Ticket_Closed_Date").FirstOrDefault() != null ? System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Ticket_Closed_Date").FirstOrDefault().Index) : 0;

                //var OrganisationIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Organisation").FirstOrDefault().Index);
                //var CommentsIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Comments").FirstOrDefault().Index);

                var reportModel = new StatusReport_Template1Model();

                var currentDateStartDate = FirstDayOfMonth();
                var validationErrors = false;

                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                {
                    //CHECK IF THE REPORT CONTAINS OLD MONTH DATA ONLY
                    //CHECKING WITH CREATED DATE AND CLOSED DATE VALUES
                    var createdDateValue = Ticket_Created_DateIndex != 0 && workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value != null && !string.IsNullOrWhiteSpace(workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value.ToString()) ? System.Convert.ToDateTime(workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value.ToString()) : DateTime.MinValue;
                    if (createdDateValue > currentDateStartDate)
                    {
                        validationErrors = true;
                        model.Response.StatusCode = 400;
                        model.Response.Message = "Looks like in the uploaded report Created Date field <br>is having future date's like.. (" + createdDateValue + ").<br> You are not allowed to upload future date records";
                        break;
                    }

                    var ClosedDateValue = Ticket_Closed_DateIndex != 0 && workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value != null && !string.IsNullOrWhiteSpace(workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value.ToString()) ? System.Convert.ToDateTime(workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value.ToString()) : DateTime.MinValue;
                    if (ClosedDateValue > currentDateStartDate)
                    {
                        validationErrors = true;
                        model.Response.StatusCode = 400;
                        model.Response.Message = "Looks like in the uploaded report Closed Date field <br>is having future date's like.. (" + ClosedDateValue + ").<br> You are not allowed to upload future date records";
                        break;
                    }

                    //UNIQUE KEY VALUES CONDITION TO BE CHECKED HERE
                    if (workSheet.Cells[rowIterator, Ticket_NumberIndex].Value != null)
                    {
                        //This logic for report graphs
                        var reportTicketPriority = "";
                        if (Ticket_PriorityIndex != 0 && workSheet.Cells[rowIterator, Ticket_PriorityIndex].Value != null)
                        {
                            var rowIteratorPriority = workSheet.Cells[rowIterator, Ticket_PriorityIndex].Value.ToString();
                            var mappingListItem = MappingValuesList.Where(x => x.Index == rowIteratorPriority).FirstOrDefault();
                            if (mappingListItem != null)
                            {
                                reportTicketPriority = mappingListItem.FieldName.Trim();
                            }
                        }
                        else
                        {
                            reportTicketPriority = "Low";
                        }

                        bool IsOpenTicket = false;
                        bool IsClosedTicket = false;
                        bool IsTODOTicket = false;
                        bool IsCancelledTicket = false;
                        if (workSheet.Cells[rowIterator, Ticket_StatusIndex].Value != null)
                        {
                            var rowIteratorStatus = workSheet.Cells[rowIterator, Ticket_StatusIndex].Value.ToString();
                            var mappingClosedListItem = MappingValuesList.Where(x => x.FieldName == "Closed" && x.Index.Contains(rowIteratorStatus)).FirstOrDefault();
                            if (mappingClosedListItem != null)
                            {
                                rowIteratorStatus = mappingClosedListItem.FieldName.Trim();
                                IsClosedTicket = true;
                            }
                            else
                            {
                                var mappingTODOListItem = MappingValuesList.Where(x => x.FieldName == "TODO" && x.Index.Contains(rowIteratorStatus)).FirstOrDefault();
                                if (mappingTODOListItem != null)
                                {
                                    IsTODOTicket = true;
                                }

                                var mappingCancelledListItem = MappingValuesList.Where(x => x.FieldName == "Cancelled" && x.Index.Contains(rowIteratorStatus)).FirstOrDefault();
                                if (mappingCancelledListItem != null)
                                {
                                    IsCancelledTicket = true;
                                }

                                if (!IsCancelledTicket)
                                {
                                    IsOpenTicket = true;
                                }
                            }
                        }

                        bool IsNewTicket = false;
                        var ticketAge = 0;
                        var closedYear = 0;
                        var closedMonth = 0;
                        var createdYear = 0;
                        var createdMonth = 0;

                        if (Ticket_Created_DateIndex != 0 && workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value != null && !string.IsNullOrWhiteSpace(workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value.ToString()))
                        {
                            var ticketDateCreated = System.Convert.ToDateTime(workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value.ToString());
                            var ticketMonthYear = ticketDateCreated.ToString("MMM") + "-" + ticketDateCreated.Year;
                            createdYear = ticketDateCreated.Year;
                            createdMonth = ticketDateCreated.Month;

                            if (fileData.Month == ticketMonthYear)
                            {
                                IsNewTicket = true;
                            }

                            if (IsClosedTicket)
                            {
                                if (Ticket_Closed_DateIndex != 0 && workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value != null && !string.IsNullOrWhiteSpace(workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value.ToString()))
                                {
                                    var ticketDateClosed = System.Convert.ToDateTime(workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value.ToString());
                                    ticketAge = System.Convert.ToInt32(GetBusinessDays(ticketDateCreated, ticketDateClosed));

                                    closedYear = ticketDateClosed.Year;
                                    closedMonth = ticketDateClosed.Month;
                                }
                                else
                                {
                                    ticketAge = 5;
                                }
                            }
                        }

                        reportModel.Template1Reports.Add(new monthlyreports_Template1()
                        {
                            Ticket_Number = workSheet.Cells[rowIterator, Ticket_NumberIndex].Value != null ? workSheet.Cells[rowIterator, Ticket_NumberIndex].Value.ToString() : "",
                            //Ticket_Summary = workSheet.Cells[rowIterator, Ticket_SummaryIndex].Value != null ? workSheet.Cells[rowIterator, Ticket_SummaryIndex].Value.ToString() : "",
                            //Ticket_Summary = fileData.ToolName,
                            Ticket_Created_Date = Ticket_Created_DateIndex != 0 && workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value != null && !string.IsNullOrWhiteSpace(workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value.ToString()) ? System.Convert.ToDateTime(workSheet.Cells[rowIterator, Ticket_Created_DateIndex].Value.ToString()) : DateTime.MinValue,
                            Ticket_Category = fileData.IsAuditReport == false && Ticket_CategoryIndex != 0 && workSheet.Cells[rowIterator, Ticket_CategoryIndex].Value != null ? workSheet.Cells[rowIterator, Ticket_CategoryIndex].Value.ToString() : "",
                            Ticket_Priority = Ticket_PriorityIndex != 0 && workSheet.Cells[rowIterator, Ticket_PriorityIndex].Value != null ? workSheet.Cells[rowIterator, Ticket_PriorityIndex].Value.ToString() : "",
                            //Ticket_Raisedby = workSheet.Cells[rowIterator, Ticket_RaisedbyIndex].Value != null ? workSheet.Cells[rowIterator, Ticket_RaisedbyIndex].Value.ToString() : "",
                            Ticket_Status = workSheet.Cells[rowIterator, Ticket_StatusIndex].Value != null ? workSheet.Cells[rowIterator, Ticket_StatusIndex].Value.ToString() : "",
                            Ticket_Closed_Date = Ticket_Closed_DateIndex != 0 && workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value != null && !string.IsNullOrWhiteSpace(workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value.ToString()) ? System.Convert.ToDateTime(workSheet.Cells[rowIterator, Ticket_Closed_DateIndex].Value.ToString()) : DateTime.MinValue,
                            //Organisation = workSheet.Cells[rowIterator, OrganisationIndex].Value != null ? workSheet.Cells[rowIterator, OrganisationIndex].Value.ToString() : "",
                            //Comments = workSheet.Cells[rowIterator, CommentsIndex].Value != null ? workSheet.Cells[rowIterator, CommentsIndex].Value.ToString() : "",
                            Ticket_Raisedby = "DELETE",
                            Uploadedby = fileData.Uploadedby,
                            FileNamee = file.FileName,
                            Consultant_Name = fileData.EmployeeName,
                            Uploaded_Month = fileData.Month,
                            Client_Name = fileData.ClientName,
                            Is_Closed = IsClosedTicket,
                            Is_Open = IsOpenTicket,
                            Is_Newly_created = IsNewTicket,
                            ReportPriority = reportTicketPriority,
                            Ticket_Age = ticketAge,
                            Closed_Month = closedMonth,
                            Closed_Year = closedYear,
                            Created_Month = createdMonth,
                            Created_Year = createdYear,
                            Is_Cancelled = IsCancelledTicket,
                            Is_ToDo = IsTODOTicket,
                            EmplyeeID = fileData.EmployeeID,
                            ProjectID = System.Convert.ToInt32(fileData.ProjectID),
                            IsAuditReport = fileData.IsAuditReport,
                            TicketingToolName = fileData.ToolName,
                            Uniquekey = fileData.EmployeeID + "_" + workSheet.Cells[rowIterator, Ticket_NumberIndex].Value.ToString() + "_" + fileData.Month + "_" + fileData.ProjectID
                        }); ;
                    }
                }

                using (var context = new TimeSheetEntities())
                {
                    if (!validationErrors)
                    {
                        context.monthlyreports_Template1.AddRange(reportModel.Template1Reports);
                        context.SaveChanges();
                        model.Response.StatusCode = 200;
                        model.Response.Message = "Status Report Uploaded Successfully!";
                    }

                }
            }
            catch (Exception ex)
            {
                StatusReportReponse(model, ex);
            }
        }

        private static void StatusReportReponse(RMA_StatusReportModel model, Exception ex)
        {
            model.Response.StatusCode = 500;
            if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
            {
                var actuallErrors = ex.InnerException.InnerException.Message.Split('.');

                foreach (var actuallError in actuallErrors)
                {
                    if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                    {
                        model.Response.Message = actuallError;
                    }
                }
            }
            else
            {
                model.Response.Message = ex.Message;
            }
        }

        private static void Template2Updates(StatusReportModel fileData, RMA_StatusReportModel model)
        {
            try
            {

                //Ticket_Prioriy & Ticket& Status
                var projectList = new List<ProjectModel>();
                projectList = JsonConvert.DeserializeObject<List<ProjectModel>>(fileData.ProjectList);

                var reportModel = new StatusReport_Template2Model();

                foreach (var project in projectList)
                {

                    //PRIORITY LOGIC
                    var projectPriority = "";
                    if (!string.IsNullOrWhiteSpace(project.Project_Priority))
                    {
                        projectPriority = project.Project_Priority;
                    }
                    else
                    {
                        projectPriority = "Low";
                    }

                    bool IsOpenProject = false;
                    bool IsClosedProject = false;
                    bool IsTODOProject = false;
                    bool IsCancelledProject = false;

                    var defaultStatus = "default";

                    if (!string.IsNullOrWhiteSpace(project.Project_Status))
                    {
                        if (project.Project_Status == "Closed")
                        {
                            IsClosedProject = true;
                        }
                        else
                        {
                            if (project.Project_Status == "TODO")
                            {
                                IsTODOProject = true;
                            }

                            if (project.Project_Status == "Cancelled")
                            {
                                IsCancelledProject = true;
                            }

                            if (!IsCancelledProject)
                            {
                                IsOpenProject = true;
                            }
                        }
                    }
                    else
                    {
                        IsOpenProject = true;
                    }

                    var closedYear = 0;
                    var closedMonth = 0;
                    var createdYear = 0;
                    var createdMonth = 0;

                    if (project.Project_Created_Date != DateTime.MinValue)
                    {
                        var ticketDateCreated = System.Convert.ToDateTime(project.Project_Created_Date);
                        var ticketMonthYear = ticketDateCreated.ToString("MMM") + "-" + ticketDateCreated.Year;
                        createdYear = ticketDateCreated.Year;
                        createdMonth = ticketDateCreated.Month;
                    }

                    if (project.Project_Created_Date != DateTime.MinValue)
                    {
                        var ticketDateClosed = System.Convert.ToDateTime(project.Project_Created_Date);
                        closedYear = ticketDateClosed.Year;
                        closedMonth = ticketDateClosed.Month;
                    }

                    reportModel.Template2Reports.Add(new monthlyreports_Template2()
                    {
                        Project_Closed_Date_Actual = project.Project_Closed_Date_Actual,
                        Project_Closing_Date_Target = project.Project_Closing_Date_Target,
                        Project_Created_Date = project.Project_Created_Date,
                        Project_Name = project.Project_Name,
                        Project_Summary = project.Project_Summary,
                        Project_Priority = project.Project_Priority,
                        Project_Status = project.Project_Status,
                        Project_Category = fileData.ProjectCategory,

                        CompletedPercentage = project.CompletedPercentage,
                        RemainingPercentage = project.RemainingPercentage,

                        Uploadedby = fileData.Uploadedby,
                        FileNamee = "NA",
                        ConsultantName = fileData.EmployeeName,
                        Uploaded_Month = fileData.Month,
                        ProjectID = System.Convert.ToInt32(fileData.ProjectID),
                        EmplyeeID = fileData.EmployeeID,
                        Is_Cancelled = IsCancelledProject,
                        Is_Closed = IsClosedProject,
                        Is_Open = IsOpenProject,
                        Is_ToDo = IsTODOProject,

                        Created_Month = createdMonth,
                        Created_Year = createdYear,
                        Closed_Month = closedMonth,
                        Closed_Year = closedYear,
                        Project_Raisedby = "NA",
                        Client_Name = fileData.ClientName,

                        //NEED TO DECIDE
                        uniquekey = fileData.EmployeeID + "_" + project.Project_Name + "_" + fileData.Month + "_" + fileData.ProjectID

                    });
                }

                using (var context = new TimeSheetEntities())
                {
                    context.monthlyreports_Template2.AddRange(reportModel.Template2Reports);
                    context.SaveChanges();
                    model.Response.StatusCode = 200;
                    model.Response.Message = "Status Report Uploaded Successfully!";
                }
            }
            catch (Exception ex)
            {
                StatusReportReponse(model, ex);
            }
        }

        private static void Template3Updates(StatusReportModel fileData, HttpPostedFileBase file, ExcelWorksheet workSheet, int noOfRow, List<FieldsIndex> indexList, RMA_StatusReportModel model)
        {
            try
            {
                int Test_IDIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Test_ID").FirstOrDefault().Index);
                int Control_Group_NameIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Control_Group_Name").FirstOrDefault().Index);

                var Test_CompletedIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Test_Completed").FirstOrDefault().Index);
                var Test_Due_DateIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Test_Due_Date").FirstOrDefault().Index);

                var Test_Completion_DateIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Test_Completion_Date").FirstOrDefault().Index);
                var Test_ConclusionIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Test_Conclusion").FirstOrDefault().Index);

                var Control_OwnerIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Control_Owner").FirstOrDefault().Index);
                var Control_PerformerIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Control_Performer").FirstOrDefault().Index);

                var Test_applicationIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Test_application").FirstOrDefault().Index);
                var OrganisationIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Organisation").FirstOrDefault().Index);

                var LayerIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Layer").FirstOrDefault().Index);
                var FrequencyIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Frequency").FirstOrDefault().Index);

                var Current_stepIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Current_step").FirstOrDefault().Index);
                var CommentsIndex = System.Convert.ToInt32(indexList.Where(x => x.FieldName == "Comments").FirstOrDefault().Index);

                var reportModel = new StatusReport_Template3Model();

                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                {
                    if (workSheet.Cells[rowIterator, Test_IDIndex].Value != null)
                    {
                        reportModel.Template3Reports.Add(new monthlyreports_Template3()
                        {
                            Control_Group_Name = workSheet.Cells[rowIterator, Control_Group_NameIndex].Value != null ? workSheet.Cells[rowIterator, Control_Group_NameIndex].Value.ToString() : "",
                            Control_Owner = workSheet.Cells[rowIterator, Control_OwnerIndex].Value != null ? workSheet.Cells[rowIterator, Control_OwnerIndex].Value.ToString() : "",
                            Control_Performer = workSheet.Cells[rowIterator, Control_PerformerIndex].Value != null ? workSheet.Cells[rowIterator, Control_PerformerIndex].Value.ToString() : "",
                            Current_step = workSheet.Cells[rowIterator, Current_stepIndex].Value != null ? workSheet.Cells[rowIterator, Current_stepIndex].Value.ToString() : "",
                            Frequency = workSheet.Cells[rowIterator, FrequencyIndex].Value != null ? workSheet.Cells[rowIterator, FrequencyIndex].Value.ToString() : "",
                            Layer = workSheet.Cells[rowIterator, LayerIndex].Value != null ? workSheet.Cells[rowIterator, LayerIndex].Value.ToString() : "",
                            Test_application = workSheet.Cells[rowIterator, Test_applicationIndex].Value != null ? workSheet.Cells[rowIterator, Test_applicationIndex].Value.ToString() : "",
                            Test_Completed = workSheet.Cells[rowIterator, Test_CompletedIndex].Value != null ? workSheet.Cells[rowIterator, Test_CompletedIndex].Value.ToString() : "",
                            Organisation = workSheet.Cells[rowIterator, OrganisationIndex].Value != null ? workSheet.Cells[rowIterator, OrganisationIndex].Value.ToString() : "",
                            Comments = workSheet.Cells[rowIterator, CommentsIndex].Value != null ? workSheet.Cells[rowIterator, CommentsIndex].Value.ToString() : "",
                            Test_Completion_Date = workSheet.Cells[rowIterator, Test_Completion_DateIndex].Value != null && workSheet.Cells[rowIterator, Test_Completion_DateIndex].Value.ToString() != string.Empty ? System.Convert.ToDateTime(workSheet.Cells[rowIterator, Test_Completion_DateIndex].Value.ToString()) : DateTime.MinValue,
                            Test_Conclusion = workSheet.Cells[rowIterator, Test_ConclusionIndex].Value != null ? workSheet.Cells[rowIterator, Test_ConclusionIndex].Value.ToString() : "",
                            Test_Due_Date = workSheet.Cells[rowIterator, Test_Due_DateIndex].Value != null && workSheet.Cells[rowIterator, Test_Due_DateIndex].Value.ToString() != string.Empty ? System.Convert.ToDateTime(workSheet.Cells[rowIterator, Test_Due_DateIndex].Value.ToString()) : DateTime.MinValue,
                            Test_ID = workSheet.Cells[rowIterator, Test_IDIndex].Value != null ? workSheet.Cells[rowIterator, Test_IDIndex].Value.ToString() : "",
                            Uploadedby = fileData.EmployeeName,
                            FileNamee = file.FileName,
                            Consultantname = fileData.EmployeeName,
                            Uploadedmonth = fileData.Month,

                            //NEED TO DECIDE

                            Uniquekey = fileData.EmployeeID + "_" + workSheet.Cells[rowIterator, Test_IDIndex].Value.ToString() + "_" + fileData.Month + "_" + fileData.ProjectID

                        });
                    }

                }
                using (var context = new TimeSheetEntities())
                {
                    context.monthlyreports_Template3.AddRange(reportModel.Template3Reports);
                    context.SaveChanges();
                    model.Response.StatusCode = 200;
                    model.Response.Message = "Status Report Uploaded Successfully!";
                }
            }
            catch (Exception ex)
            {
                StatusReportReponse(model, ex);
            }
        }

        public JsonResult ReadExcelColumnNames(StatusReportModel fileData)
        {
            HttpPostedFileBase file = fileData.ExcelFile;
            var columnNames = new Dictionary<string, int>();

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    for (int columnIterator = 1; columnIterator <= noOfCol; columnIterator++)
                    {
                        if (workSheet.Cells[1, columnIterator].Value != null)
                            columnNames.Add(workSheet.Cells[1, columnIterator].Value.ToString(), columnIterator);
                    }
                }
            }

            return Json(columnNames, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReadSelectedColumnUniqueValues(StatusReportModel fileData)
        {
            HttpPostedFileBase file = fileData.ExcelFile;
            var columnValues = new List<string>();

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    var selectedColumnIndex = System.Convert.ToInt32(fileData.SelectedColumnIndex);

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (workSheet.Cells[rowIterator, selectedColumnIndex].Value != null)
                            columnValues.Add(workSheet.Cells[rowIterator, selectedColumnIndex].Value.ToString());
                    }
                }
            }

            if (columnValues.Count > 0)
            {
                var distinctColumnsValues = columnValues.Distinct();
                return Json(JsonSerializer.Serialize(distinctColumnsValues), JsonRequestBehavior.AllowGet);
            }

            return Json(null);

        }


        //Status REPORt View Report Code
        public ActionResult StatusReport()
        {
            var model = new RMA_StatusReportViewModel();

            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            model.RMA_StatusReportModel.RMA_EmployeeModel = employeeModel;
            return View(model);
        }

        static string getAbbreviatedName(DateTime selectedDate)
        {
            DateTime date = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            return date.ToString("MMM") + "-" + selectedDate.Year;
        }

        public static MonthWiseReportModel ReportGetMonthInfo(DateTime selectedDate)
        {
            DateTime date = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            DateTime lastDay = new DateTime();

            if (selectedDate.Month != 12)
            {
                lastDay = new DateTime(selectedDate.Year, selectedDate.Month + 1, 1).AddDays(-1);
            }
            else
            {
                lastDay = new DateTime(selectedDate.Year + 1, 1, 1).AddDays(-1);

            }

            var monthInfo = new MonthWiseReportModel()
            {
                Month = date.ToString("MMM") + "-" + selectedDate.Year,
                Year = date.Year,
                MonthNumber = date.Month,
                StartDateOfTheMonth = selectedDate,
                EndDateOfTheMonth = lastDay
            };

            return monthInfo;
        }



        public JsonResult GetMonthsBasedOnYear(int year, string reportType)
        {

            var selectedDate = new DateTime(year, 1, 1);

            //Current month  report cant be seen, hence excluding the dropdown
            var selctedDateMonth = System.Convert.ToInt32(DateTime.Now.ToString("MM"));

            if (year != DateTime.Now.Year)
            {
                selctedDateMonth = 12;
            }

            var monthsList = new Dictionary<string, string>();

            if (reportType == "Month Report")
            {
                var currentMonthStartdate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                for (int month = 1; month <= selctedDateMonth; month++)
                {
                    var newDate = new DateTime(year, month, 1);

                    if (newDate != currentMonthStartdate)
                    {
                        var monthName = getAbbreviatedName(newDate);
                        monthsList.Add(System.Convert.ToString(monthName + "&" + month), monthName);
                    }
                }

            }

            var inputMonthList = new Dictionary<string, int>();

            //Based on this value in loop month start will be looped
            var monthStartFrom = 0;

            if (reportType == "Quarterly Report")
            {
                inputMonthList.Add("Quarter-1", 3);
                inputMonthList.Add("Quarter-2", 6);
                inputMonthList.Add("Quarter-3", 9);
                inputMonthList.Add("Quarter-4", 12);

                monthStartFrom = 2;
            }

            if (reportType == "Half Year Report")
            {
                inputMonthList.Add("First Half Year", 6);
                inputMonthList.Add("Second Half Year", 12);

                monthStartFrom = 5;
            }

            if (reportType == "Annual Report")
            {
                inputMonthList.Add("Annual Report", 12);
                monthStartFrom = 11;
            }

            foreach (var quarter in inputMonthList)
            {
                var monthNames = string.Empty;
                var quarterExists = false;

                if (quarter.Value <= selctedDateMonth)
                {
                    var startMonth = quarter.Value - monthStartFrom;
                    for (int month = startMonth; month <= quarter.Value; month++)
                    {
                        var newDate = new DateTime(year, month, 1);
                        monthNames += getAbbreviatedName(newDate) + "&" + month + "|";
                        quarterExists = true;
                    }
                }
                if (quarterExists)
                {
                    monthsList.Add(monthNames, quarter.Key);
                }
            }


            var requiredMonthsLost = monthsList.Reverse();
            return Json(JsonSerializer.Serialize(requiredMonthsLost), JsonRequestBehavior.AllowGet);
        }

        static double PercentageCalculate(int num, int totalNum)
        {
            var percenatage = (double)num / totalNum * 100;
            return percenatage;
        }

        static double PercentageCalculateCustom(int num, int totalNum)
        {
            //var percenatage = (double)num / totalNum * 100;
            var percenatage = (int)Math.Round((double)(100 * num) / totalNum, MidpointRounding.AwayFromZero);
            return percenatage;
        }

        public string EmployeeProfileImagePath(string EmpID)
        {
            var imageUrl = "/Assets/EmployeeImagesPNG/" + EmpID + ".png";
            var baseUri = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));
            var url = new Uri(baseUri, VirtualPathUtility.ToAbsolute(imageUrl));
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    imageUrl = "/Assets/EmployeeImagesPNG/MaleDefault.png";
                }
            }

            return imageUrl;
        }

        public ActionResult StatusGraphChartReport(StatusReportChartModel StatusReportChartModel)
        {
            var graphModel = new GraphChartModel();

            graphModel.ViewModel = new List<GraphChartViewModel>();
            var selectedReportedMonthStartDate = new DateTime();
            var requiredReportMonths = new List<MonthWiseReportModel>();

            var carryForwardMonthInfo = new MonthWiseReportModel();

            Decimal emplyeeAvailabiliy = 0;
            if (StatusReportChartModel.ReportType == "Month Report")
            {
                var selectedMonth = StatusReportChartModel.Month;
                var selectedMonthNumber = System.Convert.ToInt32(selectedMonth.Split('&')[1]);
                selectedReportedMonthStartDate = new DateTime(StatusReportChartModel.Year, selectedMonthNumber, 1);
                graphModel.SelectedReportMonth = SelectedMonthRelatedInfo(selectedReportedMonthStartDate, StatusReportChartModel);

                requiredReportMonths.Add(ReportGetMonthInfo(selectedReportedMonthStartDate));
                requiredReportMonths.Add(ReportGetMonthInfo(selectedReportedMonthStartDate.AddMonths(-1)));
                //requiredReportMonths.Add(ReportGetMonthInfo(selectedReportedMonthStartDate.AddMonths(-2)));

                //var startingMonthForTheReport = ReportGetMonthInfo(selectedReportedMonthStartDate.AddMonths(-3));
                //carryForwardMonthInfo = ReportGetMonthInfo(selectedReportedMonthStartDate.AddMonths(-4));

                var startingMonthForTheReport = ReportGetMonthInfo(selectedReportedMonthStartDate.AddMonths(-2));

                requiredReportMonths.Add(startingMonthForTheReport);
                graphModel.SelectedReportMonth.ReportStartMonth = startingMonthForTheReport.Month;
                requiredReportMonths.Reverse();

                graphModel.SelectedReportMonth.ReportType = StatusReportChartModel.ReportType;
                graphModel.SelectedReportMonth.QuarterName = graphModel.SelectedReportMonth.MonthName;

            }

            else
            {
                var selectedMonth = StatusReportChartModel.Month;
                var selectedMonthNumbers = selectedMonth.Split('|');
                int firstMonthFromTheSelection = 0;
                var reportStartMonth = "";

                var selectedMonthNumberCount = 0;

                foreach (var selectedMonthNumber in selectedMonthNumbers)
                {
                    if (selectedMonthNumber != string.Empty)
                    {
                        selectedMonthNumberCount++;
                        var selectedMonthNum = System.Convert.ToInt32(selectedMonthNumber.Split('&')[1]);
                        selectedReportedMonthStartDate = new DateTime(StatusReportChartModel.Year, selectedMonthNum, 1);

                        graphModel.SelectedReportMonth = SelectedMonthRelatedInfo(selectedReportedMonthStartDate, StatusReportChartModel);

                        var selectedMonthInfo = ReportGetMonthInfo(selectedReportedMonthStartDate);
                        if (firstMonthFromTheSelection == 0)
                        {
                            reportStartMonth = selectedMonthInfo.Month;

                            if (selectedMonthNum == 1)
                            {
                                var carryForwardYear = StatusReportChartModel.Year - 1;
                                var carryForwardMonthNum = 12;
                                var carryForwardMonth = new DateTime(carryForwardYear, carryForwardMonthNum, 1);
                                carryForwardMonthInfo = ReportGetMonthInfo(carryForwardMonth);
                            }

                        }
                        requiredReportMonths.Add(selectedMonthInfo);
                        firstMonthFromTheSelection++;
                    }
                }
                graphModel.SelectedReportMonth.ReportStartMonth = reportStartMonth;

                var SelectedreportStartMonth = graphModel.SelectedReportMonth.ReportStartMonth.Split('-')[0];

                if (selectedMonthNumberCount == 3)
                {
                    switch (SelectedreportStartMonth)
                    {
                        case "Jan":
                            graphModel.SelectedReportMonth.QuarterName = "Q1";
                            break;
                        case "Apr":
                            graphModel.SelectedReportMonth.QuarterName = "Q2";
                            break;
                        case "Jul":
                            graphModel.SelectedReportMonth.QuarterName = "Q3";
                            break;
                        case "Oct":
                            graphModel.SelectedReportMonth.QuarterName = "Q4";
                            break;
                    }

                }

                if (selectedMonthNumberCount == 6 && SelectedreportStartMonth == "Jan")
                {
                    graphModel.SelectedReportMonth.QuarterName = "Q1 & Q2";
                }

                if (selectedMonthNumberCount == 6 && SelectedreportStartMonth == "Jul")
                {
                    graphModel.SelectedReportMonth.QuarterName = "Q3 & Q4";
                }

                if (selectedMonthNumberCount == 12)
                {
                    graphModel.SelectedReportMonth.QuarterName = "Annual Report";
                }
            }

            var graph1Reports = new List<Root>();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {

                foreach (var empID in StatusReportChartModel.EmployeeID)
                {
                    var model = new GraphChartViewModel();
                    model.EmployeeImage = EmployeeProfileImagePath(empID);

                    model.AMBC_Active_Emp_view = db.AMBC_Active_Emp_view.Where(x => x.Employee_ID == empID).FirstOrDefault();
                    var MonthWisenewlyRaisedTickets = new List<Graph1DataPoint.DataPoint>();
                    var MonthWiseOpenTickets = new List<Graph1DataPoint.DataPoint>();
                    var MonthWiseTotalClosedTickets = new List<Graph1DataPoint.DataPoint>();
                    var MonthSpecificClosedTickets = new List<Graph1DataPoint.DataPoint>();

                    var MonthWiseCriticlOpenTickets = new List<Graph1DataPoint.MonthWiseDataPoint>();
                    var MonthWiseHighOpenTickets = new List<Graph1DataPoint.MonthWiseDataPoint>();
                    var MonthWiseMediumOpenTickets = new List<Graph1DataPoint.MonthWiseDataPoint>();
                    var MonthWiseLowOpenTickets = new List<Graph1DataPoint.MonthWiseDataPoint>();

                    var ProjectReports = new List<ProjectGraphDataPoint.Reports>();
                    var FutureProjectReports = new List<ProjectGraphDataPoint.Reports>();

                    var RegularProjectReports = new List<ProjectGraphDataPoint.Reports>();


                    var MonthWiseAuditTickets = new List<Graph1DataPoint.DataPoint>();
                    var MonthWiseEfficientClosedTickets = new List<Graph1DataPoint.DataPoint>();
                    var MonthWiseInEfficientClosedTickets = new List<Graph1DataPoint.DataPoint>();

                    var MonthWiseAttedenceFlowTillDate = new List<Graph1DataPoint.AvailabilityDataPoint>();

                    //This object for category based chart
                    var MonthWiseTotalCreatedTickes = new List<monthlyreports_Template1>();

                    var MonthWiseCriticlTotalTickets = 0;
                    var MonthWiseHighOpenTotalTickets = 0;
                    var MonthWiseMediumOpenTotalTickets = 0;
                    var MonthWiseLowOpenTotalTickets = 0;

                    var sameDayCTCount = 0;
                    var Twoto5DayCTCount = 0;
                    var Sixto10DayCTCount = 0;
                    var Elevento15DayCTCount = 0;
                    var GT15DayCTCount = 0;

                    var totalNewlyRaisedTickets = 0;
                    var totalOpenTickets = 0;
                    var totalClosedTickets = 0;

                    int firstMonthFromrequiredReportMonths = 0;

                    foreach (var requiredReportMonth in requiredReportMonths)
                    {
                        var specifMonthAvailablity = db.consultantavailiability_Final.Where(Employee => Employee.Employee_Code == empID && Employee.Month_Year == requiredReportMonth.Month).FirstOrDefault();

                        if (specifMonthAvailablity != null)
                        {
                            var consultantAvailabity = specifMonthAvailablity.ConslAvl.Replace("%", "");

                            var reqConsultantAvailabity = consultantAvailabity.Contains('.') ? System.Convert.ToDecimal(consultantAvailabity.Split('.')[0]) : System.Convert.ToDecimal(consultantAvailabity);

                            if (StatusReportChartModel.ReportType == "Monthe Report")
                            {
                                if (StatusReportChartModel.Month == (requiredReportMonth.Month + "&" + requiredReportMonth.MonthNumber))
                                {
                                    emplyeeAvailabiliy += reqConsultantAvailabity;
                                }
                            }
                            else
                            {
                                var selectiedPeriod = StatusReportChartModel.Month.TrimEnd('|').Split('|').Last();
                                if (selectiedPeriod != null && selectiedPeriod == (requiredReportMonth.Month + "&" + requiredReportMonth.MonthNumber))
                                {
                                    emplyeeAvailabiliy += reqConsultantAvailabity;
                                }
                            }


                            MonthWiseAttedenceFlowTillDate.Add(new Graph1DataPoint.AvailabilityDataPoint()
                            {
                                y = System.Convert.ToDouble(reqConsultantAvailabity),
                                label = requiredReportMonth.Month,
                                markerColor = "rgb(81, 205, 160)",
                                indexLabelFontColor = "rgb(81, 205, 160)"
                            });
                        }
                        else
                        {
                            MonthWiseAttedenceFlowTillDate.Add(new Graph1DataPoint.AvailabilityDataPoint()
                            {
                                y = 0,
                                label = requiredReportMonth.Month,
                                markerColor = "rgb(81, 205, 160)",
                                indexLabelFontColor = "rgb(81, 205, 160)"
                            });
                        }

                        var selectedMonthTickets = db.monthlyreports_Template1.Where(ticket => ticket.Uploaded_Month == requiredReportMonth.Month && ticket.Is_Cancelled == false && ticket.EmplyeeID == empID && ticket.Client_Name == StatusReportChartModel.ClientName && StatusReportChartModel.ToolName.Contains(ticket.TicketingToolName) && (ticket.IsAuditReport == null || ticket.IsAuditReport == false)).ToList();
                        if (selectedMonthTickets != null && selectedMonthTickets.Count() > 0)
                        {
                            //This is for category chart
                            MonthWiseTotalCreatedTickes.AddRange(selectedMonthTickets);

                            graphModel.IsIncidentReportExists = true;
                        }
                        var newlyCreatedTickets = selectedMonthTickets.Where(ticket => ticket.Is_Newly_created == true).ToList();
                        if (newlyCreatedTickets != null && newlyCreatedTickets.Count() > 0)
                        {

                            totalNewlyRaisedTickets += System.Convert.ToInt32(newlyCreatedTickets.Count());
                        }

                        MonthWisenewlyRaisedTickets.Add(new Graph1DataPoint.DataPoint()
                        {
                            label = requiredReportMonth.Month,
                            y = newlyCreatedTickets != null && newlyCreatedTickets.Count() > 0 ? System.Convert.ToInt32(newlyCreatedTickets.Count()) : 0
                        });

                        var OpenTickets = selectedMonthTickets.Where(ticket => ticket.Is_Open == true).ToList();

                        MonthWiseOpenTickets.Add(new Graph1DataPoint.DataPoint()
                        {
                            label = requiredReportMonth.Month,
                            y = OpenTickets != null && OpenTickets.Count() > 0 ? System.Convert.ToInt32(OpenTickets.Count()) : 0
                        });

                        if (OpenTickets != null && OpenTickets.Count > 0)
                        {
                            var ctiticalTickets = OpenTickets.Where(x => x.ReportPriority == "Critical").ToList();

                            if (ctiticalTickets != null && ctiticalTickets.Count > 0)
                            {
                                MonthWiseCriticlTotalTickets += ctiticalTickets.Count;
                            }

                            MonthWiseCriticlOpenTickets.Add(new Graph1DataPoint.MonthWiseDataPoint()
                            {
                                Priority = "Critical",
                                label = requiredReportMonth.Month,
                                y = ctiticalTickets != null && ctiticalTickets.Count > 0 ? ctiticalTickets.Count : 0,
                                totalTickets = MonthWiseCriticlTotalTickets
                            });


                            var highTickets = OpenTickets.Where(x => x.ReportPriority == "High").ToList();
                            if (highTickets != null && highTickets.Count > 0)
                            {
                                MonthWiseHighOpenTotalTickets += highTickets.Count;
                            }
                            MonthWiseHighOpenTickets.Add(new Graph1DataPoint.MonthWiseDataPoint()
                            {
                                Priority = "High",
                                label = requiredReportMonth.Month,
                                y = highTickets != null && highTickets.Count > 0 ? highTickets.Count : 0,
                                totalTickets = MonthWiseHighOpenTotalTickets
                            });


                            var mediumTickets = OpenTickets.Where(x => x.ReportPriority == "Medium").ToList();
                            if (mediumTickets != null && mediumTickets.Count > 0)
                            {
                                MonthWiseMediumOpenTotalTickets += mediumTickets.Count;
                            }
                            MonthWiseMediumOpenTickets.Add(new Graph1DataPoint.MonthWiseDataPoint()
                            {
                                Priority = "Medium",
                                label = requiredReportMonth.Month,
                                y = mediumTickets != null && mediumTickets.Count > 0 ? mediumTickets.Count : 0,
                                totalTickets = MonthWiseMediumOpenTotalTickets
                            });

                            var lowTickets = OpenTickets.Where(x => x.ReportPriority == "Low").ToList();
                            if (lowTickets != null && lowTickets.Count > 0)
                            {
                                MonthWiseLowOpenTotalTickets += lowTickets.Count;
                            }
                            MonthWiseLowOpenTickets.Add(new Graph1DataPoint.MonthWiseDataPoint()
                            {
                                Priority = "Low",
                                label = requiredReportMonth.Month,
                                y = lowTickets != null && lowTickets.Count > 0 ? lowTickets.Count : 0,
                                totalTickets = MonthWiseLowOpenTotalTickets
                            });

                            totalOpenTickets += OpenTickets.Count;
                        }

                        var closedTickets = selectedMonthTickets.Where(ticket => ticket.Is_Closed == true).ToList();
                        MonthWiseTotalClosedTickets.Add(new Graph1DataPoint.DataPoint()
                        {
                            label = requiredReportMonth.Month,
                            y = closedTickets != null && closedTickets.Count() > 0 ? System.Convert.ToInt32(closedTickets.Count()) : 0
                        });

                        if (closedTickets != null && closedTickets.Count > 0)
                        {
                            var sameDayTickets = closedTickets.Where(x => x.Ticket_Age == 1).ToList();
                            sameDayCTCount += sameDayTickets != null && sameDayTickets.Count > 0 ? sameDayTickets.Count : 0;

                            var twoTo5Tickets = closedTickets.Where(x => x.Ticket_Age >= 2 && x.Ticket_Age <= 5).ToList();
                            Twoto5DayCTCount += twoTo5Tickets != null && twoTo5Tickets.Count > 0 ? twoTo5Tickets.Count : 0;

                            var sixTo10Tickets = closedTickets.Where(x => x.Ticket_Age >= 6 && x.Ticket_Age <= 10).ToList();
                            Sixto10DayCTCount += sixTo10Tickets != null && sixTo10Tickets.Count > 0 ? sixTo10Tickets.Count : 0;

                            var elevanTo15Tickets = closedTickets.Where(x => x.Ticket_Age >= 11 && x.Ticket_Age <= 15).ToList();
                            Elevento15DayCTCount += elevanTo15Tickets != null && elevanTo15Tickets.Count > 0 ? elevanTo15Tickets.Count : 0;

                            var Greater15Tickets = closedTickets.Where(x => x.Ticket_Age > 15).ToList();
                            GT15DayCTCount += Greater15Tickets != null && Greater15Tickets.Count > 0 ? Greater15Tickets.Count : 0;

                            totalClosedTickets += closedTickets.Count;
                        }

                        var monthSpecificLosedTicketsCount = 0;
                        var monthSpecifcClosedTockets = db.monthlyreports_Template1.Where(ticket => ticket.Closed_Month == requiredReportMonth.MonthNumber && ticket.Closed_Year == requiredReportMonth.Year && ticket.Is_Cancelled == false && ticket.EmplyeeID == empID && ticket.Client_Name == StatusReportChartModel.ClientName && StatusReportChartModel.ToolName.Contains(ticket.TicketingToolName) && (ticket.IsAuditReport == null || ticket.IsAuditReport == false)).ToList();

                        if (monthSpecifcClosedTockets != null && monthSpecifcClosedTockets.Count() > 0)
                        {
                            if (monthSpecifcClosedTockets != null && monthSpecifcClosedTockets.Count > 0)
                            {
                                monthSpecificLosedTicketsCount = monthSpecifcClosedTockets.Count;
                            }
                        }
                        MonthSpecificClosedTickets.Add(new Graph1DataPoint.DataPoint()
                        {
                            label = requiredReportMonth.Month,
                            y = monthSpecificLosedTicketsCount
                        });


                        //TEMPLATE2 code updates
                        if (firstMonthFromrequiredReportMonths == 0)
                        {
                            var projectDetailsForCarryForwardMonth = db.monthlyreports_Template2.Where(project => project.Uploaded_Month == carryForwardMonthInfo.Month && project.EmplyeeID == empID && project.Is_Cancelled == false && project.Client_Name == StatusReportChartModel.ClientName && project.Project_Category != "regularprojects").ToList();

                            if (projectDetailsForCarryForwardMonth != null && projectDetailsForCarryForwardMonth.Count() > 0)
                            {
                                ProjectReport(ProjectReports, projectDetailsForCarryForwardMonth, true);
                            }
                        }


                        var projectDetailsForSelectedMonth = db.monthlyreports_Template2.Where(project => project.Uploaded_Month == requiredReportMonth.Month && project.EmplyeeID == empID && project.Is_Cancelled == false && project.Is_ToDo == false && project.Client_Name == StatusReportChartModel.ClientName && project.Project_Category != "regularprojects").ToList();

                        //In case of Month report for selected month only report will generate
                        //if (StatusReportChartModel.ReportType == "Month Report" && graphModel.SelectedReportMonth.ShortFormat == requiredReportMonth.Month)
                        //{
                        //    ProjectReport(ProjectReports, projectDetailsForSelectedMonth);
                        //}
                        //if (StatusReportChartModel.ReportType != "Month Report")
                        //{
                        ProjectReport(ProjectReports, projectDetailsForSelectedMonth);
                        //}

                        //REPEATED MONTHLY ACTIVITIES CONSIDERING HERE
                        var runningProjectDetailsForSelectedMonth = db.monthlyreports_Template2.Where(project => project.Uploaded_Month == requiredReportMonth.Month && project.EmplyeeID == empID && project.Is_Cancelled == false && project.Is_ToDo == false && project.Client_Name == StatusReportChartModel.ClientName && project.Project_Category == "regularprojects").ToList();
                        ProjectReport(RegularProjectReports, runningProjectDetailsForSelectedMonth);

                        firstMonthFromrequiredReportMonths++;

                        //TEMPLATE3 code updates
                        var AuditReportsForSelectedMonth = db.monthlyreports_Template1.Where(ticket => ticket.Uploaded_Month == requiredReportMonth.Month && ticket.Is_Cancelled == false && ticket.EmplyeeID == empID && ticket.Client_Name == StatusReportChartModel.ClientName && StatusReportChartModel.ToolName.Contains(ticket.TicketingToolName) && ticket.IsAuditReport == true).ToList();

                        if (AuditReportsForSelectedMonth != null && AuditReportsForSelectedMonth.Count() > 0)
                        {
                            graphModel.IsAuditReportExists = true;
                            var EfficientClosedTickets = AuditReportsForSelectedMonth.Where(ticket => ticket.Is_Closed == true).ToList();
                            var InEfficientClosedTickets = AuditReportsForSelectedMonth.Where(ticket => ticket.Is_Closed == false).ToList();

                            MonthWiseAuditTickets.Add(new Graph1DataPoint.DataPoint()
                            {
                                label = requiredReportMonth.Month,
                                y = AuditReportsForSelectedMonth != null && AuditReportsForSelectedMonth.Count() > 0 ? System.Convert.ToInt32(AuditReportsForSelectedMonth.Count()) : 0
                            });

                            MonthWiseEfficientClosedTickets.Add(new Graph1DataPoint.DataPoint()
                            {
                                label = requiredReportMonth.Month,
                                y = EfficientClosedTickets != null && EfficientClosedTickets.Count() > 0 ? System.Convert.ToInt32(EfficientClosedTickets.Count()) : 0
                            });

                            MonthWiseInEfficientClosedTickets.Add(new Graph1DataPoint.DataPoint()
                            {
                                label = requiredReportMonth.Month,
                                y = InEfficientClosedTickets != null && InEfficientClosedTickets.Count() > 0 ? System.Convert.ToInt32(InEfficientClosedTickets.Count()) : 0
                            });
                        }
                        else
                        {
                            MonthWiseAuditTickets.Add(new Graph1DataPoint.DataPoint()
                            {
                                label = requiredReportMonth.Month,
                                y = 0
                            });

                            MonthWiseEfficientClosedTickets.Add(new Graph1DataPoint.DataPoint()
                            {
                                label = requiredReportMonth.Month,
                                y = 0
                            });

                            MonthWiseInEfficientClosedTickets.Add(new Graph1DataPoint.DataPoint()
                            {
                                label = requiredReportMonth.Month,
                                y = 0
                            });
                        }


                    }

                    var IncidentsPieChart = new List<Graph1DataPoint.PieDataPoint>();
                    var totalClosedInciendents = sameDayCTCount + Twoto5DayCTCount + Sixto10DayCTCount + Elevento15DayCTCount + GT15DayCTCount;

                    IncidentsPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = "",
                        y = PercentageCalculate(sameDayCTCount, totalClosedInciendents),
                        Percentage = PercentageCalculateCustom(sameDayCTCount, totalClosedInciendents),
                        legendText = "Same Day",
                        indexLabelFontColor = "rgb(109, 120, 173)"
                    });
                    IncidentsPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = "",
                        y = PercentageCalculate(Twoto5DayCTCount, totalClosedInciendents),
                        Percentage = PercentageCalculateCustom(Twoto5DayCTCount, totalClosedInciendents),
                        legendText = "2-5",
                        indexLabelFontColor = "rgb(81, 205, 160)"
                    });
                    IncidentsPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = "",
                        y = PercentageCalculate(Sixto10DayCTCount, totalClosedInciendents),
                        Percentage = PercentageCalculateCustom(Sixto10DayCTCount, totalClosedInciendents),
                        legendText = "6-10",
                        indexLabelFontColor = "rgb(223, 121, 112)"
                    });
                    IncidentsPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = "",
                        y = PercentageCalculate(Elevento15DayCTCount, totalClosedInciendents),
                        Percentage = PercentageCalculateCustom(Elevento15DayCTCount, totalClosedInciendents),
                        legendText = "11-15",
                        indexLabelFontColor = "rgb(76, 156, 160)"
                    });
                    IncidentsPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = "",
                        y = PercentageCalculate(GT15DayCTCount, totalClosedInciendents),
                        Percentage = PercentageCalculateCustom(GT15DayCTCount, totalClosedInciendents),
                        legendText = "Grtr-15",
                        indexLabelFontColor = "rgb(174, 125, 153)"
                    });

                    var highestCosedDateRange = IncidentsPieChart.OrderByDescending(percentage => percentage.y).FirstOrDefault();
                    if (highestCosedDateRange != null)
                    {
                        var grpah4OverallStatus = "" + highestCosedDateRange.Percentage + "% tickets closed within " + highestCosedDateRange.legendText + " days from the logged date.";
                        model.Graph4OverallStatus = grpah4OverallStatus;
                    }


                    var IncidentsSummaryPieChart = new List<Graph1DataPoint.PieDataPoint>();
                    IncidentsSummaryPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = System.Convert.ToString(totalClosedTickets) + "/" + System.Convert.ToString(totalOpenTickets + totalClosedTickets),
                        y = PercentageCalculate(totalClosedTickets, totalClosedTickets + totalOpenTickets),
                        Percentage = PercentageCalculateCustom(totalClosedTickets, totalClosedTickets + totalOpenTickets),
                        legendText = "Closed",
                        indexLabelFontColor = "rgb(81, 205, 160)",
                        color = "rgb(81, 205, 160)",
                    });
                    IncidentsSummaryPieChart.Add(new Graph1DataPoint.PieDataPoint()
                    {
                        label = System.Convert.ToString(totalOpenTickets) + "/" + System.Convert.ToString(totalOpenTickets + totalClosedTickets),
                        y = PercentageCalculate(totalOpenTickets, totalClosedTickets + totalOpenTickets),
                        Percentage = PercentageCalculateCustom(totalOpenTickets, totalClosedTickets + totalOpenTickets),
                        legendText = "Open",
                        indexLabelFontColor = "rgb(247, 150, 71)",
                        color = "rgb(247, 150, 71)"
                    });

                    var grpah5OverallStatus = "Total " + IncidentsSummaryPieChart[0].Percentage + "% tickets closed and " + IncidentsSummaryPieChart[1].Percentage + "% open till date.";
                    model.Graph5OverallStatus = grpah5OverallStatus;


                    //EMPLOYEE DETAILS
                    //var employeeTotalAvailabity = (emplyeeAvailabiliy / requiredReportMonths.Count).ToString();

                    //Int32 availabity = employeeTotalAvailabity.Contains('.') ? System.Convert.ToInt32(employeeTotalAvailabity.Split('.')[0]) + 1 : System.Convert.ToInt32(employeeTotalAvailabity);
                    // model.EmaployeeAvailabity = availabity

                    model.EmaployeeAvailabity = System.Convert.ToInt32(emplyeeAvailabiliy);

                    //GRAPH1
                    var overallTicketRunRate = PercentageCalculateCustom(totalClosedTickets, totalNewlyRaisedTickets);
                    model.Graph1OverallStatus = overallTicketRunRate;
                    model.MNRTDataPoints = JsonConvert.SerializeObject(MonthWisenewlyRaisedTickets);
                    model.MOTDataPoints = JsonConvert.SerializeObject(MonthWiseOpenTickets);
                    model.MCTTotalDataPoints = JsonConvert.SerializeObject(MonthWiseTotalClosedTickets);


                    //GRAPH2
                    var newlyRaiseTicketsOrder = MonthWisenewlyRaisedTickets.OrderByDescending(ticket => ticket.y).ToList();
                    double highestNewRiasedTickNum = 0;
                    string highestNewRiasedTickMonth = "";
                    if (newlyRaiseTicketsOrder != null && newlyRaiseTicketsOrder.Count() > 0)
                    {
                        highestNewRiasedTickMonth = newlyRaiseTicketsOrder[0].label;
                        highestNewRiasedTickNum = newlyRaiseTicketsOrder[0].y;
                    }

                    var closedTicketsOrder = MonthSpecificClosedTickets.OrderByDescending(ticket => ticket.y).ToList();
                    double highestClosedTickNum = 0;
                    string highestClosedTickMonth = "";
                    if (closedTicketsOrder != null && closedTicketsOrder.Count() > 0)
                    {
                        highestClosedTickMonth = closedTicketsOrder[0].label;
                        highestClosedTickNum = closedTicketsOrder[0].y;
                    }
                    if (highestNewRiasedTickNum > 0 && highestNewRiasedTickMonth != string.Empty)
                    {
                        var grpah2OverallStatus = "In " + highestClosedTickMonth + " we closed ~" + highestClosedTickNum + " tickets";
                        model.Graph2OverallStatus = grpah2OverallStatus;
                    }

                    model.MSpecifCTDataPoints = JsonConvert.SerializeObject(MonthSpecificClosedTickets);


                    //OPEN Tickets will be considered for Selected month only 
                    //GRAPH3
                    var allPriorityTickets = MonthWiseCriticlOpenTickets.Concat(MonthWiseHighOpenTickets).Concat(MonthWiseMediumOpenTickets).Concat(MonthWiseLowOpenTickets);
                    var highestPrirityTickets = allPriorityTickets.OrderByDescending(ticket => ticket.totalTickets).FirstOrDefault();
                    if (highestPrirityTickets != null)
                    {
                        var grpah3OverallStatus = "" + highestPrirityTickets.totalTickets + " " + highestPrirityTickets.Priority + " priority tickets are in Open till date.";
                        model.Graph3OverallStatus = grpah3OverallStatus;
                    }


                    model.MCRITOTDataPoints = JsonConvert.SerializeObject(MonthWiseCriticlOpenTickets);
                    model.MHIGOTDataPoints = JsonConvert.SerializeObject(MonthWiseHighOpenTickets);
                    model.MMEDIOTDataPoints = JsonConvert.SerializeObject(MonthWiseMediumOpenTickets);
                    model.MLOWOTDataPoints = JsonConvert.SerializeObject(MonthWiseLowOpenTickets);


                    //CLOSED REPORT TREND
                    //GRAPH4
                    model.ClosedTrend = JsonConvert.SerializeObject(IncidentsPieChart);

                    //INCIDENTS SUMMARY
                    //GRAPH5
                    model.IncidentsSummary = JsonConvert.SerializeObject(IncidentsSummaryPieChart);

                    //******************* Past and Future Attednce Flow      ***********************//

                    var totalMonthsAttedenceCaptured = requiredReportMonths.Count();
                    var lastMonthAttedenceCaptured = requiredReportMonths[totalMonthsAttedenceCaptured - 1];
                    var lastMonthNumberVal = lastMonthAttedenceCaptured.MonthNumber.ToString();

                    var lastMonthNum = System.Convert.ToInt32(lastMonthNumberVal);
                    var nextThreeMonthsForAttedence = new List<MonthWiseReportModel>();
                    var funtureMonthNumber = lastMonthNum;

                    var futureMonthCount = 0;
                    if (lastMonthNum != 12)
                    {
                        //Considering only one future month hence changes i < 3 to i < 1
                        for (int i = 0; i < 1; i++)
                        {
                            funtureMonthNumber = funtureMonthNumber + 1;
                            futureMonthCount++;
                            if (funtureMonthNumber <= 12 && futureMonthCount <= 3)
                            {
                                var futureMonth = new DateTime(StatusReportChartModel.Year, funtureMonthNumber, 1);
                                nextThreeMonthsForAttedence.Add(ReportGetMonthInfo(futureMonth));
                            }
                        }

                    }

                    foreach (var futureMonth in nextThreeMonthsForAttedence)
                    {
                        var locationSpecifcHolidays = db.tblambcholidays.Where(holiday => holiday.holiday_date >= futureMonth.StartDateOfTheMonth && holiday.holiday_date <= futureMonth.EndDateOfTheMonth && holiday.region == model.AMBC_Active_Emp_view.Location).ToList();

                        if (locationSpecifcHolidays != null)
                        {
                            graphModel.HolidayList = locationSpecifcHolidays;
                        }
                        //var specifMonthAvailablity = db.consultantavailiability_Final.Where(Employee => Employee.Employee_Code == empID && Employee.Month_Year == futureMonth.Month).FirstOrDefault();

                        //if (specifMonthAvailablity != null)
                        //{
                        //    var consultantAvailabity = specifMonthAvailablity.ConslAvl.Replace("%", "");
                        //    var futureMonthAvailability = consultantAvailabity.Contains('.') ? System.Convert.ToDecimal(consultantAvailabity.Split('.')[0]) : System.Convert.ToDecimal(consultantAvailabity);
                        //    MonthWiseAttedenceFlowTillDate.Add(new Graph1DataPoint.AvailabilityDataPoint()
                        //    {
                        //        y = System.Convert.ToDouble(futureMonthAvailability),
                        //        label = futureMonth.Month,
                        //        markerColor = "orange",
                        //        indexLabelFontColor = "orange"
                        //    });
                        //}
                        //else
                        //{
                        //    MonthWiseAttedenceFlowTillDate.Add(new Graph1DataPoint.AvailabilityDataPoint()
                        //    {
                        //        y = 0,
                        //        label = futureMonth.Month,
                        //        markerColor = "orange",
                        //        indexLabelFontColor = "orange"
                        //    });
                        //}
                    }

                    model.PastAttedenceFlowTillDate = JsonConvert.SerializeObject(MonthWiseAttedenceFlowTillDate);

                    //TEMPLTE 2 UPDATES  
                    var empBasedFutureProjects = db.monthlyreports_Template2.Where(project => project.Is_ToDo == true && project.Client_Name == StatusReportChartModel.ClientName && project.EmplyeeID == empID && project.Project_Category != "regularprojects").ToList();

                    if (empBasedFutureProjects != null && empBasedFutureProjects.Count() > 0)
                    {
                        model.FutureProjects = ProjectReport(FutureProjectReports, empBasedFutureProjects, false);
                    }

                    var UniqueProjectDataPoints = new List<ProjectGraphDataPoint.DataPoint>();
                    var ProjectComppletionDataPoints = new List<ProjectGraphDataPoint.DataPoint>();
                    var ProjectRemainingDataPoints = new List<ProjectGraphDataPoint.DataPoint>();

                    var ProjectsChart = new List<ProjectChartInfo>();
                    var colorCodes = ResourceManagement.Helpers.ColorCodes.Colors();
                    var index = 1;
                    if (ProjectReports != null && ProjectReports.Count > 0)
                    {                      
                        graphModel.IsProjectReportExists = true;
                        var allProjects = ProjectReports.OrderByDescending(x => x.completionPercenatge).ToList();
                        var projectReportHeight = "140px";

                        model.ProjectReportHeight = projectReportHeight;

                        var requiredProjectCount = 0;
                        foreach (var project in allProjects)
                        {
                            if (UniqueProjectDataPoints.Where(x => x.label == project.label).FirstOrDefault() == null)
                            {
                                requiredProjectCount++;
                                UniqueProjectDataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                {
                                    label = project.label,
                                    y = project.completionPercenatge
                                });

                                ProjectComppletionDataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                {
                                    label = project.ChartLabelWithStartEndDate,
                                    y = project.completionPercenatge
                                });

                                ProjectRemainingDataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                {
                                    label = project.ChartLabelWithStartEndDate,
                                    y = 100 - project.completionPercenatge
                                });
                            }
                        }

                        if (requiredProjectCount > 2 && requiredProjectCount <= 5)
                        {
                            var height = requiredProjectCount * 45;
                            model.ProjectReportHeight = height + "px";
                        }

                        if (requiredProjectCount > 5)
                        {
                            var height = requiredProjectCount * 35;
                            model.ProjectReportHeight = height + "px";
                        }

                        //IN CASE OF BAR CHART SHOIN THE PROJECTS BASED ON SELECTION
                        // CARRY FORWARD PROJECTS ARE IGNORED HERE
                        var uniqueProjects = allProjects.Where(x => x.IsCarryForwardMonth == false).Select(x => x.label).Distinct().ToList();
                        //var uniqueProjects = allProjects.Select(x => x.label).Distinct().ToList();

                        var projectReportMonths = new List<MonthWiseReportModel>();
                        if (StatusReportChartModel.ReportType != "Month Report")
                        {
                            projectReportMonths.Add(carryForwardMonthInfo);
                        }

                        //Adding carry forward month to list
                        projectReportMonths.AddRange(requiredReportMonths);

                        //Incase wnats to display future remaining percebatge un comment below 3 lins
                        //var dummyMonthToCalculatePendingPercenatge = new MonthWiseReportModel();
                        //dummyMonthToCalculatePendingPercenatge.Month = "pending-month";

                        //projectReportMonths.Add(dummyMonthToCalculatePendingPercenatge);

                        var projectsCompletedStatus = new Dictionary<string, decimal?>();

                        var firstMonthOfTheReport = 1;
                        foreach (var projectReportMonth in projectReportMonths)
                        {
                            var color = colorCodes[index];
                            index = index + 1;

                            var chartInfo = new ProjectChartInfo();
                            if (StatusReportChartModel.ReportType == "Month Report" && firstMonthOfTheReport == 1)
                            {
                                chartInfo.name = "till " + projectReportMonth.Month;
                                firstMonthOfTheReport++;
                                chartInfo.color = color;
                               
                            }
                            else
                            {
                                chartInfo.name = projectReportMonth.Month == carryForwardMonthInfo.Month ? "till " + projectReportMonth.Month : projectReportMonth.Month == "pending-month" ? "Remaining" : projectReportMonth.Month;
                                chartInfo.color = color;
                            }

                            foreach (var uniqueProject in uniqueProjects)
                            {
                                var requiredProject = allProjects.Where(x => x.label == uniqueProject && x.MonthName == projectReportMonth.Month).FirstOrDefault();

                                if (requiredProject != null)
                                {
                                    Decimal? actualCompletedPercenatge = 0;

                                    if (!projectsCompletedStatus.ContainsKey(uniqueProject))
                                    {
                                        projectsCompletedStatus.Add(uniqueProject, requiredProject.completionPercenatge);
                                        actualCompletedPercenatge = requiredProject.completionPercenatge;
                                    }
                                    else
                                    {
                                        //OLD LOGIC Show month wise actual completed percentage
                                        //var currentMonthCompletePercentage = requiredProject.completionPercenatge;
                                        //var previousMonthsCompletePercenatge = projectsCompletedStatus[uniqueProject].Value;

                                        //if (currentMonthCompletePercentage > previousMonthsCompletePercenatge)
                                        //{
                                        //    actualCompletedPercenatge = currentMonthCompletePercentage - previousMonthsCompletePercenatge;
                                        //}
                                        //else
                                        //{
                                        //    actualCompletedPercenatge = previousMonthsCompletePercenatge - currentMonthCompletePercentage;
                                        //}

                                        //projectsCompletedStatus.Remove(uniqueProject);

                                        //var totalCompletedPercenatage = actualCompletedPercenatge + previousMonthsCompletePercenatge;

                                        //projectsCompletedStatus.Add(uniqueProject, totalCompletedPercenatage);

                                        //NEW LOGIC show what ever we conter as a completed percenatge
                                        var currentMonthCompletePercentage = requiredProject.completionPercenatge;
                                        var previousMonthsCompletePercenatge = 0;

                                        if (currentMonthCompletePercentage > previousMonthsCompletePercenatge)
                                        {
                                            actualCompletedPercenatge = currentMonthCompletePercentage - previousMonthsCompletePercenatge;
                                        }
                                        else
                                        {
                                            actualCompletedPercenatge = previousMonthsCompletePercenatge - currentMonthCompletePercentage;
                                        }

                                        projectsCompletedStatus.Remove(uniqueProject);

                                        var totalCompletedPercenatage = actualCompletedPercenatge + previousMonthsCompletePercenatge;

                                        projectsCompletedStatus.Add(uniqueProject, totalCompletedPercenatage);
                                       
                                    }

                                    chartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                    {
                                        label = requiredProject.ChartLabelWithStartEndDate,
                                        y = actualCompletedPercenatge
                                    });

                                    chartInfo.indexLabel = "{y}%";
                                    chartInfo.ProjestStartDate = requiredProject.ProjestStartDate;
                                    chartInfo.TargetClosingDate = requiredProject.TargetClosingDate;
                                    chartInfo.ActualClosedDate = requiredProject.ActualClosedDate;
                                    chartInfo.color = color;

                                }
                                else
                                {
                                    if (projectReportMonth.Month != "pending-month")
                                    {
                                        chartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                        {
                                            label = allProjects.Where(x => x.label == uniqueProject).FirstOrDefault().ChartLabelWithStartEndDate,
                                            y = 0
                                        });

                                        chartInfo.indexLabel = "{y}%";
                                        chartInfo.color = color;

                                        if (!projectsCompletedStatus.ContainsKey(uniqueProject))
                                        {
                                            projectsCompletedStatus.Add(uniqueProject, 0);
                                        }
                                    }
                                    else
                                    {
                                        var completedTillMonth = projectsCompletedStatus[uniqueProject].Value;
                                        var pendingCompletion = 100 - completedTillMonth;

                                        chartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                        {
                                            label = allProjects.Where(x => x.label == uniqueProject).FirstOrDefault().ChartLabelWithStartEndDate,
                                            y = pendingCompletion
                                        });

                                        chartInfo.indexLabel = "{y}%";
                                        chartInfo.color = color;
                                    }

                                }

                            }
                            ProjectsChart.Add(chartInfo);
                        }
                        //TODO

                        if (uniqueProjects.Count() > 2)
                        {
                            var height = uniqueProjects.Count() * 100;
                            model.ProjectReportbarChartHeight = height + "px";
                        }
                        else
                        {
                            var height = uniqueProjects.Count() * 300;
                            model.ProjectReportbarChartHeight = height + "px";
                        }
                    }

                    var RegularProjectChart = new List<ProjectChartInfo>();

                    var regularUniqueProjects = RegularProjectReports.Select(x => x.label.Trim()).Distinct().ToList();

                    if (regularUniqueProjects.Count > 0)
                    {
                        model.RegularProjectHeight = System.Convert.ToString(regularUniqueProjects.Count * 70);
                    }

                    if (RegularProjectReports != null && RegularProjectReports.Count > 0)
                    {
                        graphModel.IsRegularProjectReportExists = true;
                        var projectReportMonths = new List<MonthWiseReportModel>();

                        //if (StatusReportChartModel.ReportType != "Month Report")
                        //{
                        //    projectReportMonths.Add(carryForwardMonthInfo);
                        //}

                        ////Adding carry forward month to list
                        projectReportMonths.AddRange(requiredReportMonths);

                        //var dummyMonthToCalculatePendingPercenatge = new MonthWiseReportModel();
                        //dummyMonthToCalculatePendingPercenatge.Month = "pending-month";

                        //projectReportMonths.Add(dummyMonthToCalculatePendingPercenatge);

                        projectReportMonths.Reverse();

                        foreach (var regularProject in regularUniqueProjects)
                        {
                            var runningProjcetChartInfo = new ProjectChartInfo();
                            foreach (var projectReportMonth in projectReportMonths)
                            {
                                var monthName = projectReportMonth.Month;
                                var requiredRunningProject = RegularProjectReports.Where(x => x.label.Trim() == regularProject.Trim() && x.MonthName == projectReportMonth.Month).FirstOrDefault();
                                if (requiredRunningProject != null)
                                {
                                    runningProjcetChartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                    {
                                        label = monthName,
                                        y = requiredRunningProject.completionPercenatge
                                    });
                                }

                                else
                                {
                                    runningProjcetChartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                    {
                                        label = monthName,
                                        y = 0
                                    });

                                    //runningProjcetChartInfo.name = regularProject;
                                }
                            }

                            runningProjcetChartInfo.indexLabel = "";
                            //runningProjcetChartInfo.ProjestStartDate = requiredRunningProject.ProjestStartDate;
                            //runningProjcetChartInfo.TargetClosingDate = requiredRunningProject.TargetClosingDate;
                            //runningProjcetChartInfo.ActualClosedDate = requiredRunningProject.ActualClosedDate;
                            runningProjcetChartInfo.name = regularProject;

                            RegularProjectChart.Add(runningProjcetChartInfo);
                        }
                    }


                    //var chartProjectInfo = new ProjectChartInfoData();
                    //chartProjectInfo.data.AddRange(ProjectsChart);

                    model.ProjectChartsMonthWise = JsonConvert.SerializeObject(ProjectsChart);
                    model.ProjectComppletionDataPoints = JsonConvert.SerializeObject(ProjectComppletionDataPoints);
                    model.ProjectRemainingDataPoints = JsonConvert.SerializeObject(ProjectRemainingDataPoints);

                    model.RegularProjectChartsMonthWise = JsonConvert.SerializeObject(RegularProjectChart);

                    //TEMPLATE 3 UPDATES
                    model.MSpecifcAudDataoints = JsonConvert.SerializeObject(MonthWiseAuditTickets);
                    model.MSpecifcEffeAudDataoints = JsonConvert.SerializeObject(MonthWiseEfficientClosedTickets);
                    model.MSpecifcInEffeAudDataoints = JsonConvert.SerializeObject(MonthWiseInEfficientClosedTickets);

                    var TicketCategoryChart = new List<ProjectChartInfo>();
                    if (MonthWiseTotalCreatedTickes != null && MonthWiseTotalCreatedTickes.Count > 0)
                    {
                        var uniqueCategories = MonthWiseTotalCreatedTickes.Select(x => x.Ticket_Category).Distinct();

                        if (uniqueCategories != null && uniqueCategories.Count() > 1)
                        {
                            graphModel.IsCategoryBasedIncidentsExists = true;

                            var chartStatus = new List<string>();
                            var colors = ResourceManagement.Helpers.ColorCodes.Colors();

                            chartStatus.Add("Newly Raised");
                            chartStatus.Add("Closed");

                            var requiredStatuses = new List<CategoryModel>();

                            int colrIndex = 1;
                            foreach (var uniqueCategorie in uniqueCategories)
                            {
                                if (!string.IsNullOrEmpty(uniqueCategorie))
                                {
                                    foreach (var status in chartStatus)
                                    {
                                        requiredStatuses.Add(new CategoryModel()
                                        {
                                            CategoryName = uniqueCategorie,
                                            StausName = uniqueCategorie + " - " + status,
                                            ColorCode = colors[colrIndex],
                                        });

                                        colrIndex = colrIndex + 1;
                                    }
                                }
                            }

                            var categoryMonths = requiredReportMonths;
                            categoryMonths.Reverse();

                            foreach (var requiredStatus in requiredStatuses)
                            {
                                var categoryChartInfo = new ProjectChartInfo();

                                foreach (var requiredReportMonth in categoryMonths)
                                {
                                    IEnumerable<monthlyreports_Template1> maonthWiseCategoryTickets = null;
                                    if (requiredStatus.StausName.Contains("Newly Raised"))
                                    {
                                        maonthWiseCategoryTickets = MonthWiseTotalCreatedTickes.Where(x => x.Ticket_Category == requiredStatus.CategoryName && x.Uploaded_Month == requiredReportMonth.Month && x.Is_Newly_created == true);
                                    }
                                    if (requiredStatus.StausName.Contains("Closed"))
                                    {
                                        maonthWiseCategoryTickets = MonthWiseTotalCreatedTickes.Where(x => x.Ticket_Category == requiredStatus.CategoryName && x.Uploaded_Month == requiredReportMonth.Month && x.Is_Closed == true);
                                    }


                                    if (maonthWiseCategoryTickets != null && maonthWiseCategoryTickets.Count() > 0)
                                    {
                                        categoryChartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                        {
                                            label = requiredReportMonth.Month,
                                            y = maonthWiseCategoryTickets.Count(),
                                            color = requiredStatus.ColorCode
                                        });

                                        categoryChartInfo.indexLabel = "{y}";
                                        categoryChartInfo.name = requiredStatus.StausName;
                                        categoryChartInfo.indexLabelFontSize = 11;
                                        categoryChartInfo.legendMarkerColor = requiredStatus.ColorCode;
                                    }
                                    else
                                    {
                                        categoryChartInfo.dataPoints.Add(new ProjectGraphDataPoint.DataPoint()
                                        {
                                            label = requiredReportMonth.Month,
                                            y = 0,
                                            color = requiredStatus.ColorCode
                                        });
                                        categoryChartInfo.indexLabel = "{y}";
                                        categoryChartInfo.name = requiredStatus.StausName;
                                        categoryChartInfo.indexLabelFontSize = 11;
                                        categoryChartInfo.legendMarkerColor = requiredStatus.ColorCode;
                                    }
                                }

                                TicketCategoryChart.Add(categoryChartInfo);
                            }
                        }

                    }

                    model.CategoryWiseIncidents = JsonConvert.SerializeObject(TicketCategoryChart);

                    graphModel.ViewModel.Add(model);

                }

            }
            return PartialView(graphModel);
        }

        private static List<ProjectGraphDataPoint.Reports> ProjectReport(List<ProjectGraphDataPoint.Reports> ProjectReports, List<monthlyreports_Template2> projectDetailsForSelectedMonth, bool isCarryForardMonthInfo = false)
        {
            foreach (var projectDetailForSelectedMonth in projectDetailsForSelectedMonth)
            {
                var projectStartMonthYear = "";
                var projectEndMonthYear = "";

                var requiredProjectStartEndDate = "";

                if (projectDetailForSelectedMonth.Project_Created_Date != DateTime.MinValue)
                {
                    projectStartMonthYear = projectDetailForSelectedMonth.Project_Created_Date.ToString("MMM") + "," + projectDetailForSelectedMonth.Project_Created_Date.ToString("yyyy");
                }

                if (projectDetailForSelectedMonth.Project_Closing_Date_Target != DateTime.MinValue)
                {
                    projectEndMonthYear = " to " + projectDetailForSelectedMonth.Project_Closing_Date_Target?.ToString("MMM") + "," + projectDetailForSelectedMonth.Project_Closing_Date_Target?.ToString("yyyy");
                }

                if (projectStartMonthYear != "" && projectEndMonthYear != "")
                {
                    requiredProjectStartEndDate = " (" + projectStartMonthYear + projectEndMonthYear + ") ";
                }
                if (projectStartMonthYear != "" && projectEndMonthYear == "")
                {
                    requiredProjectStartEndDate = " (" + projectStartMonthYear + ") ";
                }

                ProjectReports.Add(new ProjectGraphDataPoint.Reports()
                {
                    label = projectDetailForSelectedMonth.Project_Name,
                    ChartLabelWithStartEndDate = projectDetailForSelectedMonth.Project_Name + requiredProjectStartEndDate,
                    completionPercenatge = projectDetailForSelectedMonth.CompletedPercentage,
                    remainingPercenatge = projectDetailForSelectedMonth.RemainingPercentage,
                    MonthName = projectDetailForSelectedMonth.Uploaded_Month,
                    ProjestStartDate = projectDetailForSelectedMonth.Project_Created_Date,
                    ActualClosedDate = projectDetailForSelectedMonth.Project_Closed_Date_Actual,
                    TargetClosingDate = projectDetailForSelectedMonth.Project_Closing_Date_Target,
                    IsCarryForwardMonth = isCarryForardMonthInfo,
                    Summary = projectDetailForSelectedMonth.Project_Summary,
                    ProjectCategory = projectDetailForSelectedMonth.Project_Category
                });
            }

            return ProjectReports;
        }

        public static SelectedReportMonthModel SelectedMonthRelatedInfo(DateTime inputDateTime, StatusReportChartModel StatusReportChartModel)
        {
            return new SelectedReportMonthModel()
            {
                MonthEndDate = System.DateTime.DaysInMonth(inputDateTime.Year, inputDateTime.Month).ToString(),
                MonthName = inputDateTime.ToString("MMM"),
                year = inputDateTime.Year.ToString(),
                ShortFormat = inputDateTime.ToString("MMM") + "-" + inputDateTime.Year.ToString(),
                ReportType = StatusReportChartModel.ReportType
            };
        }

        public string MonthShortFormat(DateTime datetime)
        {
            return datetime.ToString("MMM") + "-" + datetime.Year.ToString();
        }

        public ActionResult UploadedStatusReportView(StatusReportChartModel StatusReportChartModel)
        {
            RMA_UploadedStatusReportViewModel model = StatusUploadedReportModel(StatusReportChartModel);
            return PartialView(model);
        }

        private RMA_UploadedStatusReportViewModel StatusUploadedReportModel(StatusReportChartModel StatusReportChartModel)
        {
            var model = new RMA_UploadedStatusReportViewModel();
            model.AjaxModel = StatusReportChartModel;
            var selectedReportedMonthStartDate = new DateTime();
            var requiredReportMonths = new List<MonthWiseReportModel>();

            if (StatusReportChartModel.ReportType == "Month Report")
            {
                var selectedMonth = StatusReportChartModel.Month;
                var selectedMonthNumber = System.Convert.ToInt32(selectedMonth.Split('&')[1]);
                selectedReportedMonthStartDate = new DateTime(StatusReportChartModel.Year, selectedMonthNumber, 1);
                requiredReportMonths.Add(ReportGetMonthInfo(selectedReportedMonthStartDate));

                model.SelectedReportMonth = SelectedMonthRelatedInfo(selectedReportedMonthStartDate, StatusReportChartModel);
                var startingMonthForTheReport = ReportGetMonthInfo(selectedReportedMonthStartDate);

                model.SelectedReportMonth.ReportStartMonth = startingMonthForTheReport.Month;
            }

            else
            {
                var selectedMonth = StatusReportChartModel.Month;
                var selectedMonthNumbers = selectedMonth.Split('|');
                int firstMonthFromTheSelection = 0;
                var reportStartMonth = "";

                foreach (var selectedMonthNumber in selectedMonthNumbers)
                {
                    if (selectedMonthNumber != string.Empty)
                    {

                        var selectedMonthNum = System.Convert.ToInt32(selectedMonthNumber.Split('&')[1]);
                        selectedReportedMonthStartDate = new DateTime(StatusReportChartModel.Year, selectedMonthNum, 1);
                        var selectedMonthInfo = ReportGetMonthInfo(selectedReportedMonthStartDate);
                        requiredReportMonths.Add(selectedMonthInfo);

                        if (firstMonthFromTheSelection == 0)
                        {
                            reportStartMonth = selectedMonthInfo.Month;
                        }
                        requiredReportMonths.Add(selectedMonthInfo);
                        firstMonthFromTheSelection++;

                        model.SelectedReportMonth = SelectedMonthRelatedInfo(selectedReportedMonthStartDate, StatusReportChartModel);
                    }
                }
                model.SelectedReportMonth.ReportStartMonth = reportStartMonth;
            }

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                foreach (var empID in StatusReportChartModel.EmployeeID)
                {
                    var empReportModel = new RMA_UploadedStatusReportModel();

                    empReportModel.AMBC_Active_Emp_view = db.AMBC_Active_Emp_view.Where(x => x.Employee_ID == empID).FirstOrDefault();
                    empReportModel.EmployeeImage = EmployeeProfileImagePath(empReportModel.AMBC_Active_Emp_view.Employee_ID.ToString());

                    Decimal emplyeeAvailabiliy = 0;

                    foreach (var requiredReportMonth in requiredReportMonths)
                    {
                        var specifMonthAvailablity = db.consultantavailiability_Final.Where(Employee => Employee.Employee_Code == empID && Employee.Month_Year == requiredReportMonth.Month).FirstOrDefault();

                        if (specifMonthAvailablity != null)
                        {
                            var consultantAvailabity = specifMonthAvailablity.ConslAvl.Replace("%", "");
                            emplyeeAvailabiliy += consultantAvailabity.Contains('.') ? System.Convert.ToDecimal(consultantAvailabity.Split('.')[0]) : System.Convert.ToDecimal(consultantAvailabity);
                        }

                        if (StatusReportChartModel.TemplateNumber == "Template1")
                        {
                            var selectedMonthTickets = db.monthlyreports_Template1.Where(ticket => ticket.Uploaded_Month == requiredReportMonth.Month && ticket.Is_Cancelled == false && ticket.IsAuditReport == false && ticket.EmplyeeID == empID && StatusReportChartModel.ToolName.Contains(ticket.TicketingToolName)).ToList();

                            if (StatusReportChartModel.IsDelete && selectedMonthTickets != null && selectedMonthTickets.Count > 0)
                            {
                                db.monthlyreports_Template1.RemoveRange(selectedMonthTickets);
                                db.SaveChanges();
                                selectedMonthTickets = null;
                            }

                            if (selectedMonthTickets != null && selectedMonthTickets.Count > 0)
                            {
                                empReportModel.Template1Reports.AddRange(selectedMonthTickets);
                            }
                        }

                        //TEMPLATE2 code updates
                        if (StatusReportChartModel.TemplateNumber == "Template2")
                        {
                            var monthProjectReport = db.monthlyreports_Template2.Where(project => project.Uploaded_Month == requiredReportMonth.Month && project.Is_Cancelled == false && project.EmplyeeID == empID).ToList();
                            if (StatusReportChartModel.IsDelete && monthProjectReport != null && monthProjectReport.Count > 0)
                            {
                                db.monthlyreports_Template2.RemoveRange(monthProjectReport);
                                db.SaveChanges();
                                monthProjectReport = null;
                            }

                            if (monthProjectReport != null && monthProjectReport.Count > 0)
                            {
                                empReportModel.Template2Reports.AddRange(monthProjectReport);
                            }

                        }

                        //TEMPLATE3 code updates
                        if (StatusReportChartModel.TemplateNumber == "Template3")
                        {
                            var selectedMonthTickets = db.monthlyreports_Template1.Where(ticket => ticket.Uploaded_Month == requiredReportMonth.Month && ticket.Is_Cancelled == false && ticket.IsAuditReport == true && ticket.EmplyeeID == empID && StatusReportChartModel.ToolName.Contains(ticket.TicketingToolName)).ToList();
                            if (selectedMonthTickets != null && selectedMonthTickets.Count > 0)
                            {
                                empReportModel.Template1Reports.AddRange(selectedMonthTickets);
                            }
                        }

                    }

                    //EMPLOYEE DETAILS
                    var employeeTotalAvailabity = (emplyeeAvailabiliy / requiredReportMonths.Count).ToString();
                    Int32 availabity = employeeTotalAvailabity.Contains('.') ? System.Convert.ToInt32(employeeTotalAvailabity.Split('.')[0]) + 1 : System.Convert.ToInt32(employeeTotalAvailabity);
                    empReportModel.EmaployeeAvailabity = availabity;

                    model.ViewModel.Add(empReportModel);
                }
            }

            return model;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExportStatusReports(string GridHtml)
        {
            StatusReportChartModel ajaxReportModel = JsonConvert.DeserializeObject<StatusReportChartModel>(GridHtml);
            List<SourceFile> sourceFiles = new List<SourceFile>();
            var requiredZIPFileName = ajaxReportModel.ClientName + "-" + ajaxReportModel.TemplateType + "-" + ajaxReportModel.ReportType;

            string htmlContent = "";
            var excelFileName = "";

            var isMultipleEmployeesSelected = ajaxReportModel.EmployeeID.Count() == 1 ? false : true;

            if (ajaxReportModel != null)
            {
                foreach (var employee in ajaxReportModel.EmployeeID)
                {
                    var selectedEmp = new List<string>();
                    selectedEmp.Add(employee);

                    ajaxReportModel.EmployeeID = selectedEmp;
                    var model = StatusUploadedReportModel(ajaxReportModel);
                    model.IsExcelReport = true;

                    htmlContent = RenderPartialToString(this, "UploadedStatusExcelReportView", model, ViewData, TempData);

                    byte[] byteArray = Encoding.ASCII.GetBytes(htmlContent);

                    excelFileName = model.ViewModel[0].AMBC_Active_Emp_view.Employee_Name + "-" + ajaxReportModel.TemplateType + "-" + ajaxReportModel.ReportType + ".xls";

                    sourceFiles.Add(new SourceFile()
                    {
                        FileBytes = byteArray,
                        Extension = ".xls",
                        Name = excelFileName
                    });

                }
            }

            if (!isMultipleEmployeesSelected)
            {
                return File(Encoding.ASCII.GetBytes(htmlContent), "application/vnd.ms-excel", excelFileName);
            }
            else
            {
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
                Response.AddHeader("Content-Disposition", "attachment; filename=" + requiredZIPFileName + ".zip");
                return File(fileBytes, "application/zip");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SampleIncidentReport(string GridHtml)
        {

            var filePath = @"C:\inetpub\wwwroot\Reports\Incident-Report.xlsx";
            var fileName = "Incident-Template.xlsx";
            var mimeType = "application/vnd.ms-excel";
            return File(new FileStream(filePath, FileMode.Open), mimeType, fileName);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SampleProjectReport(string GridHtml)
        {

            var filePath = @"C:\inetpub\wwwroot\Reports\Project-Report.xlsx";
            var fileName = "Project-Template.xlsx";
            var mimeType = "application/vnd.ms-excel";
            return File(new FileStream(filePath, FileMode.Open), mimeType, fileName);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SampleAuditReport(string GridHtml)
        {
            var filePath = @"C:\inetpub\wwwroot\Reports\Audit-Report.xlsx";
            var fileName = "Audit-Template.xlsx";
            var mimeType = "application/vnd.ms-excel";
            return File(new FileStream(filePath, FileMode.Open), mimeType, fileName);
        }

        [HttpPost]
        public JsonResult GetProjectBasedOnEmpId(string empID, string ClientName)
        {
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var employeeInfo = new List<AMBC_Active_Emp_view>();
                if (string.IsNullOrEmpty(ClientName))
                {
                    employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(empID) && a.Project_Status == "Active").ToList();
                }
                else
                {
                    employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(empID) && a.Project_Status == "Active" && a.Client == ClientName).ToList();
                }

                if (employeeInfo != null && employeeInfo.Count() > 0)
                {
                    return Json(employeeInfo, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [HttpPost]
        //projectID IS from Active EMP View table
        // for one consultant for one client will have one project code
        //ClientProject Name, this is the project which emp worked upon
        public JsonResult GetClientProjectsBasedOnEmpId(string empID, int? projectID, string projectName, string projectCategory)
        {
            var empClientProject = new List<monthlyreports_Template2>();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {

                if (projectID != null && !string.IsNullOrEmpty(empID) && string.IsNullOrWhiteSpace(projectName))
                {
                    empClientProject = db.monthlyreports_Template2.Where(a => a.EmplyeeID.Equals(empID) && a.ProjectID == projectID && a.Project_Category == projectCategory).DistinctBy(x => x.Project_Name).OrderBy(x => x.Project_Name).ToList();
                }
                else
                {
                    empClientProject = db.monthlyreports_Template2.Where(a => a.EmplyeeID.Equals(empID) && a.ProjectID == projectID && a.Project_Name == projectName && a.Project_Category == projectCategory).OrderBy(x => x.CompletedPercentage).ToList();
                    empClientProject.Reverse();
                }

                if (empClientProject != null && empClientProject.Count() > 0)
                {
                    return Json(JsonConvert.SerializeObject(empClientProject.DistinctBy(x => x.Project_Name)), JsonRequestBehavior.AllowGet);
                }
            }
            return Json(JsonConvert.SerializeObject(empClientProject), JsonRequestBehavior.AllowGet); ;
        }

        [HttpPost]
        public JsonResult UpdateRolesAndResposibilties(string empID, string ClientName, string Roles)
        {
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var employeeInfo = db.AMBC_Active_Emp_view.Where(a => a.Employee_ID.Equals(empID) && a.Project_Status == "Active" && a.Client == ClientName).FirstOrDefault();
                if (employeeInfo != null)
                {
                    employeeInfo.Roles_Responsibilities = Roles;
                    db.SaveChanges();
                    return Json(employeeInfo, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        public JsonResult ISStatusReportSubmitted(StatusReportRemainderModel StatusReportRemainderModel)
        {
            var response = new JsonResponseModel();
            var model = new StatusReportRemainderViewModel();
            var clients = new List<string>();
            if (StatusReportRemainderModel.ClientName.Contains(","))
            {
                var empClients = StatusReportRemainderModel.ClientName.Split(',').Reverse();
                clients.AddRange(empClients);
            }
            else
            {
                clients.Add(StatusReportRemainderModel.ClientName);
            }

            foreach (var client in clients)
            {
                StatusReportRemainderModel.ClientName = client.TrimStart().TrimEnd();
                StatusReportRemainderDetails(StatusReportRemainderModel, model);
                if (model.RemainderEmployees != null && model.RemainderEmployees.Count > 0)
                {
                    response.StatusCode = 404;
                    break;
                }
            }

            if (response.StatusCode == 404)
            {
                response.Message = "Status Report not submitted for the selected period!";
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            return Json(null);
        }


        public ActionResult StatusReportRemainder(StatusReportRemainderModel StatusReportRemainderModel)
        {
            var model = new StatusReportRemainderViewModel();
            StatusReportRemainderDetails(StatusReportRemainderModel, model);
            return PartialView(model);
        }

        private static StatusReportRemainderViewModel StatusReportRemainderDetails(StatusReportRemainderModel StatusReportRemainderModel, StatusReportRemainderViewModel model)
        {
            model.ClientName = StatusReportRemainderModel.ClientName;

            var selectedReportedMonthStartDate = new DateTime();
            var requiredReportMonths = new List<MonthWiseReportModel>();

            if (StatusReportRemainderModel.ReportType == "Month Report")
            {
                var selectedMonth = StatusReportRemainderModel.Period;
                var selectedMonthNumber = System.Convert.ToInt32(selectedMonth.Split('&')[1]);
                selectedReportedMonthStartDate = new DateTime(System.Convert.ToInt32(StatusReportRemainderModel.Year), selectedMonthNumber, 1);
                requiredReportMonths.Add(ReportGetMonthInfo(selectedReportedMonthStartDate));
            }
            else
            {
                var selectedMonth = StatusReportRemainderModel.Period;
                var selectedMonthNumbers = selectedMonth.Split('|');

                foreach (var selectedMonthNumber in selectedMonthNumbers)
                {
                    if (selectedMonthNumber != string.Empty)
                    {
                        var selectedMonthNum = System.Convert.ToInt32(selectedMonthNumber.Split('&')[1]);
                        selectedReportedMonthStartDate = new DateTime(System.Convert.ToInt32(StatusReportRemainderModel.Year), selectedMonthNum, 1);
                        var selectedMonthInfo = ReportGetMonthInfo(selectedReportedMonthStartDate);
                        requiredReportMonths.Add(selectedMonthInfo);
                    }
                }
            }

            foreach (var requiredReportMonth in requiredReportMonths)
            {
                var remainderEmplyees = new List<StatusReportRemainderEmpModel>();

                foreach (var empID in StatusReportRemainderModel.Employees)
                {
                    var remainderEmpInfo = new StatusReportRemainderEmpModel();
                    using (TimeSheetEntities db = new TimeSheetEntities())
                    {
                        var currentEmpTemplate1Repots = db.monthlyreports_Template1.Where(a => a.EmplyeeID.Equals(empID) && a.Uploaded_Month == requiredReportMonth.Month && a.Client_Name == StatusReportRemainderModel.ClientName).ToList();
                        if (currentEmpTemplate1Repots != null && currentEmpTemplate1Repots.Count() > 0)
                        {
                            continue;
                        }
                        var currentEmpTemplate2Repots = db.monthlyreports_Template2.Where(a => a.EmplyeeID.Equals(empID) && a.Uploaded_Month == requiredReportMonth.Month && a.Client_Name == StatusReportRemainderModel.ClientName).ToList();
                        if (currentEmpTemplate1Repots != null && currentEmpTemplate1Repots.Count() > 0)
                        {
                            continue;
                        }

                        var currentEmpInfo = db.AMBC_Active_Emp_view.Where(emp => emp.Employee_ID == empID && emp.Client == StatusReportRemainderModel.ClientName && emp.Project_Status == "Active").ToList();

                        if (currentEmpInfo != null && currentEmpInfo.Count() > 0)
                        {
                            remainderEmpInfo.RemainderMonthInfo = requiredReportMonth;
                            remainderEmpInfo.RemainderEmployee = currentEmpInfo[0];
                        }
                    }

                    model.RemainderEmployees.Add(remainderEmpInfo);
                }
            }

            return model;
        }

        public JsonResult SendStatusReportRemainderEmail(StatusReportRemainderEmailModel StatusReportEmpRemainder)
        {
            try
            {
                if (StatusReportEmpRemainder != null && StatusReportEmpRemainder.selctedempmodel.Count() > 0)
                {
                    var remainderMonth = StatusReportEmpRemainder.RemainderMonth;

                    if (StatusReportEmpRemainder.SendSingleEmailToAllEmp == false)
                    {
                        foreach (var email in StatusReportEmpRemainder.selctedempmodel)
                        {
                            using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], email.selectedemployeeemail))
                            {
                                mm.Subject = "REMINDER: Dashboard Report for the month - " + email.RemainderMonth;

                                var emailBody = RenderPartialToString(this, "StatusReportRemainderEmail", email, ViewData, TempData);

                                mm.Body = emailBody;

                                if (email.selectedemploymanageremail != string.Empty)
                                {
                                    mm.CC.Add(email.selectedemploymanageremail);
                                }

                                if (StatusReportEmpRemainder.LogedInEmpEmail != string.Empty)
                                {
                                    mm.CC.Add(StatusReportEmpRemainder.LogedInEmpEmail);
                                }

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
                    }

                    //else
                    //{
                    //    var model = new StatusReport_RemainderEmailSelectedEmpModel();
                    //    using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], StatusReportEmpRemainder.selctedempmodel[0].selectedemployeeemail))
                    //    {
                    //        mm.Subject = "REMINDER: Dashboard Report for the month - " + remainderMonth;
                    //        int firstSelectedEmp = 0;

                    //        foreach (var selectedEmp in StatusReportEmpRemainder.selctedempmodel)
                    //        {
                    //            if (firstSelectedEmp > 0)
                    //            {
                    //                mm.To.Add(selectedEmp.selectedemployeeemail);
                    //            }
                    //            firstSelectedEmp++;

                    //            mm.CC.Add(selectedEmp.selectedemploymanageremail);
                    //        }

                    //        model.SendSingleEmailToAllEmp = StatusReportEmpRemainder.SendSingleEmailToAllEmp;

                    //        var emailBody = RenderPartialToString(this, "StatusReportRemainderEmail", model, ViewData, TempData);

                    //        mm.Body = emailBody;

                    //        if (StatusReportEmpRemainder.LogedInEmpEmail != string.Empty)
                    //        {
                    //            mm.CC.Add(StatusReportEmpRemainder.LogedInEmpEmail);
                    //        }

                    //        mm.IsBodyHtml = true;
                    //        SmtpClient smtp = new SmtpClient();
                    //        smtp.Host = ConfigurationManager.AppSettings["SMTPHost"];
                    //        smtp.EnableSsl = true;
                    //        NetworkCredential credentials = new NetworkCredential();
                    //        credentials.UserName = ConfigurationManager.AppSettings["SMTPUserName"];
                    //        credentials.Password = ConfigurationManager.AppSettings["SMTPPassword"];
                    //        smtp.UseDefaultCredentials = true;
                    //        smtp.Credentials = credentials;
                    //        smtp.Port = System.Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    //        smtp.Send(mm);
                    //    }
                    //}

                }

                var emailRemainderResponse = new JsonResponseModel();

                emailRemainderResponse.StatusCode = 200;
                emailRemainderResponse.Message = "Status Report Remainder Email Sent Successfully!";
                return Json(emailRemainderResponse);
            }
            catch (Exception ex)
            {
            }

            return Json(null);
        }
    }
}

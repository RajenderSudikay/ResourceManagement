using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    using Models;
    using OfficeOpenXml;
    using SelectPdf;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Text.Json;
    using System.Web;
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
                                    LeaveType = empHolidayInf.comments

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
                                    LeaveType = empHalfDayHolidayInf.Leave_Type

                                };
                                reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
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
                            LeaveType = empHolidayInf.comments

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
                            LeaveType = empHalfDayHolidayInf.Leave_Type

                        };
                        reportModel.timeSheetLeaveOrHolidayInfo.Add(leaveInfoModel);
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
                            respone.jsonResponse.Message = "Leave Submitted Successfully!";
                        }
                    }

                    respone.AjaxleaveModel = new RMA_LeaveModel();
                    respone.AjaxleaveModel = leaveModel;

                    respone.jsonResponse = new JsonResponseModel();
                    respone.jsonResponse.StatusCode = 200;

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


        public JsonResult SubmitLeavesEmailGenerate(RMA_LeaveModel emailLeaveModel)
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
                                respone.jsonResponse.StatusCode = 200;
                                respone.jsonResponse.Message = "Leave Apply Email Sent Successfully!";
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                respone.jsonResponse.StatusCode = 500;
                respone.jsonResponse.Message = ex.Message;
            }

            return Json(respone, JsonRequestBehavior.AllowGet);
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
    }
}

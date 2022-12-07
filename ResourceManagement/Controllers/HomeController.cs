using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;


namespace ResourceManagement.Controllers
{
    using Models;
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Web.Helpers;
    using static ResourceManagement.Helpers.DateHelper;

    public class HomeController : Controller
    {
        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Charts()
        {
            return View("");
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
                    //foreach (var weekreport in weekreportmodel)
                    //{
                        hoursSpent += weekreportmodel.Where(x => x.weekday == weekday).Count() > 0 ? weekreportmodel.Where(x => x.weekday == weekday).FirstOrDefault().hoursspent : 0;
                        dataPoints.Add(new DataPoint(weekday, weekreportmodel.Where(x => x.weekday == weekday).Count() > 0 ? weekreportmodel.Where(x => x.weekday == weekday).FirstOrDefault().hoursspent : 0));
                    //}
                }
            }

            //dataPoints.Add(new DataPoint("Monday", weekreportmodel.Where(x => x.weekday == "Monday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Monday").FirstOrDefault().hoursspent : 0));
            //dataPoints.Add(new DataPoint("Tuesday", weekreportmodel.Where(x => x.weekday == "Tuesday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Tuesday").FirstOrDefault().hoursspent : 0));
            //dataPoints.Add(new DataPoint("Wednesday", weekreportmodel.Where(x => x.weekday == "Wednesday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Wednesday").FirstOrDefault().hoursspent : 0));
            //dataPoints.Add(new DataPoint("Thursday", weekreportmodel.Where(x => x.weekday == "Thursday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Thursday").FirstOrDefault().hoursspent : 0));
            //dataPoints.Add(new DataPoint("Friday", weekreportmodel.Where(x => x.weekday == "Friday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Friday").FirstOrDefault().hoursspent : 0));
            //dataPoints.Add(new DataPoint("Saturday", weekreportmodel.Where(x => x.weekday == "Saturday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Saturday").FirstOrDefault().hoursspent : 0));
            //dataPoints.Add(new DataPoint("Sunday", weekreportmodel.Where(x => x.weekday == "Sunday").Count() > 0 ? weekreportmodel.Where(x => x.weekday == "Sunday").FirstOrDefault().hoursspent : 0));



            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            ViewBag.TotalHoursSpent = hoursSpent;


            return PartialView();
        }

        //public JsonResult WeeklyChartReport()
        //{
        //    var weekReport = new List<WeekReportModel>();

        //    weekReport.Add(new WeekReportModel() { Day= "Monday", Units=123  });
        //    weekReport.Add(new WeekReportModel() { Day = "Tuesday", Units = 552 });
        //    weekReport.Add(new WeekReportModel() { Day = "Wednesday", Units = 342 });
        //    weekReport.Add(new WeekReportModel() { Day = "Thursday", Units = 431 });
        //    weekReport.Add(new WeekReportModel() { Day = "Friday", Units = 251 });
        //    weekReport.Add(new WeekReportModel() { Day = "Saturday", Units = 100 });


        //    var usersList = new List<WeekReportModel>
        //    {
        //        new WeekReportModel
        //        {
        //            Day = "Monday",
        //            Units = 123,

        //        },
        //        new WeekReportModel
        //        {
        //            Day = "Tuesday",
        //            Units = 456,

        //        },
        //        new WeekReportModel
        //        {
        //            Day = "Wednesday",
        //            Units = 345                   
        //        }
        //    };


        //    return Json(usersList, JsonRequestBehavior.AllowGet);
        //}

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
                                //var projectInfo = db.emp_project.Where(a => a.assign_emp_id.Equals(loginModel.att_username)).FirstOrDefault();
                                //if (projectInfo != null)
                                //{
                                //    employeeModel.projectInfo = projectInfo;
                                //}

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
                var startDate = System.DateTime.Now.AddMonths(-2);
                var endDate = System.DateTime.Now;

                //DateTime startDate = DateTime.Parse(InitialDate);
                //DateTime endDate = DateTime.Parse(todayDate);

                var conleaves = db.con_leaveupdate.Where(a => a.employee_id.Equals(empModel.AMBC_Active_Emp_view.Employee_ID) && a.leavedate >= startDate && a.leavedate <= endDate).ToList();
                if (conleaves != null && conleaves.Count > 0)
                {
                    //leaveOrHolidayData.AddRange(conleaves.Select(x => x.leavedate.ToString()));
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
                    //leaveOrHolidayData.AddRange(ambcLeaves.Select(x => x.holiday_date.ToString()));
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
            //if (Session["UserModel"] != null)
            //{
            //    var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            //    if (employeeModel != null && employeeModel.AMBC_Active_Emp_view != null && !string.IsNullOrWhiteSpace(employeeModel.AMBC_Active_Emp_view.Employee_ID))
            //    {
            //        employeeModel.leaveOrHolidayInfo = GetLeaveandHolidayInfofromDb(employeeModel);
            //        return View(employeeModel);
            //    }
            //    else
            //    {
            //        return RedirectToAction("Login");
            //    }
            //}
            //return RedirectToAction("Login");

            return View();
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
    }
}
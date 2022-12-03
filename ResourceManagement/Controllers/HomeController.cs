using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    using Models;
    using System;
    using System.Globalization;
    using System.Text.Json;

    public class HomeController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(emplogin loginModel)
        {
            if (ModelState.IsValid)
            {
                var employeeModel = new RMA_EmployeeModel();
                using (TimeSheetEntities db = new TimeSheetEntities())
                {
                    var loginObj = db.emplogins.Where(a => a.att_username.Equals(loginModel.att_username) && a.att_password.Equals(loginModel.att_password) && a.emp_status).FirstOrDefault();
                    if (loginObj != null)
                    {
                        var employeeInfo = db.emp_info.Where(a => a.employee_id.Equals(loginModel.att_username)).FirstOrDefault();
                        if (employeeInfo != null)
                        {
                            employeeModel.empInfo = employeeInfo;
                            var projectInfo = db.emp_project.Where(a => a.assign_emp_id.Equals(loginModel.att_username)).FirstOrDefault();
                            if (projectInfo != null)
                            {
                                employeeModel.projectInfo = projectInfo;
                            }

                        }

                        Session["UserModel"] = employeeModel;

                        return RedirectToAction("Dashboard");
                    }
                }
            }
            return View(loginModel);
        }

        [HttpGet]        
        public JsonResult GetLeaveandHolidayInfofromDb(RMA_TimeSheetWeekData timeSheetWeekInputModel)
        {
            var leaveOrHolidayData = new List<string>();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                DateTime startDate = DateTime.Parse(timeSheetWeekInputModel.StartDate);
                DateTime endDate = DateTime.Parse(timeSheetWeekInputModel.EndDate);

                var conleaves = db.con_leaveupdate.Where(a => a.employee_id.Equals(timeSheetWeekInputModel.EmpId) && a.leavedate >= startDate && a.leavedate <= endDate).ToList();
                if (conleaves != null && conleaves.Count > 0)
                {
                    //leaveOrHolidayData.AddRange(conleaves.Select(x => x.leavedate.ToString()));
                    foreach (var conleave in conleaves)
                    {
                        var conLaveDate = DateTime.ParseExact(conleave.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString();
                        leaveOrHolidayData.Add(conLaveDate);
                    }
                }

                DateTime startDate1 = DateTime.Parse(timeSheetWeekInputModel.StartDate);
                DateTime endDate1 = DateTime.Parse(timeSheetWeekInputModel.EndDate);

                var ambcLeaves = db.tblambcholidays.Where(b => b.holiday_date >= startDate && b.holiday_date <= endDate1).ToList();
                if (ambcLeaves != null && ambcLeaves.Count > 0)
                {

                    //leaveOrHolidayData.AddRange(ambcLeaves.Select(x => x.holiday_date.ToString()));
                    foreach (var ambcLeave in ambcLeaves)
                    {
                        var conLaveDate = DateTime.ParseExact(ambcLeave.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString();
                        leaveOrHolidayData.Add(ambcLeave.holiday_date.ToString());
                    }
                }
            }

            if (leaveOrHolidayData != null && leaveOrHolidayData.Count > 0)
            {
                return Json(JsonSerializer.Serialize(leaveOrHolidayData), JsonRequestBehavior.AllowGet);
            }

            return Json(null);
        }

        public ActionResult Dashboard()
        {
            if (Session["UserModel"] != null)
            {
                var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

                if (employeeModel != null && employeeModel.empInfo != null && !string.IsNullOrWhiteSpace(employeeModel.empInfo.employee_id))
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
            if (Session["UserModel"] != null)
            {
                var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

                if (employeeModel != null && employeeModel.empInfo != null && !string.IsNullOrWhiteSpace(employeeModel.empInfo.employee_id))
                {
                    return View(employeeModel);
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Login");
        }

        public JsonResult AddTimeSheet(List<ambctaskcapture> timesheetmodel)
        {
            var response = UpdateTimeSheetStatus(timesheetmodel);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public JsonResponseModel UpdateTimeSheetStatus(List<ambctaskcapture> timesheetmodel)
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
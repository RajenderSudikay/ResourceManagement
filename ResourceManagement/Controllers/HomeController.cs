using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    using Models;
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

        public JsonResult AddTimeSheet(List<RMA_TimeSheetAjaxModel> timesheetmodel)
        {
            var response = UpdateTimeSheetStatus(timesheetmodel);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public JsonResponseModel UpdateTimeSheetStatus(List<RMA_TimeSheetAjaxModel> timesheetmodel)
        {
            var respone = new JsonResponseModel();
            try
            {
                var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
                using (var context = new TimeSheetEntities())
                {
                    var tasks = new List<ambctaskcapture>();
                    foreach (var model in timesheetmodel)
                    {
                        var timesheet = new ambctaskcapture()
                        {
                            taskdate = System.Convert.ToDateTime(model.Date),
                            category = model.Category,
                            incidentnumber = model.IncidentNumber,
                            taskdetails = model.IncidentDescription,
                            requester = model.Requester,
                            callpriority = model.Urgency,
                            callstatus = model.Status,
                            closeddate = System.Convert.ToDateTime(model.ClosedDate),
                            timespent = System.Convert.ToInt32(model.TimeSpent),
                            comments = model.Comments,
                            employeename = employeeModel.empInfo.employee_name,
                            employeeid = employeeModel.empInfo.employee_id,
                            empdesignation = employeeModel.empInfo.employee_desg,
                            empmanager = employeeModel.empInfo.employee_report_manager,
                            empcontactnumber = employeeModel.empInfo.employee_mobile,
                            clientname = employeeModel.projectInfo.proj_client,
                            projectname = employeeModel.projectInfo.proj_name,
                            projstatus = employeeModel.projectInfo.project_staus,
                            uniquekey = model.Date + "_" + model.IncidentNumber + "_" + model.TimeSpent,
                            weekno = 29
                        };
                        tasks.Add(timesheet);
                    }

                    context.ambctaskcaptures.AddRange(tasks);
                    context.SaveChanges();
                    respone.StatusCode = 200;
                    respone.Message = "TimeSheet added successfully!";
                }
            }
            catch (System.Exception ex)
            {
                respone.StatusCode = 500;               
                if(ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');

                    foreach(var actuallError in actuallErrors)
                    {
                        if(actuallError.ToLowerInvariant().Contains("duplicate key value is"))
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
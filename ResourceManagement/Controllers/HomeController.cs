using Dapper;
using ResourceManagement.Entity_Framework;
using ResourceManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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

        public ActionResult AddTimeSheet(List<RMA_TimeSheetAjaxModel> timesheetmodel)
        {
            //Employye
            //Database
            //Employee 
            //
            ViewBag.Message = "Your contact page.";
            UpdateTimeSheetStatus(timesheetmodel);
            return View("~/Views/Home/Contact.cshtml");
        }


        public void UpdateTimeSheetStatus(List<RMA_TimeSheetAjaxModel> timesheetmodel)
        {
            //SqlDataReader rdr = null;
            //SqlConnection con = null;
            //SqlCommand cmd = null;

            //con = new SqlConnection(ConfigurationManager.ConnectionStrings["TimesheetDBEntities"].ToString());
            //String FirstName;
            //String LastName;

            //con.Open();
            //string CommandText = "SELECT employee_id, employee_name FROM [emp_info] WHERE employee_id='1001'";
            //cmd = new SqlCommand(CommandText);
            //cmd.Connection = con;
            //rdr = cmd.ExecuteReader();

            //while (rdr.Read())
            //{
            //    FirstName = rdr["employee_id"].ToString();
            //    LastName = rdr["employee_name"].ToString();
            //}

            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;

            using (var context = new TimeSheetEntities())
            {
                // Create and save a new Students
                Console.WriteLine("Adding new students");

                foreach (var model in timesheetmodel)
                {
                    //var customers = context.Set<TimeSheet>();
                    var timesheet = new ambctaskcapture()
                    {
                        taskdate = System.Convert.ToDateTime(model.Date),
                        category = model.Category,
                        incidentnumber = System.Convert.ToInt32(model.IncidentNumber),
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

                    context.ambctaskcaptures.Add(timesheet);
                    context.SaveChanges();

                    //var abc = new List<string>();

                    //abc.Add("employee_id");
                    //abc.Add("employee_name");
                    //abc.Add("employee_desg");

                    //var student = (from s in context.emp_info
                    //               where s.employee_id == "1001"
                    //               select s).FirstOrDefault<EmployeeModel>();


                    ////Get student name of string type
                    //var studentName = context.Database.SqlQuery<string>("Select employee_name from [emp_info] where employee_id='1001'")
                    //                        .FirstOrDefault();


                    //var studentList = context.Database.SqlQuery<string>("Select * from Students").ToList();


                    //context.Database.ExecuteSqlCommand("insert into ambctaskcapture(taskdate,category) values('" + timesheet.taskdate + "', '" + timesheet.category + "')");



                }
            }

        }
    }
}
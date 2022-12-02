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
    public class HomeController : Controller
    {

        public ActionResult Login()
        {
            return View();
        }


        public ActionResult Dashboard()
        {
            ViewBag.Message = "Dashboard page.";
            return View();
        }

        public ActionResult TimeSheet()
        {
            ViewBag.Message = "Timesheet page.";
            return View();
        }

        public ActionResult AddTimeSheet(List<TimeSheetAjaxModel> timesheetmodel)
        {
            //Employye
            //Database
            //Employee 
            //
            ViewBag.Message = "Your contact page.";
            UpdateTimeSheetStatus(timesheetmodel);
            return View("~/Views/Home/Contact.cshtml");
        }


        public void UpdateTimeSheetStatus(List<TimeSheetAjaxModel> timesheetmodel)
        {
            SqlDataReader rdr = null;
            SqlConnection con = null;
            SqlCommand cmd = null;

            con = new SqlConnection(ConfigurationManager.ConnectionStrings["TimesheetDBEntities"].ToString());
            String FirstName;
            String LastName;

            con.Open();
            string CommandText = "SELECT employee_id, employee_name FROM [emp_info] WHERE employee_id='1001'";
            cmd = new SqlCommand(CommandText);
            cmd.Connection = con;
            rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                FirstName = rdr["employee_id"].ToString();
                LastName = rdr["employee_name"].ToString();
            }



            using (var context = new RMAppContext())
            {
                // Create and save a new Students
                Console.WriteLine("Adding new students");

                foreach (var model in timesheetmodel)
                {
                    //var customers = context.Set<TimeSheet>();
                    var timesheet = new TimeSheet()
                    {
                        taskdate = model.Date,
                        category = model.Category,
                        incidentnumber = model.IncidentNumber,
                        taskdetails = model.IncidentDescription,
                        requester = model.Requester,
                        callpriority = model.Urgency,
                        callstatus = model.Status,
                        closeddate = model.ClosedDate,
                        timespent = model.TimeSpent,
                        comments = model.Comments
                    };

                    var abc = new List<string>();

                    abc.Add("employee_id");
                    abc.Add("employee_name");
                    abc.Add("employee_desg");

                    var student = (from s in context.emp_info
                                   where s.employee_id == "1001"
                                   select s).FirstOrDefault<EmployeeModel>();


                    ////Get student name of string type
                    //var studentName = context.Database.SqlQuery<string>("Select employee_name from [emp_info] where employee_id='1001'")
                    //                        .FirstOrDefault();


                    var studentList = context.Database.SqlQuery<string>("Select * from Students").ToList();


                    context.Database.ExecuteSqlCommand("insert into ambctaskcapture(taskdate,category) values('" + timesheet.taskdate + "', '" + timesheet.category + "')");

                    //context.ambctaskcapture.Add(timesheet);
                    //context.SaveChanges();

                }
            }

        }
    }
}
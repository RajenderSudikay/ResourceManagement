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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
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

                    //Get student name of string type
                    var studentName = context.Database.SqlQuery<string>("Select * from [emp_info] where employee_id='C4046'")
                                            .FirstOrDefault();

                    context.Database.ExecuteSqlCommand("insert into ambctaskcapture(taskdate,category) values('" + timesheet.taskdate + "', '" + timesheet.category + "')");

                    //context.ambctaskcapture.Add(timesheet);
                    //context.SaveChanges();

                }
            }

        }
    }
}
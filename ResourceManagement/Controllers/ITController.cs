using ResourceManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    public class ITController : Controller
    {

        // GET: Employee    
        public ActionResult Index()
        {
            return null;
        }
    
        public ActionResult AssetAddUpdate()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }

        public ActionResult AssetAssign()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }

        public ActionResult UploadReport()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }
    }
}
using ResourceManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    public class AssetController : Controller
    {

        // GET: Employee    
        public ActionResult Index()
        {
            return null;
        }
    
        public ActionResult AddUpdate()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            return View(employeeModel);
        }
    }
}
using ResourceManagement.Models;
using ResourceManagement.Models.IT;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.UI;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;

using Microsoft.Ajax.Utilities;
using static ResourceManagement.Helpers.EmployeeHelper;
using static ResourceManagement.Helpers.AssetsHelper;
using static ResourceManagement.Helpers.VendorHelper;
using ResourceManagement.Models.Emp;

namespace ResourceManagement.Controllers
{
    using static Helpers.DateHelper;
    using static Helpers.MediaHelper;
    using static Helpers.MVCExtension;
    using static Helpers.EmailHelper;


    public class EmpController : Controller
    {
        // GET: Employee    
        public ActionResult AddUpdate()
        {
            var model = FillDefaultEmpModel();
            return View(model);
        }

        private EmpModel FillDefaultEmpModel()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            var MasterModel = new EmpModel();
            MasterModel.RMA_EmployeeModel = employeeModel;
            return MasterModel;
        }
    }
}
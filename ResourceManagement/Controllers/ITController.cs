using Newtonsoft.Json;
using OfficeOpenXml;
using ResourceManagement.Models;
using ResourceManagement.Models.IT;
using System;
using System.Collections.Generic;
using System.IO;
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

        public JsonResult ITReportsUploadAjax(ITUpload fileData)
        {
            var model = new RMA_StatusReportModel();
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            model.RMA_EmployeeModel = employeeModel;

            HttpPostedFileBase file = fileData.ExcelFile;

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string dir = @"C:\\inetpub\\wwwroot\\UploadedFiles\2023\March";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string _FileName = Path.GetFileName(file.FileName);
                string _path = Path.Combine(dir, _FileName);
                file.SaveAs(_path);
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        
    }
}
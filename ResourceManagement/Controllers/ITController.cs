using ResourceManagement.Models;
using ResourceManagement.Models.IT;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;

namespace ResourceManagement.Controllers
{
    using static Helpers.DateHelper;

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

            var ITModel = new ITModel();
            ITModel.RMA_EmployeeModel = employeeModel;
            ITModel.MonthsList = MonthList();

            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var Assets = db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo != "").ToList();
                if (Assets != null && Assets.Count() > 0)
                {
                    ITModel.AmbcNewITAssetMgmt = Assets;
                }
            }

            return View(ITModel);
        }

        public JsonResult ITReportsUploadAjax(ITUpload fileData)
        {
            var model = new ITUpload();
            try
            {
                HttpPostedFileBase file = fileData.ExcelFile;
                var systemGeneratedFileName = string.Empty;

                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    using (var context = new TimeSheetEntities())
                    {
                        var contextModel = new AMBCITMonthlyMaintenance()
                        {
                            AssetID = fileData.AssetID,
                            EmployeeID = fileData.EmployeeID,
                            MaintenanceMonth = fileData.UploadedMonth,
                            //TODOD REMARKS
                            Remarks = systemGeneratedFileName,
                            EmployeeName = fileData.EmployeeName
                        };

                        context.AMBCITMonthlyMaintenances.Add(contextModel);
                        context.SaveChanges();
                    }

                    var uploadedMonth = fileData.UploadedMonth;
                    var year = uploadedMonth.Split('-')[1];
                    var month = uploadedMonth.Split('-')[0];
                    systemGeneratedFileName = fileData.AssetID + "-" + fileData.EmployeeID + "-" + file.FileName;
                    var fileLocation = ConfigurationManager.AppSettings["UploadFilePath"] + year + "\\" + month;

                    if (!Directory.Exists(fileLocation))
                    {
                        Directory.CreateDirectory(fileLocation);
                    }
                    //string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(fileLocation, systemGeneratedFileName);
                    file.SaveAs(_path);

                    model.jsonResponse.StatusCode = 200;
                }           
                           
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                model.jsonResponse.StatusCode = 500;
                if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                {
                    var actuallErrors = ex.InnerException.InnerException.Message.Split('.');
                    foreach (var actuallError in actuallErrors)
                    {
                        if (actuallError.ToLowerInvariant().Contains("duplicate key value is"))
                        {
                            model.jsonResponse.Message = actuallError;
                        }
                    }
                }
                else
                {
                    model.jsonResponse.Message = ex.Message;
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

    }
}
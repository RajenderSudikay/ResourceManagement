using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    using Newtonsoft.Json;
    using ResourceManagement.Models;
    using ResourceManagement.Models.MasterData;
    using System.Linq;

    public class MasterDataController : Controller
    {
        // GET: MasterData
        public ActionResult AddUpdate()
        {
            var model = FillDefaultMasterModel();
            return View(model);
        }

        private MasterModel FillDefaultMasterModel()
        {
            var employeeModel = Session["UserModel"] as RMA_EmployeeModel;
            var MasterModel = new MasterModel();
            MasterModel.RMA_EmployeeModel = employeeModel;
            return MasterModel;
        }

        public JsonResult ViewMasterData(InputModel inputModel)
        {           
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                var details = new object();

                if(inputModel.TypeOfData == "Category")
                {
                    details = db.Categories;
                }

                var jsonReponse = JsonConvert.SerializeObject(details);
                return Json(jsonReponse, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
using System.Web.Mvc;

namespace ResourceManagement.Controllers
{
    using Newtonsoft.Json;
    using ResourceManagement.Models;
    using ResourceManagement.Models.MasterData;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using static Helpers.MVCExtension;
    using static Helpers.MasterDataHelper;

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
            using (StreamReader r = new StreamReader(ConfigurationManager.AppSettings["MasterDataJson"]))
            {
                string json = r.ReadToEnd();
                var inputJsonModel = JsonConvert.DeserializeObject<List<MasterJsonRoot>>(json);

                var model = new MasterViewPageModel();
                model.InputJsonObject = inputJsonModel.Where(x => x.Name == inputModel.TypeOfData).FirstOrDefault();
                model.SelectedMasterTypeObject = GetMasterTableInfo(inputModel.TypeOfData);

                var viewModel = RenderPartialToString(this, "viewmasterdatapartial", model, ViewData, TempData);
                return Json(viewModel, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
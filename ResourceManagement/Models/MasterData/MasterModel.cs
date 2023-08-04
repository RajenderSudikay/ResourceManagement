using System.Collections.Generic;

namespace ResourceManagement.Models.MasterData
{
    public class MasterModel
    {
        public RMA_EmployeeModel RMA_EmployeeModel { get; set; } = new RMA_EmployeeModel();

        public InputModel InputModel { get; set; } = new InputModel();
    }

    public class InputModel
    {
        public string TypeOfData { get; set; }
    }

    public class MasterJsonFields
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
        public string Field11 { get; set; }
        public string Field12 { get; set; }
    }

    public class MasterJsonRoot
    {
        public string Name { get; set; }
        public int FieldLength { get; set; }
        public MasterJsonFields fields { get; set; }
    }


    public class MasterViewPageModel
    {
        public object InputJsonObject { get; set; }
        public object SelectedMasterTypeObject { get; set; }
    }

}
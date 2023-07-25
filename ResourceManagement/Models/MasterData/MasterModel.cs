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
}
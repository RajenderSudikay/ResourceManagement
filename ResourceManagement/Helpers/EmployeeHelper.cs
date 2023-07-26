using System.Linq;

namespace ResourceManagement.Helpers
{
    public static class EmployeeHelper
    {
        public static AMBC_Active_Emp_view GetEmpByEmpID(string empID)
        {
            var selectedEmp = new AMBC_Active_Emp_view();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                return db.AMBC_Active_Emp_view.Where(emp => emp.Employee_ID == empID).FirstOrDefault();
            }
        }
    }
}
using System.Linq;

namespace ResourceManagement.Helpers
{
    public static class VendorHelper
    {
        public static tbl_Vendor_Detail GetVendorByUniqueID(int uniqueID)
        {
            var selectedEmp = new tbl_Vendor_Detail();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                return db.tbl_Vendor_Detail.Where(x => x.UniqNo == uniqueID).FirstOrDefault();
            }
        }
    }
}
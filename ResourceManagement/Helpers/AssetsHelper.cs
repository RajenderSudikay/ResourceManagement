using System.Linq;

namespace ResourceManagement.Helpers
{
    public static class AssetsHelper
    {
        public static AmbcNewITAssetMgmt GetAssetByAssetID(string assetID)
        {
            var selectedEmp = new AmbcNewITAssetMgmt();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                return db.AmbcNewITAssetMgmts.Where(x => x.AssetSerialNo == assetID).FirstOrDefault();
            }
        }

        public static AmbcNewITAssetMgmt GetAssetByUniqueNum(int uniqueNum)
        {
            var selectedEmp = new AmbcNewITAssetMgmt();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                return db.AmbcNewITAssetMgmts.Where(x => x.UniqNo == uniqueNum).FirstOrDefault();
            }
        }
    }
}
namespace ResourceManagement.Helpers
{
    public static class MasterDataHelper
    {
        public static object GetMasterTableInfo(string tableName)
        {
            var details = new object();
            using (TimeSheetEntities db = new TimeSheetEntities())
            {
                switch (tableName)
                {
                    case "Category":
                        details = db.Categories;
                        break;
                    case "Client":
                        details = db.AmbcClients;
                        break;
                    case "Location":
                        details = db.Locations;
                        break;
                    case "AssetTypes":
                        details = db.AssetTypes;
                        break;
                    case "OS Details":
                        details = db.SysOSDetails;
                        break;
                    case "Priority":
                        details = db.ticket_priority;
                        break;
                    case "RAM Size":
                        details = db.SysRAMDetails;
                        break;
                    default:
                        // code block
                        break;
                }

                return details;

            }
        }
    }
}
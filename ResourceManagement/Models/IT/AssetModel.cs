using System;

namespace ResourceManagement.Models.IT
{
    public class AssetModelData
    {
        //
        public string AllocationStatus { get; set; }
        //
        public string Description { get; set; }

        public string AssetType { get; set; }

        public string AssetManufacturer { get; set; }

        //
        public string AssignedAsset { get; set; }

        //Charger Details
        public string ChargerDetails { get; set; }

        public string ChargerCapicity { get; set; }
        public string AssetHostName { get; set; }

        //
        public string MouseDetails { get; set; }
        public string AssetModel { get; set; }

        //
        public string AccessControl { get; set; }

        //Charger Asset ID
        public string ChargerSerialNo { get; set; }

        //
        public string Functionality { get; set; }

        public string USBPortStatus { get; set; }

        //
        public string ExpressCode { get; set; }

        //
        public string Headsets { get; set; }

        //
        public string BarcodeID { get; set; }

        public string ServiceTag { get; set; }

        public string AssetMacNo { get; set; }

        //Asset ID
        public string AssetSerialNo { get; set; }

        //OS Version
        public string OperatingSystemDetail { get; set; }

        //
        public string IPAddress { get; set; }

        //
        public string WIFIIP { get; set; }

        public DateTime WarrentyStartDate { get; set; }

        public DateTime WarrentyEndDate { get; set; }

        //
        public DateTime PurchaseDate { get; set; }

        //
        public string PurchaseLocation { get; set; }

        public string WarrentyStatus { get; set; }

        //RAM
        public string RAM_Size { get; set; }

        //
        public string OS { get; set; }

        public string Remarks { get; set; }

        public string CreatedByName { get; set; }

        public string CreatedByID { get; set; }


        public JsonResponseModel jsonResponse { get; set; } = new JsonResponseModel();

    }

    public class GetAssetModelByEmp
    {
        //
        public string EmpID { get; set; }
    }
}
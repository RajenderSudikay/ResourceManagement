using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace ResourceManagement.Models
{
    public class JsonResponseModel
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("statuscode")]
        public int StatusCode { get; set; } = 500;

        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [JsonPropertyName("IsEditedd")]
        public bool IsEditedd { get; set; } = false;

        [JsonPropertyName("IsAdded")]
        public bool IsAdded { get; set; } = false;

    }

    public class SignInOurResponseModel
    {
        public JsonResponseModel jsonResponse { get; set; } = new JsonResponseModel();

        [JsonPropertyName("signin")]
        public bool signin { get; set; } = false;

        [JsonPropertyName("signout")]
        public bool signout { get; set; } = false;


        [JsonPropertyName("empname")]
        public string empname { get; set; } = string.Empty;

        [JsonPropertyName("empemailid")]
        public string empemailid { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string type { get; set; } = string.Empty;


    }

    public class WeekReportModel
    {
        [JsonPropertyName("weekday")]
        public string weekday { get; set; } = string.Empty;

        [JsonPropertyName("hoursspent")]
        public int hoursspent { get; set; } = 500;

        [JsonPropertyName("overtime")]
        public int overtime { get; set; } = 0;

    }

    public class LeaveEmailModel
    {
        public JsonResponseModel jsonResponse { get; set; } = new JsonResponseModel();
        public RMA_LeaveModel AjaxleaveModel { get; set; }

        public bool EmailSent { get; set; }
    }

    public class ApplyLeaveForMissedSidnInModel
    {
        public JsonResponseModel jsonResponse { get; set; } = new JsonResponseModel();
        public Dictionary<string, string> MissingLeaveDates { get; set; } = new Dictionary<string, string>();
    }

    public class RMA_ClientBasedEmpJson
    {
        public AMBC_Active_Emp_view ClientBasedAMBCEmp { get; set; } = new AMBC_Active_Emp_view();

        public JsonResponseModel jsonResponse { get; set; } = new JsonResponseModel();
    }
}
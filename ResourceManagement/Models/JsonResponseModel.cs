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

    }

    public class WeekReportModel
    {
        [JsonPropertyName("weekday")]
        public string weekday { get; set; } = string.Empty;

        [JsonPropertyName("hoursspent")]
        public int hoursspent { get; set; } = 500;

    }
}
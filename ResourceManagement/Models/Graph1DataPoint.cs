using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ResourceManagement.Models
{
    [DataContract]
    public class Graph1DataPoint
    {
        public class DataPoint
        {
            public string label { get; set; }
            public double y { get; set; }
        }

        public class MonthWiseDataPoint
        {
            public string Priority { get; set; }
            public string label { get; set; }
            public double y { get; set; }
            public double totalTickets { get; set; }
        }

        public class PieDataPoint
        {
            public string label { get; set; }
            public double y { get; set; }
            public string legendText { get; set; }
            public string indexLabelFontColor { get; set; }
            public string color { get; set; }

            public double Percentage { get; set; }
        }

        public class Root
        {
            public string type { get; set; }
            public string name { get; set; }
            public bool showInLegend { get; set; }
            public string legendText { get; set; }
            public string legendMarkerColor { get; set; }
            public string yValueFormatString { get; set; }
            public List<DataPoint> dataPoints { get; set; } = new List<DataPoint>();
            public string axisYType { get; set; }
        }

    }

    public class ProjectGraphDataPoint
    {
        public class Reports
        {
            public string label { get; set; }
            public string ChartLabelWithStartEndDate { get; set; }
            public decimal? completionPercenatge { get; set; }
            public decimal? remainingPercenatge { get; set; }
            public string MonthName { get; set; }
            public System.DateTime? ProjestStartDate { get; set; }
            public System.DateTime? TargetClosingDate { get; set; }
            public System.DateTime? ActualClosedDate { get; set; }
            public bool IsCarryForwardMonth { get; set; } = false;
        }

        public class DataPoint
        {
            public string label { get; set; }
            public decimal? y { get; set; }
        }

        public class ProjectChartInfo
        {
            [DataMember(Name = "indexLabel")]
            public string indexLabel { get; set; } = "{y}%";

            [DataMember(Name = "indexLabelFontColor")]
            public string indexLabelFontColor { get; set; } = "white";

            [DataMember(Name = "indexLabelPlacement")]
            public string indexLabelPlacement { get; set; } = "inside";

            [DataMember(Name = "type")]
            public string type { get; set; } = "stackedBar100";

            [DataMember(Name = "name")]
            public string name { get; set; }

            [DataMember(Name = "showInLegend")]
            public bool showInLegend { get; set; } = true;

            //[DataMember(Name = "y")]
            //public decimal? y { get; set; }

            [DataMember(Name = "visible")]
            public bool visible { get; set; } = true;


            [DataMember(Name = "indexLabelFontSize")]
            public int indexLabelFontSize { get; set; } = 8;
         
            
            public System.DateTime? ProjestStartDate { get; set; }
            public System.DateTime? TargetClosingDate { get; set; }
            public System.DateTime? ActualClosedDate { get; set; }

            public List<DataPoint> dataPoints = new List<DataPoint>();
        }

        public class ProjectChartInfoData
        {
            public List<ProjectChartInfo> data { get; set; } = new List<ProjectChartInfo>();
        }


    }

}
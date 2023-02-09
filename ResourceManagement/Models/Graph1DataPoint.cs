﻿using System.Collections.Generic;
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

        public class PieDataPoint
        {
            public string label { get; set; }
            public double y { get; set; }
            public string legendText { get; set; }
            public string indexLabelFontColor { get; set; }
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
}
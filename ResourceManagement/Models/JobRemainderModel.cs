using System;

namespace ResourceManagement.Models
{
    public class JobRemainderModel
    {
        public string RemainderType { get; set; }
        public DateTime StartDateTime { get; set; }
        public string StartDate { get; set; }
        public DateTime EndDateTime { get; set; }
        public string EndDate { get; set; }
        public DateTime CurrentMonthDate { get; set; }
        public string CurrentMonth { get; set; }
        public string CurrentMonthShortFormat { get; set; }
        public DateTime PreviousMonthDate { get; set; }
        public string PreviousMonth { get; set; }
        public string PreviousMonthShortFormat { get; set; }
    }
}
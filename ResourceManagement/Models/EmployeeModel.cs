﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static ResourceManagement.Models.TimesheetReportModel;

namespace ResourceManagement.Models
{
    public class RMA_EmployeeModel
    {
        public AMBC_Active_Emp_view AMBC_Active_Emp_view { get; set; }
        public List<AMBC_Active_Emp_view> projectInfo { get; set; }
        public string leaveOrHolidayInfo { get; set; }
        public tbld_ambclogininformation signInOutInfo { get; set; }
        public List<TimeSheetReportViewModel> TimeSheetReports { get; set; }
        public RMA_SystemDetails SystemInfo { get; set; }      
    }

    public class RMA_SystemDetails
    {
        public string SystemHostName { get; set; }
        public string SystemIP { get; set; }

    }

    public class RMA_TimeSheetAjaxModel
    {
        public string Date { get; set; }
        public string Category { get; set; }
        public string IncidentNumber { get; set; }
        public string IncidentDescription { get; set; }
        public string Requester { get; set; }
        public string Urgency { get; set; }
        public string Status { get; set; }
        public string ClosedDate { get; set; }
        public string TimeSpent { get; set; }
        public string Comments { get; set; }
    }

    public class RMA_TimeSheetWeekData
    {
        public string EmpId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class RMA_LeaveOrHolidayInfo
    {
        public string LeaveOrHolidayDate { get; set; }
        public string Reason { get; set; }
        public DateTime? DefaultLeaveOrHolidayDate { get; set; }
    }

    public class RMA_LeaveHolidaySignInModel
    {
        public List<RMA_LeaveOrHolidayInfo> LeaveHolidayInfo { get; set; } = new List<RMA_LeaveOrHolidayInfo>();

        public List<RMA_SignInInfo> SignInInfo { get; set; } = new List<RMA_SignInInfo>();
    }

    public class RMA_SignInInfo
    {
        public string SignInDate { get; set; }
        public string Reason { get; set; }
    }

    public class RMA_ReportEmployeeModel
    {
        public string Employee_Name { get; set; }
        public string Employee_ID { get; set; }
    }

    public class RMA_SignInOutEmailModel
    {
        public string empname { get; set; }
        public string empemailid { get; set; }
        public string type { get; set; }
    }

    public class RMA_TimeSheetReportsFromDB
    {
        [JsonPropertyName("viewreports")]
        public List<ambctaskcapture> Viewreports { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("statuscode")]
        public int StatusCode { get; set; } = 500;
    }

    public class RMA_TimeSheetRemainder
    {
        [JsonPropertyName("viewreports")]
        public List<AMBC_Active_Emp_view> EmpListForremainder { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("statuscode")]
        public int StatusCode { get; set; } = 500;
    }


    public class RMA_TimeSheetRemainderEmail
    {
        public List<RMA_RemainderEmailSelectedEmpModel> selctedempmodel { get; set; }
        public string weekstartdate { get; set; }
        public string weekenddate { get; set; }
        public string LogedInEmpId { get; set; }
        public string LogedInEmpName { get; set; }
        public string LogedInEmpEmail { get; set; }
        public bool SendSingleEmailToAllEmp { get; set; }
    }

    public class RMA_RemainderEmailSelectedEmpModel
    {
        public string selectedemployeeemail { get; set; }
        public string selectedemployeeemailid { get; set; }
        public string selectedemployeeempname { get; set; }
        public string selectedemploymanager { get; set; }
        public string selectedemploymanageremail { get; set; }
        public string selectedweekstartdate { get; set; }
        public string selectedweekenddate { get; set; }
        public bool SendSingleEmailToAllEmp { get; set; }
        public string RemainderMonth { get; set; }
        public string EmailType { get; set; }
    }

    public class RMA_LeaveModel
    {
        public string SelectedEmpId { get; set; }
        public string SelectedEmpName { get; set; }
        public string LogedInEmpId { get; set; }
        public string LogedInEmpName { get; set; }
        public string LogedInEmpEmail { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ApplyFor { get; set; }
        public string Reason { get; set; }
        public string LeaveType { get; set; }
        public string LeaveCategory { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmissionType { get; set; }
    }

    public class RMA_ClientBasedEmpModel
    {
        public string EmpId { get; set; }
        public string ProjectID { get; set; }
        public string ClientName { get; set; }
    }

    public class BannerInfo
    {
        public string BannerName { get; set; }
        public bool ShowBanner { get; set; } = true;
        public string BannerImagePath { get; set; }
    }
}
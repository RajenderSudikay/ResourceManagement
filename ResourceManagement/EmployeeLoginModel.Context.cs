﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ResourceManagement
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TimeSheetEntities : DbContext
    {
        public TimeSheetEntities()
            : base("name=TimeSheetEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ambctaskcapture> ambctaskcaptures { get; set; }
        public virtual DbSet<con_leaveupdate> con_leaveupdate { get; set; }
        public virtual DbSet<desk_att_taskcapture> desk_att_taskcapture { get; set; }
        public virtual DbSet<emp_info> emp_info { get; set; }
        public virtual DbSet<emp_info_log> emp_info_log { get; set; }
        public virtual DbSet<emp_project> emp_project { get; set; }
        public virtual DbSet<emp_project_log> emp_project_log { get; set; }
        public virtual DbSet<emplogin> emplogins { get; set; }
        public virtual DbSet<emplogin_log> emplogin_log { get; set; }
        public virtual DbSet<Error_Log> Error_Log { get; set; }
        public virtual DbSet<tbl_ambclogininformation> tbl_ambclogininformation { get; set; }
        public virtual DbSet<tbl_LoginInformation> tbl_LoginInformation { get; set; }
        public virtual DbSet<tblambcholiday> tblambcholidays { get; set; }
        public virtual DbSet<tblambcholidaylog> tblambcholidaylogs { get; set; }
        public virtual DbSet<timesheet_submission_log> timesheet_submission_log { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryList> CategoryLists { get; set; }
        public virtual DbSet<CategoryType> CategoryTypes { get; set; }
        public virtual DbSet<con_leaveupdate_12072021> con_leaveupdate_12072021 { get; set; }
        public virtual DbSet<con_leaveupdate_20220517> con_leaveupdate_20220517 { get; set; }
        public virtual DbSet<con_leaveupdate20220510> con_leaveupdate20220510 { get; set; }
        public virtual DbSet<tbl_LoginInformation_bkp> tbl_LoginInformation_bkp { get; set; }
        public virtual DbSet<tblambcholiday_06May2022> tblambcholiday_06May2022 { get; set; }
        public virtual DbSet<AMBC_Active_Emp> AMBC_Active_Emp { get; set; }
        public virtual DbSet<AMBC_Active_Emp_view> AMBC_Active_Emp_view { get; set; }
        public virtual DbSet<today_att> today_att { get; set; }
        public virtual DbSet<tbld_ambclogininformation> tbld_ambclogininformation { get; set; }
        public virtual DbSet<ambclogin_leave_view> ambclogin_leave_view { get; set; }
        public virtual DbSet<halfday_leave_view> halfday_leave_view { get; set; }
        public virtual DbSet<monthlyreports_Template1> monthlyreports_Template1 { get; set; }
        public virtual DbSet<monthlyreports_Template2> monthlyreports_Template2 { get; set; }
        public virtual DbSet<monthlyreports_Template3> monthlyreports_Template3 { get; set; }
        public virtual DbSet<consultantavailiability_Final> consultantavailiability_Final { get; set; }
        public virtual DbSet<consultantavailiability1> consultantavailiability1 { get; set; }
        public virtual DbSet<consultantavailiability2> consultantavailiability2 { get; set; }
        public virtual DbSet<consultantavailiability3> consultantavailiability3 { get; set; }
        public virtual DbSet<consultantavailiability4> consultantavailiability4 { get; set; }
        public virtual DbSet<consultantavailiability5> consultantavailiability5 { get; set; }
        public virtual DbSet<AMBCITMonthlyMaintenance> AMBCITMonthlyMaintenances { get; set; }
        public virtual DbSet<AmbcNewITAssetMgmt> AmbcNewITAssetMgmts { get; set; }
        public virtual DbSet<AmbcNewPeripheral> AmbcNewPeripherals { get; set; }
    }
}

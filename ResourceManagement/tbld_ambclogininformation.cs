//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class tbld_ambclogininformation
    {
        public int login_id { get; set; }
        public string Employee_Code { get; set; }
        public string Employee_Name { get; set; }
        public string Employee_Designation { get; set; }
        public string Employee_Shift { get; set; }
        public System.DateTime Login_date { get; set; }
        public System.DateTime Signin_Time { get; set; }
        public Nullable<System.DateTime> Signout_Time { get; set; }
        public Nullable<int> Working_Hours { get; set; }
        public string Employee_Hostname { get; set; }
        public string Employee_IP { get; set; }
        public string Concat_loginstring { get; set; }
        public Nullable<bool> IsSignOutBySystem { get; set; }
        public string Employee_LoginLocation { get; set; }
    }
}

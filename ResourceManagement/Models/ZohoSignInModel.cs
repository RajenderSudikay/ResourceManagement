﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace ResourceManagement.Models
{
    public class ZohoSignInModel
    {
        //To change label title value  
        [DisplayName("Member Name")]
        public string Name { get; set; }

        //To change label title value  
        [DisplayName("Telephone / Mobile Number")]
        public string PhoneNumber { get; set; }

        //To change label title value  
        [DisplayName("Adjustment Date")]
        public DateTime AdjustmentDate { get; set; }

        //To change label title value  
        [DisplayName("Upload Excel File")]
        public string ImagePath { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
        
        public string SuccessMessage { get; set; }

        public string FailureMessage { get; set; }
    }
}
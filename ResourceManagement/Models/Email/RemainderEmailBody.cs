using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourceManagement.Models.Email
{
    public static class RemainderEmailBody
    {
        public static string GenerateEmailBody(RMA_RemainderEmailSelectedEmpModel remainderModel)
        {
            var eamilBody = "";

            if (remainderModel.EmailType == "TimeSheet")
            {

                eamilBody =

            @"<style>
    body {
        background-color: #fff;
        font-family: Calibri;
        font-size: initial;
        color: #2167ae;
    }

    table {
        border-collapse: collapse;
        width: 100%;
    }

    td, th {
        text-align: left;
        padding: 8px;
        border: 0px !important;
    }

    tr:nth-child(even) {
        background-color: grey;
    }

    .time-header {
        color: white;
    }


    tr:nth-child(even) {
        background-color: #f2f2f2;
    }

    .emailbody {
        font-family: 'Lato', sans-serif;
    }

    .email-footer {
        background-color: #eee;
        font-size: medium;
        height: 25px;
        border-radius: 20px;
    }

    .emailbodycontent {
        font-family: 'Lato', sans-serif;
        color: #2167ae;
    }

    #specifyColor {
        accent-color: #d3d3d3;
    }

    .circle {
        height: 20px;
        width: 20px;
        background-color: #d3d3d3;
        border-radius: 50%;
    }
</style>

    <div class='emailbodycontent'>
        <div style='font-family: Calibri; color: #2167ae;'>
           
                <h2>Dear <b>{{userName}}</b>,</h2>
            

        </div>

        <div style='font-family: Calibri; color: #2167ae; '>
            A gentle reminder on timesheets for the week (@Model.selectedweekstartdate to @Model.selectedweekenddate).
            <br />
            Kindly submit your timesheets <b>today ASAP.</b>
        </div>


        <br />
        <br />
        <div style='font-family: Calibri; color: #2167ae; font-size: initial'>
            Thanks & Regards,
            <br />
            <b>HR Department</b>
            <br />
            Admin Office: Elcot IT Park – Madurai, India.  Ph:  0452 6610202
            <br />
            Development Office: Hi-Tech City - Hyderabad, India.  Ph: 040 66577488
            <br />
            Centre of Excellence: Madurai, India. Ph: 0452 4500127
            <br />
            <a href='www.ambconline.com'>www.ambconline.com</a>
            <br />
            <br />
            <img class='preload-me' src='https://ambconline.com/wp-content/uploads/2022/12/AMBC-ISO-LOGO.png' srcset='https://ambconline.com/wp-content/uploads/2022/12/AMBC-ISO-LOGO.png 188w, https://ambconline.com/wp-content/uploads/2022/12/AMBC-ISO-LOGO.png 188w' width='300' height='85'' sizes='188p'' alt='AMBC Inc'>
            <br />
            <br />
           'Pursuant to Federal Law, if you do not wish to receive future email messages from us, please send an email to remove@ambconline.com. If you wish to receive future email messages from us, please reply to this message with “ADD” in the subject line. Any inconvenience caused is unintentional and is sincerely regretted. Thank you'.
        </div>
        <br />
        <br />
        <div class='email-foote' style='text-align: center; padding-top: 6px; vertical-align: middle; font-family: Calibri; '>
            Autogenarted email.Please do  not reply.
        </div>
    </div>";

                eamilBody = eamilBody.Replace("{{userName}}", remainderModel.selectedemployeeempname).Replace("@Model.selectedweekstartdate", remainderModel.selectedweekstartdate).Replace("@Model.selectedweekenddate", remainderModel.selectedweekenddate);
            }

            if (remainderModel.EmailType == "StatusReport")
            {
                eamilBody =

            @"<style>
    body {
        background-color: #fff;
        font-family: Calibri;
        font-size: initial;
        color: #2167ae;
    }

    table {
        border-collapse: collapse;
        width: 100%;
    }

    td, th {
        text-align: left;
        padding: 8px;
        border: 0px !important;
    }

    tr:nth-child(even) {
        background-color: grey;
    }

    .time-header {
        color: white;
    }


    tr:nth-child(even) {
        background-color: #f2f2f2;
    }

    .emailbody {
        font-family: 'Lato', sans-serif;
    }

    .email-footer {
        background-color: #eee;
        font-size: medium;
        height: 25px;
        border-radius: 20px;
    }

    .emailbodycontent {
        font-family: 'Lato', sans-serif;
        color: #2167ae;
    }

    #specifyColor {
        accent-color: #d3d3d3;
    }

    .circle {
        height: 20px;
        width: 20px;
        background-color: #d3d3d3;
        border-radius: 50%;
    }
</style>

    <div class='emailbodycontent'>
        <div style='font-family: Calibri; color: #2167ae;'>
           
                <h2>Dear <b>{{userName}}</b>,</h2>
            

        </div>

         <div style='font-family: Calibri; color: #2167ae; '>
            A gentle reminder on dashboard report for the month of(@Model.RemainderMonth).
            <br />
            Kindly submit your Dashboard Report <b> today ASAP.</b>
        </div>


        <br />
        <br />
        <div style='font-family: Calibri; color: #2167ae; font-size: initial'>
            Thanks & Regards,
            <br />
            <b>HR Department</b>
            <br />
            Admin Office: Elcot IT Park – Madurai, India.  Ph:  0452 6610202
            <br />
            Development Office: Hi-Tech City - Hyderabad, India.  Ph: 040 66577488
            <br />
            Centre of Excellence: Madurai, India. Ph: 0452 4500127
            <br />
            <a href='www.ambconline.com'>www.ambconline.com</a>
            <br />
            <br />
            <img class='preload-me' src='https://ambconline.com/wp-content/uploads/2022/12/AMBC-ISO-LOGO.png' srcset='https://ambconline.com/wp-content/uploads/2022/12/AMBC-ISO-LOGO.png 188w, https://ambconline.com/wp-content/uploads/2022/12/AMBC-ISO-LOGO.png 188w' width='300' height='85'' sizes='188p'' alt='AMBC Inc'>
            <br />
            <br />
           'Pursuant to Federal Law, if you do not wish to receive future email messages from us, please send an email to remove@ambconline.com. If you wish to receive future email messages from us, please reply to this message with “ADD” in the subject line. Any inconvenience caused is unintentional and is sincerely regretted. Thank you'.
        </div>
        <br />
        <br />
        <div class='email-foote' style='text-align: center; padding-top: 6px; vertical-align: middle; font-family: Calibri; '>
            Autogenarted email.Please do  not reply.
        </div>
    </div>";

                eamilBody = eamilBody.Replace("{{userName}}", remainderModel.selectedemployeeempname).Replace("@Model.RemainderMonth", remainderModel.RemainderMonth);
            }

            return eamilBody;
        }
    }
}
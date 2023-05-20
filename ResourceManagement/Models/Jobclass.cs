using Quartz;
using System.Configuration;
using System.Net;

namespace ResourceManagement.Models
{
    public class Jobclass : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            if(ConfigurationManager.AppSettings["RunSchedularJob"] == "true")
            {
                using (var client = new WebClient())
                {
                    var jobURL = ConfigurationManager.AppSettings["SiteURL"] + "/RunSchedularJob";
                    var contents = client.DownloadString(jobURL);                 
                }
            }

            //using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["SMTPUserName"], "rajendersudikay@ambconline.com"))
            //{
            //    mm.Subject = "Schedular Email" + DateTime.Now.ToString();
            //    var emailBody = "Hello Schedular Job " + DateTime.Now.ToString();
            //    mm.Body = emailBody;

            //    mm.IsBodyHtml = true;
            //    SmtpClient smtp = new SmtpClient();
            //    smtp.Host = ConfigurationManager.AppSettings["SMTPHost"];
            //    smtp.EnableSsl = true;
            //    NetworkCredential credentials = new NetworkCredential();
            //    credentials.UserName = ConfigurationManager.AppSettings["SMTPUserName"];
            //    credentials.Password = ConfigurationManager.AppSettings["SMTPPassword"];
            //    smtp.UseDefaultCredentials = true;
            //    smtp.Credentials = credentials;
            //    smtp.Port = System.Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
            //    smtp.Send(mm);
            //}
        }

    }

}
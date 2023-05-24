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
        }
    }
}
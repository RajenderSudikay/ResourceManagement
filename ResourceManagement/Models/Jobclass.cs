using Quartz;
using ResourceManagement.Controllers;
using System.Configuration;
using System.Net;

namespace ResourceManagement.Models
{
    public class Jobclass : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            if (ConfigurationManager.AppSettings["RunSchedularJob"] == "true")
            {
                using (var client = new WebClient())
                {                   
                    HomeController ext = new HomeController();
                    ext.RunSchedularJob();
                }
            }
        }
    }
}
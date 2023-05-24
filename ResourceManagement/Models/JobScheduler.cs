using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
namespace ResourceManagement.Models
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<Jobclass>().Build();

            ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .WithCronSchedule("0 0 17 ? * MON,TUE,WED,THU,FRI *")
            .StartAt(DateTime.UtcNow)
            .WithPriority(1)
            .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
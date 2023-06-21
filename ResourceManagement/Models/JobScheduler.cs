using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
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
            .WithCronSchedule(ConfigurationManager.AppSettings["SchedularCornExpression"])
            .StartAt(DateTime.UtcNow)
            .WithPriority(1)
            .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
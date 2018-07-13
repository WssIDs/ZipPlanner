using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipPlanner.Models
{
    public class ArchiveScheduler
    {
        public static bool Start(ArchiveSavedJob archiveJob)
        {

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.Start();

                IJobDetail job = JobBuilder.Create<ArchiveJob>().WithDescription("Архивация").Build();

                CronExpression.ValidateExpression(archiveJob.CronExpression);

                JobDataMap jobMap = new JobDataMap();
                jobMap.Put("Filter", archiveJob.Filter);

                ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                    .WithIdentity(archiveJob.Name, archiveJob.Group)     // идентифицируем триггер с именем и группой
                    .StartNow()
                    .WithCronSchedule(archiveJob.CronExpression)      // каждую минуту 
                    .UsingJobData("StartPath", archiveJob.StartPath)
                    .UsingJobData("EndPath", archiveJob.EndPath)
                    .UsingJobData("EndFileName",archiveJob.EndFileName)
                    .UsingJobData(jobMap)
                    .UsingJobData("DateTimeFormat", archiveJob.DateTimeFormat)
                    .UsingJobData("DeleteFiles", archiveJob.DeleteFiles)
                    .UsingJobData("UseName", archiveJob.UseName)
                    .UsingJobData("UseGroup", archiveJob.UseGroup)
                    .UsingJobData("UseDateTimeFormat", archiveJob.UseDateTimeFormat)
                    .UsingJobData("UseGuid", archiveJob.UseGuid)
                    .ForJob(job)
                    .Build();                               // создаем триггер


                if (job != null && trigger != null)
                {
                    scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы

                    logger.Info("Инициализация планировщика - " + archiveJob.Name + ": " + archiveJob.Group);

                    return true;

                    //
                }
                else
                {
                    logger.Error("Ошибка инициализации планировщика - " + archiveJob.Name + ": " + archiveJob.Group);
                }
            }
            catch (Exception e)
            {
                logger.Error("Ошибка - " + e.Message);
            }

            //return ScheludeStatus.STOPPED;

            return false;
        }

        public static bool GetStatusAsync(ArchiveSavedJob job)
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            TriggerKey key = new TriggerKey(job.Name, job.Group);

            if (scheduler.IsStarted == true)
            {
                return true;
            }

            return false;
        }

        public static bool Stop(ArchiveSavedJob job)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

                TriggerKey key = new TriggerKey(job.Name, job.Group);

                scheduler.UnscheduleJob(key);

                logger.Info("Задание - " + key.Name + ": " + key.Group + " остановлено !");

                return true;
            }
            catch (Exception e)
            {
                logger.Error("Ошибка -" + e.Message);
            }

            return false;
        }
    }
}

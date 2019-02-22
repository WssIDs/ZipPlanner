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
    public class CopyScheduler
    {
        public static bool Start(CopySavedJob copyJob)
        {

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.Start();

                IJobDetail job = JobBuilder.Create<ArchiveJob>().WithDescription("Копирование").Build();

                CronExpression.ValidateExpression(copyJob.CronExpression);

                JobDataMap jobMap = new JobDataMap();
                jobMap.Put("Filter", copyJob.Filter);

                ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                    .WithIdentity(copyJob.Name, copyJob.Group)     // идентифицируем триггер с именем и группой
                    .StartNow()
                    .WithCronSchedule(copyJob.CronExpression)      // каждую минуту 
                    .UsingJobData("StartPath", copyJob.StartPath)
                    .UsingJobData("EndPath", copyJob.EndPath)
                    .UsingJobData(jobMap)
                    .UsingJobData("DeleteFiles", copyJob.DeleteFiles)
                    .ForJob(job)
                    .Build();                               // создаем триггер


                if (job != null && trigger != null)
                {
                    scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
                    logger.Info("Инициализация планировщика - " + copyJob.Name + ": " + copyJob.Group);
                    return true;
                }
                else
                {
                    logger.Error("Ошибка инициализации планировщика - " + copyJob.Name + ": " + copyJob.Group);
                }
            }
            catch (Exception e)
            {
                logger.Error("Ошибка - " + e.Message);
            }

            //return ScheludeStatus.STOPPED;

            return false;
        }

        public static bool GetStatusAsync(CopySavedJob job)
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            TriggerKey key = new TriggerKey(job.Name, job.Group);

            if (scheduler.IsStarted == true)
            {
                return true;
            }

            return false;
        }

        public static bool Stop(CopySavedJob job)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

                TriggerKey key = new TriggerKey(job.Name, job.Group);

                scheduler.UnscheduleJob(key);
                scheduler.Shutdown();

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

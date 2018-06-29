using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NCrontab;
using NLog;
using Quartz;
using Quartz.Impl;

using ZipPlanner.Models;
using ZipPlanner.Properties;
using ZipPlanner.Windows;

namespace ZipPlanner
{
    public class ArchiveJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example
            string command = dataMap.GetString("Command");
            string startpath = dataMap.GetString("StartPath");
            string endpath = dataMap.GetString("EndPath");
            string endfilename = dataMap.GetString("EndFileName");
            string searchpattern = dataMap.GetString("SearchPattern");
            bool deletefiles = dataMap.GetBoolean("DeleteFiles");
            
            DateTime date = DateTime.Now;

            string FileName = endpath+endfilename+"_"+context.Trigger.Key.Name+"_"+ context.Trigger.Key.Group + "_" + date.Day + "_" + date.Month + "_" + date.Year + "_" + date.Minute +".zip";

            DirectoryInfo di = new DirectoryInfo(startpath);

            var logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Info("Выполнение задания - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group);
            logger.Info("Автоматическое удаление исходных файлов включено");
            logger.Info("Создание архива - " + FileName);

            List<FileInfo> files = di.GetFiles(searchpattern, SearchOption.TopDirectoryOnly).ToList();

            if (files != null)
            {
                if (files.Count > 0)
                {
                    using (FileStream zipToOpen = new FileStream(FileName, FileMode.Create))
                    {
                        using (ZipArchive archive = new ZipArchive(zipToOpen, mode: ZipArchiveMode.Create))
                        {
                            foreach (var file in files)
                            {
                                archive.CreateEntryFromFile(file.FullName, file.Name);
                            }
                        }
                    }

                    logger.Info("Архив - " + FileName + " создан");
                    logger.Info("Задание - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group + " завершено!");

                    if (deletefiles)
                    {
                        foreach (var file in files)
                        {
                            File.Delete(file.FullName);
                        }
                    }
                }
                else
                {
                    logger.Warn("Архив - " + FileName + " не создан");
                    logger.Warn("Отсутствуют файлы в директории");
                }

            }
            else
            {
                logger.Warn("Архив - " + FileName + " не создан");
                logger.Warn("Не удалость получить список файлов");
            }

            return Task.CompletedTask;
        }
    }

    public class ArchiveSheduler
    {
        public static async void Start(ArchiveSavedJob archiveJob)
        {

            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<ArchiveJob>().WithDescription("Архивация").Build();

                CronExpression.ValidateExpression(archiveJob.CronExpression);

                ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                    .WithIdentity(archiveJob.Name, archiveJob.Group)     // идентифицируем триггер с именем и группой
                    .StartNow()
                    .WithCronSchedule(archiveJob.CronExpression)      // каждую минуту 
                    .UsingJobData("StartPath", archiveJob.StartPath)
                    .UsingJobData("EndPath", archiveJob.EndPath)
                    .UsingJobData("EndFileName", archiveJob.EndFileName)
                    .UsingJobData("SearchPattern", archiveJob.SearchPattern)
                    .UsingJobData("DeleteFiles", archiveJob.DeleteFiles)
                    .ForJob(job)
                    .Build();                               // создаем триггер


                if (job != null && trigger != null)
                {
                    await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы

                    logger.Info("Инициализация планировщика - " + archiveJob.Name + ": " + archiveJob.Group);

                    archiveJob.Status = true;

                    //
                }
                else
                {
                    logger.Error("Ошибка инициализации планировщика - " + archiveJob.Name + ": " + archiveJob.Group);
                }
            }
            catch (Exception e)
            {
                logger.Error("Ошибка - "+e.Message);
            }

            //return ScheludeStatus.STOPPED;

            archiveJob.Status = false;
        }

        public static async Task<bool> GetStatusAsync(ArchiveSavedJob job)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            TriggerKey key = new TriggerKey(job.Name, job.Group);

            if (scheduler.IsStarted == true)
            {
                return true;
            }

            return false;
        }

        public static async void Stop(ArchiveSavedJob job)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

                TriggerKey key = new TriggerKey(job.Name, job.Group);

                await scheduler.UnscheduleJob(key);

                logger.Info("Задание - " + key.Name + ": " + key.Group + " остановлено !");
            }
            catch (Exception e)
            {
                logger.Error("Ошибка -" + e.Message);
            }
        }
    }


    public class CustomJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context)
        {

            var logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Info("Выполнение задания - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group);

            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example
            string command = dataMap.GetString("Command");


            logger.Info("Запуск команды - " + command);

            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.GetEncoding(866),
                StandardOutputEncoding = Encoding.GetEncoding(866),
            };

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                logger.Info("output>> " + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                logger.Info("error>> " + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            if(process.ExitCode == 0)
            {
                logger.Info("Успешное завершение команды - " + command);
            }
            else
            {
                logger.Info("Ошибка выполнения команды - " + command);
            }

            process.Close();

            logger.Info("Задание - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group + " завершено!");

            return Task.CompletedTask;
        }
    }

    public class CustomSheduler
    {
        public static async void Start(string jobName, string groupName, string command, string cronExpression = "0 0/1 * * * ?")
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<CustomJob>().WithDescription("Пользовательская настройка").Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity(jobName, groupName)     // идентифицируем триггер с именем и группой
                .StartNow()
                .WithCronSchedule(cronExpression)      // каждую минуту 
                .UsingJobData("Command", command)
                .ForJob(job)
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы

            var logger = LogManager.GetCurrentClassLogger();

            logger.Warn("Инициализация планировщика - " + jobName + ": " + groupName);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<ArchiveSavedJob> archiveJobs = new ObservableCollection<ArchiveSavedJob>();
        ObservableCollection<CustomSavedJob> customJobs = new ObservableCollection<CustomSavedJob>();

        Mutex mutexObj;
        Logger logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            logger.Info("Запуск приложения");

            bool existed;
            // получаем GIUD приложения
            string guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();

            mutexObj = new Mutex(true, guid, out existed);
            if (existed)
            {

                InitializeComponent();


                if (File.Exists("archive-jobs.dat"))
                {
                    // десериализация из файла people.dat
                    using (FileStream fs = new FileStream("archive-jobs.dat", FileMode.OpenOrCreate))
                    {
                        BinaryFormatter bf = new BinaryFormatter();

                        object obj = bf.Deserialize(fs);

                        var objects = obj as ObservableCollection<ArchiveSavedJob>;
                        archiveJobs = objects;
                    }
                }


                if (Settings.Default.bAutoStartScheduler)
                {
                    if (archiveJobs.Count > 0)
                    {
                        foreach (ArchiveSavedJob job in archiveJobs)
                        {
                            // Запуск планировщика
                            ArchiveSheduler.Start(job);

                        }

                    }
                }
                db_archivejobs.DataContext = archiveJobs;

            }
            else
            {
                MessageBox.Show("Приложение уже запущено! Запуск более одной копии приложения невозможен","Ошибка");
                logger.Error("Попытка запуска второй копии приложения");
                Close();
            }

        }

        private void Scheluder_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream("archive-jobs.dat", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, archiveJobs);
            }

            foreach(var job in archiveJobs)
            {
                logger.Warn("Завершение задания (Завершение работы приложения) - " + job.Name + ": " + job.Group);
            }

            logger.Info("Завершение работы приложения");
        }



        private void InitDataGrid()
        {
            Style rowStyle = new Style(typeof(DataGridRow));
            rowStyle.Setters.Add(new EventSetter(DataGridRow.MouseDoubleClickEvent,
                                     new MouseButtonEventHandler(Row_DoubleClick)));
            db_archivejobs.RowStyle = rowStyle;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;

            var item = row.Item as ArchiveSavedJob;

            var addSchelude_dlg = new AddSchelude(item);

            if (addSchelude_dlg.ShowDialog() == true)
            {
                ArchiveSheduler.Stop(item);
                //archiveJobs.
                MessageBox.Show("Успешно изменено");
                ArchiveSheduler.Start(item);
            }

            // Some operations with this row
        }
    }
}

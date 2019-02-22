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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoMapper;
using NCrontab;
using NLog;
using Quartz;
using Quartz.Impl;

using ZipPlanner.Models;
using ZipPlanner.Properties;
using ZipPlanner.Windows;

namespace ZipPlanner
{
    public class CustomJob : IJob
    {
        public void Execute(IJobExecutionContext context)
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

            if (process.ExitCode == 0)
            {
                logger.Info("Успешное завершение команды - " + command);
            }
            else
            {
                logger.Info("Ошибка выполнения команды - " + command);
            }

            process.Close();

            logger.Info("Задание - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group + " завершено");
        }

        //Task IJob.Execute(IJobExecutionContext context)
        //{

        //    var logger = NLog.LogManager.GetCurrentClassLogger();

        //    logger.Info("Выполнение задания - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group);

        //    JobKey key = context.JobDetail.Key;

        //    JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example
        //    string command = dataMap.GetString("Command");


        //    logger.Info("Запуск команды - " + command);

        //    var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
        //    {
        //        CreateNoWindow = true,
        //        UseShellExecute = false,
        //        RedirectStandardError = true,
        //        RedirectStandardOutput = true,
        //        StandardErrorEncoding = Encoding.GetEncoding(866),
        //        StandardOutputEncoding = Encoding.GetEncoding(866),
        //    };

        //    var process = Process.Start(processInfo);

        //    process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
        //        logger.Info("output>> " + e.Data);
        //    process.BeginOutputReadLine();

        //    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
        //        logger.Info("error>> " + e.Data);
        //    process.BeginErrorReadLine();

        //    process.WaitForExit();

        //    if(process.ExitCode == 0)
        //    {
        //        logger.Info("Успешное завершение команды - " + command);
        //    }
        //    else
        //    {
        //        logger.Info("Ошибка выполнения команды - " + command);
        //    }

        //    process.Close();

        //    logger.Info("Задание - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group + " завершено!");

        //    return Task.CompletedTask;
        //}
    }

    public class CustomSheduler
    {
        public static void Start(string jobName, string groupName, string command, string cronExpression = "0 0/1 * * * ?")
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<CustomJob>().WithDescription("Пользовательская настройка").Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity(jobName, groupName)     // идентифицируем триггер с именем и группой
                .StartNow()
                .WithCronSchedule(cronExpression)      // каждую минуту 
                .UsingJobData("Command", command)
                .ForJob(job)
                .Build();                               // создаем триггер

            scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы

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


                var objects = LoadData("archive-jobs.dat") as ObservableCollection<ArchiveSavedJob>;
                if (objects != null)
                {
                    archiveJobs = objects;

                    if (Settings.Default.bAutoStartScheduler)
                    {
                        if (archiveJobs.Count > 0)
                        {
                            for (int i = 0; i < archiveJobs.Count; i++)
                            {
                                archiveJobs[i].Status = ArchiveScheduler.Start(archiveJobs[i]);
                            }
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

            SaveData("archive-jobs.dat", archiveJobs);

            foreach(var job in archiveJobs)
            {
                logger.Warn("Завершение задания (Завершение работы приложения) - " + job.Name + ": " + job.Group);
            }

            logger.Info("Завершение работы приложения");
        }

        private void addarch_bt_Click(object sender, RoutedEventArgs e)
        {
            ArchiveSavedJob archiveJob = new ArchiveSavedJob();
            archiveJob.Id = Guid.NewGuid();
            var addSchelude_dlg = new AddSchedule(archiveJob);

            if (addSchelude_dlg.ShowDialog() == true)
            {
                archiveJobs.Add(archiveJob);
                //archiveJobs.
                MessageBox.Show("Успешно изменено");
                archiveJob.Status = ArchiveScheduler.Start(archiveJob);

                SaveData("archive-jobs.dat", archiveJobs);
            }

            db_archivejobs.Items.Refresh();
        }

        private void removearch_bt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void edit_bt_Click(object sender, RoutedEventArgs e)
        {
            var item = db_archivejobs.SelectedItem as ArchiveSavedJob;

            Mapper.Reset();
            Mapper.Initialize(cfg => cfg.CreateMap<ArchiveSavedJob, ArchiveSavedJob>());
            ArchiveSavedJob temp = Mapper.Map<ArchiveSavedJob>(item);

            var addSchelude_dlg = new AddSchedule(item);
            addSchelude_dlg.Owner = this;

            if (addSchelude_dlg.ShowDialog() == true)
            {
                item.Status = !ArchiveScheduler.Stop(item);
                //archiveJobs.
                item.Status = ArchiveScheduler.Start(item);
            }
            else
            { 
                archiveJobs[db_archivejobs.SelectedIndex] = temp;
            }

            db_archivejobs.Items.Refresh();

        }

        private void stop_bt_Click(object sender, RoutedEventArgs e)
        {
            var item = db_archivejobs.SelectedItem as ArchiveSavedJob;

            if(item != null)
            {
                if(item.Status == true)
                {
                    item.Status = !ArchiveScheduler.Stop(item);
                    db_archivejobs.Items.Refresh();
                }
            }

        }

        private void start_bt_Click(object sender, RoutedEventArgs e)
        {
            var item = db_archivejobs.SelectedItem as ArchiveSavedJob;

            if (item != null)
            {
                if (item.Status == false)
                {
                    item.Status = ArchiveScheduler.Start(item);
                    db_archivejobs.Items.Refresh();
                }
            }

        }

        private void stopAllSchelude_bt_Click(object sender, RoutedEventArgs e)
        {
            if (archiveJobs.Count > 0)
            {
                for (int i = 0; i < archiveJobs.Count; i++)
                {
                    archiveJobs[i].Status = !ArchiveScheduler.Stop(archiveJobs[i]);
                    db_archivejobs.Items.Refresh();
                }
            }
        }

        private void startAllSchelude_bt_Click(object sender, RoutedEventArgs e)
        {
            if (archiveJobs.Count > 0)
            {
                for (int i = 0; i < archiveJobs.Count; i++)
                {
                    archiveJobs[i].Status = ArchiveScheduler.Start(archiveJobs[i]);
                    db_archivejobs.Items.Refresh();
                }
            }
        }


        private void SaveData(string filename,object jobs)
        {
            if (jobs != null)
            {
                // создаем объект BinaryFormatter
                BinaryFormatter formatter = new BinaryFormatter();
                // получаем поток, куда будем записывать сериализованный объект
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    formatter.Serialize(fs, jobs);
                }
            }
        }

        private object LoadData(string filename)
        {

            if (File.Exists(filename))
            {
                // десериализация из файла people.dat
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    object obj = bf.Deserialize(fs);

                    return obj;
                }
            }

            return null;

        }
    }
}

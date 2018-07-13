using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ZipPlanner.Models
{
    public class ArchiveJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example

                // Получение свойств
                //string Command = dataMap.GetString("Command");
                string Startpath = dataMap.GetString("StartPath");
                string Endpath = dataMap.GetString("EndPath");
                string EndFileName = dataMap.GetString("EndFileName");
                ObservableCollection<string> Filter = (ObservableCollection<string>)dataMap.Get("Filter");
                string DateTimeFormat = dataMap.GetString("DateTimeFormat");

                bool DeleteFiles = dataMap.GetBoolean("DeleteFiles");

                bool UseName = dataMap.GetBoolean("UseName");
                bool UseGroup = dataMap.GetBoolean("UseGroup");
                bool UseDateTimeFormat = dataMap.GetBoolean("UseDateTimeFormat");
                bool UseGuid = dataMap.GetBoolean("UseGuid");

                // Разделитель в имени файла
                string separator = "_";
                string FileName = Endpath + @"\" + EndFileName;

                if (UseName)
                {
                    string TriggerName = separator + context.Trigger.Key.Name;
                    FileName += TriggerName;
                }
                if (UseGroup)
                {
                    string TriggerGroup = separator + context.Trigger.Key.Group;
                    FileName += TriggerGroup;
                }
                if (UseDateTimeFormat)
                {

                    DateTime date = DateTime.Now;


                    DateTimeFormat = DateTimeFormat.Replace(":", separator);
                    DateTimeFormat = "{0:" + DateTimeFormat + "}";
                    DateTimeFormat = DateTimeFormat.Replace(" ", separator);


                    DateTimeFormat = String.Format(DateTimeFormat, date);
                    DateTimeFormat = separator + DateTimeFormat;
                    FileName += DateTimeFormat;
                }
                if (UseGuid)
                {
                    string guid = separator + Guid.NewGuid().ToString();
                    FileName += guid;
                }

                FileName += ".zip";

                DirectoryInfo di = new DirectoryInfo(Startpath);

                logger.Info("Выполнение задания - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group);
                if (DeleteFiles)
                {
                    logger.Info("Автоматическое удаление исходных файлов включено");
                }
                else
                {
                    logger.Info("Автоматическое удаление исходных файлов отключено");
                }

                logger.Info("Создание архива - " + FileName);

                List<FileInfo> files = getFiles(Startpath, Filter, SearchOption.TopDirectoryOnly);

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

                        if (DeleteFiles)
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

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        public List<FileInfo> getFiles(string SourceFolder, ObservableCollection<string> Filter, SearchOption searchOption)
        {
            // ArrayList will hold all file names
            List<FileInfo> files = new List<FileInfo>();

            // for each filter find mathing file names
            foreach (string FileFilter in Filter)
            {
                DirectoryInfo di = new DirectoryInfo(SourceFolder);
                // add found file names to array list
                files.AddRange(di.GetFiles(FileFilter, searchOption).ToList());
            }

            // returns string array of relevant file names
            return files;
        }

        //Task IJob.Execute(IJobExecutionContext context)
        //{
        //    var logger = LogManager.GetCurrentClassLogger();

        //    try
        //    {
        //        JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example

        //        // Получение свойств
        //        //string Command = dataMap.GetString("Command");
        //        string Startpath = dataMap.GetString("StartPath");
        //        string Endpath = dataMap.GetString("EndPath");
        //        string EndFileName = dataMap.GetString("EndFileName");
        //        ObservableCollection<string> Filter = (ObservableCollection<string>)dataMap.Get("Filter");
        //        string DateTimeFormat = dataMap.GetString("DateTimeFormat");

        //        bool DeleteFiles = dataMap.GetBoolean("DeleteFiles");

        //        bool UseName = dataMap.GetBoolean("UseName");
        //        bool UseGroup = dataMap.GetBoolean("UseGroup");
        //        bool UseDateTimeFormat = dataMap.GetBoolean("UseDateTimeFormat");
        //        bool UseGuid = dataMap.GetBoolean("UseGuid");

        //        // Разделитель в имени файла
        //        string separator = "_";
        //        string FileName = Endpath + @"\"+EndFileName;

        //        if (UseName)
        //        {
        //            string TriggerName = separator + context.Trigger.Key.Name;
        //            FileName += TriggerName;
        //        }
        //        if (UseGroup)
        //        {
        //            string TriggerGroup = separator + context.Trigger.Key.Group;
        //            FileName += TriggerGroup;
        //        }
        //        if(UseDateTimeFormat)
        //        {

        //            DateTime date = DateTime.Now;


        //            DateTimeFormat = DateTimeFormat.Replace(":", separator);
        //            DateTimeFormat = "{0:" + DateTimeFormat + "}";
        //            DateTimeFormat = DateTimeFormat.Replace(" ", separator);


        //            DateTimeFormat = String.Format(DateTimeFormat, date);
        //            DateTimeFormat = separator + DateTimeFormat;
        //            FileName += DateTimeFormat;
        //        }
        //        if(UseGuid)
        //        {
        //            string guid = separator + Guid.NewGuid().ToString();
        //            FileName += guid;
        //        }

        //        FileName += ".zip";

        //        DirectoryInfo di = new DirectoryInfo(Startpath);

        //        logger.Info("Выполнение задания - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group);
        //        if (DeleteFiles)
        //        {
        //            logger.Info("Автоматическое удаление исходных файлов включено");
        //        }
        //        else
        //        {
        //            logger.Info("Автоматическое удаление исходных файлов отключено");
        //        }

        //        logger.Info("Создание архива - " + FileName);

        //        List<FileInfo> files = getFiles(Startpath, Filter, SearchOption.TopDirectoryOnly);

        //        if (files != null)
        //        {
        //            if (files.Count > 0)
        //            {
        //                using (FileStream zipToOpen = new FileStream(FileName, FileMode.Create))
        //                {
        //                    using (ZipArchive archive = new ZipArchive(zipToOpen, mode: ZipArchiveMode.Create))
        //                    {
        //                        foreach (var file in files)
        //                        {
        //                            archive.CreateEntryFromFile(file.FullName, file.Name);
        //                        }
        //                    }
        //                }

        //                logger.Info("Архив - " + FileName + " создан");
        //                logger.Info("Задание - " + context.Trigger.Key.Name + ": " + context.Trigger.Key.Group + " завершено!");

        //                if (DeleteFiles)
        //                {
        //                    foreach (var file in files)
        //                    {
        //                        File.Delete(file.FullName);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                logger.Warn("Архив - " + FileName + " не создан");
        //                logger.Warn("Отсутствуют файлы в директории");
        //            }

        //        }
        //        else
        //        {
        //            logger.Warn("Архив - " + FileName + " не создан");
        //            logger.Warn("Не удалость получить список файлов");
        //        }

        //        return Task.CompletedTask;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e.Message);
        //        return Task.FromException(e);
        //    }
        //}
    }
}

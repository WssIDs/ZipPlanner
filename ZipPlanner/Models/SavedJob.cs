using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCrontab;
using Quartz;
using Quartz.Impl;
namespace ZipPlanner.Models
{
    public enum ScheludeStatus
    {
        STOPPED,
        PAUSED,
        STARTED
    }


    [Serializable]
    abstract public class SavedJob //: IDataErrorInfo
    {
        //public string this[string columnName]
        //{
        //    get
        //    {
        //        string error = String.Empty;
        //        switch (columnName)
        //        {
        //            case "Name":
        //                if (Name == "" || Name == null)
        //                {
        //                    error = "Имя задания не должно быть пустым";
        //                }
        //                else
        //                {
        //                    if (Name.Length < 3 || Name.Length > 9)
        //                    {
        //                        error = "Имя задания должно быть больше 3 и меньше 10";
        //                    }
        //                }
        //                break;
        //            case "Group":
        //                if (Group == "" || Group == null)
        //                {
        //                    error = "Имя группы задания не должно быть пустым";
        //                }
        //                else
        //                {
        //                    if (Group.Length < 3 || Group.Length > 9)
        //                    {
        //                        error = "Имя группы задания должно быть больше 3 и меньше 10";
        //                    }
        //                }
        //                break;
        //            case "CronExpression":
        //                if (CronExpression == null)
        //                {
        //                    error = "Выражение Сron не должно быть пустым";
        //                }
        //                else
        //                {
        //                    if (!Quartz.CronExpression.IsValidExpression(CronExpression))
        //                    {
        //                        error = "Выражение Сron некорректное";
        //                    }
        //                }
        //                break;
        //        }
        //        return error;
        //    }
        //}

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string CronExpression { get; set; }

        //[NonSerialized]
        public bool Status { get; set; }
    }


    [Serializable]
    public class ArchiveSavedJob : SavedJob, IDataErrorInfo
    {
        public ArchiveSavedJob()
        {
            Filter = new ObservableCollection<string>();
        }


        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "Name":
                        if (Name == "" || Name == null)
                        {
                            error = "Имя задания не должно быть пустым";
                        }
                        else
                        {
                            if (Name.Length < 3 || Name.Length > 9)
                            {
                                error = "Имя задания должно быть больше 3 и меньше 10";
                            }
                        }
                        break;
                    case "Group":
                        if (Group == "" || Group == null)
                        {
                            error = "Имя группы задания не должно быть пустым";
                        }
                        else
                        {
                            if (Group.Length < 2 || Group.Length > 15)
                            {
                                error = "Имя группы задания должно быть больше 2 и меньше 15";
                            }
                        }
                        break;
                    case "CronExpression":
                        if (CronExpression == null)
                        {
                            error = "Выражение Сron не должно быть пустым";
                        }
                        else
                        {
                            if (!Quartz.CronExpression.IsValidExpression(CronExpression))
                            {
                                error = "Выражение Сron некорректное";
                            }
                        }
                        break;
                    case "EndFileName":
                        if (EndFileName == "" || EndFileName == null)
                        {
                            error = "Имя файла не должно быть пустым";
                        }
                        else
                        {
                            if (EndFileName.Length < 3 || EndFileName.Length > 9)
                            {
                                error = "Имя файла должно быть больше 3 и меньше 10";
                            }
                        }
                        break;
                    case "Filter":
                        if (Filter.Count == 0 || Filter == null)
                        {
                            error = "Фильтр не должен быть пустым";
                        }
                        break;
                    case "StartPath":
                        if (!Directory.Exists(StartPath))
                        {
                            error = "Папка по текущему пути не найдена. Введите корректный путь";
                        }
                        break;
                    case "EndPath":
                        if (!Directory.Exists(EndPath))
                        {
                            error = "Папка по текущему пути не найдена. Введите корректный путь";
                        }
                        break;
                    case "DateTimeFormat":
                        if(DateTimeFormat == null || DateTimeFormat == "")
                        {
                            error = "Формат вывода даты не может быть пустым";
                        }
                        break;
                }
                return error;
            }
        }


        public string StartPath { get; set; }
        public string EndPath { get; set; }
        public string EndFileName { get; set; }
        public ObservableCollection<string> Filter { get; set; }
        public string DateTimeFormat { get; set; }


        public bool UseName { get; set; }
        public bool UseGroup { get; set; }
        public bool UseDateTimeFormat { get; set; }
        public bool UseGuid { get; set; }


        public bool DeleteFiles { get; set; }


        public string Error => throw new NotImplementedException();
    }

    [Serializable]
    public class CustomSavedJob : SavedJob
    {
        public string Command { get; set; }
    }


    public class CopySavedJob : SavedJob, IDataErrorInfo
    {
        public CopySavedJob()
        {
            Filter = new ObservableCollection<string>();
        }


        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "Name":
                        if (Name == "" || Name == null)
                        {
                            error = "Имя задания не должно быть пустым";
                        }
                        else
                        {
                            if (Name.Length < 3 || Name.Length > 9)
                            {
                                error = "Имя задания должно быть больше 3 и меньше 10";
                            }
                        }
                        break;
                    case "Group":
                        if (Group == "" || Group == null)
                        {
                            error = "Имя группы задания не должно быть пустым";
                        }
                        else
                        {
                            if (Group.Length < 2 || Group.Length > 15)
                            {
                                error = "Имя группы задания должно быть больше 2 и меньше 15";
                            }
                        }
                        break;
                    case "CronExpression":
                        if (CronExpression == null)
                        {
                            error = "Выражение Сron не должно быть пустым";
                        }
                        else
                        {
                            if (!Quartz.CronExpression.IsValidExpression(CronExpression))
                            {
                                error = "Выражение Сron некорректное";
                            }
                        }
                        break;
                    case "Filter":
                        if (Filter.Count == 0 || Filter == null)
                        {
                            error = "Фильтр не должен быть пустым";
                        }
                        break;
                    case "StartPath":
                        if (!Directory.Exists(StartPath))
                        {
                            error = "Папка по текущему пути не найдена. Введите корректный путь";
                        }
                        break;
                    case "EndPath":
                        if (!Directory.Exists(EndPath))
                        {
                            error = "Папка по текущему пути не найдена. Введите корректный путь";
                        }
                        break;
                }
                return error;
            }
        }


        public string StartPath { get; set; }
        public string EndPath { get; set; }
        public ObservableCollection<string> Filter { get; set; }

        public bool DeleteFiles { get; set; }

        public string Error => throw new NotImplementedException();
    }
}

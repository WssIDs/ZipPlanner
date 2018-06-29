using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipPlanner.Models
{
    public enum ScheludeStatus
    {
        STOPPED,
        PAUSED,
        STARTED
    }


    [Serializable]
    abstract public class SavedJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string CronExpression { get; set; }

        [NonSerialized]
        public bool Status;
    }


    [Serializable]
    public class ArchiveSavedJob : SavedJob
    {
        public string StartPath { get; set; }
        public string EndPath { get; set; }

        public string EndFileName { get; set; }

        public string SearchPattern { get; set; }

        public bool DeleteFiles { get; set; }
    }

    [Serializable]
    public class CustomSavedJob : SavedJob
    {
        public string Command { get; set; }
    }
}

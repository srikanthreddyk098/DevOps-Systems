using System;

namespace CalpineAzureDashboard.Models
{
    public class TagModel
    {
        public string Application_Name { get; set; }
        public ProjectInfo Project_Info { get; set; }
        public Backup Backup { get; set; }
        public string Reserved_Instance { get; set; }
        public string Server_Type { get; set; }
    }

    public class ProjectInfo
    {
        public string Code { get; set; }
        public Duration Duration { get; set; }

    }

    public class Duration
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class Backup
    {
        public string Policy { get; set; }
        public string Frequency { get; set; }
    }
}
using Microsoft.PowerBI.Api.V2.Models;

namespace CalpineAzureDashboard.Web.Models
{
    public class ReportViewModel
    {
        public string GroupId { get; set; }
        public Report Report { get; set; }
    }
}
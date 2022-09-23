using System.Collections.Generic;
using Microsoft.PowerBI.Api.V2.Models;

namespace CalpineAzureDashboard.Web.Models
{
    public class ReportsViewModel
    {
        public string GroupId { get; set; }
        public List<Report> Reports { get; set; }
    }
}
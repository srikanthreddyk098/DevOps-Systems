using AzureAutomation.Models;

namespace AzureAutomation.Web.ViewModels
{
    public class VmViewModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public ScheduleTagsModel Schedule { get; set; }
        public double? HourlyCost { get; set; }
        public double? WeeklyCost { get; set; }
        public double? WeeklySavings { get; set; }
        public double? WeeklySavingsPercent { get; set; }
    }
}

namespace AzureAutomation.Models
{
    public class AzureVmPrice
    {
        public string Name { get; set; }
        public string RegionId { get; set; }
        public double LinuxPrice { get; set; }
        public double WindowsPrice { get; set; }
    }
}
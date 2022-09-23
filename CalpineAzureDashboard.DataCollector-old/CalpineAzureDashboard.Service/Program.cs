using System.ServiceProcess;

namespace CalpineAzureDashboard.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            var service = new CalpineAzureDashboardService();
            service.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new CalpineAzureDashboardService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

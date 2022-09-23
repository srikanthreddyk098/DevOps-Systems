using System.ServiceProcess;

namespace CalpineAzureMonitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            var service = new CalpineAzureMonitor();
            service.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] {
                new CalpineAzureMonitor()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}

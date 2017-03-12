using System.ServiceProcess;

namespace Logs
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new Logs() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

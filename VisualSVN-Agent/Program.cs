using System.ServiceProcess;

namespace VisualSVN_Agent
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 
        static void Main()
        {
#if DEBUG
            MainAgent ma = new MainAgent();
            ma.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#else
        
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainAgent()
            };
            ServiceBase.Run(ServicesToRun);
        
#endif

        }

    }
}

using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Linq;

namespace Asus.Helpers
{
    public static class ServiceHelper
    {
        private static readonly string[] EssentialServices = { "ASUSOptimization", "AsusPTPService" };
        private static readonly string[] BloatwareServices = { 
            "AsusAppService", 
            "ASUSSoftwareManager", 
            "ASUSSwitch", 
            "ASUSSystemAnalysis", 
            "ASUSSystemDiagnosis",
            "ASUSLinkRemote",
            "ASUSLinkNear"
        };

        public static bool IsBloatwareRunning()
        {
            try
            {
                var services = ServiceController.GetServices();
                foreach (var name in BloatwareServices)
                {
                    var sc = services.FirstOrDefault(s => s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (sc != null && (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Failed to get services: " + ex.Message);
            }
            return false;
        }

        public static void StopBloatwareServices()
        {
            if (!ProcessHelper.IsUserAdministrator())
            {
                ProcessHelper.RunAsAdmin("stop-services");
                return;
            }

            foreach (var name in BloatwareServices)
            {
                RunSc($"stop \"{name}\"");
                RunSc($"config \"{name}\" start= disabled");
            }
        }

        public static void StartBloatwareServices()
        {
            if (!ProcessHelper.IsUserAdministrator())
            {
                ProcessHelper.RunAsAdmin("start-services");
                return;
            }

            foreach (var name in BloatwareServices)
            {
                RunSc($"config \"{name}\" start= demand");
                RunSc($"start \"{name}\"");
            }
        }

        private static void RunSc(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(psi)?.WaitForExit(3000);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Failed to run sc {arguments}: {ex.Message}");
            }
        }
    }
}

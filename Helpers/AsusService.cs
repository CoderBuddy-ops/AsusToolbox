using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Linq;
using System.Collections.Generic;

namespace Asus.Helpers
{
    public static class AsusService
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

        public static bool IsAsusOptimizationRunning()
        {
            return IsServiceRunning("ASUSOptimization");
        }

        public static bool IsArmouryRunning() => false;
        public static void RunArmouryUninstaller() {}
        public static bool IsOSDRunning() => false;

        public static int GetRunningCount()
        {
            try
            {
                int count = 0;
                var services = ServiceController.GetServices();
                foreach (var name in BloatwareServices)
                {
                    var sc = services.FirstOrDefault(s => s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (sc != null && (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending))
                    {
                        count++;
                    }
                }
                return count;
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Failed to get running count: " + ex.Message);
                return 0;
            }
        }

        public static void StopAsusServices()
        {
            foreach (var name in BloatwareServices)
            {
                RunSc($"stop \"{name}\"");
                RunSc($"config \"{name}\" start= disabled");
            }
        }

        public static void StartAsusServices()
        {
            foreach (var name in BloatwareServices)
            {
                RunSc($"config \"{name}\" start= demand");
                RunSc($"start \"{name}\"");
            }
        }

        public static void StopOnStartup()
        {
            // Do nothing on startup by default, wait for explicit user action
        }

        private static bool IsServiceRunning(string name)
        {
            try
            {
                var services = ServiceController.GetServices();
                var sc = services.FirstOrDefault(s => s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase));
                return sc != null && (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending);
            }
            catch
            {
                return false;
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
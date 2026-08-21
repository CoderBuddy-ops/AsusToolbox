using System.Diagnostics;

namespace Asus.Helpers
{
    /// <summary>
    /// One point-in-time snapshot of this process's resource usage. Used to
    /// verify the "tiny tray utility" resource budget (idle RAM, CPU time,
    /// thread/handle counts) without a profiler.
    /// </summary>
    public readonly record struct DiagnosticSnapshot(
        DateTime Timestamp,
        long WorkingSetBytes,
        long PrivateMemoryBytes,
        double TotalCpuSeconds,
        int ThreadCount,
        int HandleCount)
    {
        public double WorkingSetMb => WorkingSetBytes / 1024.0 / 1024.0;
        public double PrivateMemoryMb => PrivateMemoryBytes / 1024.0 / 1024.0;

        public string Summary()
            => $"WorkingSet={WorkingSetMb:F1}MB Private={PrivateMemoryMb:F1}MB CPU={TotalCpuSeconds:F1}s Threads={ThreadCount} Handles={HandleCount}";
    }

    public static class Diagnostics
    {
        /// <summary>Captures the current process's resource usage.</summary>
        public static DiagnosticSnapshot Capture()
        {
            using var p = Process.GetCurrentProcess();
            p.Refresh();
            return new DiagnosticSnapshot(
                DateTime.UtcNow,
                p.WorkingSet64,
                p.PrivateMemorySize64,
                p.TotalProcessorTime.TotalSeconds,
                p.Threads.Count,
                p.HandleCount);
        }
    }
}

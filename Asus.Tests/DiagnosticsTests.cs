using Xunit;

namespace Asus.Tests;

/// <summary>
/// Lightweight process diagnostics (Diagnostics.Capture) used to verify the
/// idle resource budget of the tray process.
/// </summary>
public class DiagnosticsTests
{
    [Fact]
    public void Capture_ReturnsSaneProcessMetrics()
    {
        var snapshot = Asus.Helpers.Diagnostics.Capture();

        Assert.True(snapshot.WorkingSetBytes > 0, "working set should be non-zero");
        Assert.True(snapshot.PrivateMemoryBytes > 0, "private memory should be non-zero");
        Assert.True(snapshot.ThreadCount >= 1, "a process has at least one thread");
        Assert.True(snapshot.HandleCount > 0, "a process holds at least one handle");
        Assert.True(snapshot.TotalCpuSeconds >= 0);
    }

    [Fact]
    public void Summary_IsReadable()
    {
        var summary = Asus.Helpers.Diagnostics.Capture().Summary();
        Assert.Contains("WorkingSet", summary);
        Assert.Contains("Threads", summary);
        Assert.Contains("Handles", summary);
    }
}

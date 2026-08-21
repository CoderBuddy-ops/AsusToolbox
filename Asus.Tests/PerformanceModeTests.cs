using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Performance-mode switching logic (ModeControl.SetPerformanceMode) with the
/// ACPI device mocked and the Windows power scheme untouched (skip_powermode).
/// </summary>
public class PerformanceModeTests
{
    readonly FakeAcpi acpi;
    readonly ModeControl modeControl;

    public PerformanceModeTests()
    {
        acpi = TestEnv.Reset();
        modeControl = Program.modeControl;
    }

    [Fact]
    public void SetPerformanceMode_SetsBalancedBase()
    {
        modeControl.SetPerformanceMode(0);
        modeControl.WaitForApply();

        Assert.Equal(AsusACPI.PerformanceBalanced, acpi.PerformanceModeSetTo);
        Assert.Equal(0, AppConfig.Get("performance_mode"));
    }

    [Fact]
    public void SetPerformanceMode_SetsTurboBase()
    {
        modeControl.SetPerformanceMode(1);
        modeControl.WaitForApply();

        Assert.Equal(AsusACPI.PerformanceTurbo, acpi.PerformanceModeSetTo);
        Assert.Equal(1, AppConfig.Get("performance_mode"));
    }

    [Fact]
    public void SetPerformanceMode_SetsSilentBase()
    {
        modeControl.SetPerformanceMode(2);
        modeControl.WaitForApply();

        Assert.Equal(AsusACPI.PerformanceSilent, acpi.PerformanceModeSetTo);
    }

    [Fact]
    public void SetPerformanceMode_UsesManual_WhenManualModeRequired()
    {
        AppConfig.Set("auto_apply_power_0", 1);
        AppConfig.Set("manual_mode", 1);

        modeControl.SetPerformanceMode(0);
        modeControl.WaitForApply();

        Assert.Equal(AsusACPI.PerformanceManual, acpi.PerformanceModeSetTo);
    }

    [Fact]
    public void SetPerformanceMode_WritesStatusLedBytes_WhenStatusModeEnabled()
    {
        AppConfig.Set("status_mode", 1);

        modeControl.SetPerformanceMode(0); // Balanced base -> 0x03 (not silent)
        modeControl.WaitForApply();

        var call = Assert.Single(acpi.ByteSetCalls);
        Assert.Equal(AsusACPI.StatusMode, call.Id);
        Assert.Equal(new byte[] { 0x00, 0x03 }, call.Data);
    }

    [Fact]
    public void SetPerformanceMode_WritesSilentStatusByte_ForSilentMode()
    {
        AppConfig.Set("status_mode", 1);

        modeControl.SetPerformanceMode(2); // Silent base -> 0x02
        modeControl.WaitForApply();

        var call = Assert.Single(acpi.ByteSetCalls);
        Assert.Equal(new byte[] { 0x00, 0x02 }, call.Data);
    }

    [Fact]
    public void SetPerformanceMode_DoesNotWriteStatus_WhenStatusModeDisabled()
    {
        modeControl.SetPerformanceMode(0);
        modeControl.WaitForApply();

        Assert.Empty(acpi.ByteSetCalls);
    }
}

using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Power-limit application logic (ModeControl.SetPower) with the ACPI device mocked.
/// </summary>
public class PowerLimitTests
{
    readonly FakeAcpi acpi;
    readonly ModeControl modeControl;

    public PowerLimitTests()
    {
        acpi = TestEnv.Reset();
        modeControl = Program.modeControl;
    }

    [Fact]
    public void SetPower_WritesNothing_WhenTotalAboveMaximum()
    {
        AppConfig.Set("limit_total_0", AsusACPI.MaxTotal + 1);
        AppConfig.Set("limit_cpu_0", 60);
        AppConfig.Set("limit_fast_0", 90);

        modeControl.SetPower();

        Assert.Empty(acpi.DeviceValues);
    }

    [Fact]
    public void SetPower_WritesNothing_WhenTotalBelowMinimum()
    {
        AppConfig.Set("limit_total_0", AsusACPI.MinTotal - 1);
        AppConfig.Set("limit_cpu_0", 60);
        AppConfig.Set("limit_fast_0", 90);

        modeControl.SetPower();

        Assert.Empty(acpi.DeviceValues);
    }

    [Fact]
    public void SetPower_WritesNothing_WhenPeripheralLimitsOutOfRange()
    {
        AppConfig.Set("limit_total_0", 80);
        AppConfig.Set("limit_cpu_0", AsusACPI.MinCPU - 1); // too low
        AppConfig.Set("limit_fast_0", 90);

        modeControl.SetPower();

        Assert.Empty(acpi.DeviceValues);
    }

    [Fact]
    public void SetPower_SetsAPULimits_WhenApuEndpointsSupported()
    {
        acpi.Supported.Add(AsusACPI.PPT_APUA0);
        AppConfig.Set("limit_total_0", 80);
        AppConfig.Set("limit_slow_0", 70);
        AppConfig.Set("limit_cpu_0", 60);
        AppConfig.Set("limit_fast_0", 90);

        modeControl.SetPower();

        Assert.Equal(80, acpi.DeviceValues[AsusACPI.PPT_APUA3]); // SPL (sustained)
        Assert.Equal(70, acpi.DeviceValues[AsusACPI.PPT_APUA0]); // sPPT (slow)
        Assert.False(acpi.DeviceValues.ContainsKey(AsusACPI.PPT_CPUB0));
    }

    [Fact]
    public void SetPower_FallsBackSlowLimitToTotal_WhenNotConfigured()
    {
        acpi.Supported.Add(AsusACPI.PPT_APUA0);
        AppConfig.Set("limit_total_0", 80);
        AppConfig.Set("limit_cpu_0", 60);
        AppConfig.Set("limit_fast_0", 90);

        modeControl.SetPower();

        Assert.Equal(80, acpi.DeviceValues[AsusACPI.PPT_APUA3]);
        Assert.Equal(80, acpi.DeviceValues[AsusACPI.PPT_APUA0]);
    }

    [Fact]
    public void SetPower_SetsCpuLimit_WhenAllAmdPlatform()
    {
        acpi.AllAmdPpt = true;
        acpi.Supported.Add(AsusACPI.PPT_APUA0);
        AppConfig.Set("limit_total_0", 80);
        AppConfig.Set("limit_cpu_0", 60);
        AppConfig.Set("limit_fast_0", 90);

        modeControl.SetPower();

        Assert.Equal(80, acpi.DeviceValues[AsusACPI.PPT_APUA3]);
        Assert.Equal(80, acpi.DeviceValues[AsusACPI.PPT_APUA0]);
        Assert.Equal(60, acpi.DeviceValues[AsusACPI.PPT_CPUB0]);
    }
}

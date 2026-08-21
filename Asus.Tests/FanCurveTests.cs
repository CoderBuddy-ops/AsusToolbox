using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Fan-curve application logic (ModeControl.AutoFans) with the ACPI device mocked.
/// </summary>
public class FanCurveTests
{
    readonly FakeAcpi acpi;
    readonly ModeControl modeControl;

    public FanCurveTests()
    {
        acpi = TestEnv.Reset();
        modeControl = Program.modeControl;
    }

    void EnableCustomFans()
    {
        AppConfig.Set("auto_apply_0", 1);
        TestEnv.SetFanProfiles(acpi);
    }

    [Fact]
    public void AutoFans_AppliesCustomFanCurves_WhenEnabled()
    {
        EnableCustomFans();

        modeControl.AutoFans();

        Assert.True(acpi.FanCurveCalls.ContainsKey(AsusFan.CPU));
        Assert.True(acpi.FanCurveCalls.ContainsKey(AsusFan.GPU));
        Assert.Equal(16, acpi.FanCurveCalls[AsusFan.CPU].Length);
        Assert.Equal(16, acpi.FanCurveCalls[AsusFan.GPU].Length);
        Assert.Empty(acpi.FanRangeCalls);
        Assert.DoesNotContain(acpi.Calls, c => c.Contains("Reset Mode"));
    }

    [Fact]
    public void AutoFans_AppliesMidFanCurve_WhenEnabled()
    {
        EnableCustomFans();
        AppConfig.Set("mid_fan", 1);
        TestEnv.SetFanProfiles(acpi, mid: true);

        modeControl.AutoFans();

        Assert.True(acpi.FanCurveCalls.ContainsKey(AsusFan.Mid));
    }

    [Fact]
    public void AutoFans_FallsBackToFanRange_WhenCurveRejected()
    {
        EnableCustomFans();
        acpi.SetFanCurveResult = -1;
        acpi.SetFanRangeResult = 1;

        modeControl.AutoFans();

        Assert.True(acpi.FanRangeCalls.ContainsKey(AsusFan.CPU));
        Assert.True(acpi.FanRangeCalls.ContainsKey(AsusFan.GPU));
        Assert.DoesNotContain(acpi.Calls, c => c.Contains("Reset Mode"));
    }

    [Fact]
    public void AutoFans_ResetsMode_WhenBothCurveAndRangeRejected()
    {
        EnableCustomFans();
        acpi.SetFanCurveResult = -1;
        acpi.SetFanRangeResult = -1;

        modeControl.AutoFans();

        Assert.True(acpi.FanRangeCalls.ContainsKey(AsusFan.CPU));
        Assert.Contains(acpi.Calls, c => c.Contains("Reset Mode"));
        Assert.Equal(Modes.GetCurrentBase(), acpi.DeviceValues[AsusACPI.PerformanceMode]);
    }

    [Fact]
    public void AutoFans_AppliesHysteresis_WhenConfigured()
    {
        EnableCustomFans();
        AppConfig.Set("hysteresis_up_0", 60);
        AppConfig.Set("hysteresis_down_0", 20);

        modeControl.AutoFans();

        Assert.Contains((60, 20), acpi.HysteresisCalls);
    }

    [Fact]
    public void AutoFans_SkipsCurves_WhenNotEnabled()
    {
        modeControl.AutoFans();

        Assert.Empty(acpi.FanCurveCalls);
        Assert.Empty(acpi.FanRangeCalls);
    }
}

using Asus.Fan;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Fan display/calibration logic used by the Fans form.
/// </summary>
public class FanSensorControlTests
{
    readonly FakeAcpi acpi;

    public FanSensorControlTests()
    {
        acpi = TestEnv.Reset();
        // Restore the static fan state to known defaults between tests.
        FanSensorControl.fanRpm = false;
        FanSensorControl.SetFanMax(AsusFan.CPU, FanSensorControl.DEFAULT_FAN_MAX);
        FanSensorControl.SetFanMax(AsusFan.GPU, FanSensorControl.DEFAULT_FAN_MAX);
        FanSensorControl.SetFanMax(AsusFan.Mid, FanSensorControl.DEFAULT_FAN_MAX);
    }

    [Fact]
    public void FormatFan_ShowsPercentOfMax_WhenRpmDisabled()
    {
        Assert.Equal("50%", FanSensorControl.FormatFan(AsusFan.CPU, 29)); // 29/58
    }

    [Fact]
    public void FormatFan_ShowsRpm_WhenRpmEnabled()
    {
        FanSensorControl.fanRpm = true;

        Assert.Equal("2900RPM", FanSensorControl.FormatFan(AsusFan.CPU, 29));

        FanSensorControl.fanRpm = false;
    }

    [Fact]
    public void FormatFan_RaisesMeasuredMax_WhenReadingAboveConfiguredMax()
    {
        Assert.Equal(FanSensorControl.DEFAULT_FAN_MAX, FanSensorControl.GetFanMax(AsusFan.CPU));

        Assert.Equal("100%", FanSensorControl.FormatFan(AsusFan.CPU, 80));

        Assert.Equal(80, FanSensorControl.GetFanMax(AsusFan.CPU));
        Assert.Equal(80, AppConfig.Get("fan_max_0"));
    }

    [Fact]
    public void FormatFan_IgnoresAbsurdReadings_AboveCalibrationCeiling()
    {
        Assert.Equal(FanSensorControl.DEFAULT_FAN_MAX, FanSensorControl.GetFanMax(AsusFan.CPU));

        FanSensorControl.FormatFan(AsusFan.CPU, FanSensorControl.INADEQUATE_MAX + 1);

        Assert.Equal(FanSensorControl.DEFAULT_FAN_MAX, FanSensorControl.GetFanMax(AsusFan.CPU));
    }

    [Fact]
    public void GetFanMin_ReturnsDefault()
    {
        Assert.Equal(FanSensorControl.DEFAULT_FAN_MIN, FanSensorControl.GetFanMin(AsusFan.CPU));
    }

    [Fact]
    public void SetFanMax_PersistsToConfig()
    {
        FanSensorControl.SetFanMax(AsusFan.GPU, 70);

        Assert.Equal(70, FanSensorControl.GetFanMax(AsusFan.GPU));
        Assert.Equal(70, AppConfig.Get("fan_max_1"));
    }

    [Fact]
    public void StartCalibration_SetsTurboAndFullSpeedCurvesOnAllFans()
    {
        var sensorControl = new FanSensorControl(null!);

        sensorControl.StartCalibration();

        Assert.Equal(AsusACPI.PerformanceTurbo, acpi.DeviceValues[AsusACPI.PerformanceMode]);
        Assert.Equal(3, acpi.FanCurveCalls.Count);
        Assert.Equal(16, acpi.FanCurveCalls[AsusFan.CPU].Length);
        Assert.Equal(16, acpi.FanCurveCalls[AsusFan.GPU].Length);
        Assert.Equal(16, acpi.FanCurveCalls[AsusFan.Mid].Length);

        sensorControl.AbortCalibration();
    }
}

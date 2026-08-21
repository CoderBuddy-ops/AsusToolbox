using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// "AI Auto" adaptive performance mode decision logic (AutoModeControl).
/// </summary>
public class AutoModeControlTests
{
    public AutoModeControlTests()
    {
        TestEnv.Reset();
    }

    [Fact]
    public void Defaults_MatchPreviousHardcodedBehavior()
    {
        Assert.Equal(75, AutoModeControl.TempHigh);
        Assert.Equal(55, AutoModeControl.TempLow);
        Assert.Equal(5, AutoModeControl.Hysteresis);
        Assert.Equal(1, AutoModeControl.IntervalSeconds);
    }

    [Fact]
    public void GetTargetMode_AboveHighThreshold_SelectsTurbo()
    {
        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(80, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(76, AsusACPI.PerformanceBalanced));
    }

    [Fact]
    public void GetTargetMode_BelowLowThreshold_SelectsSilent()
    {
        Assert.Equal(AsusACPI.PerformanceSilent, AutoModeControl.GetTargetMode(40, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceSilent, AutoModeControl.GetTargetMode(54, AsusACPI.PerformanceBalanced));
    }

    [Fact]
    public void GetTargetMode_InBetween_SelectsBalanced()
    {
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(60, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(55, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(75, AsusACPI.PerformanceBalanced));
    }

    [Fact]
    public void GetTargetMode_Hysteresis_KeepsTurboUntilBelowHighMinusHysteresis()
    {
        // Currently Turbo at 72°C (> 75 - 5) -> stay Turbo
        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(72, AsusACPI.PerformanceTurbo));
        // At or below 70°C -> drop back to Balanced
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(70, AsusACPI.PerformanceTurbo));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(69, AsusACPI.PerformanceTurbo));
    }

    [Fact]
    public void GetTargetMode_Hysteresis_KeepsSilentUntilAboveLowPlusHysteresis()
    {
        // Currently Silent at 58°C (< 55 + 5) -> stay Silent
        Assert.Equal(AsusACPI.PerformanceSilent, AutoModeControl.GetTargetMode(58, AsusACPI.PerformanceSilent));
        // At or above 60°C -> back to Balanced
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(60, AsusACPI.PerformanceSilent));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(61, AsusACPI.PerformanceSilent));
    }

    [Fact]
    public void GetTargetMode_ZeroHysteresis_MatchesSimpleThresholdLogic()
    {
        AppConfig.Set("ai_auto_hysteresis", 0);

        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(76, AsusACPI.PerformanceTurbo));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(75, AsusACPI.PerformanceTurbo));
        Assert.Equal(AsusACPI.PerformanceSilent, AutoModeControl.GetTargetMode(54, AsusACPI.PerformanceSilent));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(55, AsusACPI.PerformanceSilent));
    }

    [Fact]
    public void GetTargetMode_RespectsCustomThresholds()
    {
        AppConfig.Set("ai_auto_temp_high", 90);
        AppConfig.Set("ai_auto_temp_low", 60);
        AppConfig.Set("ai_auto_hysteresis", 10);

        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(91, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(85, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceSilent, AutoModeControl.GetTargetMode(59, AsusACPI.PerformanceBalanced));
        // hysteresis band: stay Turbo down to 80, stay Silent up to 70
        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(82, AsusACPI.PerformanceTurbo));
        Assert.Equal(AsusACPI.PerformanceSilent, AutoModeControl.GetTargetMode(68, AsusACPI.PerformanceSilent));
    }

    [Fact]
    public void GetTargetMode_IgnoresInvalidTemperature()
    {
        Assert.Equal(AsusACPI.PerformanceBalanced, AutoModeControl.GetTargetMode(0, AsusACPI.PerformanceBalanced));
        Assert.Equal(AsusACPI.PerformanceTurbo, AutoModeControl.GetTargetMode(-1, AsusACPI.PerformanceTurbo));
    }

    [Fact]
    public void IntervalSeconds_IsAtLeastOne()
    {
        AppConfig.Set("ai_auto_interval", 0);
        Assert.Equal(1, AutoModeControl.IntervalSeconds);
        AppConfig.Set("ai_auto_interval", -5);
        Assert.Equal(1, AutoModeControl.IntervalSeconds);
        AppConfig.Set("ai_auto_interval", 10);
        Assert.Equal(10, AutoModeControl.IntervalSeconds);
    }
}

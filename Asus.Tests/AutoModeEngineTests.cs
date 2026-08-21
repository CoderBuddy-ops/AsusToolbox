using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Rule-based AI Auto decision engine (AutoModeEngine): workload classification,
/// decision reasons, cooldown streak, thermal fallback and invalid-data safety.
/// </summary>
public class AutoModeEngineTests
{
    private static AutoModeConfig DefaultConfig() => new()
    {
        TempHigh = 75,
        TempLow = 55,
        Hysteresis = 5,
        CooldownSamples = 3,
        ThermalLimit = 90,
        HeavyLoadPercent = 70,
        LightLoadPercent = 20,
    };

    private static AutoModeEngine Engine() => new(DefaultConfig());

    [Fact]
    public void ClassifyLoad_Boundaries()
    {
        var cfg = DefaultConfig();
        Assert.Equal(WorkloadLevel.Light, AutoModeEngine.ClassifyLoad(0, cfg));
        Assert.Equal(WorkloadLevel.Light, AutoModeEngine.ClassifyLoad(20, cfg));
        Assert.Equal(WorkloadLevel.Moderate, AutoModeEngine.ClassifyLoad(21, cfg));
        Assert.Equal(WorkloadLevel.Moderate, AutoModeEngine.ClassifyLoad(69, cfg));
        Assert.Equal(WorkloadLevel.Heavy, AutoModeEngine.ClassifyLoad(70, cfg));
        Assert.Equal(WorkloadLevel.Heavy, AutoModeEngine.ClassifyLoad(100, cfg));
    }

    [Fact]
    public void SuggestedInterval_ScalesWithWorkload()
    {
        Assert.Equal(5000, AutoModeEngine.GetSuggestedIntervalMs(WorkloadLevel.Idle));
        Assert.Equal(3000, AutoModeEngine.GetSuggestedIntervalMs(WorkloadLevel.Light));
        Assert.Equal(2000, AutoModeEngine.GetSuggestedIntervalMs(WorkloadLevel.Moderate));
        Assert.Equal(1000, AutoModeEngine.GetSuggestedIntervalMs(WorkloadLevel.Heavy));
    }

    [Fact]
    public void InvalidSensorData_KeepsCurrentMode_AndDoesNotCrash()
    {
        var engine = Engine();
        foreach (float temp in new[] { 0f, -5f, float.NaN })
        {
            var d = engine.Evaluate(temp, 30f, false, AsusACPI.PerformanceBalanced);
            Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
        }

        var nullLoad = engine.Evaluate(50f, null, false, AsusACPI.PerformanceTurbo);
        Assert.Equal(AsusACPI.PerformanceTurbo, nullLoad.TargetMode);
        Assert.Equal("No sensor data", nullLoad.Reason);

        var negativeLoad = engine.Evaluate(50f, -1f, false, AsusACPI.PerformanceSilent);
        Assert.Equal(AsusACPI.PerformanceSilent, negativeLoad.TargetMode);
    }

    [Fact]
    public void ThermalSafety_OverridesEverything()
    {
        // Light workload, cool ambient... no: temp is the override trigger.
        var d = Engine().Evaluate(95f, 10f, false, AsusACPI.PerformanceSilent);
        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
        Assert.Contains("Thermal", d.Reason);
    }

    [Fact]
    public void ThermalSafety_Boundary_AtExactLimit()
    {
        var d = Engine().Evaluate(90f, 10f, false, AsusACPI.PerformanceSilent);
        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
    }

    [Fact]
    public void HotTemperature_SelectsTurbo_WithReason()
    {
        var d = Engine().Evaluate(80f, 30f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
        Assert.Contains("High CPU temperature", d.Reason);
    }

    [Fact]
    public void OnBattery_WithLightWorkload_SelectsSilent()
    {
        var d = Engine().Evaluate(60f, 10f, true, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceSilent, d.TargetMode);
        Assert.Contains("battery", d.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OnBattery_WithModerateWorkload_StaysBalanced()
    {
        var d = Engine().Evaluate(60f, 45f, true, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
    }

    [Fact]
    public void SustainedHeavyLoad_RequiresCooldownStreak()
    {
        var engine = Engine();

        // First two heavy samples: still not enough to upgrade.
        var first = engine.Evaluate(60f, 90f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, first.TargetMode);
        Assert.Contains("warming up", first.Reason);

        var second = engine.Evaluate(60f, 90f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, second.TargetMode);

        // Third consecutive heavy sample upgrades to Performance.
        var third = engine.Evaluate(60f, 90f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceTurbo, third.TargetMode);
        Assert.Contains("Sustained high CPU load", third.Reason);
    }

    [Fact]
    public void LoadBurst_DoesNotUpgrade_ButSustainedDoes()
    {
        var engine = Engine();
        _ = engine.Evaluate(60f, 95f, false, AsusACPI.PerformanceBalanced); // burst sample 1
        _ = engine.Evaluate(60f, 5f, false, AsusACPI.PerformanceBalanced);  // idle resets streak
        var afterIdle = engine.Evaluate(60f, 95f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, afterIdle.TargetMode);   // streak was reset
    }

    [Fact]
    public void ColdTemperature_SelectsSilent()
    {
        var d = Engine().Evaluate(50f, 30f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceSilent, d.TargetMode);
        Assert.Contains("Low CPU temperature", d.Reason);
    }

    [Fact]
    public void ModerateLoad_NormalTemp_StaysBalanced()
    {
        var d = Engine().Evaluate(60f, 45f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
        Assert.Contains("Moderate workload", d.Reason);
    }

    [Fact]
    public void LightWorkload_NormalTemp_ReportsLightReason()
    {
        var d = Engine().Evaluate(60f, 10f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
        Assert.Contains("Light workload", d.Reason);
    }

    [Fact]
    public void Reset_ClearsHeavyStreak()
    {
        var engine = Engine();
        _ = engine.Evaluate(60f, 95f, false, AsusACPI.PerformanceBalanced);
        _ = engine.Evaluate(60f, 95f, false, AsusACPI.PerformanceBalanced);
        engine.Reset();
        var d = engine.Evaluate(60f, 95f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode); // streak was cleared
    }

    [Fact]
    public void CustomConfig_Respected()
    {
        var cfg = DefaultConfig() with { TempHigh = 80, TempLow = 60, CooldownSamples = 2 };
        var engine = new AutoModeEngine(cfg);

        // 78°C is above the default 75 but below the custom 80 → not hot yet.
        Assert.Equal(AsusACPI.PerformanceBalanced, engine.Evaluate(78f, 30f, false, AsusACPI.PerformanceBalanced).TargetMode);

        // Two-sample cooldown instead of three.
        _ = engine.Evaluate(65f, 90f, false, AsusACPI.PerformanceBalanced);
        var d = engine.Evaluate(65f, 90f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
    }

    [Fact]
    public void ClassifyApp_CategorizesCorrectly()
    {
        Assert.Equal(AppCategory.Productivity, AutoModeEngine.ClassifyApp("devenv"));
        Assert.Equal(AppCategory.Productivity, AutoModeEngine.ClassifyApp("code"));
        Assert.Equal(AppCategory.Productivity, AutoModeEngine.ClassifyApp("rider64"));
        Assert.Equal(AppCategory.Productivity, AutoModeEngine.ClassifyApp("msbuild"));

        Assert.Equal(AppCategory.Everyday, AutoModeEngine.ClassifyApp("chrome"));
        Assert.Equal(AppCategory.Everyday, AutoModeEngine.ClassifyApp("msedge"));
        Assert.Equal(AppCategory.Everyday, AutoModeEngine.ClassifyApp("spotify"));
        Assert.Equal(AppCategory.Everyday, AutoModeEngine.ClassifyApp("winword"));

        Assert.Equal(AppCategory.GamingOrHeavy, AutoModeEngine.ClassifyApp("steam"));
        Assert.Equal(AppCategory.GamingOrHeavy, AutoModeEngine.ClassifyApp("blender"));
        Assert.Equal(AppCategory.GamingOrHeavy, AutoModeEngine.ClassifyApp("handbrake"));

        Assert.Equal(AppCategory.General, AutoModeEngine.ClassifyApp("unknown_process"));
        Assert.Equal(AppCategory.General, AutoModeEngine.ClassifyApp(null));
    }

    [Fact]
    public void GamingApp_WithActiveWorkload_SelectsTurboOnPlugged()
    {
        var engine = Engine();
        var d = engine.Evaluate(65f, 50f, onBattery: false, AsusACPI.PerformanceBalanced, appProfileMode: null, appName: "steam");
        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
        Assert.Contains("Gaming / heavy workload", d.Reason);
    }

    [Fact]
    public void ProductivityApp_WithModerateLoad_ShowsProductivityLabel()
    {
        var engine = Engine();
        var d = engine.Evaluate(65f, 40f, onBattery: false, AsusACPI.PerformanceBalanced, appProfileMode: null, appName: "code");
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
        Assert.Contains("Productivity (code)", d.Reason);
    }
}

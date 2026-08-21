using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// AI Auto behaviour profiles (AutoBehaviorPreset): the preset → engine-setting
/// mapping and the apply-to-config path used by the AI Auto page.
/// </summary>
public class AutoBehaviorPresetsTests
{
    [Fact]
    public void Adaptive_IsTheDefault_AndMatchesShippedDefaults()
    {
        var preset = AutoBehaviorPreset.For(AutoBehavior.Adaptive);
        Assert.Equal(75, preset.TempHigh);
        Assert.Equal(55, preset.TempLow);
        Assert.Equal(5, preset.Hysteresis);
        Assert.Equal(3, preset.CooldownSamples);
        Assert.Equal(AutoBehaviorPreset.AdaptiveDefaults, preset);
    }

    [Fact]
    public void Conservative_UpgradesLater_WithMoreHysteresis()
    {
        var preset = AutoBehaviorPreset.For(AutoBehavior.Conservative);
        Assert.True(preset.TempHigh > AutoBehaviorPreset.AdaptiveDefaults.TempHigh,
            "Conservative must upgrade later (higher hot threshold) than Adaptive");
        Assert.True(preset.Hysteresis > AutoBehaviorPreset.AdaptiveDefaults.Hysteresis,
            "Conservative must be more hesitant than Adaptive");
    }

    [Fact]
    public void Aggressive_ReactsFaster_WithLessHysteresis()
    {
        var preset = AutoBehaviorPreset.For(AutoBehavior.Aggressive);
        Assert.True(preset.TempHigh < AutoBehaviorPreset.AdaptiveDefaults.TempHigh,
            "Aggressive must upgrade earlier (lower hot threshold) than Adaptive");
        Assert.True(preset.Hysteresis < AutoBehaviorPreset.AdaptiveDefaults.Hysteresis,
            "Aggressive must be quicker to react than Adaptive");
    }

    [Fact]
    public void Apply_WritesPresetToAppConfig()
    {
        TestEnv.Reset();
        AutoBehaviorPreset.For(AutoBehavior.Conservative).Apply();

        Assert.Equal(82, AppConfig.Get("ai_auto_temp_high"));
        Assert.Equal(58, AppConfig.Get("ai_auto_temp_low"));
        Assert.Equal(6, AppConfig.Get("ai_auto_hysteresis"));
        Assert.Equal(5, AppConfig.Get("ai_auto_cooldown"));
    }

    [Fact]
    public void Apply_OverwritesPreviousPresetValues()
    {
        TestEnv.Reset();
        AutoBehaviorPreset.For(AutoBehavior.Aggressive).Apply();
        AutoBehaviorPreset.For(AutoBehavior.Conservative).Apply();

        Assert.Equal(82, AppConfig.Get("ai_auto_temp_high"));
        Assert.Equal(5, AppConfig.Get("ai_auto_cooldown"));
    }

    [Fact]
    public void Engine_ReadsPresetValuesFromConfig()
    {
        TestEnv.Reset();
        AutoBehaviorPreset.For(AutoBehavior.Conservative).Apply();

        var cfg = AutoModeConfig.FromAppConfig();
        Assert.Equal(82, cfg.TempHigh);
        Assert.Equal(58, cfg.TempLow);
        Assert.Equal(6, cfg.Hysteresis);
        Assert.Equal(5, cfg.CooldownSamples);

        // Conservative: 80°C is NOT yet hot (hot threshold is 82).
        var engine = new AutoModeEngine(cfg);
        var d = engine.Evaluate(80f, 30f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
    }
}

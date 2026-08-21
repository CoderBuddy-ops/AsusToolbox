using Asus.Mode;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Application profiles (AppProfiles): exe-name normalisation, config storage,
/// matching, and the AI Auto engine's profile-override contract (profile wins
/// over workload decisions but never over thermal safety).
/// </summary>
public class AppProfilesTests
{
    [Theory]
    [InlineData("chrome.exe", "chrome")]
    [InlineData("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", "chrome")]
    [InlineData("Devenv.EXE", "devenv")]
    [InlineData("notepad", "notepad")]
    [InlineData("", "")]
    [InlineData("  ", "")]
    public void NormalizeExe_StripsPathCaseAndExtension(string input, string expected)
    {
        Assert.Equal(expected, AppProfiles.NormalizeExe(input));
    }

    [Fact]
    public void SetAndGet_RoundTripsThroughConfig()
    {
        TestEnv.Reset();
        AppProfiles.Set("chrome.exe", AsusACPI.PerformanceBalanced);

        Assert.Equal(AsusACPI.PerformanceBalanced, AppProfiles.GetMode("chrome"));
        Assert.Equal(AsusACPI.PerformanceBalanced, AppProfiles.GetMode("chrome.exe"));
        Assert.Equal(AsusACPI.PerformanceBalanced, AppProfiles.GetMode("CHROME.EXE"));
        Assert.Equal(AsusACPI.PerformanceBalanced, AppProfiles.GetMode("C:\\x\\chrome.exe"));
    }

    [Fact]
    public void GetMode_ReturnsNull_WhenNoProfile()
    {
        TestEnv.Reset();
        Assert.Null(AppProfiles.GetMode("notepad.exe"));
    }

    [Fact]
    public void Remove_DeletesProfile()
    {
        TestEnv.Reset();
        AppProfiles.Set("devenv.exe", AsusACPI.PerformanceTurbo);
        AppProfiles.Remove("devenv");

        Assert.Null(AppProfiles.GetMode("devenv"));
    }

    [Fact]
    public void All_And_Clear_Work()
    {
        TestEnv.Reset();
        AppProfiles.Set("a.exe", AsusACPI.PerformanceSilent);
        AppProfiles.Set("b.exe", AsusACPI.PerformanceTurbo);

        Assert.Equal(2, AppProfiles.All().Count);
        AppProfiles.Clear();
        Assert.Empty(AppProfiles.All());
    }

    [Fact]
    public void Match_IsCaseInsensitive_AndTolerant()
    {
        TestEnv.Reset();
        AppProfiles.Set("VisualStudio.exe", AsusACPI.PerformanceTurbo);

        Assert.Equal(AsusACPI.PerformanceTurbo, AppProfiles.Match("visualstudio.exe"));
        Assert.Equal(AsusACPI.PerformanceTurbo, AppProfiles.Match("VISUALSTUDIO"));
        Assert.Null(AppProfiles.Match(null));
        Assert.Null(AppProfiles.Match(""));
    }

    [Fact]
    public void Engine_ProfileOverridesWorkloadDecision()
    {
        TestEnv.Reset();
        var cfg = new AutoModeConfig
        {
            TempHigh = 75, TempLow = 55, Hysteresis = 5, CooldownSamples = 3,
            ThermalLimit = 90, HeavyLoadPercent = 70, LightLoadPercent = 20,
        };
        var engine = new AutoModeEngine(cfg);

        // Light load, cool temps would normally stay Balanced — but the profile
        // pins the app to Performance.
        var d = engine.Evaluate(60f, 10f, false, AsusACPI.PerformanceBalanced,
            appProfileMode: AsusACPI.PerformanceTurbo, appName: "devenv");

        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
        Assert.Contains("Application profile", d.Reason);
    }

    [Fact]
    public void Engine_ProfileSilent_AppliesOnBatteryOrAC()
    {
        TestEnv.Reset();
        var engine = new AutoModeEngine(new AutoModeConfig
        {
            TempHigh = 75, TempLow = 55, Hysteresis = 5, CooldownSamples = 3,
            ThermalLimit = 90, HeavyLoadPercent = 70, LightLoadPercent = 20,
        });

        var d = engine.Evaluate(60f, 90f, false, AsusACPI.PerformanceBalanced,
            appProfileMode: AsusACPI.PerformanceSilent, appName: "music");

        Assert.Equal(AsusACPI.PerformanceSilent, d.TargetMode);
        Assert.Contains("Application profile", d.Reason);
    }

    [Fact]
    public void Engine_ThermalSafety_AlwaysOverridesProfile()
    {
        TestEnv.Reset();
        var engine = new AutoModeEngine(new AutoModeConfig
        {
            TempHigh = 75, TempLow = 55, Hysteresis = 5, CooldownSamples = 3,
            ThermalLimit = 90, HeavyLoadPercent = 70, LightLoadPercent = 20,
        });

        // Profile pins Silent, but the CPU is at 96°C — thermal safety must win.
        var d = engine.Evaluate(96f, 10f, false, AsusACPI.PerformanceSilent,
            appProfileMode: AsusACPI.PerformanceSilent, appName: "sleeper");

        Assert.Equal(AsusACPI.PerformanceTurbo, d.TargetMode);
        Assert.Contains("Thermal", d.Reason);
    }

    [Fact]
    public void Engine_NoProfile_BehavesAsBefore()
    {
        TestEnv.Reset();
        var cfg = new AutoModeConfig
        {
            TempHigh = 75, TempLow = 55, Hysteresis = 5, CooldownSamples = 3,
            ThermalLimit = 90, HeavyLoadPercent = 70, LightLoadPercent = 20,
        };
        var engine = new AutoModeEngine(cfg);

        var d = engine.Evaluate(60f, 10f, false, AsusACPI.PerformanceBalanced);
        Assert.Equal(AsusACPI.PerformanceBalanced, d.TargetMode);
        Assert.Contains("Light workload", d.Reason);
    }
}

using Asus.Mode;

namespace Asus.Tests;

/// <summary>
/// Resets static application state between tests and points configuration and
/// logging at isolated temp files so no real user data is read or written.
/// </summary>
public static class TestEnv
{
    static readonly string tempDir;

    static TestEnv()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "Asus.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        Logger.appPath = tempDir;
        AppConfig.UseConfigFile(Path.Combine(tempDir, "config.json"));
    }

    /// <summary>Fresh config state + fresh ModeControl + a fresh FakeAcpi wired into Program.</summary>
    public static FakeAcpi Reset()
    {
        AppConfig.UseConfigFile(Path.Combine(tempDir, "config.json"));
        SetBaseline();

        var acpi = new FakeAcpi();
        Program.acpi = acpi;
        Program.settingsForm = null!;
        ModeControl.SettingsOverride = null;
        Program.modeControl = new ModeControl();
        return acpi;
    }

    /// <summary>Defaults that keep ModeControl away from real hardware and real Windows power changes.</summary>
    static void SetBaseline()
    {
        AppConfig.Set("performance_mode", 0);     // Balanced
        AppConfig.Set("skip_powermode", 1);       // never touch the real Windows power scheme
        AppConfig.Set("status_mode", 0);
        AppConfig.Set("manual_mode", 0);
        AppConfig.Set("auto_boost_0", -1);        // skip CPU boost override
        AppConfig.Set("auto_apply_0", 0);         // no custom fan curves by default
        AppConfig.Set("auto_apply_power_0", 0);   // no custom power limits by default
    }

    /// <summary>A 16-point fan curve in the same "XX-XX-..." hex format AppConfig stores.</summary>
    public static string FanCurveString()
    {
        var bytes = Enumerable.Range(0, 16).Select(i => (byte)Math.Min(10 * (i + 1), 100));
        return BitConverter.ToString(bytes.ToArray());
    }

    /// <summary>Set per-mode fan profiles for the current mode (0 = Balanced).</summary>
    public static void SetFanProfiles(FakeAcpi acpi, bool mid = false)
    {
        AppConfig.Set("fan_profile_cpu_0", FanCurveString());
        AppConfig.Set("fan_profile_gpu_0", FanCurveString());
        if (mid) AppConfig.Set("fan_profile_mid_0", FanCurveString());
    }
}

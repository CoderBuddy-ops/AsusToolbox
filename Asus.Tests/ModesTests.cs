using Asus.Mode;
using Xunit;

namespace Asus.Tests;

public class ModesTests
{
    readonly FakeAcpi acpi;

    public ModesTests()
    {
        acpi = TestEnv.Reset();
    }

    [Fact]
    public void DefaultModes_MapToBalancedTurboSilent()
    {
        Assert.Equal(AsusACPI.PerformanceBalanced, Modes.GetBase(0));
        Assert.Equal(AsusACPI.PerformanceTurbo, Modes.GetBase(1));
        Assert.Equal(AsusACPI.PerformanceSilent, Modes.GetBase(2));
        Assert.True(Modes.Exists(0));
        Assert.True(Modes.Exists(1));
        Assert.True(Modes.Exists(2));
        Assert.False(Modes.Exists(3));
        Assert.Equal("Balanced", Modes.GetName(0));
        Assert.Equal("Turbo", Modes.GetName(1));
        Assert.Equal("Silent", Modes.GetName(2));
    }

    [Fact]
    public void GetDictonary_ReturnsBuiltInModesFirst()
    {
        var modes = Modes.GetDictonary();
        Assert.Equal(new[] { 2, 0, 1 }, modes.Keys.ToArray());
        Assert.Equal("Silent", modes[2]);
        Assert.Equal("Balanced", modes[0]);
        Assert.Equal("Turbo", modes[1]);
    }

    [Fact]
    public void GetCurrentBase_ReflectsCurrentMode()
    {
        AppConfig.Set("performance_mode", 2); // Silent
        Assert.Equal(AsusACPI.PerformanceSilent, Modes.GetCurrentBase());
        Assert.Equal("Silent", Modes.GetCurrentName());
    }

    [Fact]
    public void Add_CreatesCustomModeAndCopiesPerModeSettings()
    {
        AppConfig.Set("limit_total_0", 80);
        AppConfig.Set("fan_profile_cpu_0", TestEnv.FanCurveString());

        int custom = Modes.Add();

        Assert.Equal(3, custom);
        Assert.True(Modes.Exists(3));
        Assert.Equal(0, Modes.GetBase(3)); // inherits current base (Balanced)
        Assert.Equal("Custom 1", Modes.GetName(3));
        Assert.Equal(80, AppConfig.Get("limit_total_3"));
        Assert.Equal(TestEnv.FanCurveString(), AppConfig.GetString("fan_profile_cpu_3"));
    }

    [Fact]
    public void Add_ReturnsMinusOne_WhenAllCustomSlotsUsed()
    {
        for (int i = 3; i < 20; i++)
            AppConfig.Set("mode_base_" + i, 0);

        Assert.Equal(-1, Modes.Add());
    }

    [Fact]
    public void Remove_DeletesCustomModeKeys()
    {
        Modes.Add();
        Assert.True(Modes.Exists(3));

        Modes.Remove(3);

        Assert.False(Modes.Exists(3));
        Assert.Null(AppConfig.GetString("mode_name_3"));
    }

    [Fact]
    public void GetNext_CyclesThroughModes()
    {
        AppConfig.Set("performance_mode", 0); // Balanced
        Assert.Equal(1, Modes.GetNext());     // -> Turbo
        AppConfig.Set("performance_mode", 1);
        Assert.Equal(2, Modes.GetNext());     // -> Silent
        AppConfig.Set("performance_mode", 2);
        Assert.Equal(0, Modes.GetNext());     // -> wraps to Balanced
        AppConfig.Set("performance_mode", 0);
        Assert.Equal(2, Modes.GetNext(back: true)); // -> wraps back to Silent
    }

    [Fact]
    public void InitFullSpeed_CreatesFullSpeedMode_WhenVivoBookModeHasFlag()
    {
        acpi.DeviceValues[AsusACPI.VivoBookMode] = 0x40000;

        Modes.InitFullSpeed();

        Assert.True(Modes.Exists(3));
        Assert.Equal(AsusACPI.PerformanceFullSpeed, Modes.GetBase(3));
        Assert.Equal("Full Speed", Modes.GetName(3));
    }

    [Fact]
    public void InitFullSpeed_DoesNothing_WhenUnsupported()
    {
        Modes.InitFullSpeed(); // fake returns -1
        Assert.False(Modes.Exists(3));
    }

    [Fact]
    public void InitFullSpeed_DoesNothing_WhenFlagMissing()
    {
        acpi.DeviceValues[AsusACPI.VivoBookMode] = 0x10000;

        Modes.InitFullSpeed();

        Assert.False(Modes.Exists(3));
    }

    [Fact]
    public void InitFullSpeed_DoesNotDuplicate_WhenFullSpeedModeAlreadyExists()
    {
        acpi.DeviceValues[AsusACPI.VivoBookMode] = 0x40000;

        Modes.InitFullSpeed();
        Modes.InitFullSpeed();

        Assert.True(Modes.Exists(3));
        Assert.False(Modes.Exists(4));
    }
}

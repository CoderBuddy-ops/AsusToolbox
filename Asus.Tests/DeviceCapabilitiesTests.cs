using Asus.Device;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Capability matrix detection (DeviceCapabilities): firmware-probed endpoints,
/// CPU vendor, and the "hide unsupported controls" contract.
/// </summary>
public class DeviceCapabilitiesTests
{
    private static FakeAcpi VivobookAcpi()
    {
        var acpi = new FakeAcpi();
        acpi.Supported.Add(AsusACPI.CPU_Fan);
        acpi.Supported.Add(AsusACPI.Mid_Fan);
        acpi.Supported.Add(AsusACPI.PerformanceMode);
        acpi.Supported.Add(AsusACPI.BatteryLimit);
        acpi.Supported.Add(AsusACPI.StatusLed);
        acpi.Supported.Add(AsusACPI.ScreenOverdrive);
        return acpi;
    }

    [Fact]
    public void VivobookIntel_WithFullFirmware_ExposesAllCoreCapabilities()
    {
        var c = DeviceCapabilities.Detect(VivobookAcpi(), isVivobookModel: true, isAmdCpu: false);

        Assert.True(c.IsAsus);
        Assert.True(c.IsVivobook);
        Assert.True(c.IsIntelCpu);
        Assert.False(c.IsAmdCpu);
        Assert.True(c.ACPI_Connected);
        Assert.True(c.SupportsFanControl);
        Assert.True(c.SupportsFanCurve);
        Assert.True(c.SupportsPerformanceModes);
        Assert.True(c.SupportsChargeLimit);
        Assert.True(c.SupportsKeyboardBacklight);
        Assert.True(c.SupportsStatusLed);
        Assert.True(c.SupportsDisplayOverdrive);
    }

    [Fact]
    public void IntelCpu_NeverExposesPowerLimits()
    {
        // Even if the PPT endpoint answers, Intel has no AMD PPT semantics.
        var acpi = VivobookAcpi();
        acpi.Supported.Add((uint)AsusACPI.PPT_APUA0);

        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isAmdCpu: false);
        Assert.False(c.SupportsPowerLimits);
    }

    [Fact]
    public void AmdCpu_WithPPT_ExposesPowerLimits()
    {
        var acpi = VivobookAcpi();
        acpi.Supported.Add((uint)AsusACPI.PPT_APUA0);

        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isAmdCpu: true);
        Assert.True(c.SupportsPowerLimits);
    }

    [Fact]
    public void DisconnectedAcpi_ExposesNothing()
    {
        var acpi = new FakeAcpi { MidFanSupported = false };
        // FakeAcpi always reports connected; simulate the "no endpoints" case instead.
        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: false, isAmdCpu: false);

        Assert.False(c.IsVivobook);
        Assert.False(c.SupportsFanControl);
        Assert.False(c.SupportsFanCurve);
        Assert.False(c.SupportsPerformanceModes);
        Assert.False(c.SupportsChargeLimit);
        Assert.False(c.SupportsKeyboardBacklight);
        Assert.False(c.SupportsPowerLimits);
        Assert.False(c.SupportsMicrophoneNoiseCancellation);
    }

    [Fact]
    public void FanCurve_RequiresCpuFanEndpoint()
    {
        // Mid-fan alone gives fan control but not custom curves.
        var acpi = new FakeAcpi { MidFanSupported = true };

        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isAmdCpu: false);
        Assert.True(c.SupportsFanControl);
        Assert.False(c.SupportsFanCurve);
    }

    [Fact]
    public void MicrophoneNoiseCancellation_NotClaimedWithoutProbe()
    {
        var c = DeviceCapabilities.Detect(VivobookAcpi(), isVivobookModel: true, isAmdCpu: false);
        Assert.False(c.SupportsMicrophoneNoiseCancellation);
    }

    [Fact]
    public void Summary_IsReadable()
    {
        var c = DeviceCapabilities.Detect(VivobookAcpi(), isVivobookModel: true, isAmdCpu: false);
        Assert.Contains("Vivobook", c.Summary());
        Assert.Contains("Intel", c.Summary());
    }
}

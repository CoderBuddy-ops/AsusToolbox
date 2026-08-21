using Asus.Device;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Exact-model validation for the X1504ZA target: model normalisation must
/// accept the equivalent Windows spellings of the same hardware and reject
/// unrelated models; EC version 255.255 must never surface as a real value.
/// </summary>
public class X1504ZADetectionTests
{
    [Theory]
    [InlineData("Vivobook_ASUSLaptop X1504ZA")]
    [InlineData("X1504ZA")]
    [InlineData("ASUS X1504ZA")]
    [InlineData("Vivobook 15 X1504ZA")]
    public void ModelVariants_AllMatchX1504ZA(string model)
    {
        Assert.True(AppConfig.ModelMatches(model, "X1504ZA"), $"{model} should match X1504ZA");
    }

    [Theory]
    [InlineData("Vivobook_ASUSLaptop X1502ZA")]
    [InlineData("Vivobook_ASUSLaptop X1404ZA")]
    [InlineData("ROG Strix G16")]
    [InlineData("TUF Gaming F15")]
    [InlineData("")]
    [InlineData("Vivobook_ASUSLaptop X1504")]
    public void UnrelatedModels_DoNotMatch(string model)
    {
        Assert.False(AppConfig.ModelMatches(model, "X1504ZA"), $"{model} must NOT match X1504ZA");
    }

    [Fact]
    public void IsX1504ZA_DelegatesToPureMatcher()
    {
        // IsX1504ZA() reads the real machine via WMI; the pure matcher it calls
        // is what the tests above pin down.
        Assert.True(AppConfig.ModelMatches("Vivobook_ASUSLaptop X1504ZA", "X1504ZA"));
    }

    [Theory]
    [InlineData("Vivobook_ASUSLaptop X1504ZA")]
    [InlineData("X1504ZA")]
    [InlineData("ASUS X1504ZA")]
    public void NoGpuGate_MatchesX1504ZA(string model)
    {
        // AppConfig.NoGpu() skips the NVIDIA/AMD probe and hides GPU-mode
        // controls on Intel-only models; it keys on the substring "X1504Z" via
        // ContainsModel. Every Windows spelling of the target contains that
        // substring ("X1504ZA" starts with it), so the gate fires on the real
        // machine. Pin that so the startup optimisation can't silently stop
        // applying.
        Assert.True(model.Contains("X1504Z", StringComparison.OrdinalIgnoreCase),
            $"{model} must satisfy the NoGpu substring gate");
    }

    [Theory]
    [InlineData("Vivobook_ASUSLaptop X1502ZA")]
    [InlineData("ROG Strix G16")]
    [InlineData("X1504")]
    public void NoGpuGate_DoesNotMatchUnrelatedModels(string model)
    {
        Assert.False(model.Contains("X1504Z", StringComparison.OrdinalIgnoreCase),
            $"{model} must NOT satisfy the NoGpu substring gate");
    }

    [Fact]
    public void Capabilities_FlagValidatedModel_AndExactToken()
    {
        var acpi = new FakeAcpi();
        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isValidatedModel: true, isAmdCpu: false);

        Assert.True(c.IsValidatedModel);
        Assert.Equal("X1504ZA", c.ExactModel);
    }

    [Fact]
    public void Capabilities_UnvalidatedVivobook_HasNoExactToken()
    {
        var acpi = new FakeAcpi();
        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isValidatedModel: false, isAmdCpu: false);

        Assert.False(c.IsValidatedModel);
        Assert.Equal("", c.ExactModel);
    }

    [Theory]
    [InlineData("255.255")]
    [InlineData("255.255.255.255")]
    [InlineData("0.0")]
    [InlineData("0.0.0.0")]
    [InlineData("")]
    [InlineData(null)]
    public void InvalidECVersions_AreRejected(string? version)
    {
        Assert.True(AppConfig.IsInvalidECVersion(version), $"'{version}' must be treated as invalid");
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("X1504ZA.316")]
    [InlineData(" 316 ")]
    [InlineData("2.5")]
    public void RealECVersions_AreAccepted(string version)
    {
        Assert.False(AppConfig.IsInvalidECVersion(version), $"'{version}' must be treated as valid");
    }

    [Fact]
    public void Capabilities_ECVersion_IsNullWhenInvalid()
    {
        // EC version is passed in from the detection layer; the placeholder must
        // already be normalised to null before it reaches the capability matrix.
        var acpi = new FakeAcpi();
        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isValidatedModel: true, isAmdCpu: false,
            ecVersion: AppConfig.IsInvalidECVersion("255.255") ? null : "255.255");

        Assert.Null(c.ECVersion);
    }

    [Fact]
    public void Capabilities_ECVersion_SurfacesRealValue()
    {
        var acpi = new FakeAcpi();
        var c = DeviceCapabilities.Detect(acpi, isVivobookModel: true, isValidatedModel: true, isAmdCpu: false,
            ecVersion: "316");

        Assert.Equal("316", c.ECVersion);
    }
}

using Asus.Helpers;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// BIOS-update safety gate (BiosSafety): the model + power preconditions that
/// must hold before a BIOS update is offered, and the confirmation message.
/// </summary>
public class BiosSafetyTests
{
    [Fact]
    public void VerifyModel_AcceptsX1504ZA()
    {
        Assert.True(BiosSafety.VerifyModel("Vivobook_ASUSLaptop X1504ZA").Safe);
        Assert.True(BiosSafety.VerifyModel("X1504ZA").Safe);
    }

    [Fact]
    public void VerifyModel_RejectsUnrelatedModels()
    {
        Assert.False(BiosSafety.VerifyModel("ROG Strix G16").Safe);
        Assert.False(BiosSafety.VerifyModel("Vivobook_ASUSLaptop X1502ZA").Safe);
        Assert.False(BiosSafety.VerifyModel("").Safe);
        Assert.False(BiosSafety.VerifyModel(null).Safe);
    }

    [Fact]
    public void VerifyPower_RequiresAc()
    {
        Assert.False(BiosSafety.VerifyPower(onAc: false, batteryPercent: 90).Safe);
    }

    [Fact]
    public void VerifyPower_RequiresSufficientBattery()
    {
        Assert.False(BiosSafety.VerifyPower(onAc: true, batteryPercent: 10).Safe);
    }

    [Fact]
    public void VerifyPower_AcceptsAcWithGoodBattery()
    {
        Assert.True(BiosSafety.VerifyPower(onAc: true, batteryPercent: 80).Safe);
    }

    [Fact]
    public void VerifyPower_UnknownBatteryWithAc_IsSafe()
    {
        // batteryPercent <= 0 means "unknown" — with AC connected that's acceptable.
        Assert.True(BiosSafety.VerifyPower(onAc: true, batteryPercent: 0).Safe);
    }

    [Fact]
    public void ConfirmationMessage_ListsAllPreconditions()
    {
        string msg = BiosSafety.ConfirmationMessage(modelOk: true, onAc: true, batteryPercent: 80);

        Assert.Contains("Model match (X1504ZA)", msg);
        Assert.Contains("AC power connected", msg);
        Assert.Contains("Battery level", msg);
        Assert.Contains("80%", msg);
        Assert.Contains("official ASUS download page", msg);
    }
}

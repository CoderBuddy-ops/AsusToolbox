using Asus.USB;
using System.Management;

namespace Asus.Tests;

/// <summary>
/// In-memory fake for the ACPI device. Records every call and lets tests
/// control the responses (device values, supported endpoints, fan results).
/// </summary>
public sealed class FakeAcpi : IAsusACPI
{
    public readonly List<string> Calls = new();
    public readonly Dictionary<uint, int> DeviceValues = new();
    public readonly HashSet<uint> Supported = new();
    public readonly Dictionary<AsusFan, byte[]> FanCurveCalls = new();
    public readonly Dictionary<AsusFan, byte[]> FanRangeCalls = new();
    public readonly List<(uint Id, byte[] Data, string? Log)> ByteSetCalls = new();
    public readonly List<(int Up, int Down)> HysteresisCalls = new();
    public readonly Dictionary<AsusFan, int> FanValues = new();

    public int SetFanCurveResult = 1;
    public int SetFanRangeResult = 1;
    public int? PerformanceModeSetTo;
    public bool AllAmdPpt;
    public bool XgConnected;
    public int VivoBookMode = -1;
    public bool MidFanSupported = true;

    void Record(string method, params object?[] args) =>
        Calls.Add(method + "(" + string.Join(", ", args) + ")");

    public void RunListener() => throw new NotImplementedException();

    public bool IsConnected() => true;

    public byte[] DeviceInit() => Array.Empty<byte>();

    public byte[] DeviceWatchDog() => Array.Empty<byte>();

    public int DeviceSet(uint DeviceID, int Status, string? logName)
    {
        Record(nameof(DeviceSet), $"0x{DeviceID:X}", Status, logName);
        DeviceValues[DeviceID] = Status;
        return 1;
    }

    public int DeviceSet(uint DeviceID, byte[] Params, string? logName)
    {
        Record(nameof(DeviceSet), $"0x{DeviceID:X}", "bytes", logName);
        ByteSetCalls.Add((DeviceID, Params, logName));
        return 1;
    }

    public int DeviceGet(uint DeviceID)
    {
        Record(nameof(DeviceGet), $"0x{DeviceID:X}");
        return DeviceValues.TryGetValue(DeviceID, out var value) ? value : -1;
    }

    public byte[] DeviceGetBuffer(uint DeviceID, uint Status = 0) => Array.Empty<byte>();

    public int SetVivoMode(int mode)
    {
        Record(nameof(SetVivoMode), mode);
        return 1;
    }

    public int SetPerformanceMode(int mode, string log = "Mode")
    {
        PerformanceModeSetTo = mode;
        Record(nameof(SetPerformanceMode), mode, log);
        return 1;
    }

    public int SetGPUEco(int eco)
    {
        Record(nameof(SetGPUEco), eco);
        return 1;
    }

    public int GetFan(AsusFan device)
    {
        Record(nameof(GetFan), device);
        return FanValues.TryGetValue(device, out var value) ? value : 0;
    }

    public bool IsMidFanSupported() => MidFanSupported;

    public int SetFanRange(AsusFan device, byte[] curve)
    {
        FanRangeCalls[device] = curve;
        Record(nameof(SetFanRange), device, curve.Length);
        return SetFanRangeResult;
    }

    public int SetFanCurve(AsusFan device, byte[] curve)
    {
        FanCurveCalls[device] = curve;
        Record(nameof(SetFanCurve), device, curve.Length);
        return SetFanCurveResult;
    }

    public byte[] GetFanCurve(AsusFan device, int mode = 0) => Array.Empty<byte>();

    public (int up, int down) GetFanHysteresis() => (60, 20);

    public int SetFanHysteresis(int up, int down)
    {
        HysteresisCalls.Add((up, down));
        Record(nameof(SetFanHysteresis), up, down);
        return 1;
    }

    public bool IsXGConnected() => XgConnected;

    public bool IsAllAmdPPT() => AllAmdPpt;

    public bool IsOverdriveSupported() => false;

    public bool IsSupported(uint DeviceID)
    {
        Record(nameof(IsSupported), $"0x{DeviceID:X}");
        return Supported.Contains(DeviceID);
    }

    public bool IsNVidiaGPU() => false;

    public void SetAPUMem(int memory = 4) { }

    public int GetAPUMem() => -1;

    public int[] GetVramOptions(out int unitMb)
    {
        unitMb = 0;
        return Array.Empty<int>();
    }

    public int GetVramMem() => -1;

    public void SetVramMem(int value) { }

    public (int, int) GetCores(uint device = AsusACPI.CORES_CPU) => (0, 0);

    public void SetCores(int eCores, int pCores) { }

    public string ScanRange() => "";

    public void TUFKeyboardBrightness(int brightness, string log = "TUF Backlight") { }

    public void TUFKeyboardRGB(AuraMode mode, Color color, int speed, string? log = "TUF RGB") { }

    public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false) { }

    public void SubscribeToEvents(Action<object, EventArrivedEventArgs> EventHandler) { }

    public decimal? GetBatteryDischarge() => null;
}

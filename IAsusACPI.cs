using System;
using System.Drawing;
using System.Management;
using Asus.USB;

namespace Asus;

public interface IAsusACPI
{
	void RunListener();

	bool IsConnected();

	byte[] DeviceInit();

	byte[] DeviceWatchDog();

	int DeviceSet(uint DeviceID, int Status, string? logName);

	int DeviceSet(uint DeviceID, byte[] Params, string? logName);

	int DeviceGet(uint DeviceID);

	byte[] DeviceGetBuffer(uint DeviceID, uint Status = 0u);

	int SetVivoMode(int mode);

	int SetPerformanceMode(int mode, string log = "Mode");

	int SetGPUEco(int eco);

	int GetFan(AsusFan device);

	bool IsMidFanSupported();

	int SetFanRange(AsusFan device, byte[] curve);

	int SetFanCurve(AsusFan device, byte[] curve);

	byte[] GetFanCurve(AsusFan device, int mode = 0);

	(int up, int down) GetFanHysteresis();

	int SetFanHysteresis(int up, int down);

	bool IsXGConnected();

	bool IsAllAmdPPT();

	bool IsOverdriveSupported();

	bool IsSupported(uint DeviceID);

	bool IsNVidiaGPU();

	void SetAPUMem(int memory = 4);

	int GetAPUMem();

	int[] GetVramOptions(out int unitMb);

	int GetVramMem();

	void SetVramMem(int value);

	(int, int) GetCores(uint device = 1179858u);

	void SetCores(int eCores, int pCores);

	string ScanRange();

	void TUFKeyboardBrightness(int brightness, string log = "TUF Backlight");

	void TUFKeyboardRGB(AuraMode mode, Color color, int speed, string? log = "TUF RGB");

	void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false);

	void SubscribeToEvents(Action<object, EventArrivedEventArgs> EventHandler);

	decimal? GetBatteryDischarge();
}

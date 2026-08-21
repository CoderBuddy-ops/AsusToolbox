using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Asus;
using Asus.USB;

public class AsusACPI : IAsusACPI
{
	private const string FILE_NAME = "\\\\.\\\\ATKACPI";

	private const uint CONTROL_CODE = 2237452u;

	private const uint DSTS = 1398035268u;

	private const uint DEVS = 1398162756u;

	private const uint INIT = 1414090313u;

	private const uint WDOG = 1196377175u;

	public const uint UniversalControl = 1048609u;

	public const int Airplane = 136;

	public const int KB_Light_Up = 196;

	public const int KB_Light_Down = 197;

	public const int Brightness_Down = 16;

	public const int Brightness_Up = 32;

	public const int KB_Sleep = 108;

	public const int KB_TouchpadToggle = 107;

	public const int KB_MuteToggle = 124;

	public const int KB_FNlockToggle = 78;

	public const int KB_DUO_PgUpDn = 75;

	public const int KB_DUO_SecondDisplay = 106;

	public const int Touchpad_Toggle = 107;

	public const int ChargerMode = 1179756;

	public const int ChargerUSB = 2;

	public const int ChargerBarrel = 1;

	public const uint CPU_Fan = 1114131u;

	public const uint GPU_Fan = 1114132u;

	public const uint Mid_Fan = 1114161u;

	public const uint BatteryDischarge = 1179738u;

	public const uint StatusMode = 589873u;

	public const uint PowerSavingMode = 589874u;

	public const uint PerformanceMode = 1179765u;

	public const uint VivoBookMode = 1114137u;

	public const uint GPUEcoROG = 589856u;

	public const uint GPUEcoVivo = 590112u;

	public const uint GPUXGConnected = 589848u;

	public const uint GPUXG = 589849u;

	public const uint GPUMuxROG = 589846u;

	public const uint GPUMuxVivo = 589862u;

	public const uint BatteryLimit = 1179735u;

	public const uint ScreenOverdrive = 327705u;

	public const uint ScreenMiniled1 = 327710u;

	public const uint ScreenMiniled2 = 327726u;

	public const uint ScreenFHD = 327708u;

	public const uint ScreenHDRControl = 327793u;

	public const uint ScreenOptimalBrightness = 327722u;

	public const uint ScreenInit = 327697u;

	public const uint DevsCPUFan = 1114146u;

	public const uint DevsGPUFan = 1114147u;

	public const uint DevsCPUFanCurve = 1114148u;

	public const uint DevsGPUFanCurve = 1114149u;

	public const uint DevsMidFanCurve = 1114162u;

	public const uint FanHysteresis = 1114164u;

	public const int Temp_CPU = 1179796;

	public const int Temp_GPU = 1179799;

	public const int PPT_APUA0 = 1179808;

	public const int PPT_EDCA1 = 1179809;

	public const int PPT_TDCA2 = 1179810;

	public const int PPT_APUA3 = 1179811;

	public const int PPT_CPUB0 = 1179824;

	public const int PPT_CPUB1 = 1179825;

	public const int PPT_GPUC0 = 1179840;

	public const int PPT_APUC1 = 1179841;

	public const int PPT_GPUC2 = 1179842;

	public const uint CORES_CPU = 1179858u;

	public const uint CORES_MAX = 1179859u;

	public const uint CORES_MIN = 1179860u;

	public const uint GPU_BASE = 1179801u;

	public const uint GPU_POWER = 1179800u;

	public const int APU_MEM = 393409;

	public const int VRAM_MEM = 393412;

	public const int TUF_KB_BRIGHTNESS = 327713;

	public const int KBD_BACKLIGHT_OOBE = 327727;

	public const int TUF_KB = 1048662;

	public const int TUF_KB2 = 1048666;

	public const int TUF_KB_STATE = 1048663;

	public const int MicMuteLed = 262167;

	public const int SoundMuteLed = 262172;

	public const int SlateMode = 1179747;

	public const int TabletState = 393335;

	public const int TentState = 393314;

	public const int FnLock = 1048611;

	public const int ScreenPadToggle = 327729;

	public const int ScreenPadBrightness = 327730;

	public const int CameraShutter = 393336;

	public const int CameraLed = 393337;

	public const int StatusLed = 393410;

	public const int BootSound = 1245218;

	public const int Tablet_Notebook = 0;

	public const int Tablet_Tablet = 1;

	public const int Tablet_Tent = 2;

	public const int Tablet_Rotated = 3;

	public const int PerformanceBalanced = 0;

	public const int PerformanceTurbo = 1;

	public const int PerformanceSilent = 2;

	public const int PerformanceFullSpeed = 3;

	public const int PerformanceManual = 4;

	public const int GPUModeEco = 0;

	public const int GPUModeStandard = 1;

	public const int GPUModeUltimate = 2;

	public const int MinTotal = 5;

	public static int MaxTotal = 150;

	public static int DefaultTotal = 80;

	public const int MinCPU = 5;

	public static int MaxCPU = 100;

	public const int DefaultCPU = 80;

	public const int MinGPUBoost = 5;

	public static int MaxGPUBoost = 25;

	public static int MinGPUPower = 0;

	public static int MaxGPUPower = 70;

	public const int MinGPUTemp = 75;

	public const int MaxGPUTemp = 87;

	public const int PCoreMin = 4;

	public const int ECoreMin = 0;

	public const int PCoreMax = 16;

	public const int ECoreMax = 16;

	private bool? _allAMD;

	private readonly Dictionary<uint, bool> _supportCache = new Dictionary<uint, bool>();

	private const uint GENERIC_READ = 2147483648u;

	private const uint GENERIC_WRITE = 1073741824u;

	private const uint OPEN_EXISTING = 3u;

	private const uint FILE_ATTRIBUTE_NORMAL = 128u;

	private const uint FILE_SHARE_READ = 1u;

	private const uint FILE_SHARE_WRITE = 2u;

	private nint handle;

	private nint eventHandle;

	private bool _connected;

	private static readonly int[] apuMemEnum = new int[9] { 0, 2, 3, 4, 5, 7, 8, 9, 6 };

	private const int ASUS_WMI_KEYBOARD_POWER_BOOT = 196608;

	private const int ASUS_WMI_KEYBOARD_POWER_AWAKE = 786432;

	private const int ASUS_WMI_KEYBOARD_POWER_SLEEP = 3145728;

	private const int ASUS_WMI_KEYBOARD_POWER_SHUTDOWN = 12582912;

	private ManagementEventWatcher? watcher;

	public static uint GPUEco
	{
		get
		{
			if (!AppConfig.IsVivoZenPro())
			{
				return 589856u;
			}
			return 590112u;
		}
	}

	public static uint GPUMux
	{
		get
		{
			if (!AppConfig.IsVivoZenPro())
			{
				return 589846u;
			}
			return 589862u;
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, nint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, nint hTemplateFile);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DeviceIoControl(nint hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, nint lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(nint hObject);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint CreateEvent(nint lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool WaitForSingleObject(nint hHandle, int dwMilliseconds);

	public void RunListener()
	{
		eventHandle = CreateEvent(IntPtr.Zero, bManualReset: false, bInitialState: false, "ATK4001");
		byte[] array = new byte[16];
		byte[] array2 = new byte[8]
		{
			BitConverter.GetBytes(((IntPtr)eventHandle).ToInt32())[0],
			BitConverter.GetBytes(((IntPtr)eventHandle).ToInt32())[1],
			0,
			0,
			0,
			0,
			0,
			0
		};
		Control(2237440u, array2, array);
		Logger.WriteLine("ACPI :" + BitConverter.ToString(array2) + "|" + BitConverter.ToString(array));
		while (true)
		{
			WaitForSingleObject(eventHandle, -1);
			Control(2237448u, new byte[0], array);
			Logger.WriteLine("ACPI Code: " + BitConverter.ToInt32(array));
		}
	}

	public bool IsConnected()
	{
		return _connected;
	}

	public AsusACPI()
	{
		try
		{
			handle = CreateFile("\\\\.\\\\ATKACPI", 3221225472u, 3u, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
			_connected = handle != new IntPtr(-1);
		}
		catch (Exception ex)
		{
			Logger.WriteLine("Can't connect to ACPI: " + ex.Message);
		}
		if (AppConfig.IsAdvantageEdition())
		{
			MaxTotal = 250;
		}
		if (AppConfig.IsG14AMD())
		{
			DefaultTotal = 125;
		}
		if (AppConfig.IsX13())
		{
			MaxTotal = 75;
			DefaultTotal = 50;
		}
		if (AppConfig.IsIntelHX())
		{
			MaxTotal = 175;
		}
		if (AppConfig.DynamicBoost5())
		{
			MaxGPUBoost = 5;
		}
		if (AppConfig.DynamicBoost20())
		{
			MaxGPUBoost = 20;
		}
		if (AppConfig.DynamicBoost15())
		{
			MaxGPUBoost = 15;
		}
		if (AppConfig.IsCPULight())
		{
			MaxTotal = 90;
		}
		if (AppConfig.IsOnlyAIMAX())
		{
			MaxTotal = 115;
			MaxCPU = 115;
		}
	}

	public void Control(uint dwIoControlCode, byte[] lpInBuffer, byte[] lpOutBuffer)
	{
		uint lpBytesReturned = 0u;
		DeviceIoControl(handle, dwIoControlCode, lpInBuffer, (uint)lpInBuffer.Length, lpOutBuffer, (uint)lpOutBuffer.Length, ref lpBytesReturned, IntPtr.Zero);
	}

	public void Close()
	{
		CloseHandle(handle);
	}

	protected byte[] CallMethod(uint MethodID, byte[] args)
	{
		byte[] array = new byte[8 + args.Length];
		byte[] array2 = new byte[16];
		BitConverter.GetBytes(MethodID).CopyTo(array, 0);
		BitConverter.GetBytes((uint)args.Length).CopyTo(array, 4);
		Array.Copy(args, 0, array, 8, args.Length);
		Control(2237452u, array, array2);
		return array2;
	}

	public byte[] DeviceInit()
	{
		byte[] args = new byte[8];
		return CallMethod(1414090313u, args);
	}

	public byte[] DeviceWatchDog()
	{
		byte[] args = new byte[8];
		return CallMethod(1196377175u, args);
	}

	public int DeviceSet(uint DeviceID, int Status, string? logName)
	{
		byte[] array = new byte[8];
		BitConverter.GetBytes(DeviceID).CopyTo(array, 0);
		BitConverter.GetBytes((uint)Status).CopyTo(array, 4);
		int num = BitConverter.ToInt32(CallMethod(1398162756u, array), 0);
		if (logName != null)
		{
			Logger.WriteLine(logName + " = " + Status + " : " + ((num == 1) ? "OK" : ((object)num)));
		}
		return num;
	}

	public int DeviceSet(uint DeviceID, byte[] Params, string? logName)
	{
		byte[] array = new byte[4 + Params.Length];
		BitConverter.GetBytes(DeviceID).CopyTo(array, 0);
		Params.CopyTo(array, 4);
		byte[] value = CallMethod(1398162756u, array);
		int num = BitConverter.ToInt32(value, 0);
		if (logName != null)
		{
			Logger.WriteLine(logName + " = " + BitConverter.ToString(Params) + " : " + ((num == 1) ? "OK" : ((object)num)));
		}
		return BitConverter.ToInt32(value, 0);
	}

	public static void DeviceSetWmi(uint DeviceID, int Status)
	{
		try
		{
			using ManagementObject managementObject = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM AsusAtkWmi_WMNB").Get().Cast<ManagementObject>().First();
			ManagementBaseObject methodParameters = managementObject.GetMethodParameters("DEVS");
			methodParameters["Device_ID"] = DeviceID;
			methodParameters["Control_status"] = (uint)Status;
			int num = Convert.ToInt32(managementObject.InvokeMethod("DEVS", methodParameters, null)["result"]);
			Logger.WriteLine("WMI DEVS = " + Status + " : " + ((num == 1) ? "OK" : ((object)num)));
		}
		catch (Exception ex)
		{
			Logger.WriteLine("WMI DEVS: " + ex.Message);
		}
	}

	public int DeviceGet(uint DeviceID)
	{
		byte[] array = new byte[8];
		BitConverter.GetBytes(DeviceID).CopyTo(array, 0);
		return BitConverter.ToInt32(CallMethod(1398035268u, array), 0) - 65536;
	}

	public byte[] DeviceGetBuffer(uint DeviceID, uint Status = 0u)
	{
		byte[] array = new byte[8];
		BitConverter.GetBytes(DeviceID).CopyTo(array, 0);
		BitConverter.GetBytes(Status).CopyTo(array, 4);
		return CallMethod(1398035268u, array);
	}

	public decimal? GetBatteryDischarge()
	{
		byte[] array = DeviceGetBuffer(1179738u);
		if (array[2] > 0)
		{
			array[2] = 0;
			return (decimal)BitConverter.ToInt16(array, 0) / 100m;
		}
		return null;
	}

	public int SetVivoMode(int mode)
	{
		switch (mode)
		{
		case 1:
			mode = 2;
			break;
		case 2:
			mode = 1;
			break;
		}
		return Program.acpi.DeviceSet(1114137u, mode, "VivoMode");
	}

	public int SetPerformanceMode(int mode, string log = "Mode")
	{
		if (IsSupported(1179765u))
		{
			return DeviceSet(1179765u, mode, log);
		}
		if (IsSupported(1114137u))
		{
			return SetVivoMode(mode);
		}
		int num = DeviceSet(1179765u, mode, log);
		if (num != 1)
		{
			num = SetVivoMode(mode);
		}
		return num;
	}

	public int SetGPUEco(int eco)
	{
		uint gPUEco = GPUEco;
		int num = DeviceGet(gPUEco);
		if (num < 0)
		{
			return -1;
		}
		if (num == 1 && eco == 0)
		{
			return DeviceSet(gPUEco, eco, "GPUEco");
		}
		if (num == 0 && eco == 1)
		{
			return DeviceSet(gPUEco, eco, "GPUEco");
		}
		return -1;
	}

	public int GetFan(AsusFan device)
	{
		int num = Program.acpi.DeviceGet(device switch
		{
			AsusFan.GPU => 1114132u, 
			AsusFan.Mid => 1114161u, 
			_ => 1114131u, 
		});
		int num2 = num & 0xFFFF;
		if (num2 > 120 || (num2 == 0 && num < 0))
		{
			num2 = -1;
		}
		return num2;
	}

	public bool IsMidFanSupported()
	{
		return IsSupported(1114161u);
	}

	public int SetFanRange(AsusFan device, byte[] curve)
	{
		if (curve.Length != 16)
		{
			return -1;
		}
		if (curve.All((byte singleByte) => singleByte == 0))
		{
			return -1;
		}
		byte b = (byte)(curve[8] * 255 / 100);
		byte b2 = (byte)(curve[15] * 255 / 100);
		byte[] @params = new byte[2] { b, b2 };
		if (device == AsusFan.GPU)
		{
			return DeviceSet(1114147u, @params, "FanRangeGPU");
		}
		return DeviceSet(1114146u, @params, "FanRangeCPU");
	}

	public int SetFanCurve(AsusFan device, byte[] curve)
	{
		if (curve.Length != 16)
		{
			return -1;
		}
		if (curve.All((byte singleByte) => singleByte == 0))
		{
			return -1;
		}
		int num = AppConfig.Get("fan_scale", 100);
		if (num != 100 && device == AsusFan.CPU)
		{
			Logger.WriteLine("Custom fan scale: " + num);
		}
		for (int i = 8; i < curve.Length; i++)
		{
			curve[i] = (byte)(Math.Max((byte)0, Math.Min((byte)100, curve[i])) * num / 100);
		}
		return device switch
		{
			AsusFan.GPU => DeviceSet(1114149u, curve, "FanGPU"), 
			AsusFan.Mid => DeviceSet(1114162u, curve, "FanMid"), 
			_ => DeviceSet(1114148u, curve, "FanCPU"), 
		};
	}

	public byte[] GetFanCurve(AsusFan device, int mode = 0)
	{
		uint status = mode switch
		{
			1 => 2u, 
			2 => 1u, 
			_ => 0u, 
		};
		return device switch
		{
			AsusFan.GPU => DeviceGetBuffer(1114149u, status), 
			AsusFan.Mid => DeviceGetBuffer(1114162u, status), 
			_ => DeviceGetBuffer(1114148u, status), 
		};
	}

	public static bool IsInvalidCurve(byte[] curve)
	{
		if (curve.Length == 16)
		{
			return IsEmptyCurve(curve);
		}
		return true;
	}

	public static bool IsEmptyCurve(byte[] curve)
	{
		return curve.All((byte singleByte) => singleByte == 0);
	}

	public (int up, int down) GetFanHysteresis()
	{
		int num = DeviceGet(1114164u);
		if (num < 0)
		{
			return (up: -1, down: -1);
		}
		int num2 = num & 0xFF;
		int num3 = (num >> 8) & 0xFF;
		Logger.WriteLine($"FanHysteresis Read: up={num2} down={num3} (raw=0x{num:X4})");
		return (up: num2, down: num3);
	}

	public int SetFanHysteresis(int up, int down)
	{
		int result = -1;
		int value = (down << 8) | up;
		if (IsSupported(1114164u))
		{
			byte[] array = new byte[16];
			int num = (AppConfig.Is("mid_fan") ? 3 : 2);
			for (int i = 0; i < num; i++)
			{
				array[i * 4] = (byte)up;
				array[i * 4 + 1] = (byte)down;
			}
			Logger.WriteLine($"FanHysteresis Write: up={up} down={down} (per-fan=0x{value:X4}, slots={num})");
			result = DeviceSet(1114164u, array, "FanHysteresis");
		}
		return result;
	}

	public static byte[] FixFanCurve(byte[] curve)
	{
		if (curve.Length != 16)
		{
			throw new Exception("Incorrect curve");
		}
		Dictionary<byte, byte> dictionary = new Dictionary<byte, byte>();
		byte b = 0;
		for (int i = 0; i < 8; i++)
		{
			if (curve[i] <= b)
			{
				curve[i] = (byte)Math.Min(100, b + 6);
			}
			dictionary[curve[i]] = curve[i + 8];
			b = curve[i];
		}
		Dictionary<byte, byte> dictionary2 = new Dictionary<byte, byte>();
		bool flag = false;
		int num = 0;
		foreach (KeyValuePair<byte, byte> item in dictionary.OrderBy((KeyValuePair<byte, byte> x) => x.Key))
		{
			if (num == 0 && item.Key >= 40)
			{
				flag = true;
				dictionary2.Add(30, 0);
			}
			if (num != 3 || !flag)
			{
				dictionary2.Add(item.Key, item.Value);
			}
			num++;
		}
		num = 0;
		foreach (KeyValuePair<byte, byte> item2 in dictionary2.OrderBy((KeyValuePair<byte, byte> x) => x.Key))
		{
			int num2 = item2.Key;
			if (AppConfig.IsClampFanDots())
			{
				int num3 = 30 + num * 10;
				int val = num3 + 10;
				num2 = Math.Max(num3, Math.Min(val, num2));
			}
			curve[num] = (byte)num2;
			curve[num + 8] = item2.Value;
			num++;
		}
		return curve;
	}

	public bool IsXGConnected()
	{
		if (IsSupported(589848u))
		{
			return DeviceGet(589848u) == 1;
		}
		return false;
	}

	public bool IsAllAmdPPT()
	{
		bool? allAMD = _allAMD;
		if (!allAMD.HasValue)
		{
			_allAMD = IsSupported(1179824u) && !IsSupported(1179840u) && !AppConfig.IsAMDiGPU();
		}
		return _allAMD.Value;
	}

	public bool IsOverdriveSupported()
	{
		return IsSupported(327705u);
	}

	public bool IsSupported(uint DeviceID)
	{
		if (!_supportCache.TryGetValue(DeviceID, out var value))
		{
			value = DeviceGet(DeviceID) >= 0;
			_supportCache[DeviceID] = value;
		}
		return value;
	}

	public bool IsNVidiaGPU()
	{
		if (!IsAllAmdPPT())
		{
			return IsSupported(GPUEco);
		}
		return false;
	}

	public void SetAPUMem(int memory = 4)
	{
		if (memory >= 0 && memory < apuMemEnum.Length)
		{
			Program.acpi.DeviceSet(393409u, (memory != 0) ? (0x100 | apuMemEnum[memory]) : 0, "APU Mem");
		}
	}

	public int GetAPUMem()
	{
		int num = Program.acpi.DeviceGet(393409u);
		if (num < 0)
		{
			return -1;
		}
		int num2 = Array.IndexOf(apuMemEnum, num - 256);
		if (num2 >= 0)
		{
			return num2;
		}
		return 4;
	}

	public int[] GetVramOptions(out int unitMb)
	{
		unitMb = 0;
		byte[] value = DeviceGetLarge(393412u);
		int num = BitConverter.ToInt32(value, 0);
		if ((num & 0x10000) == 0 || (num & 0x80000) != 0)
		{
			return Array.Empty<int>();
		}
		int num2 = num & 0xFFFF;
		if (num2 > 16)
		{
			num2 = 17;
		}
		if (num2 < 2)
		{
			return Array.Empty<int>();
		}
		unitMb = (((num & 0x20000) == 0) ? 1 : 512);
		int[] array = new int[num2];
		for (int i = 1; i < num2; i++)
		{
			array[i] = BitConverter.ToUInt16(value, 6 + i * 2);
		}
		return array;
	}

	public int GetVramMem()
	{
		return (int)BitConverter.ToUInt32(DeviceGetLarge(393412u), 4);
	}

	public void SetVramMem(int value)
	{
		DeviceSet(393412u, value, "VRAM Mem");
	}

	public (int, int) GetCores(uint device = 1179858u)
	{
		int num = Program.acpi.DeviceGet(device);
		Logger.WriteLine("Cores " + device.ToString("X8") + ": " + ((num < 0) ? "unsupported" : ("0x" + num.ToString("X4"))));
		if (num < 0)
		{
			return (-1, -1);
		}
		return ((num >> 8) & 0xFF, num & 0xFF);
	}

	public void SetCores(int eCores, int pCores)
	{
		if (eCores < 0 || eCores > 16 || pCores < 1 || pCores > 16)
		{
			Logger.WriteLine($"Incorrect Core config ({eCores}, {pCores})");
		}
		else
		{
			int status = (eCores << 8) | pCores;
			Program.acpi.DeviceSet(1179858u, status, "Cores (0x" + status.ToString("X4") + ")");
		}
	}

	public string ScanRange()
	{
		string text = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Asus", "\\scan.txt");
		using StreamWriter streamWriter = File.AppendText(text);
		streamWriter.WriteLine($"Scan started {DateTime.Now}");
		for (uint num = 0u; num <= 1441792; num += 65536)
		{
			for (uint num2 = 0u; num2 <= 255; num2++)
			{
				uint deviceID = num + num2;
				byte[] array = DeviceGetLarge(deviceID);
				uint num3 = BitConverter.ToUInt32(array, 0);
				if ((num3 & 0x10000) == 0)
				{
					continue;
				}
				bool flag = false;
				for (int i = 4; i < array.Length; i++)
				{
					if (array[i] != 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					streamWriter.WriteLine(deviceID.ToString("X8") + ": BUF " + BitConverter.ToString(array));
					continue;
				}
				int num4 = (int)(num3 - 65536);
				streamWriter.WriteLine(deviceID.ToString("X8") + ": " + num4.ToString("X4") + " (" + num4 + ")");
			}
		}
		streamWriter.WriteLine("---------------------");
		streamWriter.Close();
		return text;
	}

	private byte[] DeviceGetLarge(uint DeviceID, int extraIn = 8, int outSize = 40)
	{
		byte[] array = new byte[12 + extraIn];
		byte[] array2 = new byte[outSize];
		BitConverter.GetBytes(1398035268u).CopyTo(array, 0);
		BitConverter.GetBytes((uint)(4 + extraIn)).CopyTo(array, 4);
		BitConverter.GetBytes(DeviceID).CopyTo(array, 8);
		Control(2237452u, array, array2);
		return array2;
	}

	public void TUFKeyboardBrightness(int brightness, string log = "TUF Backlight")
	{
		int status = 0x80 | (brightness & 0x7F);
		DeviceSet(327713u, status, log);
	}

	public void TUFKeyboardRGB(AuraMode mode, Color color, int speed, string? log = "TUF RGB")
	{
		byte[] array = new byte[6]
		{
			180,
			(byte)mode,
			color.R,
			color.G,
			color.B,
			(byte)speed
		};
		if (DeviceSet(1048662u, array, log) != 1)
		{
			array[0] = 179;
			DeviceSet(1048666u, array, log);
			array[0] = 180;
			DeviceSet(1048666u, array, log);
		}
	}

	public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false)
	{
		int num = 189;
		if (boot)
		{
			num |= 0x30000;
		}
		if (awake)
		{
			num |= 0xC0000;
		}
		if (sleep)
		{
			num |= 0x300000;
		}
		if (shutdown)
		{
			num |= 0xC00000;
		}
		num |= 0x100;
		DeviceSet(1048663u, num, "TUF_KB");
		if (AppConfig.IsVivoZenPro() && IsSupported(327727u))
		{
			DeviceSet(327727u, 1, "VIVO OOBE");
		}
	}

	public void SubscribeToEvents(Action<object, EventArrivedEventArgs> EventHandler)
	{
		try
		{
			watcher = new ManagementEventWatcher();
			watcher.EventArrived += EventHandler.Invoke;
			watcher.Scope = new ManagementScope("root\\wmi");
			watcher.Query = new WqlEventQuery("SELECT * FROM AsusAtkWmiEvent");
			watcher.Start();
		}
		catch
		{
			Logger.WriteLine("Can't connect to ASUS WMI events");
		}
	}
}

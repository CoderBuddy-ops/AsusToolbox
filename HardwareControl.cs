using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Asus;
using Asus.Battery;
using Asus.Fan;
using Asus.Helpers;
using PawnIO;

public static class HardwareControl
{
#pragma warning disable CS0649
	private struct SYSTEM_BATTERY_STATE
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool AcOnLine;

		[MarshalAs(UnmanagedType.U1)]
		public bool BatteryPresent;

		[MarshalAs(UnmanagedType.U1)]
		public bool Charging;

		[MarshalAs(UnmanagedType.U1)]
		public bool Discharging;

		public byte Spare1;

		public byte Spare2;

		public byte Spare3;

		public byte Spare4;

		public uint MaxCapacity;

		public uint RemainingCapacity;

		public int Rate;

		public uint EstimatedTime;

		public uint DefaultAlert1;

		public uint DefaultAlert2;
	}
#pragma warning restore CS0649

	private struct SYSTEM_POWER_STATUS
	{
		public byte ACLineStatus;

		public byte BatteryFlag;

		public byte BatteryLifePercent;

		public byte SystemStatusFlag;

		public int BatteryLifeTime;

		public int BatteryFullLifeTime;
	}

	private struct SP_DEVICE_INTERFACE_DATA
	{
		public uint cbSize;

		public Guid InterfaceClassGuid;

		public uint Flags;

		public nint Reserved;
	}

	private struct BATTERY_WAIT_STATUS
	{
		public uint BatteryTag;

		public uint Timeout;

		public uint PowerState;

		public uint LowCapacity;

		public uint HighCapacity;
	}

	private struct BATTERY_STATUS
	{
		public uint PowerState;

		public uint Capacity;

		public int Voltage;

		public int Rate;
	}

	private class EnergyChannel
	{
		private readonly PerformanceCounter _energy;

		private readonly PerformanceCounter _time;

		private long _rawEnergy;

		private long _rawTime;

		private float _watts;

		private int _stale;

		public EnergyChannel(string instance)
		{
			_energy = new PerformanceCounter("Energy Meter", "Energy", instance, readOnly: true);
			_time = new PerformanceCounter("Energy Meter", "Time", instance, readOnly: true);
			_rawEnergy = _energy.RawValue;
			_rawTime = _time.RawValue;
		}

		public float? Sample()
		{
			try
			{
				long rawValue = _energy.RawValue;
				long rawValue2 = _time.RawValue;
				if (rawValue2 != _rawTime)
				{
					_watts = (float)(rawValue - _rawEnergy) * 3.6E-06f / (float)(rawValue2 - _rawTime);
					_rawEnergy = rawValue;
					_rawTime = rawValue2;
					_stale = 0;
				}
				else if (++_stale >= 5)
				{
					return null;
				}
				return (_watts > 0f) ? new float?(_watts) : ((float?)null);
			}
			catch
			{
				return null;
			}
		}
	}

	private struct MEMORYSTATUSEX
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}


	public static float? cpuTemp = -1f;

	public static float? gpuTemp = -1f;

	public static float? cpuPower;


	public static decimal? batteryRate = default(decimal);

	public static decimal batteryHealth = -1m;

	public static decimal batteryCapacity = -1m;

	public static decimal? designCapacity;

	public static decimal? fullCapacity;

	public static decimal? chargeCapacity;

	public static string? batteryCharge;

	public static string? cpuFan;

	public static string? gpuFan;

	public static string? midFan;

	public static int? cpuFanRPM;


	public static int? cpuUsage;


	public static int? ramUsage;


	public static int? ramUsedMb;


	private static long lastUpdate;

	private static bool isPZ13 = AppConfig.IsPZ13();


	private static bool _chargeWatt = AppConfig.Is("charge_watt");

	private static PerformanceCounter? _cpuTempCounter;

	private const int SystemBatteryState = 5;

	private static readonly Guid GUID_DEVINTERFACE_BATTERY = new Guid("72631E54-78A4-11D0-BCF7-00AA00B7B32A");

	private const uint DIGCF_PRESENT = 2u;

	private const uint DIGCF_DEVICEINTERFACE = 16u;

	private const uint GENERIC_READ = 2147483648u;

	private const uint GENERIC_WRITE = 1073741824u;

	private const uint FILE_SHARE_READ = 1u;

	private const uint FILE_SHARE_WRITE = 2u;

	private const uint OPEN_EXISTING = 3u;

	private const uint FILE_ATTRIBUTE_NORMAL = 128u;

	private static readonly nint INVALID_HANDLE_VALUE = new IntPtr(-1);

	private const uint IOCTL_BATTERY_QUERY_TAG = 2703424u;

	private const uint IOCTL_BATTERY_QUERY_STATUS = 2703436u;

	private static string? _batteryDevicePath;

	private static long _lastBatteryRead;

	private static PerformanceCounter? _cpuPowerCounter;

	private static bool _cpuPowerCounterFailed;

	private static bool _cpuPowerInitStarted;


	private static int _cpuPowerReadErrors;

	private const int CpuPowerMaxReadErrors = 3;

	private static readonly string[] _powerCounterInstances = new string[5] { "Apu Power", "RAPL_Package0_PKG", "CPU Power", "Socket Power", "Current Socket Power" };



	private static long _cpuLastIdle;

	private static long _cpuLastKernel;

	private static long _cpuLastUser;

	private static long _cpuLastTick;

	private static bool _cpuUsageBaseline;


	private static IntelMsr? _intelMsr;

	private static bool _intelMsrPowerFailed;

	public static bool chargeWatt
	{
		get
		{
			return _chargeWatt;
		}
		set
		{
			AppConfig.Set("charge_watt", value ? 1 : 0);
			_chargeWatt = value;
		}
	}

	[DllImport("powrprof.dll", SetLastError = true)]
	private static extern uint CallNtPowerInformation(int InformationLevel, nint InputBuffer, uint InputBufferLength, nint OutputBuffer, uint OutputBufferLength);

	private static SYSTEM_BATTERY_STATE? GetNativeBatteryState()
	{
		int num = Marshal.SizeOf<SYSTEM_BATTERY_STATE>();
		nint num2 = Marshal.AllocHGlobal(num);
		try
		{
			if (CallNtPowerInformation(5, IntPtr.Zero, 0u, num2, (uint)num) == 0)
			{
				return Marshal.PtrToStructure<SYSTEM_BATTERY_STATE>(num2);
			}
			return null;
		}
		finally
		{
			Marshal.FreeHGlobal(num2);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetSystemPowerStatus(ref SYSTEM_POWER_STATUS lpSystemPowerStatus);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint SetupDiGetClassDevs(ref Guid classGuid, nint enumerator, nint hwndParent, uint flags);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern bool SetupDiEnumDeviceInterfaces(nint deviceInfoSet, nint deviceInfoData, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetupDiGetDeviceInterfaceDetail(nint deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, nint deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, nint deviceInfoData);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern bool SetupDiDestroyDeviceInfoList(nint deviceInfoSet);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, nint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, nint hTemplateFile);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DeviceIoControl(nint hDevice, uint dwIoControlCode, ref uint lpInBuffer, uint nInBufferSize, ref uint lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, nint lpOverlapped);

	[DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
	private static extern bool DeviceIoControlStatus(nint hDevice, uint dwIoControlCode, ref BATTERY_WAIT_STATUS lpInBuffer, uint nInBufferSize, ref BATTERY_STATUS lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, nint lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(nint hObject);

	private static string? GetBatteryDevicePath()
	{
		if (_batteryDevicePath != null)
		{
			return _batteryDevicePath;
		}
		Guid classGuid = GUID_DEVINTERFACE_BATTERY;
		nint num = SetupDiGetClassDevs(ref classGuid, IntPtr.Zero, IntPtr.Zero, 18u);
		if (num == INVALID_HANDLE_VALUE)
		{
			return null;
		}
		try
		{
			SP_DEVICE_INTERFACE_DATA deviceInterfaceData = default(SP_DEVICE_INTERFACE_DATA);
			deviceInterfaceData.cbSize = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>();
			if (!SetupDiEnumDeviceInterfaces(num, IntPtr.Zero, ref classGuid, 0u, ref deviceInterfaceData))
			{
				return null;
			}
			SetupDiGetDeviceInterfaceDetail(num, ref deviceInterfaceData, IntPtr.Zero, 0u, out var requiredSize, IntPtr.Zero);
			if (requiredSize == 0)
			{
				return null;
			}
			nint num2 = Marshal.AllocHGlobal((int)requiredSize);
			try
			{
				Marshal.WriteInt32(num2, (IntPtr.Size == 8) ? 8 : 6);
				if (!SetupDiGetDeviceInterfaceDetail(num, ref deviceInterfaceData, num2, requiredSize, out var _, IntPtr.Zero))
				{
					return null;
				}
				_batteryDevicePath = Marshal.PtrToStringAuto(num2 + 4);
				return _batteryDevicePath;
			}
			finally
			{
				Marshal.FreeHGlobal(num2);
			}
		}
		finally
		{
			SetupDiDestroyDeviceInfoList(num);
		}
	}

	private static BATTERY_STATUS? QueryBatteryStatus()
	{
		string batteryDevicePath = GetBatteryDevicePath();
		if (batteryDevicePath == null)
		{
			return null;
		}
		nint num = CreateFile(batteryDevicePath, 3221225472u, 3u, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
		if (num == INVALID_HANDLE_VALUE)
		{
			return null;
		}
		try
		{
			uint lpInBuffer = 0u;
			uint lpOutBuffer = 0u;
			if (!DeviceIoControl(num, 2703424u, ref lpInBuffer, 4u, ref lpOutBuffer, 4u, out var lpBytesReturned, IntPtr.Zero) || lpOutBuffer == 0)
			{
				return null;
			}
			BATTERY_WAIT_STATUS bATTERY_WAIT_STATUS = default(BATTERY_WAIT_STATUS);
			bATTERY_WAIT_STATUS.BatteryTag = lpOutBuffer;
			BATTERY_WAIT_STATUS lpInBuffer2 = bATTERY_WAIT_STATUS;
			BATTERY_STATUS lpOutBuffer2 = default(BATTERY_STATUS);
			if (!DeviceIoControlStatus(num, 2703436u, ref lpInBuffer2, (uint)Marshal.SizeOf<BATTERY_WAIT_STATUS>(), ref lpOutBuffer2, (uint)Marshal.SizeOf<BATTERY_STATUS>(), out lpBytesReturned, IntPtr.Zero))
			{
				return null;
			}
			return lpOutBuffer2;
		}
		finally
		{
			CloseHandle(num);
		}
	}


	public static void ReadBatteryState()
	{
		long num = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		if (Math.Abs(num - _lastBatteryRead) < 5000)
		{
			FormatBatteryCharge();
			return;
		}
		_lastBatteryRead = num;
		batteryRate = default(decimal);
		chargeCapacity = default(decimal);
		try
		{
			Task<BATTERY_STATUS?> task = Task.Run((Func<BATTERY_STATUS?>)QueryBatteryStatus);
			BATTERY_STATUS? bATTERY_STATUS = (task.Wait(1000) ? task.Result : ((BATTERY_STATUS?)null));
			if (bATTERY_STATUS.HasValue)
			{
				chargeCapacity = bATTERY_STATUS.Value.Capacity;
				if (bATTERY_STATUS.Value.Rate != 0)
				{
					batteryRate = (decimal)bATTERY_STATUS.Value.Rate / 1000m;
				}
			}
			decimal? num2 = fullCapacity;
			if ((!num2.HasValue || num2.GetValueOrDefault() == 0m) ? true : false)
			{
				SYSTEM_BATTERY_STATE? nativeBatteryState = GetNativeBatteryState();
				if (nativeBatteryState.HasValue && nativeBatteryState.Value.MaxCapacity != 0)
				{
					fullCapacity = nativeBatteryState.Value.MaxCapacity;
				}
			}
		}
		catch (Exception)
		{
		}
		FormatBatteryCharge();
	}

	private static void FormatBatteryCharge()
	{
		decimal? num = fullCapacity;
		if (!((num.GetValueOrDefault() > default(decimal)) & num.HasValue))
		{
			return;
		}
		num = chargeCapacity;
		if ((num.GetValueOrDefault() > default(decimal)) & num.HasValue)
		{
			batteryCapacity = Math.Min(100m, chargeCapacity.Value / fullCapacity.Value * 100m);
			if (batteryCapacity > 99m && BatteryControl.chargeFull)
			{
				BatteryControl.UnSetBatteryLimitFull();
			}
			batteryCharge = (chargeWatt ? (Math.Round(chargeCapacity.Value / 1000m, 1) + "Wh") : (Math.Round(batteryCapacity, 1) + "%"));
		}
	}

	public static void ReadDesignCapacity()
	{
		decimal? num = designCapacity;
		if ((num.GetValueOrDefault() > default(decimal)) & num.HasValue)
		{
			return;
		}
		try
		{
			ManagementScope scope = new ManagementScope("root\\WMI");
			ObjectQuery query = new ObjectQuery("SELECT DesignedCapacity FROM BatteryStaticData");
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(scope, query);
			foreach (ManagementObject item in managementObjectSearcher.Get().Cast<ManagementObject>())
			{
				using (item)
				{
					designCapacity = Convert.ToDecimal(item["DesignedCapacity"]);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void RefreshBatteryHealth()
	{
		decimal? num = designCapacity;
		if (!num.HasValue)
		{
			ReadDesignCapacity();
		}
		num = fullCapacity;
		if ((!num.HasValue || num.GetValueOrDefault() == 0m) ? true : false)
		{
			ReadBatteryState();
		}
		num = designCapacity;
		if (num.HasValue)
		{
			num = fullCapacity;
			if (num.HasValue)
			{
				num = designCapacity;
				if (!((num.GetValueOrDefault() == default(decimal)) & num.HasValue))
				{
					num = fullCapacity;
					if (!((num.GetValueOrDefault() == default(decimal)) & num.HasValue))
					{
						decimal num2 = fullCapacity.Value / designCapacity.Value;
						string[] obj = new string[7] { "Design Capacity: ", null, null, null, null, null, null };
						num = designCapacity;
						obj[1] = num.ToString();
						obj[2] = "mWh, Full Charge Capacity: ";
						num = fullCapacity;
						obj[3] = num.ToString();
						obj[4] = "mWh, Health: ";
						obj[5] = num2.ToString();
						obj[6] = "%";
						Logger.WriteLine(string.Concat(obj));
						batteryHealth = num2 * 100m;
						return;
					}
				}
			}
		}
		batteryHealth = -1m;
	}

	public static float? GetCPUTemp()
	{
		long num = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (Math.Abs(num - lastUpdate) < 2)
		{
			return cpuTemp;
		}
		lastUpdate = num;
		if (isPZ13)
		{
			return (float)GetCPUTempWMI();
		}
		cpuTemp = Program.acpi.DeviceGet(1179796u);
		if (cpuTemp < 0f)
		{
			try
			{
				if (_cpuTempCounter == null)
				{
					_cpuTempCounter = new PerformanceCounter("Thermal Zone Information", "Temperature", "\\_TZ.THRM", readOnly: true);
				}
				cpuTemp = _cpuTempCounter.NextValue() - 273f;
			}
			catch (Exception)
			{
			}
		}
		return cpuTemp;
	}

	private static double GetCPUTempWMI()
	{
		try
		{
			string queryString = "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature WHERE InstanceName = 'ACPI\\\\QCOM0C5A\\\\1_0'";
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\WMI", queryString);
			using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator();
			if (managementObjectEnumerator.MoveNext())
			{
				ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
				using (managementObject)
				{
					return Convert.ToDouble(managementObject["CurrentTemperature"]) / 10.0 - 273.15;
				}
			}
		}
		catch (Exception)
		{
		}
		return -1.0;
	}

	public static float? GetGPUTemp()
	{
		try
		{
			int num2 = Program.acpi.DeviceGet(1179799u);
			gpuTemp = ((num2 > 0 && num2 < 125) ? new float?(num2) : ((float?)null));
		}
		catch (Exception)
		{
			gpuTemp = null;
		}
		return gpuTemp;
	}

	public static void InitCPUPowerAsync()
	{
		if (_cpuPowerInitStarted)
		{
			return;
		}
		_cpuPowerInitStarted = true;
		Task.Run(delegate
		{
			string @string = AppConfig.GetString("cpu_power_counter");
			if (!string.IsNullOrEmpty(@string))
			{
				try
				{
					PerformanceCounter performanceCounter2 = new PerformanceCounter("Energy Meter", "Power", @string, readOnly: true);
					performanceCounter2.NextValue();
					_cpuPowerCounter = performanceCounter2;
					Logger.WriteLine("CPU Power source (cached): " + @string);
					return;
				}
				catch
				{
					AppConfig.Set("cpu_power_counter", "");
				}
			}
			try
			{
				string[] instanceNames2 = new PerformanceCounterCategory("Energy Meter").GetInstanceNames();
				string[] powerCounterInstances = _powerCounterInstances;
				foreach (string text in powerCounterInstances)
				{
					if (instanceNames2.Contains<string>(text, StringComparer.OrdinalIgnoreCase))
					{
						PerformanceCounter performanceCounter3 = new PerformanceCounter("Energy Meter", "Power", text, readOnly: true);
						performanceCounter3.NextValue();
						_cpuPowerCounter = performanceCounter3;
						AppConfig.Set("cpu_power_counter", text);
						Logger.WriteLine("CPU Power source: " + text);
						return;
					}
				}
				_cpuPowerCounterFailed = true;
			}
			catch
			{
				_cpuPowerCounterFailed = true;
			}
		});
	}

	private static float? GetCoresPower()
	{
		return null; // Ultralight: removed
	}

	public static float? GetCPUPower()
	{
		if (_cpuPowerCounterFailed || _cpuPowerCounter == null)
		{
			return null;
		}
		try
		{
			float num = _cpuPowerCounter.NextValue();
			if (num > 0f)
			{
				return num / 1000f;
			}
		}
		catch
		{
			_cpuPowerCounter?.Dispose();
			_cpuPowerCounter = null;
			if (++_cpuPowerReadErrors >= 3)
			{
				_cpuPowerCounterFailed = true;
			}
			else
			{
				_cpuPowerCounterFailed = false;
				_cpuPowerInitStarted = false;
			}
		}
		return null;
	}

	public static void ResetCPUPowerCounter()
	{
		_cpuPowerReadErrors = 0;
		_cpuPowerCounterFailed = false;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

	public static int? GetCPUUsage()
	{
		if (!GetSystemTimes(out var lpIdleTime, out var lpKernelTime, out var lpUserTime))
		{
			return null;
		}
		long tickCount = Environment.TickCount64;
		if (!_cpuUsageBaseline || tickCount - _cpuLastTick > 2000)
		{
			_cpuLastIdle = lpIdleTime;
			_cpuLastKernel = lpKernelTime;
			_cpuLastUser = lpUserTime;
			_cpuLastTick = tickCount;
			_cpuUsageBaseline = true;
			return null;
		}
		long num = lpIdleTime - _cpuLastIdle;
		long num2 = lpKernelTime - _cpuLastKernel + (lpUserTime - _cpuLastUser);
		_cpuLastIdle = lpIdleTime;
		_cpuLastKernel = lpKernelTime;
		_cpuLastUser = lpUserTime;
		_cpuLastTick = tickCount;
		if (num2 <= 0)
		{
			return 0;
		}
		return Math.Clamp((int)Math.Round((1.0 - (double)num / (double)num2) * 100.0), 0, 100);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

	public static (int percent, int usedMb)? GetRAMInfo()
	{
		MEMORYSTATUSEX mEMORYSTATUSEX = default(MEMORYSTATUSEX);
		mEMORYSTATUSEX.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
		MEMORYSTATUSEX lpBuffer = mEMORYSTATUSEX;
		if (!GlobalMemoryStatusEx(ref lpBuffer))
		{
			return null;
		}
		int item = (int)((lpBuffer.ullTotalPhys - lpBuffer.ullAvailPhys) / 1048576);
		return ((int)lpBuffer.dwMemoryLoad, item);
	}




	private static float? GetIntelMsrPower()
	{
		if (_intelMsrPowerFailed || CpuInfo.IsAMD)
		{
			return null;
		}
		try
		{
			if (_intelMsr == null)
			{
				IntelMsr intelMsr = new IntelMsr();
				if (!intelMsr.Initialize(typeof(HardwareControl).Assembly))
				{
					intelMsr.Dispose();
					_intelMsrPowerFailed = true;
					Logger.WriteLine("Intel MSR: PawnIO/IntelMSR module unavailable (not installed?)");
					return null;
				}
				_intelMsr = intelMsr;
				Logger.WriteLine("CPU Power source: Intel RAPL MSR (PawnIO)");
			}
			float? packagePower = _intelMsr.GetPackagePower();
			return (packagePower > 0f) ? packagePower : ((float?)null);
		}
		catch (Exception ex)
		{
			_intelMsrPowerFailed = true;
			Logger.WriteLine("Intel MSR power read failed: " + ex.Message);
			return null;
		}
	}


	public static void ReadSensors(bool log = false)
	{
		if (Program.acpi != null)
		{
			cpuFan = FanSensorControl.FormatFan(AsusFan.CPU, Program.acpi.GetFan(AsusFan.CPU));
			gpuFan = FanSensorControl.FormatFan(AsusFan.GPU, Program.acpi.GetFan(AsusFan.GPU));
			midFan = FanSensorControl.FormatFan(AsusFan.Mid, Program.acpi.GetFan(AsusFan.Mid));
			cpuTemp = GetCPUTemp();
			gpuTemp = GetGPUTemp();
			if (log)
			{
				Logger.WriteLine($"Temps: {cpuTemp} {gpuTemp} {cpuFan} {gpuFan} {midFan}");
			}
			ReadBatteryState();
		}
	}

	public static double GetBatteryChargePercentage()
	{
		try
		{
			SYSTEM_POWER_STATUS lpSystemPowerStatus = default(SYSTEM_POWER_STATUS);
			if (GetSystemPowerStatus(ref lpSystemPowerStatus) && lpSystemPowerStatus.BatteryLifePercent != byte.MaxValue)
			{
				return (int)lpSystemPowerStatus.BatteryLifePercent;
			}
		}
		catch (Exception)
		{
		}
		return 0.0;
	}


	public static void Dispose()
	{
		_cpuTempCounter?.Dispose();
		_cpuTempCounter = null;
	}
}

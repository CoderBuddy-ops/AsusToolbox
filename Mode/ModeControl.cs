using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Asus.Helpers;
using Asus.Properties;
using Asus.USB;
using PawnIO;

namespace Asus.Mode;

public class ModeControl
{
	private static bool customFans = false;

	private static int customPower = 0;

	private int _cpuUV;

	private int _igpuUV;

	private int _cpuTemp = CpuInfo.DefaultTemp;

	private bool _ryzenPower;

	private static RyzenSmuService? _smu;

	private static readonly object _smuLock = new object();

	private static System.Timers.Timer? reapplyTimer;

	private static System.Timers.Timer modeToggleTimer = null;

	private static CancellationTokenSource _modeCts = new CancellationTokenSource();

	private static Task _modeTask = Task.CompletedTask;

	private static SettingsForm? settings => SettingsOverride ?? Program.settingsForm;

	internal static SettingsForm? SettingsOverride { get; set; }

	private static RyzenSmuService? GetSmu()
	{
		lock (_smuLock)
		{
			if (_smu != null && _smu.IsInitialized)
			{
				return _smu;
			}
			_smu?.Dispose();
			_smu = new RyzenSmuService();
			if (!_smu.Initialize(Assembly.GetExecutingAssembly()))
			{
				_smu.Dispose();
				_smu = null;
			}
			else
			{
				Logger.WriteLine($"SMU Init: {_smu.CpuCodeName} ({_smu.Family}), SMU v{_smu.SmuVersion >> 16}.{(_smu.SmuVersion >> 8) & 0xFF}.{_smu.SmuVersion & 0xFF}");
			}
			return _smu;
		}
	}

	public static bool IsPawnAvailable()
	{
		return GetSmu() != null;
	}

	public static bool IsPawnInstalled()
	{
		return RyzenSmuService.IsPawnInstalled();
	}

	public ModeControl()
	{
		int num = AppConfig.Get("reapply_time", IsReapplyTempRequired() ? 30 : 0);
		if (num > 0)
		{
			reapplyTimer = new System.Timers.Timer(num * 1000);
			reapplyTimer.Elapsed += ReapplyTimer_Elapsed;
		}
	}

	private static bool IsReapplyTempRequired()
	{
		RyzenSmuService smu = GetSmu();
		bool flag = smu != null;
		if (flag)
		{
			CpuFamily family = smu.Family;
			bool flag2 = ((family == CpuFamily.Renoir || family == CpuFamily.Mobile) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private static bool IsReapplyRyzenRequired()
	{
		RyzenSmuService smu = GetSmu();
		if (smu != null)
		{
			return smu.Family == CpuFamily.Raphael;
		}
		return false;
	}

	private static void SetReapplyEnabled(bool enabled)
	{
		if (reapplyTimer != null)
		{
			reapplyTimer.Enabled = enabled;
		}
	}

	private void ReapplyTimer_Elapsed(object? sender, ElapsedEventArgs e)
	{
		SetCPUTemp(AppConfig.GetMode("cpu_temp"));
		SetRyzenPower();
	}

	public void WaitForApply()
	{
		try
		{
			_modeTask.Wait(5000);
		}
		catch
		{
		}
	}

	public void AutoPerformance(bool powerChanged = false)
	{
		int num = AppConfig.Get("performance_" + Program.PerformanceKey());
		Logger.WriteLine($"{Program.currentSource} Performance Mode: {Modes.GetName((num == -1) ? Modes.GetCurrent() : num)}");
		if (num != -1)
		{
			SetPerformanceMode(num, powerChanged);
		}
		else
		{
			SetPerformanceMode(Modes.GetCurrent());
		}
	}

	public void ResetPerformanceMode()
	{
		ResetRyzen();
		Program.acpi.SetPerformanceMode(Modes.GetCurrentBase());
		AppConfig.RemoveMode("powermode");
		PowerNative.SetPowerMode(Modes.GetCurrentBase());
	}

	public void Toast()
	{
		Program.toast.RunToast(Modes.GetCurrentName(), (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) ? ToastIcon.Charger : ToastIcon.Battery);
	}

	public void SetPerformanceMode(int mode = -1, bool notify = false)
	{
		int oldMode = Modes.GetCurrent();
		if (mode < 0)
		{
			mode = oldMode;
		}
		if (!Modes.Exists(mode))
		{
			mode = 0;
		}
		settings?.ShowMode(mode);
		Modes.SetCurrent(mode);
		_modeCts.Cancel();
		_modeCts = new CancellationTokenSource();
		CancellationToken ct = _modeCts.Token;
		_modeTask = Task.Run(async delegate
		{
			_ = 2;
			try
			{
				bool num = AppConfig.IsResetRequired() && Modes.GetBase(oldMode) == Modes.GetBase(mode) && customPower > 0 && !AppConfig.IsApplyPower();
				customFans = false;
				customPower = 0;
				SetModeLabel();
				if (num)
				{
					Program.acpi.DeviceSet(1179765u, (Modes.GetBase(oldMode) != 1) ? 1 : 0, "ModeReset");
					await Task.Delay(TimeSpan.FromMilliseconds(1500.0), ct);
				}
				ct.ThrowIfCancellationRequested();
				if (AppConfig.Is("status_mode"))
				{
					Program.acpi.DeviceSet(589873u, new byte[2]
					{
						0,
						(byte)((Modes.GetBase(mode) == 2) ? 2 : 3)
					}, "StatusMode");
				}
				Program.acpi.SetPerformanceMode(AppConfig.IsManualModeRequired() ? 4 : Modes.GetBase(mode));
				SetGPUClocks();
				await Task.Delay(TimeSpan.FromMilliseconds(100.0), ct);
				ct.ThrowIfCancellationRequested();
				AutoFans();
				await Task.Delay(TimeSpan.FromMilliseconds(1000.0), ct);
				ct.ThrowIfCancellationRequested();
				AutoPower();
				string modeString = AppConfig.GetModeString("mode_command");
				if (modeString != null)
				{
					Logger.WriteLine("Running mode command: " + modeString);
					RestrictedProcessHelper.RunAsRestrictedUser(modeString);
				}
			}
			catch (OperationCanceledException)
			{
				Logger.WriteLine($"SetPerformanceMode cancelled (mode {mode})");
			}
			catch (Exception ex2)
			{
				Logger.WriteLine($"SetPerformanceMode failed (mode {mode}): {ex2.Message}");
			}
		}, ct);
		if (notify)
		{
			Toast();
		}
		if (!AppConfig.Is("skip_powermode"))
		{
			if (AppConfig.GetModeString("powermode") != null)
			{
				PowerNative.SetPowerMode(AppConfig.GetModeString("powermode"));
			}
			else
			{
				PowerNative.SetPowerMode(Modes.GetBase(mode));
			}
			if (AppConfig.IsAutoASPM())
			{
				PowerNative.SetBalancedASPM();
			}
			if (AppConfig.IsAutoStandbyNetworking())
			{
				PowerNative.SetConnectivityInStandby();
			}
		}
		if (AppConfig.GetMode("auto_boost") != -1)
		{
			PowerNative.SetCPUBoost(AppConfig.GetMode("auto_boost"));
		}
		settings?.FansInit();
	}

	private void ModeToggleTimer_Elapsed(object? sender, ElapsedEventArgs e)
	{
		modeToggleTimer.Stop();
		Logger.WriteLine($"Hotkey mode: {Modes.GetCurrent()}");
		SetPerformanceMode();
	}

	public void CyclePerformanceMode(bool back = false)
	{
		int num = AppConfig.Get("mode_delay", 1000);
		if (modeToggleTimer == null)
		{
			modeToggleTimer = new System.Timers.Timer(num);
			modeToggleTimer.Elapsed += ModeToggleTimer_Elapsed;
		}
		modeToggleTimer.Stop();
		modeToggleTimer.Start();
		Modes.SetCurrent(Modes.GetNext(back));
		Toast();
	}

	public void AutoFans(bool force = false)
	{
		customFans = false;
		if (AppConfig.IsApplyFans() || force)
		{
			bool flag = false;
			if (AppConfig.Is("xgm_fan"))
			{
				XGM.SetFan(AppConfig.GetFanConfig(AsusFan.XGM));
				flag = Program.acpi.IsXGConnected();
			}
			int num = Program.acpi.SetFanCurve(AsusFan.CPU, AppConfig.GetFanConfig(AsusFan.CPU));
			int num2 = Program.acpi.SetFanCurve(AsusFan.GPU, AppConfig.GetFanConfig(AsusFan.GPU));
			if (AppConfig.Is("mid_fan"))
			{
				Program.acpi.SetFanCurve(AsusFan.Mid, AppConfig.GetFanConfig(AsusFan.Mid));
			}
			if (num != 1 || num2 != 1)
			{
				int num3 = Program.acpi.SetFanRange(AsusFan.CPU, AppConfig.GetFanConfig(AsusFan.CPU));
				num2 = Program.acpi.SetFanRange(AsusFan.GPU, AppConfig.GetFanConfig(AsusFan.GPU));
				if (num3 != 1 || num2 != 1)
				{
					Program.acpi.DeviceSet(1179765u, Modes.GetCurrentBase(), "Reset Mode");
					settings?.LabelFansResult("Model doesn't support custom fan curves");
				}
			}
			else
			{
				settings?.LabelFansResult("");
				customFans = true;
			}
			int mode = AppConfig.GetMode("hysteresis_up");
			int mode2 = AppConfig.GetMode("hysteresis_down");
			if (mode > 0 && mode2 > 0)
			{
				Program.acpi.SetFanHysteresis(mode, mode2);
			}
			if ((AppConfig.IsPowerRequired() || flag) && !AppConfig.IsApplyPower())
			{
				Task.Run(async delegate
				{
					await Task.Delay(TimeSpan.FromSeconds(1.0));
					Program.acpi.DeviceSet(1179808u, 80, "PowerLimit Fix A0");
					Program.acpi.DeviceSet(1179811u, 80, "PowerLimit Fix A3");
				});
			}
		}
		else
		{
			XGM.Reset();
		}
		SetModeLabel();
	}

	public void AutoPower(bool launchAsAdmin = false)
	{
		customPower = 0;
		bool num = AppConfig.IsApplyPower();
		bool flag = AppConfig.IsApplyFans();
		if (num && !flag && AppConfig.IsFanRequired())
		{
			AutoFans(force: true);
			Thread.Sleep(500);
		}
		if (num)
		{
			SetPower(launchAsAdmin);
		}
		Thread.Sleep(500);
		SetGPUPower();
		AutoRyzen();
		if (IsReapplyRyzenRequired())
		{
			Task.Delay(5000).ContinueWith(delegate
			{
				AutoRyzen();
				ReadRyzenLimits();
			});
		}
	}

	public void SetModeLabel()
	{
		settings?.SetModeLabel(Strings.PerformanceMode + ": " + Modes.GetCurrentName() + (customFans ? "+" : "") + ((customPower > 0) ? (" " + customPower + "W") : ""));
	}

	public void SetRyzenPower(bool init = false)
	{
		if (init)
		{
			_ryzenPower = true;
		}
		if (!_ryzenPower || !AppConfig.IsApplyPower())
		{
			return;
		}
		RyzenSmuService smu = GetSmu();
		if (smu == null)
		{
			return;
		}
		int mode = AppConfig.GetMode("limit_total");
		int mode2 = AppConfig.GetMode("limit_slow", mode);
		int mode3 = AppConfig.GetMode("limit_fast", mode2);
		if (mode <= AsusACPI.MaxTotal && mode >= 5)
		{
			smu.SetAllLimits(mode, mode3, mode2, out var stapm, out var fast, out var slow);
			if (init)
			{
				Logger.WriteLine($"STAPM: {mode}W {stapm} | SLOW: {mode2}W {slow} | FAST: {mode3}W {fast}");
			}
		}
	}

	public void SetPower(bool launchAsAdmin = false)
	{
		bool flag = Program.acpi.IsAllAmdPPT();
		bool isAMD = CpuInfo.IsAMD;
		int mode = AppConfig.GetMode("limit_total");
		int mode2 = AppConfig.GetMode("limit_cpu");
		int num = AppConfig.GetMode("limit_slow");
		int mode3 = AppConfig.GetMode("limit_fast");
		if (num < 0 || flag)
		{
			num = mode;
		}
		if (mode > AsusACPI.MaxTotal || mode < 5 || mode2 > AsusACPI.MaxCPU || mode2 < 5 || mode3 > AsusACPI.MaxTotal || mode3 < 5 || num > AsusACPI.MaxTotal || num < 5)
		{
			return;
		}
		if (Program.acpi.IsSupported(1179808u))
		{
			Program.acpi.DeviceSet(1179811u, mode, "PowerLimit A3");
			Program.acpi.DeviceSet(1179808u, num, "PowerLimit A0");
			customPower = mode;
		}
		else if (isAMD)
		{
			if (ProcessHelper.IsUserAdministrator())
			{
				SetRyzenPower(init: true);
			}
			else if (launchAsAdmin)
			{
				ProcessHelper.RunAsAdmin("cpu");
				return;
			}
		}
		if (flag)
		{
			Program.acpi.DeviceSet(1179824u, mode2, "PowerLimit B0");
			customPower = mode2;
		}
		else if (isAMD && Program.acpi.IsSupported(1179841u))
		{
			Program.acpi.DeviceSet(1179841u, mode3, "PowerLimit C1");
		}
		SetModeLabel();
	}

	public void SetGPUClocks(bool launchAsAdmin = true, bool reset = false)
	{
		Task.Run(delegate
		{
			int num = AppConfig.GetMode("gpu_core");
			int num2 = AppConfig.GetMode("gpu_memory");
			int num3 = AppConfig.GetMode("gpu_clock_limit");
			if (reset)
			{
				num = (num2 = (num3 = 0));
			}
			if (num != -1 || num2 != -1 || num3 != -1)
			{
				if (Program.acpi.DeviceGet(AsusACPI.GPUEco) == 1)
				{
					Logger.WriteLine("Clocks: Eco");
				}
				else						Logger.WriteLine("Clocks: GPU control not available (ultralight build)");
			}
		});
	}

	public void SetGPUPower()
	{
		int mode = AppConfig.GetMode("gpu_boost");
		int mode2 = AppConfig.GetMode("gpu_temp");
		int mode3 = AppConfig.GetMode("gpu_power");
		int num = -1;
		if (mode3 >= AsusACPI.MinGPUPower && mode3 <= AsusACPI.MaxGPUPower && Program.acpi.IsSupported(1179800u))
		{
			Program.acpi.DeviceSet(1179800u, mode3, "PowerLimit TGP (GPU VAR)");
		}
		if (mode >= 5 && mode <= AsusACPI.MaxGPUBoost && Program.acpi.IsSupported(1179840u))
		{
			num = Program.acpi.DeviceSet(1179840u, mode, "PowerLimit C0 (GPU BOOST)");
		}
		if (mode2 >= 75 && mode2 <= 87 && Program.acpi.IsSupported(1179842u))
		{
			Program.acpi.DeviceSet(1179842u, mode2, "PowerLimit C2 (GPU TEMP)");
		}
		if (num == 0)
		{
			Program.acpi.DeviceSet(1179840u, mode, "PowerLimit C0");
		}
	}

	public SmuStatus? SetCPUTemp(int cpuTemp, bool log = false)
	{
		if (cpuTemp < CpuInfo.MinTemp || cpuTemp > CpuInfo.DefaultTemp)
		{
			return null;
		}
		if (cpuTemp == CpuInfo.DefaultTemp && _cpuTemp == CpuInfo.DefaultTemp)
		{
			return null;
		}
		RyzenSmuService smu = GetSmu();
		if (smu == null)
		{
			return null;
		}
		SmuStatus smuStatus = smu.SetThm(cpuTemp);
		if (log)
		{
			Logger.WriteLine($"CPU Temp: {cpuTemp}°C {smuStatus}");
		}
		if (smuStatus == SmuStatus.OK)
		{
			_cpuTemp = cpuTemp;
		}
		return smuStatus;
	}

	public void SetUV(int cpuUV)
	{
		if (!CpuInfo.IsSupportedUV() || cpuUV < CpuInfo.MinCPUUV || cpuUV > CpuInfo.MaxCPUUV)
		{
			return;
		}
		RyzenSmuService smu = GetSmu();
		if (smu != null)
		{
			SmuStatus smuStatus = smu.SetCoAll(cpuUV);
			Logger.WriteLine($"UV: {cpuUV} {smuStatus}");
			if (smuStatus == SmuStatus.OK)
			{
				_cpuUV = cpuUV;
			}
		}
	}

	public void SetUViGPU(int igpuUV)
	{
		if (!CpuInfo.IsSupportedUViGPU() || igpuUV < CpuInfo.MinIGPUUV || igpuUV > CpuInfo.MaxIGPUUV)
		{
			return;
		}
		RyzenSmuService smu = GetSmu();
		if (smu != null)
		{
			SmuStatus smuStatus = smu.SetCoGfx(igpuUV);
			Logger.WriteLine($"iGPU UV: {igpuUV} {smuStatus}");
			if (smuStatus == SmuStatus.OK)
			{
				_igpuUV = igpuUV;
			}
		}
	}

	public string SetRyzen(bool launchAsAdmin = false)
	{
		if (!ProcessHelper.IsUserAdministrator())
		{
			if (launchAsAdmin)
			{
				ProcessHelper.RunAsAdmin("uv");
			}
			return string.Empty;
		}
		RyzenSmuService smu = GetSmu();
		if (smu == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			int mode = AppConfig.GetMode("cpu_uv", 0);
			int mode2 = AppConfig.GetMode("igpu_uv", 0);
			int mode3 = AppConfig.GetMode("cpu_temp");
			string modeString = AppConfig.GetModeString("cpu_uv_cores");
			if (CpuInfo.IsSupportedUV() && modeString != null)
			{
				int num = 0;
				string[] array = modeString.Split('-', StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					if (int.TryParse(array[i], out var result) && -result >= CpuInfo.MinCPUUV && -result <= CpuInfo.MaxCPUUV)
					{
						SmuStatus smuStatus = smu.SetCoPer(num, -result);
						Logger.WriteLine($"UV core {num}: {-result} {smuStatus}");
						if (smuStatus == SmuStatus.OK)
						{
							_cpuUV = -result;
						}
					}
					num++;
				}
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
				handler.AppendLiteral("CPU UV cores ");
				handler.AppendFormatted(modeString);
				stringBuilder3.AppendLine(ref handler);
			}
			else if (CpuInfo.IsSupportedUV() && mode >= CpuInfo.MinCPUUV && mode <= CpuInfo.MaxCPUUV)
			{
				SmuStatus smuStatus2 = smu.SetCoAll(mode);
				Logger.WriteLine($"UV: {mode} {smuStatus2}");
				if (smuStatus2 == SmuStatus.OK)
				{
					_cpuUV = mode;
				}
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(9, 2, stringBuilder2);
				handler.AppendLiteral("CPU UV ");
				handler.AppendFormatted(mode);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(smuStatus2);
				stringBuilder4.AppendLine(ref handler);
			}
			if (CpuInfo.IsSupportedUViGPU() && mode2 >= CpuInfo.MinIGPUUV && mode2 <= CpuInfo.MaxIGPUUV)
			{
				SmuStatus smuStatus3 = smu.SetCoGfx(mode2);
				Logger.WriteLine($"iGPU UV: {mode2} {smuStatus3}");
				if (smuStatus3 == SmuStatus.OK)
				{
					_igpuUV = mode2;
				}
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(10, 2, stringBuilder2);
				handler.AppendLiteral("iGPU UV ");
				handler.AppendFormatted(mode2);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(smuStatus3);
				stringBuilder5.AppendLine(ref handler);
			}
			SmuStatus? value = SetCPUTemp(mode3, log: true);
			if (value.HasValue)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(13, 2, stringBuilder2);
				handler.AppendLiteral("CPU Temp ");
				handler.AppendFormatted(mode3);
				handler.AppendLiteral("°C: ");
				handler.AppendFormatted(value);
				stringBuilder6.AppendLine(ref handler);
			}
		}
		catch (Exception ex)
		{
			Logger.WriteLine("UV Error: " + ex.ToString());
		}
		SetReapplyEnabled(AppConfig.IsApplyUV());
		return stringBuilder.ToString().TrimEnd();
	}

	public string ReadRyzenLimits()
	{
		RyzenSmuService smu = GetSmu();
		if (smu == null)
		{
			return string.Empty;
		}
		try
		{
			PowerLimits powerLimits = smu.GetPowerLimits();
			if (powerLimits == null)
			{
				return string.Empty;
			}
			string text = $"SPL: {powerLimits.Stapm:F1}W | sPPT {powerLimits.Slow:F1}W | fPPT {powerLimits.Fast:F1}W";
			if (powerLimits.ApuSlow.HasValue)
			{
				text += $" | APU {powerLimits.ApuSlow.Value:F1}W";
			}
			text += $", Temp: {powerLimits.TctlTemp:F0}°C";
			Logger.WriteLine("Ryzen Limits: " + text);
			return text;
		}
		catch (Exception ex)
		{
			Logger.WriteLine("ReadRyzenLimits Error: " + ex.ToString());
			return string.Empty;
		}
	}

	public void ResetRyzen()
	{
		if (_cpuUV != 0)
		{
			SetUV(0);
		}
		if (_igpuUV != 0)
		{
			SetUViGPU(0);
		}
		if (_cpuTemp != CpuInfo.DefaultTemp)
		{
			SetCPUTemp(CpuInfo.DefaultTemp, log: true);
		}
		SetReapplyEnabled(enabled: false);
	}

	public void AutoRyzen()
	{
		if (CpuInfo.IsAMD)
		{
			if (AppConfig.IsApplyUV())
			{
				SetRyzen();
			}
			else
			{
				ResetRyzen();
			}
		}
	}

	public void AutoCPUTemp()
	{
		if (!CpuInfo.IsAMD || !AppConfig.IsApplyUV() || !ProcessHelper.IsUserAdministrator())
		{
			return;
		}
		try
		{
			SetCPUTemp(AppConfig.GetMode("cpu_temp"), log: true);
		}
		catch (Exception ex)
		{
			Logger.WriteLine("AutoCPUTemp Error: " + ex.Message);
		}
	}

	public void ShutdownReset()
	{
		if (AppConfig.IsShutdownReset())
		{
			Program.acpi.DeviceSet(1179765u, 0, "Mode Reset");
		}
	}

	public void SleepReset()
	{
		if (AppConfig.IsSleepReset())
		{
			Program.acpi.DeviceSet(1179765u, Modes.GetCurrentBase(), "Sleep Reset");
		}
	}
}

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Asus.AutoUpdate;
using Asus.Battery;
using Asus.Device;
using Asus.Display;
using Asus.Helpers;
using Asus.Input;
using Asus.Mode;
using Asus.Properties;
using Asus.USB;
using Microsoft.Win32;

namespace Asus;

internal static class Program
{
	public enum PowerSource
	{
		Battery,
		Barrel,
		USBC
	}

	public static NotifyIcon trayIcon;

	public static IAsusACPI acpi;

	public static DeviceCapabilities capabilities;

	public static SettingsForm settingsForm;

	public static ModeControl modeControl;

	public static ClamshellModeControl clamshellControl;

	public static ToastForm toast;

	public static nint unRegPowerNotify;

	public static nint unRegPowerNotifyLid;

	public static nint unRegPowerNotifyEnergy;

	public static nint unRegSuspendResume;

	public static int WM_TASKBARCREATED = 0;

	private static long lastAuto;

	private static readonly object autoLock = new object();

	private static long lastTheme;

	public static InputDispatcher? inputDispatcher;

	public static PowerSource currentSource = PowerSource.Battery;

	private static PowerLineStatus lastLineStatus = SystemInformation.PowerStatus.PowerLineStatus;

	private static readonly System.Timers.Timer powerSettleTimer = new System.Timers.Timer
	{
		AutoReset = false
	};

	public static bool usbcProfile = AppConfig.Is("usbc_profile");

	public static void Main(string[] args)
	{
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
		{
			Logger.WriteLine("Unhandled: " + e.ExceptionObject);
		};
		TaskScheduler.UnobservedTaskException += delegate(object? s, UnobservedTaskExceptionEventArgs e)
		{
			Logger.WriteLine("Unobserved: " + e.Exception);
			e.SetObserved();
		};
		string text = "";
		if (args.Length != 0)
		{
			text = args[0];
		}
		if (text == "charge")
		{
			BatteryLimit();
			try
			{
				InputDispatcher.StartupBacklight();
			}
			catch (Exception ex)
			{
				Logger.WriteLine("Startup Backlight: " + ex.Message);
			}
			Application.Exit();
			return;
		}
		string @string = AppConfig.GetString("language");
		try
		{
			if (@string != null && @string.Length > 0)
			{
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(@string);
			}
			else
			{
				CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
				if (cultureInfo.ToString() == "kr")
				{
					cultureInfo = CultureInfo.GetCultureInfo("ko");
				}
				Thread.CurrentThread.CurrentUICulture = cultureInfo;
			}
		}
		catch
		{
			Logger.WriteLine("Unknown Language: " + @string);
		}
		Logger.WriteLine("----------------------");
		Logger.WriteLine("App launched: " + AppConfig.GetModel() + " :" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + CultureInfo.CurrentUICulture?.ToString() + (ProcessHelper.IsUserAdministrator() ? "." : ""));
		if (AutoUpdateControl.HandlePendingUpdate())
		{
			return;
		}
		settingsForm = new SettingsForm();
		modeControl = new ModeControl();
		clamshellControl = new ClamshellModeControl();
		toast = new ToastForm();
		ProcessHelper.CheckAlreadyRunning();
		ProcessHelper.SetPriority();
		CleanupLegacyFiles();
		int value = AppConfig.Get("start_count") + 1;
		AppConfig.Set("start_count", value);
		Logger.WriteLine("Start Count: " + value);
		acpi = new AsusACPI();
		capabilities = DeviceCapabilities.Detect();
		Logger.WriteLine("Capabilities: " + capabilities.Summary());
		if (!acpi.IsConnected() && AppConfig.IsASUS() && !AppConfig.IsDesktop())
		{
			if (MessageBox.Show(Strings.ACPIError, Strings.StartupError, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Process.Start(new ProcessStartInfo("https://www.asus.com/support/FAQ/1047338/")
				{
					UseShellExecute = true
				});
			}
			Application.Exit();
			return;
		}
		ProcessHelper.KillSmartDisplayControl();
		AsusService.StopOnStartup();
		Application.EnableVisualStyles();
		trayIcon = new NotifyIcon
		{
			Text = "Asus",
			Icon = Resources.standard,
			Visible = true
		};
		System.Windows.Forms.Timer trayRetry = new System.Windows.Forms.Timer
		{
			Interval = 5000
		};
		trayRetry.Tick += delegate
		{
			trayRetry.Dispose();
			trayIcon.Visible = false;
			trayIcon.Visible = true;
		};
		trayRetry.Start();
		WM_TASKBARCREATED = NativeMethods.RegisterWindowMessage("TaskbarCreated");
		Logger.WriteLine($"Tray Icon: {trayIcon.Visible} | {WM_TASKBARCREATED}");
		Modes.InitFullSpeed();
		settingsForm.SetContextMenu();
		trayIcon.MouseClick += TrayIcon_MouseClick;
		trayIcon.MouseMove += TrayIcon_MouseMove;
		inputDispatcher = new InputDispatcher();
		settingsForm.InitAura();
		ScreenControl.InitScreen();
		SetAutoModes(powerChanged: false, init: true);
		powerSettleTimer.Elapsed += OnPowerSettled;
		SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
		SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
		SystemEvents.SessionEnding += SystemEvents_SessionEnding;
		clamshellControl.RegisterDisplayEvents();
		clamshellControl.ToggleLidAction();
		unRegPowerNotify = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, NativeMethods.PowerSettingGuid.ConsoleDisplayState, 0u);
		unRegPowerNotifyLid = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, NativeMethods.PowerSettingGuid.LIDSWITCH_STATE_CHANGE, 0u);
		unRegPowerNotifyEnergy = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, NativeMethods.PowerSettingGuid.EnergySaverStatus, 0u);
		unRegSuspendResume = NativeMethods.RegisterSuspendResumeNotification(settingsForm.Handle, 0u);
		if (Environment.CurrentDirectory.Trim('\\') == Application.StartupPath.Trim('\\') || text.Length > 0)
		{
			SettingsToggle(checkForFocus: false);
		}
		switch (text)
		{
		case "cpu":
			Startup.ReScheduleAdmin();
			settingsForm.FansToggle();
			break;
		case "gpu":
			Startup.ReScheduleAdmin();
			settingsForm.FansToggle(1);
			break;
		case "services":
			settingsForm.extraForm = new Extra();
			settingsForm.extraForm.Show();
			settingsForm.extraForm.ServiesToggle();
			break;
		case "uv":
			Startup.ReScheduleAdmin();
			settingsForm.FansToggle(2);
			modeControl.SetRyzen();
			break;
		case "colors":
			Task.Run(async delegate
			{
				await ColorProfileHelper.InstallProfile();
				settingsForm.Invoke(delegate
				{
					settingsForm.InitVisual();
				});
			});
			break;
		default:
			Task.Run((Action)Startup.StartupCheck);
			break;
		}
		Task.Run(delegate
		{
			settingsForm.VisualiseArmoury(AsusService.IsArmouryRunning());
		});
		MemoryHelper.TrimAfter(null, TimeSpan.FromSeconds(8.0));
		Task.Delay(12000).ContinueWith(delegate
		{
			Logger.WriteLine("Idle Diagnostics: " + Diagnostics.Capture().Summary());
		});
		Application.Run();
	}

	private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
	{
		modeControl.ShutdownReset();
		BatteryControl.AutoBattery();
		InputDispatcher.ShutdownStatusLed();
		XGM.NotifyShutdown();
	}

	private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
	{
		if (e.Reason == SessionSwitchReason.SessionLogon || e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.ConsoleConnect)
		{
			Logger.WriteLine("Session:" + e.Reason);
			ProcessHelper.KillSmartDisplayControl();
			bool sessionLock = Aura.sessionLock;
			Aura.sessionLock = false;
			ScreenControl.AutoScreen();
			Aura.ApplyAura();
			if (sessionLock)
			{
				Task.Delay(2000).ContinueWith(delegate
				{
					if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastAuto) >= 10000)
					{
						modeControl.AutoCPUTemp();
					}
				});
			}
		}
		if (e.Reason == SessionSwitchReason.SessionLock)
		{
			Logger.WriteLine("Session:" + e.Reason);
			Aura.sessionLock = true;
		}
	}

	private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTheme) >= 2000 && e.Category == UserPreferenceCategory.General)
		{
			bool num = settingsForm.InitTheme();
			settingsForm.InitContextMenuTheme();
			settingsForm.VisualiseIcon(themeChange: true);
			settingsForm.VisualiseFnLock();
			settingsForm.VisualiseBatteryFull();
			if (num)
			{
				lastTheme = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}
			if (settingsForm.fansForm != null && settingsForm.fansForm.Text != "")
			{
				settingsForm.fansForm.InitTheme();
			}
			if (settingsForm.extraForm != null && settingsForm.extraForm.Text != "")
			{
				settingsForm.extraForm.InitTheme();
			}
			if (settingsForm.updatesForm != null && settingsForm.updatesForm.Text != "")
			{
				settingsForm.updatesForm.InitTheme();
			}

			if (settingsForm.aboutForm != null && settingsForm.aboutForm.Text != "")
			{
				settingsForm.aboutForm.InitTheme();
			}
		}
	}

	public static bool SetAutoModes(bool powerChanged = false, bool init = false, bool wakeup = false)
	{
		int num = (wakeup ? 10000 : 3000);

		lock (autoLock)
		{
			if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastAuto) < num)
			{
				return false;
			}
			lastAuto = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}
		currentSource = ReadPowerSource();
		Logger.WriteLine("AutoSetting for " + SystemInformation.PowerStatus.PowerLineStatus);
		BatteryControl.AutoBattery(init);
		DynamicLightingHelper.Init();
		ScreenControl.InitOptimalBrightness();
		inputDispatcher.Init();
		modeControl.AutoPerformance(powerChanged);
		InputDispatcher.InitStatusLed();
		if (init)
		{
			NumberPad.Init();
		}
		XGM.Init();
		InputDispatcher.AutoKeyboard();
		ScreenControl.AutoScreen();
		ScreenControl.InitMiniled();
		VisualControl.InitBrightness();
		return true;
	}

	public static PowerSource ReadPowerSource()
	{
		if (SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online)
		{
			return PowerSource.Battery;
		}
		int num = acpi?.DeviceGet(1179756u) ?? 0;
		if (num > 0 && (num & 1) == 0)
		{
			return PowerSource.USBC;
		}
		return PowerSource.Barrel;
	}

	public static int PerformanceKey()
	{
		if (!usbcProfile)
		{
			return (int)SystemInformation.PowerStatus.PowerLineStatus;
		}
		return (int)ReadPowerSource();
	}

	public static void SchedulePowerCheck()
	{
		if (!AppConfig.Is("disable_power_event"))
		{
			powerSettleTimer.Interval = Math.Max(AppConfig.Get("charger_delay"), 2000);
			powerSettleTimer.Stop();
			powerSettleTimer.Start();
		}
	}

	private static void OnPowerSettled(object? sender, ElapsedEventArgs e)
	{
		PowerSource powerSource = ReadPowerSource();
		if (powerSource != currentSource)
		{
			Logger.WriteLine($"Power source: {currentSource} -> {powerSource}");
			currentSource = powerSource;
			SetAutoModes(powerChanged: true);
		}
	}

	public static void OnChargerEvent()
	{
		SchedulePowerCheck();
	}

	private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
	{
		if (e.Mode == PowerModes.Suspend)
		{
			Logger.WriteLine("Power Mode Changed:" + e.Mode);
			modeControl.ShutdownReset();
			InputDispatcher.ShutdownStatusLed();
			XGM.NotifyShutdown();
			return;
		}
		PowerLineStatus powerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;
		if (powerLineStatus != lastLineStatus)
		{
			lastLineStatus = powerLineStatus;
			Logger.WriteLine($"Power Mode {e.Mode}: {powerLineStatus}");
		}
		SchedulePowerCheck();
	}

	public static void SettingsToggle(bool checkForFocus = true, bool trayClick = false)
	{
		if (settingsForm.Visible)
		{
			if (checkForFocus && !settingsForm.HasAnyFocus(trayClick) && !AppConfig.Is("topmost"))
			{
				settingsForm.ShowAll();
			}
			else
			{
				settingsForm.HideAll();
			}
		}
		else
		{
			settingsForm.WindowState = FormWindowState.Normal;
			settingsForm.Show();
			settingsForm.RestoreWindowPosition();
			settingsForm.ShowAll();
		}
	}

	private static void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			SettingsToggle(checkForFocus: true, trayClick: true);
		}
	}

	private static void TrayIcon_MouseMove(object? sender, MouseEventArgs e)
	{
		settingsForm.RefreshSensors();
	}

	private static void OnExit(object sender, EventArgs e)
	{
		if (trayIcon != null)
		{
			trayIcon.Visible = false;
			trayIcon.Dispose();
		}
		clamshellControl.UnregisterDisplayEvents();
		NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
		NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotifyLid);
		NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotifyEnergy);
		NativeMethods.UnregisterSuspendResumeNotification(unRegSuspendResume);
		Application.Exit();
	}

	private static void BatteryLimit()
	{
		try
		{
			int num = AppConfig.Get("charge_limit");
			if (num > 0 && num < 100)
			{
				Logger.WriteLine($"------- Startup Battery Limit {num} -------");
				Logger.WriteLine("Connecting to ACPI");
				acpi = new AsusACPI();
				Logger.WriteLine("Setting Limit");
				if (acpi.IsConnected())
				{
					acpi.DeviceSet(1179735u, num, "Limit");
				}
				else
				{
					AsusACPI.DeviceSetWmi(1179735u, num);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.WriteLine("Startup Battery Limit Error: " + ex.Message);
		}
	}

	private static void CleanupLegacyFiles()
	{
		string path = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
		string[] array = new string[2] { "WinRing0x64.sys", "WinRing0x64.dll" };
		foreach (string text in array)
		{
			string path2 = Path.Combine(path, text);
			if (File.Exists(path2))
			{
				try
				{
					File.Delete(path2);
					Logger.WriteLine("Deleted legacy file: " + text);
				}
				catch (Exception ex)
				{
					Logger.WriteLine("Failed to delete legacy file " + text + ": " + ex.Message);
				}
			}
		}
	}
}

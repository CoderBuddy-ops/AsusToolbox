using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Asus.Display;
using Asus.Helpers;
using Asus.Input;
using Asus.Mode;
using Asus.Properties;
using Asus.UI;
using Asus.USB;
using PawnIO;

namespace Asus;

public class Extra : RForm
{
	private ClamshellModeControl clamshellControl = new ClamshellModeControl();

	private int[] vramOptions = Array.Empty<int>();

	private int coresMinP = 4;

	private int coresMinE;

	private const string EMPTY = "--------------";

	private IContainer components;

	private Panel panelServices;

	private RButton buttonServices;

	private Label labelServices;

	private Panel panelBindingsHeader;

	private RButton buttonResetBindings;

	private Panel panelBindings;

	private TableLayoutPanel tableBindings;

	private Label labelFNC;

	private Label labelFNV;

	private RTextBox textM2;

	private RTextBox textM1;

	private RComboBox comboM1;

	private Label labelM1;

	private RComboBox comboM4;

	private RComboBox comboM3;

	private RTextBox textM4;

	private RTextBox textM3;

	private Label labelM4;

	private Label labelM3;

	private RComboBox comboM5;

	private RTextBox textM5;

	private Label labelM5;

	private Label labelM2;

	private RComboBox comboM2;

	private Label labelFNF4;

	private RComboBox comboFNF4;

	private RTextBox textFNF4;

	private RComboBox comboFNC;

	private RComboBox comboFNV;

	private RTextBox textFNC;

	private RTextBox textFNV;

	private PictureBox pictureHelp;

	private TableLayoutPanel tableKeys;

	private PictureBox pictureBindings;

	private Label labelBindings;

	private Panel panelBacklightHeader;

	private Panel panelBacklight;

	private Panel panelBacklightExtra;

	private NumericUpDownWithUnit numericBacklightPluggedTime;

	private NumericUpDownWithUnit numericBacklightTime;

	private Label labelBacklightTimeout;

	private Label labelSpeed;

	private RComboBox comboKeyboardSpeed;

	private Panel panelXGM;

	private CheckBox checkXGM;

	private TableLayoutPanel tableBacklight;

	private Label labelBacklightKeyboard;

	private CheckBox checkAwake;

	private CheckBox checkBoot;

	private CheckBox checkSleep;

	private Label labelBacklightLogo;

	private CheckBox checkAwakeLogo;

	private CheckBox checkBootLogo;

	private CheckBox checkSleepLogo;

	private Label labelBacklightBar;

	private CheckBox checkAwakeBar;

	private CheckBox checkBootBar;

	private CheckBox checkSleepBar;

	private Label labelBacklightLid;

	private CheckBox checkAwakeLid;

	private CheckBox checkBootLid;

	private CheckBox checkSleepLid;

	private Panel panelSettingsHeader;

	private PictureBox pictureSettings;

	private Label labelSettings;

	private Panel panelSettings;

	private CheckBox checkTopmost;

	private CheckBox checkNoOverdrive;

	private CheckBox checkUSBC;

	private CheckBox checkGpuApps;

	private PictureBox pictureBacklight;

	private Label labelBacklightTitle;

	private PictureBox pictureService;

	private Slider sliderBrightness;

	private PictureBox pictureLog;

	private CheckBox checkAutoToggleClamshellMode;

	private Label labelFNE;

	private RComboBox comboFNE;

	private RTextBox textFNE;

	private Panel panelPower;

	private PictureBox pictureHibernate;

	private Label labelHibernateAfter;

	private NumericUpDownWithUnit numericHibernateAfter;

	private ToolTip toolTip;

	private CheckBox checkBootSound;

	private CheckBox checkKeystoneSound;

	private Panel panelAPU;

	private PictureBox pictureAPUMem;

	private Label labelAPUMem;

	private RComboBox comboAPU;

	private PictureBox pictureScan;

	private Panel panelCores;

	private RComboBox comboCoresE;

	private PictureBox pictureCores;

	private Label labelCores;

	private RComboBox comboCoresP;

	private RButton buttonCores;

	private Panel panelACPI;

	private RTextBox textACPIParam;

	private RTextBox textACPICommand;

	private RButton buttonACPISend;

	private PictureBox pictureDebug;

	private Label labelACPITitle;

	private CheckBox checkStatusLed;

	private CheckBox checkNumberPad;

	private CheckBox checkAspm;

	private CheckBox checkStandbyNetworking;

	private CheckBox checkBatteryLogo;

	private CheckBox checkBattery;

	private CheckBox checkBatteryLid;

	private CheckBox checkBatteryBar;

	private CheckBox checkNVPlatform;

	private Panel panelOptimalBrightness;

	private RComboBox comboOptimalBrightness;

	private PictureBox pictureOptimalBrightness;

	private Label labelOptimalBrightness;

	private void SetKeyCombo(ComboBox combo, TextBox txbox, string name)
	{
		if (combo is RComboBox rComboBox)
		{
			rComboBox.NativeHeight = true;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "", "--------------" },
			{
				"volume_down",
				Strings.VolumeDown
			},
			{
				"volume_up",
				Strings.VolumeUp
			},
			{
				"backlight_down",
				Strings.BacklightDown
			},
			{
				"backlight_up",
				Strings.BacklightUp
			},
			{
				"mute",
				Strings.VolumeMute
			},
			{
				"screenshot",
				Strings.PrintScreen
			},
			{
				"play",
				Strings.PlayPause
			},
			{
				"aura",
				Strings.ToggleAura
			},
			{
				"performance",
				Strings.PerformanceMode
			},
			{
				"screen",
				Strings.ToggleScreen
			},
			{
				"lock",
				Strings.LockScreen
			},
			{
				"miniled",
				Strings.ToggleMiniled
			},
			{
				"fnlock",
				Strings.ToggleFnLock
			},
			{
				"brightness_down",
				Strings.BrightnessDown
			},
			{
				"brightness_up",
				Strings.BrightnessUp
			},
			{
				"visual",
				Strings.VisualMode
			},
			{
				"touchscreen",
				Strings.ToggleTouchscreen
			},
			{
				"micmute",
				Strings.MuteMic
			},
			{
				"asus",
				Strings.OpenAsus
			},
			{
				"overlay",
				Strings.Overlay
			},
			{
				"custom",
				Strings.Custom
			}
		};
		switch (name)
		{
		case "m1":
			dictionary[""] = Strings.VolumeDown;
			dictionary.Remove("volume_down");
			break;
		case "m2":
			dictionary[""] = Strings.VolumeUp;
			dictionary.Remove("volume_up");
			break;
		case "m3":
			dictionary[""] = Strings.MuteMic;
			dictionary.Remove("micmute");
			break;
		case "m4":
			dictionary[""] = Strings.OpenAsus;
			dictionary.Remove("asus");
			break;
		case "m5":
			dictionary[""] = Strings.PerformanceMode;
			dictionary.Remove("performance");
			break;
		case "fnf4":
			dictionary[""] = Strings.ToggleAura;
			dictionary.Remove("aura");
			break;
		case "fnc":
			dictionary[""] = Strings.ToggleFnLock;
			dictionary.Remove("fnlock");
			break;
		case "fnv":
			dictionary[""] = Strings.VisualMode;
			dictionary.Remove("visual");
			break;
		case "fne":
			dictionary[""] = "Calculator";
			break;
		case "paddle":
			dictionary[""] = "--------------";
			break;
		case "cc":
			dictionary[""] = "--------------";
			break;
		}
		combo.DropDownStyle = ComboBoxStyle.DropDownList;
		combo.DataSource = new BindingSource(dictionary, null);
		combo.DisplayMember = "Value";
		combo.ValueMember = "Key";
		string @string = AppConfig.GetString(name);
		combo.SelectedValue = ((@string != null) ? @string : "");
		if (combo.SelectedValue == null)
		{
			combo.SelectedValue = "";
		}
		combo.SelectedValueChanged += delegate
		{
			if (combo.SelectedValue != null)
			{
				AppConfig.Set(name, combo.SelectedValue.ToString());
			}
			if (name == "m1" || name == "m2" || name == "m3" || name == "m4" || name == "m5")
			{
				MKeyControl.ApplyAll();
				Program.inputDispatcher.RegisterKeys();
			}
		};
		txbox.Text = AppConfig.GetString(name + "_custom");
		txbox.TextChanged += delegate
		{
			AppConfig.Set(name + "_custom", txbox.Text);
		};
	}

	public Extra()
	{
		InitializeComponent();
		labelBindings.Text = Strings.KeyBindings;
		buttonResetBindings.Text = Strings.Reset;
		labelBacklightTitle.Text = Strings.LaptopBacklight;
		labelSettings.Text = Strings.Other;
		checkAwake.Text = Strings.Awake;
		checkSleep.Text = Strings.Sleep;
		checkBoot.Text = (checkBootLogo.Text = (checkBootBar.Text = (checkBootLid.Text = Strings.Boot + "/" + Strings.Shutdown)));
		checkBattery.Text = (checkBatteryLogo.Text = (checkBatteryBar.Text = (checkBatteryLid.Text = Strings.Battery)));
		checkBootSound.Text = Strings.BootSound;
		checkKeystoneSound.Text = Strings.KeystoneSound;
		checkStatusLed.Text = Strings.LEDStatusIndicators;
		labelSpeed.Text = Strings.AnimationSpeed;
		labelBacklightTimeout.Text = Strings.BacklightTimeout;
		checkNoOverdrive.Text = Strings.DisableOverdrive;
		checkTopmost.Text = Strings.WindowTop;
		checkUSBC.Text = Strings.OptimizedUSBC;
		checkAutoToggleClamshellMode.Text = Strings.ToggleClamshellMode;
		labelBacklightKeyboard.Text = Strings.Keyboard;
		labelBacklightBar.Text = Strings.Lightbar;
		labelBacklightLid.Text = Strings.Lid;
		labelBacklightLogo.Text = Strings.Logo;
		checkGpuApps.Text = Strings.KillGpuApps;
		checkAspm.Text = Strings.DisablePCIeASPM;
		checkStandbyNetworking.Text = Strings.DisableStandbyNetworking;
		checkNVPlatform.Text = Strings.StopStartNVServices;
		labelHibernateAfter.Text = Strings.HibernateAfter;
		numericHibernateAfter.OffText = Strings.Off;
		numericBacklightTime.OffText = Strings.Off;
		numericBacklightPluggedTime.OffText = Strings.Off;
		labelAPUMem.Text = Strings.APUMemory;
		labelCores.Text = Strings.CPUCoresConfiguration;
		labelOptimalBrightness.Text = Strings.OptimalDisplayBrightness;
		comboOptimalBrightness.Items[0] = Strings.Off;
		comboOptimalBrightness.Items[1] = Strings.OnAlways;
		comboOptimalBrightness.Items[2] = Strings.OnBattery;
		Text = Strings.ExtraSettings;
		panelServices.AccessibleName = Strings.AsusServicesRunning;
		panelBindings.AccessibleName = Strings.KeyBindings;
		tableBindings.AccessibleName = Strings.KeyBindings;
		comboM1.AccessibleName = "M1 Action";
		comboM2.AccessibleName = "M2 Action";
		comboM3.AccessibleName = "M3 Action";
		comboM4.AccessibleName = "M4 Action";
		comboM5.AccessibleName = "M5 Action";
		labelM5.Visible = (comboM5.Visible = (textM5.Visible = false));
		comboFNF4.AccessibleName = "Fn+F4 Action";
		comboFNC.AccessibleName = "Fn+C Action";
		comboFNV.AccessibleName = "Fn+V Action";
		comboFNE.AccessibleName = "Fn+Numpad Action";
		numericBacklightPluggedTime.AccessibleName = Strings.BacklightTimeoutPlugged;
		numericBacklightTime.AccessibleName = Strings.BacklightTimeoutBattery;
		comboKeyboardSpeed.AccessibleName = Strings.LaptopBacklight + " " + Strings.AnimationSpeed;
		comboAPU.AccessibleName = Strings.LaptopBacklight + " " + Strings.AnimationSpeed;
		checkBoot.AccessibleName = Strings.Boot + "/" + Strings.Shutdown + " " + Strings.LaptopBacklight;
		checkAwake.AccessibleName = Strings.Awake + " " + Strings.LaptopBacklight;
		checkSleep.AccessibleName = Strings.Sleep + " " + Strings.LaptopBacklight;
		panelSettings.AccessibleName = Strings.ExtraSettings;
		numericHibernateAfter.AccessibleName = Strings.HibernateAfter;
		if (AppConfig.NoMKeys())
		{
			labelM1.Text = "FN+F2";
			labelM2.Text = "FN+F3";
			labelM3.Text = "FN+F4";
			labelM4.Visible = (comboM4.Visible = (textM4.Visible = false));
			labelFNF4.Visible = (comboFNF4.Visible = (textFNF4.Visible = false));
		}
		if (AppConfig.IsVivoZenPro())
		{
			labelM1.Visible = (comboM1.Visible = (textM1.Visible = false));
			labelM2.Visible = (comboM2.Visible = (textM2.Visible = false));
			labelM3.Visible = (comboM3.Visible = (textM3.Visible = false));
			labelFNF4.Visible = (comboFNF4.Visible = (textFNF4.Visible = false));
			labelM4.Text = "FN+F12";
		}
		labelFNE.Visible = (comboFNE.Visible = (textFNE.Visible = false));
		if (AppConfig.IsNoFNV())
		{
			labelFNV.Visible = (comboFNV.Visible = (textFNV.Visible = false));
		}
		if (!Program.acpi.IsSupported(AsusACPI.GPUEco))
		{
			checkGpuApps.Visible = false;
			checkUSBC.Visible = false;
		}
		checkNoOverdrive.Visible = Program.acpi.IsOverdriveSupported();
		SetKeyCombo(comboM1, textM1, "m1");
		SetKeyCombo(comboM2, textM2, "m2");
		SetKeyCombo(comboM3, textM3, "m3");
		SetKeyCombo(comboM4, textM4, "m4");
		SetKeyCombo(comboFNF4, textFNF4, "fnf4");
		SetKeyCombo(comboFNC, textFNC, "fnc");
		SetKeyCombo(comboFNV, textFNV, "fnv");
		SetKeyCombo(comboFNE, textFNE, "fne");
		InitTheme();
		base.Shown += Keyboard_Shown;
		comboKeyboardSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
		comboKeyboardSpeed.DataSource = new BindingSource(Aura.GetSpeeds(), null);
		comboKeyboardSpeed.DisplayMember = "Value";
		comboKeyboardSpeed.ValueMember = "Key";
		comboKeyboardSpeed.SelectedValue = Aura.Speed;
		comboKeyboardSpeed.SelectedValueChanged += ComboKeyboardSpeed_SelectedValueChanged;
		checkAwake.Checked = AppConfig.IsNotFalse("keyboard_awake");
		checkBattery.Checked = AppConfig.IsOnBattery("keyboard_awake");
		checkBoot.Checked = AppConfig.IsNotFalse("keyboard_boot");
		checkSleep.Checked = AppConfig.IsNotFalse("keyboard_sleep");
		checkAwakeBar.Checked = AppConfig.IsNotFalse("keyboard_awake_bar");
		checkBatteryBar.Checked = AppConfig.IsOnBattery("keyboard_awake_bar");
		checkBootBar.Checked = AppConfig.IsNotFalse("keyboard_boot_bar");
		checkSleepBar.Checked = AppConfig.IsNotFalse("keyboard_sleep_bar");
		checkAwakeLid.Checked = AppConfig.IsNotFalse("keyboard_awake_lid");
		checkBatteryLid.Checked = AppConfig.IsOnBattery("keyboard_awake_lid");
		checkBootLid.Checked = AppConfig.IsNotFalse("keyboard_boot_lid");
		checkSleepLid.Checked = AppConfig.IsNotFalse("keyboard_sleep_lid");
		checkAwakeLogo.Checked = AppConfig.IsNotFalse("keyboard_awake_logo");
		checkBatteryLogo.Checked = AppConfig.IsOnBattery("keyboard_awake_logo");
		checkBootLogo.Checked = AppConfig.IsNotFalse("keyboard_boot_logo");
		checkSleepLogo.Checked = AppConfig.IsNotFalse("keyboard_sleep_logo");
		checkAwake.CheckedChanged += CheckPower_CheckedChanged;
		checkBattery.CheckedChanged += CheckPower_CheckedChanged;
		checkBoot.CheckedChanged += CheckPower_CheckedChanged;
		checkSleep.CheckedChanged += CheckPower_CheckedChanged;
		checkAwakeBar.CheckedChanged += CheckPower_CheckedChanged;
		checkBatteryBar.CheckedChanged += CheckPower_CheckedChanged;
		checkBootBar.CheckedChanged += CheckPower_CheckedChanged;
		checkSleepBar.CheckedChanged += CheckPower_CheckedChanged;
		checkAwakeLid.CheckedChanged += CheckPower_CheckedChanged;
		checkBatteryLid.CheckedChanged += CheckPower_CheckedChanged;
		checkBootLid.CheckedChanged += CheckPower_CheckedChanged;
		checkSleepLid.CheckedChanged += CheckPower_CheckedChanged;
		checkAwakeLogo.CheckedChanged += CheckPower_CheckedChanged;
		checkBatteryLogo.CheckedChanged += CheckPower_CheckedChanged;
		checkBootLogo.CheckedChanged += CheckPower_CheckedChanged;
		checkSleepLogo.CheckedChanged += CheckPower_CheckedChanged;
		labelBacklightKeyboard.Visible = false;
		checkBattery.Visible = false;
		if (!Aura.HasLightbar)
		{
			labelBacklightBar.Visible = false;
			checkAwakeBar.Visible = false;
			checkBatteryBar.Visible = false;
			checkBootBar.Visible = false;
			checkSleepBar.Visible = false;
		}
		if (!Aura.HasLogo)
		{
			labelBacklightLogo.Visible = false;
			checkAwakeLogo.Visible = false;
			checkBatteryLogo.Visible = false;
			checkBootLogo.Visible = false;
			checkSleepLogo.Visible = false;
		}
		if (!Aura.HasRearglow)
		{
			labelBacklightLid.Visible = false;
			checkAwakeLid.Visible = false;
			checkBatteryLid.Visible = false;
			checkBootLid.Visible = false;
			checkSleepLid.Visible = false;
		}
		checkAutoToggleClamshellMode.Checked = AppConfig.Is("toggle_clamshell_mode");
		checkAutoToggleClamshellMode.CheckedChanged += checkAutoToggleClamshellMode_CheckedChanged;
		checkTopmost.Checked = AppConfig.Is("topmost");
		checkTopmost.CheckedChanged += CheckTopmost_CheckedChanged;
		checkNoOverdrive.Checked = AppConfig.IsNoOverdrive();
		checkNoOverdrive.CheckedChanged += CheckNoOverdrive_CheckedChanged;
		checkUSBC.Checked = AppConfig.Is("optimized_usbc");
		checkUSBC.CheckedChanged += CheckUSBC_CheckedChanged;
		sliderBrightness.Value = InputDispatcher.GetBacklight();
		sliderBrightness.AccessibleName = Strings.LaptopBacklight + ": " + sliderBrightness.Value;
		sliderBrightness.ValueChanged += SliderBrightness_ValueChanged;
		panelXGM.Visible = XGM.IsConnected();
		checkXGM.Checked = AppConfig.Get("xmg_light") != 0;
		checkXGM.CheckedChanged += CheckXGM_CheckedChanged;
		numericBacklightTime.Value = AppConfig.Get("keyboard_timeout", 60);
		numericBacklightPluggedTime.Value = AppConfig.Get("keyboard_ac_timeout", 0);
		numericBacklightTime.ValueChanged += NumericBacklightTime_ValueChanged;
		numericBacklightPluggedTime.ValueChanged += NumericBacklightTime_ValueChanged;
		checkGpuApps.Checked = AppConfig.Is("kill_gpu_apps");
		checkGpuApps.CheckedChanged += CheckGpuApps_CheckedChanged;
		int num = Program.acpi.DeviceGet(1245218u);
		checkBootSound.Visible = num >= 0;
		if (num < 0 || num > 65535)
		{
			num = AppConfig.Get("boot_sound", 0);
		}
		checkBootSound.Checked = num == 1;
		checkBootSound.CheckedChanged += CheckBootSound_CheckedChanged;
		int num2 = Program.acpi.DeviceGet(393410u);
		checkStatusLed.Visible = num2 >= 0;
		checkStatusLed.Checked = num2 > 0;
		checkStatusLed.CheckedChanged += CheckLEDStatus_CheckedChanged;
		int num3 = (AppConfig.IsNumberPad() ? NumberPad.Get() : (-1));
		checkNumberPad.Visible = num3 >= 0;
		checkNumberPad.Checked = num3 == 1;
		checkNumberPad.CheckedChanged += CheckNumberPad_CheckedChanged;
		int optimalBrightness = ScreenControl.GetOptimalBrightness();
		if (optimalBrightness >= 0)
		{
			panelOptimalBrightness.Visible = true;
			comboOptimalBrightness.DropDownStyle = ComboBoxStyle.DropDownList;
			comboOptimalBrightness.SelectedIndex = AppConfig.Get("optimal_brightness", optimalBrightness);
			comboOptimalBrightness.SelectedIndexChanged += OptimalBrightness_Changed;
		}
		pictureHelp.Click += PictureHelp_Click;
		buttonResetBindings.Click += ButtonResetBindings_Click;
		buttonServices.Click += ButtonServices_Click;
		pictureLog.Click += PictureLog_Click;
		checkNVPlatform.Visible = Program.acpi.IsNVidiaGPU();
		checkNVPlatform.Checked = AppConfig.IsNVPlatform();
		checkNVPlatform.CheckedChanged += CheckNVPlatform_CheckedChanged;
		checkAspm.Checked = AppConfig.IsAutoASPM();
		checkAspm.CheckedChanged += CheckAspm_CheckedChanged;
		checkStandbyNetworking.Checked = AppConfig.IsAutoStandbyNetworking();
		checkStandbyNetworking.CheckedChanged += CheckStandbyNetworking_CheckedChanged;
		checkKeystoneSound.Visible = AppConfig.IsKeystone();
		checkKeystoneSound.Checked = Keystone.IsEnabled();
		checkKeystoneSound.CheckedChanged += CheckKeystoneSoundCheckedChanged;
		toolTip.SetToolTip(checkAutoToggleClamshellMode, Strings.ClamshellModeTooltip);
		toolTip.SetToolTip(checkNVPlatform, Strings.NVPlatformTooltip);
		toolTip.SetToolTip(checkAspm, Strings.DisablePCIeASPMTooltip);
		toolTip.SetToolTip(checkStandbyNetworking, Strings.DisableStandbyNetworkingTooltip);
		InitCores();
		InitServices();
		InitHibernate();
		InitVramMem();
		InitACPITesting();
	}

	private void CheckKeystoneSoundCheckedChanged(object? sender, EventArgs e)
	{
		Keystone.SetEnabled(checkKeystoneSound.Checked);
	}

	private void CheckAspm_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("aspm", checkAspm.Checked ? 1 : 0);
		PowerNative.SetBalancedASPM((!checkAspm.Checked) ? 2 : 0);
	}

	private void CheckStandbyNetworking_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("standby_networking", checkStandbyNetworking.Checked ? 1 : 0);
		if (checkStandbyNetworking.Checked)
		{
			PowerNative.SetConnectivityInStandby();
		}
		else
		{
			PowerNative.SetConnectivityInStandby(1, 2);
		}
	}

	private void CheckNVPlatform_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("nv_platform", checkNVPlatform.Checked ? 1 : 0);
	}

	private void OptimalBrightness_Changed(object? sender, EventArgs e)
	{
		ScreenControl.SetOptimalBrightness(comboOptimalBrightness.SelectedIndex);
	}

	private void CheckLEDStatus_CheckedChanged(object? sender, EventArgs e)
	{
		InputDispatcher.SetStatusLED(checkStatusLed.Checked);
	}

	private void CheckNumberPad_CheckedChanged(object? sender, EventArgs e)
	{
		NumberPad.Set(checkNumberPad.Checked);
	}

	private void InitACPITesting()
	{
		pictureScan.Visible = true;
		pictureScan.Click += PictureScan_Click;
		if (AppConfig.Is("debug"))
		{
			panelACPI.Visible = true;
			textACPICommand.Text = "110034";
			textACPIParam.Text = "0x0303";
			buttonACPISend.Click += ButtonACPISend_Click;
		}
	}

	private void ButtonACPISend_Click(object? sender, EventArgs e)
	{
		try
		{
			int deviceID = Convert.ToInt32(textACPICommand.Text, 16);
			int status = Convert.ToInt32(textACPIParam.Text, textACPIParam.Text.Contains("x") ? 16 : 10);
			int num = Program.acpi.DeviceSet((uint)deviceID, status, "TestACPI " + deviceID.ToString("X8") + " " + status.ToString("X4"));
			labelACPITitle.Text = "ACPI DEVS Test : " + num;
		}
		catch (Exception ex)
		{
			Logger.WriteLine(ex.Message);
		}
	}

	private void InitCores()
	{
		var (num, num2) = Program.acpi.GetCores();
		int num4;
		int num3;
		(num3, num4) = Program.acpi.GetCores(1179859u);
		if (num < 0 || num2 < 0 || num3 < 0 || num4 < 0)
		{
			panelCores.Visible = false;
			return;
		}
		if (num3 == 0)
		{
			num3 = 8;
		}
		if (num4 == 0)
		{
			num4 = 6;
		}
		if (AppConfig.Is8Ecores())
		{
			num3 = Math.Max(8, num3);
		}
		num3 = Math.Max(4, num3);
		var (val, num5) = Program.acpi.GetCores(1179860u);
		if (num5 >= 1)
		{
			coresMinP = Math.Min(num5, num4);
			coresMinE = Math.Min(val, num3);
		}
		panelCores.Visible = true;
		comboCoresE.DropDownStyle = ComboBoxStyle.DropDownList;
		comboCoresP.DropDownStyle = ComboBoxStyle.DropDownList;
		for (int i = coresMinP; i <= num4; i++)
		{
			comboCoresP.Items.Add(i + " Pcores");
		}
		for (int j = coresMinE; j <= num3; j++)
		{
			comboCoresE.Items.Add(j + " Ecores");
		}
		comboCoresP.SelectedIndex = Math.Max(Math.Min(num2 - coresMinP, comboCoresP.Items.Count - 1), 0);
		comboCoresE.SelectedIndex = Math.Max(Math.Min(num - coresMinE, comboCoresE.Items.Count - 1), 0);
		buttonCores.Click += ButtonCores_Click;
	}

	private void ButtonCores_Click(object? sender, EventArgs e)
	{
		if (MessageBox.Show(Strings.AlertAPUMemoryRestart, Strings.AlertAPUMemoryRestartTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			Program.acpi.SetCores(coresMinE + comboCoresE.SelectedIndex, coresMinP + comboCoresP.SelectedIndex);
			Process.Start("shutdown", "/r /t 1");
		}
	}

	private void PictureScan_Click(object? sender, EventArgs e)
	{
		string fileName = Program.acpi.ScanRange();
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(fileName)
		{
			UseShellExecute = true
		};
		process.Start();
	}

	private void InitVramMem()
	{
		int unitMb = 0;
		if (CpuInfo.IsAMD)
		{
			vramOptions = Program.acpi.GetVramOptions(out unitMb);
		}
		if (vramOptions.Length != 0)
		{
			comboAPU.Items.Clear();
			int[] array = vramOptions;
			foreach (int num in array)
			{
				comboAPU.Items.Add((num == 0) ? Strings.AutoMode : (((double)num * (double)unitMb / 1024.0).ToString("0.#") + "G"));
			}
			int num2 = Program.acpi.GetVramMem();
			if (num2 == 0)
			{
				num2 = AppConfig.Get("vram_mem", 0);
			}
			comboAPU.SelectedIndex = Math.Max(0, Array.IndexOf(vramOptions, num2));
		}
		else
		{
			int aPUMem = Program.acpi.GetAPUMem();
			if (aPUMem < 0)
			{
				return;
			}
			comboAPU.SelectedIndex = aPUMem;
		}
		panelAPU.Visible = true;
		comboAPU.DropDownStyle = ComboBoxStyle.DropDownList;
		comboAPU.SelectedIndexChanged += ComboAPU_SelectedIndexChanged;
	}

	private void ComboAPU_SelectedIndexChanged(object? sender, EventArgs e)
	{
		int selectedIndex = comboAPU.SelectedIndex;
		if (vramOptions.Length == 0)
		{
			Program.acpi.SetAPUMem(selectedIndex);
		}
		else
		{
			Program.acpi.SetVramMem(vramOptions[selectedIndex]);
			AppConfig.Set("vram_mem", vramOptions[selectedIndex]);
		}
		if (MessageBox.Show(Strings.AlertAPUMemoryRestart, Strings.AlertAPUMemoryRestartTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			Process.Start("shutdown", "/r /t 1");
		}
	}

	private void CheckBootSound_CheckedChanged(object? sender, EventArgs e)
	{
		int num = (checkBootSound.Checked ? 1 : 0);
		Program.acpi.DeviceSet(1245218u, num, "BootSound");
		AppConfig.Set("boot_sound", num);
	}

	private void InitHibernate()
	{
		try
		{
			int num = PowerNative.GetHibernateAfter();
			if (num < 0 || (decimal)num > numericHibernateAfter.Maximum)
			{
				num = 0;
			}
			numericHibernateAfter.Value = num;
			numericHibernateAfter.ValueChanged += NumericHibernateAfter_ValueChanged;
		}
		catch (Exception ex)
		{
			panelPower.Visible = false;
			Logger.WriteLine(ex.ToString());
		}
	}

	private void NumericHibernateAfter_ValueChanged(object? sender, EventArgs e)
	{
		PowerNative.SetHibernateAfter((int)numericHibernateAfter.Value);
	}

	private void PictureLog_Click(object? sender, EventArgs e)
	{
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(Logger.logFile)
		{
			UseShellExecute = true
		};
		process.Start();
	}

	private void SliderBrightness_ValueChanged(object? sender, EventArgs e)
	{
		if (SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online)
		{
			AppConfig.Set("keyboard_brightness_ac", sliderBrightness.Value);
		}
		else
		{
			AppConfig.Set("keyboard_brightness", sliderBrightness.Value);
		}
		Aura.ApplyBrightness(sliderBrightness.Value, "Slider");
		sliderBrightness.AccessibleName = Strings.LaptopBacklight + ": " + sliderBrightness.Value;
	}

	public void VisualiseBacklight(int backlight)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseBacklight(backlight);
			});
			return;
		}
		sliderBrightness.ValueChanged -= SliderBrightness_ValueChanged;
		sliderBrightness.Value = backlight;
		sliderBrightness.AccessibleName = Strings.LaptopBacklight + ": " + sliderBrightness.Value;
		sliderBrightness.ValueChanged += SliderBrightness_ValueChanged;
	}

	private void InitServices()
	{
		int runningCount = AsusService.GetRunningCount();
		if (runningCount > 0)
		{
			buttonServices.Text = Strings.Stop;
			labelServices.ForeColor = RForm.colorTurbo;
		}
		else
		{
			buttonServices.Text = Strings.Start;
			labelServices.ForeColor = RForm.colorStandard;
		}
		labelServices.Text = Strings.AsusServicesRunning + ":  " + runningCount;
		buttonServices.Enabled = true;
	}

	public void ServiesToggle()
	{
		buttonServices.Enabled = false;
		if (AsusService.GetRunningCount() > 0)
		{
			labelServices.Text = Strings.StoppingServices + " ...";
			Task.Run(delegate
			{
				AsusService.StopAsusServices();
				Program.inputDispatcher.Init();
				BeginInvoke(delegate
				{
					InitServices();
				});
			});
			return;
		}
		labelServices.Text = Strings.StartingServices + " ...";
		Task.Run(delegate
		{
			AsusService.StartAsusServices();
			BeginInvoke(delegate
			{
				InitServices();
			});
		});
	}

	private void ButtonServices_Click(object? sender, EventArgs e)
	{
		if (ProcessHelper.IsUserAdministrator())
		{
			ServiesToggle();
		}
		else
		{
			ProcessHelper.RunAsAdmin("services");
		}
	}

	private void CheckGpuApps_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("kill_gpu_apps", checkGpuApps.Checked ? 1 : 0);
	}

	private void NumericBacklightTime_ValueChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("keyboard_timeout", (int)numericBacklightTime.Value);
		AppConfig.Set("keyboard_ac_timeout", (int)numericBacklightPluggedTime.Value);
		Program.inputDispatcher.InitBacklightTimer();
	}

	private void CheckXGM_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("xmg_light", checkXGM.Checked ? 1 : 0);
		XGM.Light(checkXGM.Checked);
	}

	private void CheckUSBC_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("optimized_usbc", checkUSBC.Checked ? 1 : 0);
	}

	private void PictureHelp_Click(object? sender, EventArgs e)
	{
		Process.Start(new ProcessStartInfo("https://github.com/seerge/g-helper/wiki/Power-user-settings#custom-hotkey-actions")
		{
			UseShellExecute = true
		});
	}

	private void ButtonResetBindings_Click(object? sender, EventArgs e)
	{
		comboM1.SelectedValue = "";
		comboM2.SelectedValue = "";
		comboM3.SelectedValue = "";
		comboM4.SelectedValue = "";
		comboM5.SelectedValue = "";
		RTextBox rTextBox = textM1;
		RTextBox rTextBox2 = textM2;
		RTextBox rTextBox3 = textM3;
		RTextBox rTextBox4 = textM4;
		string text2 = (textM5.Text = "");
		string text4 = (rTextBox4.Text = text2);
		string text6 = (rTextBox3.Text = text4);
		string text8 = (rTextBox2.Text = text6);
		rTextBox.Text = text8;
		MKeyControl.Reset();
		Program.inputDispatcher.RegisterKeys();
	}

	private void CheckNoOverdrive_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("no_overdrive", checkNoOverdrive.Checked ? 1 : 0);
		ScreenControl.AutoScreen(force: true);
	}

	private void CheckTopmost_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("topmost", checkTopmost.Checked ? 1 : 0);
		Program.settingsForm.TopMost = checkTopmost.Checked;
	}

	private void CheckPower_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("keyboard_awake", checkAwake.Checked ? 1 : 0);
		AppConfig.Set("keyboard_boot", checkBoot.Checked ? 1 : 0);
		AppConfig.Set("keyboard_sleep", checkSleep.Checked ? 1 : 0);
		AppConfig.Set("keyboard_shutdown", checkBoot.Checked ? 1 : 0);
		AppConfig.Set("keyboard_awake_bar", checkAwakeBar.Checked ? 1 : 0);
		AppConfig.Set("keyboard_boot_bar", checkBootBar.Checked ? 1 : 0);
		AppConfig.Set("keyboard_sleep_bar", checkSleepBar.Checked ? 1 : 0);
		AppConfig.Set("keyboard_shutdown_bar", checkBootBar.Checked ? 1 : 0);
		AppConfig.Set("keyboard_awake_lid", checkAwakeLid.Checked ? 1 : 0);
		AppConfig.Set("keyboard_boot_lid", checkBootLid.Checked ? 1 : 0);
		AppConfig.Set("keyboard_sleep_lid", checkSleepLid.Checked ? 1 : 0);
		AppConfig.Set("keyboard_shutdown_lid", checkBootLid.Checked ? 1 : 0);
		AppConfig.Set("keyboard_awake_logo", checkAwakeLogo.Checked ? 1 : 0);
		AppConfig.Set("keyboard_boot_logo", checkBootLogo.Checked ? 1 : 0);
		AppConfig.Set("keyboard_sleep_logo", checkSleepLogo.Checked ? 1 : 0);
		AppConfig.Set("keyboard_shutdown_logo", checkBootLogo.Checked ? 1 : 0);
		Aura.ApplyPower();
		if (Aura.IsOldStrix)
		{
			Aura.ApplyAura();
		}
	}

	private void ComboKeyboardSpeed_SelectedValueChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("aura_speed", (int)comboKeyboardSpeed.SelectedValue);
		Aura.ApplyAura();
	}

	private void Keyboard_Shown(object? sender, EventArgs e)
	{
		if (base.Height > Program.settingsForm.Height)
		{
			int num = Program.settingsForm.Top + Program.settingsForm.Height - base.Height;
			if (num < 0)
			{
				MaximumSize = new Size(base.Width, Program.settingsForm.Height);
				base.Top = Program.settingsForm.Top;
			}
			else
			{
				base.Top = num;
			}
		}
		else
		{
			base.Top = Program.settingsForm.Top;
		}
		base.Left = Program.settingsForm.Left - base.Width - 5;
	}

	private void checkAutoToggleClamshellMode_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("toggle_clamshell_mode", checkAutoToggleClamshellMode.Checked ? 1 : 0);
		if (checkAutoToggleClamshellMode.Checked)
		{
			clamshellControl.ToggleLidAction();
		}
		else
		{
			ClamshellModeControl.DisableClamshellMode();
		}
	}

	private void panelAPU_Paint(object sender, PaintEventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.panelServices = new System.Windows.Forms.Panel();
		this.pictureService = new System.Windows.Forms.PictureBox();
		this.labelServices = new System.Windows.Forms.Label();
		this.buttonServices = new Asus.UI.RButton();
		this.panelBindingsHeader = new System.Windows.Forms.Panel();
		this.buttonResetBindings = new Asus.UI.RButton();
		this.pictureBindings = new System.Windows.Forms.PictureBox();
		this.pictureHelp = new System.Windows.Forms.PictureBox();
		this.labelBindings = new System.Windows.Forms.Label();
		this.panelBindings = new System.Windows.Forms.Panel();
		this.tableBindings = new System.Windows.Forms.TableLayoutPanel();
		this.labelFNE = new System.Windows.Forms.Label();
		this.comboFNE = new Asus.UI.RComboBox();
		this.textFNE = new Asus.UI.RTextBox();
		this.labelFNV = new System.Windows.Forms.Label();
		this.comboFNV = new Asus.UI.RComboBox();
		this.textFNV = new Asus.UI.RTextBox();
		this.labelFNC = new System.Windows.Forms.Label();
		this.comboFNC = new Asus.UI.RComboBox();
		this.textFNC = new Asus.UI.RTextBox();
		this.labelFNF4 = new System.Windows.Forms.Label();
		this.comboFNF4 = new Asus.UI.RComboBox();
		this.textFNF4 = new Asus.UI.RTextBox();
		this.comboM4 = new Asus.UI.RComboBox();
		this.textM4 = new Asus.UI.RTextBox();
		this.labelM4 = new System.Windows.Forms.Label();
		this.comboM5 = new Asus.UI.RComboBox();
		this.textM5 = new Asus.UI.RTextBox();
		this.labelM5 = new System.Windows.Forms.Label();
		this.comboM3 = new Asus.UI.RComboBox();
		this.textM3 = new Asus.UI.RTextBox();
		this.labelM3 = new System.Windows.Forms.Label();
		this.textM2 = new Asus.UI.RTextBox();
		this.labelM2 = new System.Windows.Forms.Label();
		this.comboM2 = new Asus.UI.RComboBox();
		this.textM1 = new Asus.UI.RTextBox();
		this.comboM1 = new Asus.UI.RComboBox();
		this.labelM1 = new System.Windows.Forms.Label();
		this.tableKeys = new System.Windows.Forms.TableLayoutPanel();
		this.panelBacklightHeader = new System.Windows.Forms.Panel();
		this.sliderBrightness = new Asus.UI.Slider();
		this.pictureBacklight = new System.Windows.Forms.PictureBox();
		this.labelBacklightTitle = new System.Windows.Forms.Label();
		this.panelBacklight = new System.Windows.Forms.Panel();
		this.panelBacklightExtra = new System.Windows.Forms.Panel();
		this.numericBacklightPluggedTime = new NumericUpDownWithUnit();
		this.numericBacklightTime = new NumericUpDownWithUnit();
		this.labelBacklightTimeout = new System.Windows.Forms.Label();
		this.labelSpeed = new System.Windows.Forms.Label();
		this.comboKeyboardSpeed = new Asus.UI.RComboBox();
		this.panelXGM = new System.Windows.Forms.Panel();
		this.checkXGM = new System.Windows.Forms.CheckBox();
		this.tableBacklight = new System.Windows.Forms.TableLayoutPanel();
		this.labelBacklightKeyboard = new System.Windows.Forms.Label();
		this.checkAwake = new System.Windows.Forms.CheckBox();
		this.checkBoot = new System.Windows.Forms.CheckBox();
		this.checkSleep = new System.Windows.Forms.CheckBox();
		this.checkBattery = new System.Windows.Forms.CheckBox();
		this.labelBacklightLogo = new System.Windows.Forms.Label();
		this.checkAwakeLogo = new System.Windows.Forms.CheckBox();
		this.checkBootLogo = new System.Windows.Forms.CheckBox();
		this.checkSleepLogo = new System.Windows.Forms.CheckBox();
		this.checkBatteryLogo = new System.Windows.Forms.CheckBox();
		this.labelBacklightBar = new System.Windows.Forms.Label();
		this.checkAwakeBar = new System.Windows.Forms.CheckBox();
		this.checkBootBar = new System.Windows.Forms.CheckBox();
		this.checkSleepBar = new System.Windows.Forms.CheckBox();
		this.checkBatteryBar = new System.Windows.Forms.CheckBox();
		this.labelBacklightLid = new System.Windows.Forms.Label();
		this.checkAwakeLid = new System.Windows.Forms.CheckBox();
		this.checkBootLid = new System.Windows.Forms.CheckBox();
		this.checkSleepLid = new System.Windows.Forms.CheckBox();
		this.checkBatteryLid = new System.Windows.Forms.CheckBox();
		this.panelSettingsHeader = new System.Windows.Forms.Panel();
		this.pictureScan = new System.Windows.Forms.PictureBox();
		this.pictureLog = new System.Windows.Forms.PictureBox();
		this.pictureSettings = new System.Windows.Forms.PictureBox();
		this.labelSettings = new System.Windows.Forms.Label();
		this.panelSettings = new System.Windows.Forms.Panel();
		this.checkAutoToggleClamshellMode = new System.Windows.Forms.CheckBox();
		this.checkTopmost = new System.Windows.Forms.CheckBox();
		this.checkNoOverdrive = new System.Windows.Forms.CheckBox();
		this.checkBootSound = new System.Windows.Forms.CheckBox();
		this.checkKeystoneSound = new System.Windows.Forms.CheckBox();
		this.checkUSBC = new System.Windows.Forms.CheckBox();
		this.checkGpuApps = new System.Windows.Forms.CheckBox();
		this.checkNVPlatform = new System.Windows.Forms.CheckBox();
		this.checkStatusLed = new System.Windows.Forms.CheckBox();
		this.checkNumberPad = new System.Windows.Forms.CheckBox();
		this.checkAspm = new System.Windows.Forms.CheckBox();
		this.checkStandbyNetworking = new System.Windows.Forms.CheckBox();
		this.panelPower = new System.Windows.Forms.Panel();
		this.numericHibernateAfter = new NumericUpDownWithUnit();
		this.labelHibernateAfter = new System.Windows.Forms.Label();
		this.pictureHibernate = new System.Windows.Forms.PictureBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.panelAPU = new System.Windows.Forms.Panel();
		this.comboAPU = new Asus.UI.RComboBox();
		this.pictureAPUMem = new System.Windows.Forms.PictureBox();
		this.labelAPUMem = new System.Windows.Forms.Label();
		this.panelCores = new System.Windows.Forms.Panel();
		this.buttonCores = new Asus.UI.RButton();
		this.comboCoresP = new Asus.UI.RComboBox();
		this.comboCoresE = new Asus.UI.RComboBox();
		this.pictureCores = new System.Windows.Forms.PictureBox();
		this.labelCores = new System.Windows.Forms.Label();
		this.panelACPI = new System.Windows.Forms.Panel();
		this.textACPIParam = new Asus.UI.RTextBox();
		this.textACPICommand = new Asus.UI.RTextBox();
		this.buttonACPISend = new Asus.UI.RButton();
		this.pictureDebug = new System.Windows.Forms.PictureBox();
		this.labelACPITitle = new System.Windows.Forms.Label();
		this.panelOptimalBrightness = new System.Windows.Forms.Panel();
		this.comboOptimalBrightness = new Asus.UI.RComboBox();
		this.pictureOptimalBrightness = new System.Windows.Forms.PictureBox();
		this.labelOptimalBrightness = new System.Windows.Forms.Label();
		this.panelServices.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureService).BeginInit();
		this.panelBindingsHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBindings).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHelp).BeginInit();
		this.panelBindings.SuspendLayout();
		this.tableBindings.SuspendLayout();
		this.panelBacklightHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBacklight).BeginInit();
		this.panelBacklight.SuspendLayout();
		this.panelBacklightExtra.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericBacklightPluggedTime).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericBacklightTime).BeginInit();
		this.panelXGM.SuspendLayout();
		this.tableBacklight.SuspendLayout();
		this.panelSettingsHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureScan).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureLog).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureSettings).BeginInit();
		this.panelSettings.SuspendLayout();
		this.panelPower.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericHibernateAfter).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHibernate).BeginInit();
		this.panelAPU.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureAPUMem).BeginInit();
		this.panelCores.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureCores).BeginInit();
		this.panelACPI.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureDebug).BeginInit();
		this.panelOptimalBrightness.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureOptimalBrightness).BeginInit();
		base.SuspendLayout();
		this.panelServices.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelServices.Controls.Add(this.pictureService);
		this.panelServices.Controls.Add(this.labelServices);
		this.panelServices.Controls.Add(this.buttonServices);
		this.panelServices.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelServices.Location = new System.Drawing.Point(15, 1778);
		this.panelServices.Name = "panelServices";
		this.panelServices.Size = new System.Drawing.Size(949, 75);
		this.panelServices.TabIndex = 5;
		this.pictureService.BackgroundImage = Asus.Properties.Resources.icons8_automation_32;
		this.pictureService.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureService.Location = new System.Drawing.Point(21, 19);
		this.pictureService.Name = "pictureService";
		this.pictureService.Size = new System.Drawing.Size(32, 32);
		this.pictureService.TabIndex = 21;
		this.pictureService.TabStop = false;
		this.labelServices.AutoSize = true;
		this.labelServices.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelServices.Location = new System.Drawing.Point(57, 19);
		this.labelServices.Name = "labelServices";
		this.labelServices.Size = new System.Drawing.Size(273, 32);
		this.labelServices.TabIndex = 20;
		this.labelServices.Text = "Asus Services Running";
		this.buttonServices.Activated = false;
		this.buttonServices.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonServices.BackColor = System.Drawing.SystemColors.ButtonHighlight;
		this.buttonServices.BorderColor = System.Drawing.Color.Transparent;
		this.buttonServices.BorderRadius = 2;
		this.buttonServices.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonServices.Location = new System.Drawing.Point(713, 11);
		this.buttonServices.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.buttonServices.Name = "buttonServices";
		this.buttonServices.Secondary = false;
		this.buttonServices.Size = new System.Drawing.Size(256, 53);
		this.buttonServices.TabIndex = 19;
		this.buttonServices.Text = "Start Services";
		this.buttonServices.UseVisualStyleBackColor = false;
		this.panelBindingsHeader.AutoSize = true;
		this.panelBindingsHeader.BackColor = System.Drawing.SystemColors.ControlLight;
		this.panelBindingsHeader.Controls.Add(this.buttonResetBindings);
		this.panelBindingsHeader.Controls.Add(this.pictureBindings);
		this.panelBindingsHeader.Controls.Add(this.pictureHelp);
		this.panelBindingsHeader.Controls.Add(this.labelBindings);
		this.panelBindingsHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBindingsHeader.Location = new System.Drawing.Point(15, 15);
		this.panelBindingsHeader.Name = "panelBindingsHeader";
		this.panelBindingsHeader.Padding = new System.Windows.Forms.Padding(11, 5, 11, 5);
		this.panelBindingsHeader.Size = new System.Drawing.Size(949, 51);
		this.panelBindingsHeader.TabIndex = 4;
		this.pictureBindings.BackgroundImage = Asus.Properties.Resources.icons8_keyboard_32;
		this.pictureBindings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBindings.Location = new System.Drawing.Point(21, 11);
		this.pictureBindings.Name = "pictureBindings";
		this.pictureBindings.Size = new System.Drawing.Size(32, 32);
		this.pictureBindings.TabIndex = 1;
		this.pictureBindings.TabStop = false;
		this.buttonResetBindings.Activated = false;
		this.buttonResetBindings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonResetBindings.BackColor = System.Drawing.SystemColors.ButtonHighlight;
		this.buttonResetBindings.BorderColor = System.Drawing.Color.Transparent;
		this.buttonResetBindings.BorderRadius = 2;
		this.buttonResetBindings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonResetBindings.Location = new System.Drawing.Point(727, 5);
		this.buttonResetBindings.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.buttonResetBindings.Name = "buttonResetBindings";
		this.buttonResetBindings.Secondary = true;
		this.buttonResetBindings.Size = new System.Drawing.Size(150, 41);
		this.buttonResetBindings.TabIndex = 12;
		this.buttonResetBindings.Text = "Reset";
		this.buttonResetBindings.UseVisualStyleBackColor = false;
		this.pictureHelp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.pictureHelp.BackgroundImage = Asus.Properties.Resources.icons8_help_32;
		this.pictureHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureHelp.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureHelp.Location = new System.Drawing.Point(897, 11);
		this.pictureHelp.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.pictureHelp.Name = "pictureHelp";
		this.pictureHelp.Size = new System.Drawing.Size(32, 32);
		this.pictureHelp.TabIndex = 11;
		this.pictureHelp.TabStop = false;
		this.labelBindings.AutoSize = true;
		this.labelBindings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBindings.Location = new System.Drawing.Point(56, 8);
		this.labelBindings.Name = "labelBindings";
		this.labelBindings.Size = new System.Drawing.Size(114, 32);
		this.labelBindings.TabIndex = 0;
		this.labelBindings.Text = "Bindings";
		this.panelBindings.AutoSize = true;
		this.panelBindings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelBindings.Controls.Add(this.tableBindings);
		this.panelBindings.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBindings.Location = new System.Drawing.Point(15, 66);
		this.panelBindings.Name = "panelBindings";
		this.panelBindings.Padding = new System.Windows.Forms.Padding(0, 0, 11, 5);
		this.panelBindings.Size = new System.Drawing.Size(949, 395);
		this.panelBindings.TabIndex = 1;
		this.panelBindings.TabStop = true;
		this.tableBindings.AccessibleRole = System.Windows.Forms.AccessibleRole.Table;
		this.tableBindings.AutoSize = true;
		this.tableBindings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.tableBindings.ColumnCount = 3;
		this.tableBindings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20f));
		this.tableBindings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40f));
		this.tableBindings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40f));
		this.tableBindings.Controls.Add(this.labelFNE, 0, 8);
		this.tableBindings.Controls.Add(this.comboFNE, 0, 8);
		this.tableBindings.Controls.Add(this.textFNE, 0, 8);
		this.tableBindings.Controls.Add(this.labelFNV, 0, 7);
		this.tableBindings.Controls.Add(this.comboFNV, 1, 7);
		this.tableBindings.Controls.Add(this.textFNV, 2, 7);
		this.tableBindings.Controls.Add(this.labelFNC, 0, 6);
		this.tableBindings.Controls.Add(this.comboFNC, 1, 6);
		this.tableBindings.Controls.Add(this.textFNC, 2, 6);
		this.tableBindings.Controls.Add(this.labelFNF4, 0, 5);
		this.tableBindings.Controls.Add(this.comboFNF4, 1, 5);
		this.tableBindings.Controls.Add(this.textFNF4, 2, 5);
		this.tableBindings.Controls.Add(this.comboM4, 1, 4);
		this.tableBindings.Controls.Add(this.textM4, 2, 4);
		this.tableBindings.Controls.Add(this.labelM4, 0, 4);
		this.tableBindings.Controls.Add(this.comboM5, 1, 3);
		this.tableBindings.Controls.Add(this.textM5, 2, 3);
		this.tableBindings.Controls.Add(this.labelM5, 0, 3);
		this.tableBindings.Controls.Add(this.comboM3, 1, 2);
		this.tableBindings.Controls.Add(this.textM3, 2, 2);
		this.tableBindings.Controls.Add(this.labelM3, 0, 2);
		this.tableBindings.Controls.Add(this.textM2, 2, 1);
		this.tableBindings.Controls.Add(this.labelM2, 0, 1);
		this.tableBindings.Controls.Add(this.comboM2, 1, 1);
		this.tableBindings.Controls.Add(this.textM1, 2, 0);
		this.tableBindings.Controls.Add(this.comboM1, 1, 0);
		this.tableBindings.Controls.Add(this.labelM1, 0, 0);
		this.tableBindings.Dock = System.Windows.Forms.DockStyle.Top;
		this.tableBindings.Location = new System.Drawing.Point(0, 0);
		this.tableBindings.Margin = new System.Windows.Forms.Padding(0, 3, 5, 3);
		this.tableBindings.Name = "tableBindings";
		this.tableBindings.Padding = new System.Windows.Forms.Padding(16, 11, 0, 11);
		this.tableBindings.RowCount = 9;
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBindings.Size = new System.Drawing.Size(938, 390);
		this.tableBindings.TabIndex = 12;
		this.labelFNE.AutoSize = true;
		this.labelFNE.Location = new System.Drawing.Point(16, 333);
		this.labelFNE.Margin = new System.Windows.Forms.Padding(0);
		this.labelFNE.Name = "labelFNE";
		this.labelFNE.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelFNE.Size = new System.Drawing.Size(143, 43);
		this.labelFNE.TabIndex = 20;
		this.labelFNE.Text = "FN+NmEnt:";
		this.comboFNE.BorderColor = System.Drawing.Color.White;
		this.comboFNE.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboFNE.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboFNE.FormattingEnabled = true;
		this.comboFNE.Location = new System.Drawing.Point(205, 336);
		this.comboFNE.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboFNE.Name = "comboFNE";
		this.comboFNE.Size = new System.Drawing.Size(358, 40);
		this.comboFNE.TabIndex = 8;
		this.textFNE.Dock = System.Windows.Forms.DockStyle.Top;
		this.textFNE.Location = new System.Drawing.Point(573, 336);
		this.textFNE.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textFNE.Name = "textFNE";
		this.textFNE.PlaceholderText = "action";
		this.textFNE.Size = new System.Drawing.Size(360, 39);
		this.textFNE.TabIndex = 19;
		this.textFNE.TabStop = false;
		this.labelFNV.AutoSize = true;
		this.labelFNV.Location = new System.Drawing.Point(16, 287);
		this.labelFNV.Margin = new System.Windows.Forms.Padding(0);
		this.labelFNV.Name = "labelFNV";
		this.labelFNV.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelFNV.Size = new System.Drawing.Size(85, 43);
		this.labelFNV.TabIndex = 15;
		this.labelFNV.Text = "FN+V:";
		this.comboFNV.BorderColor = System.Drawing.Color.White;
		this.comboFNV.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboFNV.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboFNV.FormattingEnabled = true;
		this.comboFNV.Location = new System.Drawing.Point(205, 290);
		this.comboFNV.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboFNV.Name = "comboFNV";
		this.comboFNV.Size = new System.Drawing.Size(358, 40);
		this.comboFNV.TabIndex = 7;
		this.textFNV.Dock = System.Windows.Forms.DockStyle.Top;
		this.textFNV.Location = new System.Drawing.Point(573, 290);
		this.textFNV.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textFNV.Name = "textFNV";
		this.textFNV.PlaceholderText = "action";
		this.textFNV.Size = new System.Drawing.Size(360, 39);
		this.textFNV.TabIndex = 18;
		this.textFNV.TabStop = false;
		this.labelFNC.AutoSize = true;
		this.labelFNC.Location = new System.Drawing.Point(16, 241);
		this.labelFNC.Margin = new System.Windows.Forms.Padding(0);
		this.labelFNC.Name = "labelFNC";
		this.labelFNC.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelFNC.Size = new System.Drawing.Size(85, 43);
		this.labelFNC.TabIndex = 15;
		this.labelFNC.Text = "FN+C:";
		this.comboFNC.BorderColor = System.Drawing.Color.White;
		this.comboFNC.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboFNC.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboFNC.FormattingEnabled = true;
		this.comboFNC.Location = new System.Drawing.Point(205, 244);
		this.comboFNC.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboFNC.Name = "comboFNC";
		this.comboFNC.Size = new System.Drawing.Size(358, 40);
		this.comboFNC.TabIndex = 6;
		this.textFNC.Dock = System.Windows.Forms.DockStyle.Top;
		this.textFNC.Location = new System.Drawing.Point(573, 244);
		this.textFNC.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textFNC.Name = "textFNC";
		this.textFNC.PlaceholderText = "action";
		this.textFNC.Size = new System.Drawing.Size(360, 39);
		this.textFNC.TabIndex = 17;
		this.textFNC.TabStop = false;
		this.labelFNF4.AutoSize = true;
		this.labelFNF4.Location = new System.Drawing.Point(16, 195);
		this.labelFNF4.Margin = new System.Windows.Forms.Padding(0);
		this.labelFNF4.Name = "labelFNF4";
		this.labelFNF4.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelFNF4.Size = new System.Drawing.Size(95, 43);
		this.labelFNF4.TabIndex = 6;
		this.labelFNF4.Text = "FN+F4:";
		this.comboFNF4.BorderColor = System.Drawing.Color.White;
		this.comboFNF4.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboFNF4.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboFNF4.FormattingEnabled = true;
		this.comboFNF4.Location = new System.Drawing.Point(205, 198);
		this.comboFNF4.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboFNF4.Name = "comboFNF4";
		this.comboFNF4.Size = new System.Drawing.Size(358, 40);
		this.comboFNF4.TabIndex = 5;
		this.textFNF4.Dock = System.Windows.Forms.DockStyle.Top;
		this.textFNF4.Location = new System.Drawing.Point(573, 198);
		this.textFNF4.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textFNF4.Name = "textFNF4";
		this.textFNF4.PlaceholderText = "action";
		this.textFNF4.Size = new System.Drawing.Size(360, 39);
		this.textFNF4.TabIndex = 8;
		this.textFNF4.TabStop = false;
		this.comboM4.BorderColor = System.Drawing.Color.White;
		this.comboM4.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboM4.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboM4.FormattingEnabled = true;
		this.comboM4.Items.AddRange(new object[3]
		{
			Asus.Properties.Strings.PerformanceMode,
			Asus.Properties.Strings.OpenAsus,
			Asus.Properties.Strings.Custom
		});
		this.comboM4.Location = new System.Drawing.Point(205, 152);
		this.comboM4.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboM4.Name = "comboM4";
		this.comboM4.Size = new System.Drawing.Size(358, 40);
		this.comboM4.TabIndex = 4;
		this.textM4.Dock = System.Windows.Forms.DockStyle.Top;
		this.textM4.Location = new System.Drawing.Point(573, 152);
		this.textM4.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textM4.Name = "textM4";
		this.textM4.PlaceholderText = "action";
		this.textM4.Size = new System.Drawing.Size(360, 39);
		this.textM4.TabIndex = 5;
		this.textM4.TabStop = false;
		this.labelM4.AutoSize = true;
		this.labelM4.Location = new System.Drawing.Point(16, 149);
		this.labelM4.Margin = new System.Windows.Forms.Padding(0);
		this.labelM4.Name = "labelM4";
		this.labelM4.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelM4.Size = new System.Drawing.Size(116, 43);
		this.labelM4.TabIndex = 2;
		this.labelM4.Text = "M4/ROG:";
		this.comboM5.BorderColor = System.Drawing.Color.White;
		this.comboM5.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboM5.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboM5.FormattingEnabled = true;
		this.comboM5.Location = new System.Drawing.Point(205, 152);
		this.comboM5.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboM5.Name = "comboM5";
		this.comboM5.Size = new System.Drawing.Size(358, 40);
		this.comboM5.TabIndex = 6;
		this.textM5.Dock = System.Windows.Forms.DockStyle.Top;
		this.textM5.Location = new System.Drawing.Point(573, 152);
		this.textM5.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textM5.Name = "textM5";
		this.textM5.PlaceholderText = "action";
		this.textM5.Size = new System.Drawing.Size(360, 39);
		this.textM5.TabIndex = 7;
		this.textM5.TabStop = false;
		this.labelM5.AutoSize = true;
		this.labelM5.Location = new System.Drawing.Point(16, 149);
		this.labelM5.Margin = new System.Windows.Forms.Padding(0);
		this.labelM5.Name = "labelM5";
		this.labelM5.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelM5.Size = new System.Drawing.Size(59, 43);
		this.labelM5.TabIndex = 2;
		this.labelM5.Text = "M4:";
		this.comboM3.BorderColor = System.Drawing.Color.White;
		this.comboM3.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboM3.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboM3.FormattingEnabled = true;
		this.comboM3.Items.AddRange(new object[6]
		{
			Asus.Properties.Strings.Default,
			Asus.Properties.Strings.VolumeMute,
			Asus.Properties.Strings.PlayPause,
			Asus.Properties.Strings.PrintScreen,
			Asus.Properties.Strings.ToggleAura,
			Asus.Properties.Strings.Custom
		});
		this.comboM3.Location = new System.Drawing.Point(205, 106);
		this.comboM3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboM3.Name = "comboM3";
		this.comboM3.Size = new System.Drawing.Size(358, 40);
		this.comboM3.TabIndex = 3;
		this.textM3.Dock = System.Windows.Forms.DockStyle.Top;
		this.textM3.Location = new System.Drawing.Point(573, 106);
		this.textM3.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textM3.Name = "textM3";
		this.textM3.PlaceholderText = "action";
		this.textM3.Size = new System.Drawing.Size(360, 39);
		this.textM3.TabIndex = 4;
		this.textM3.TabStop = false;
		this.labelM3.AutoSize = true;
		this.labelM3.Location = new System.Drawing.Point(16, 103);
		this.labelM3.Margin = new System.Windows.Forms.Padding(0);
		this.labelM3.Name = "labelM3";
		this.labelM3.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelM3.Size = new System.Drawing.Size(59, 43);
		this.labelM3.TabIndex = 0;
		this.labelM3.Text = "M3:";
		this.textM2.Dock = System.Windows.Forms.DockStyle.Top;
		this.textM2.Location = new System.Drawing.Point(573, 60);
		this.textM2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textM2.Name = "textM2";
		this.textM2.PlaceholderText = "action";
		this.textM2.Size = new System.Drawing.Size(360, 39);
		this.textM2.TabIndex = 14;
		this.textM2.TabStop = false;
		this.labelM2.AutoSize = true;
		this.labelM2.Location = new System.Drawing.Point(16, 57);
		this.labelM2.Margin = new System.Windows.Forms.Padding(0);
		this.labelM2.Name = "labelM2";
		this.labelM2.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelM2.Size = new System.Drawing.Size(59, 43);
		this.labelM2.TabIndex = 10;
		this.labelM2.Text = "M2:";
		this.comboM2.BorderColor = System.Drawing.Color.White;
		this.comboM2.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboM2.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboM2.FormattingEnabled = true;
		this.comboM2.Items.AddRange(new object[6]
		{
			Asus.Properties.Strings.Default,
			Asus.Properties.Strings.VolumeMute,
			Asus.Properties.Strings.PlayPause,
			Asus.Properties.Strings.PrintScreen,
			Asus.Properties.Strings.ToggleAura,
			Asus.Properties.Strings.Custom
		});
		this.comboM2.Location = new System.Drawing.Point(205, 60);
		this.comboM2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboM2.Name = "comboM2";
		this.comboM2.Size = new System.Drawing.Size(358, 40);
		this.comboM2.TabIndex = 2;
		this.textM1.Dock = System.Windows.Forms.DockStyle.Top;
		this.textM1.Location = new System.Drawing.Point(573, 14);
		this.textM1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textM1.Name = "textM1";
		this.textM1.PlaceholderText = "action";
		this.textM1.Size = new System.Drawing.Size(360, 39);
		this.textM1.TabIndex = 13;
		this.textM1.TabStop = false;
		this.comboM1.BorderColor = System.Drawing.Color.White;
		this.comboM1.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboM1.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboM1.FormattingEnabled = true;
		this.comboM1.Items.AddRange(new object[6]
		{
			Asus.Properties.Strings.Default,
			Asus.Properties.Strings.VolumeMute,
			Asus.Properties.Strings.PlayPause,
			Asus.Properties.Strings.PrintScreen,
			Asus.Properties.Strings.ToggleAura,
			Asus.Properties.Strings.Custom
		});
		this.comboM1.Location = new System.Drawing.Point(205, 14);
		this.comboM1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.comboM1.Name = "comboM1";
		this.comboM1.Size = new System.Drawing.Size(358, 40);
		this.comboM1.TabIndex = 1;
		this.labelM1.AutoSize = true;
		this.labelM1.Location = new System.Drawing.Point(16, 11);
		this.labelM1.Margin = new System.Windows.Forms.Padding(0);
		this.labelM1.Name = "labelM1";
		this.labelM1.Padding = new System.Windows.Forms.Padding(5, 11, 0, 0);
		this.labelM1.Size = new System.Drawing.Size(59, 43);
		this.labelM1.TabIndex = 9;
		this.labelM1.Text = "M1:";
		this.tableKeys.ColumnCount = 3;
		this.tableKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20f));
		this.tableKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40f));
		this.tableKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40f));
		this.tableKeys.Location = new System.Drawing.Point(0, 0);
		this.tableKeys.Name = "tableKeys";
		this.tableKeys.RowCount = 6;
		this.tableKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableKeys.Size = new System.Drawing.Size(200, 100);
		this.tableKeys.TabIndex = 0;
		this.panelBacklightHeader.AutoSize = true;
		this.panelBacklightHeader.BackColor = System.Drawing.SystemColors.ControlLight;
		this.panelBacklightHeader.Controls.Add(this.sliderBrightness);
		this.panelBacklightHeader.Controls.Add(this.pictureBacklight);
		this.panelBacklightHeader.Controls.Add(this.labelBacklightTitle);
		this.panelBacklightHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBacklightHeader.Location = new System.Drawing.Point(15, 461);
		this.panelBacklightHeader.Name = "panelBacklightHeader";
		this.panelBacklightHeader.Padding = new System.Windows.Forms.Padding(11, 5, 11, 5);
		this.panelBacklightHeader.Size = new System.Drawing.Size(949, 51);
		this.panelBacklightHeader.TabIndex = 2;
		this.sliderBrightness.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.sliderBrightness.Location = new System.Drawing.Point(433, 5);
		this.sliderBrightness.Margin = new System.Windows.Forms.Padding(0);
		this.sliderBrightness.Max = 3;
		this.sliderBrightness.Min = 0;
		this.sliderBrightness.Name = "sliderBrightness";
		this.sliderBrightness.Size = new System.Drawing.Size(501, 40);
		this.sliderBrightness.Step = 1;
		this.sliderBrightness.TabIndex = 50;
		this.sliderBrightness.TabStop = true;
		this.sliderBrightness.Text = "sliderBrightness";
		this.sliderBrightness.Value = 3;
		this.pictureBacklight.BackgroundImage = Asus.Properties.Resources.backlight;
		this.pictureBacklight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBacklight.Location = new System.Drawing.Point(21, 11);
		this.pictureBacklight.Name = "pictureBacklight";
		this.pictureBacklight.Size = new System.Drawing.Size(32, 32);
		this.pictureBacklight.TabIndex = 3;
		this.pictureBacklight.TabStop = false;
		this.labelBacklightTitle.AutoSize = true;
		this.labelBacklightTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBacklightTitle.Location = new System.Drawing.Point(56, 8);
		this.labelBacklightTitle.Name = "labelBacklightTitle";
		this.labelBacklightTitle.Size = new System.Drawing.Size(119, 32);
		this.labelBacklightTitle.TabIndex = 2;
		this.labelBacklightTitle.Text = "Backlight";
		this.panelBacklight.AutoSize = true;
		this.panelBacklight.Controls.Add(this.panelBacklightExtra);
		this.panelBacklight.Controls.Add(this.panelXGM);
		this.panelBacklight.Controls.Add(this.tableBacklight);
		this.panelBacklight.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBacklight.Location = new System.Drawing.Point(15, 512);
		this.panelBacklight.Name = "panelBacklight";
		this.panelBacklight.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.panelBacklight.Size = new System.Drawing.Size(949, 444);
		this.panelBacklight.TabIndex = 3;
		this.panelBacklightExtra.AutoSize = true;
		this.panelBacklightExtra.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelBacklightExtra.Controls.Add(this.numericBacklightPluggedTime);
		this.panelBacklightExtra.Controls.Add(this.numericBacklightTime);
		this.panelBacklightExtra.Controls.Add(this.labelBacklightTimeout);
		this.panelBacklightExtra.Controls.Add(this.labelSpeed);
		this.panelBacklightExtra.Controls.Add(this.comboKeyboardSpeed);
		this.panelBacklightExtra.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBacklightExtra.Location = new System.Drawing.Point(0, 324);
		this.panelBacklightExtra.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.panelBacklightExtra.Name = "panelBacklightExtra";
		this.panelBacklightExtra.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
		this.panelBacklightExtra.Size = new System.Drawing.Size(949, 115);
		this.panelBacklightExtra.TabIndex = 46;
		this.numericBacklightPluggedTime.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericBacklightPluggedTime.Location = new System.Drawing.Point(634, 63);
		this.numericBacklightPluggedTime.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.numericBacklightPluggedTime.Maximum = new decimal(new int[4] { 3600, 0, 0, 0 });
		this.numericBacklightPluggedTime.Name = "numericBacklightPluggedTime";
		this.numericBacklightPluggedTime.Size = new System.Drawing.Size(139, 39);
		this.numericBacklightPluggedTime.TabIndex = 1;
		this.numericBacklightPluggedTime.Unit = "sec";
		this.numericBacklightPluggedTime.UnitFirst = false;
		this.numericBacklightTime.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericBacklightTime.Location = new System.Drawing.Point(789, 63);
		this.numericBacklightTime.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.numericBacklightTime.Maximum = new decimal(new int[4] { 3600, 0, 0, 0 });
		this.numericBacklightTime.Name = "numericBacklightTime";
		this.numericBacklightTime.Size = new System.Drawing.Size(139, 39);
		this.numericBacklightTime.TabIndex = 2;
		this.numericBacklightTime.Unit = "sec";
		this.numericBacklightTime.UnitFirst = false;
		this.labelBacklightTimeout.Location = new System.Drawing.Point(16, 63);
		this.labelBacklightTimeout.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelBacklightTimeout.Name = "labelBacklightTimeout";
		this.labelBacklightTimeout.Size = new System.Drawing.Size(613, 47);
		this.labelBacklightTimeout.TabIndex = 46;
		this.labelBacklightTimeout.Text = "Timeout when plugged / on battery";
		this.labelSpeed.Location = new System.Drawing.Point(16, 16);
		this.labelSpeed.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelSpeed.Name = "labelSpeed";
		this.labelSpeed.Size = new System.Drawing.Size(613, 43);
		this.labelSpeed.TabIndex = 44;
		this.labelSpeed.Text = "Animation Speed";
		this.comboKeyboardSpeed.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboKeyboardSpeed.BorderColor = System.Drawing.Color.White;
		this.comboKeyboardSpeed.ButtonColor = System.Drawing.SystemColors.ControlLight;
		this.comboKeyboardSpeed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.comboKeyboardSpeed.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.comboKeyboardSpeed.FormattingEnabled = true;
		this.comboKeyboardSpeed.Items.AddRange(new object[3] { "Slow", "Normal", "Fast" });
		this.comboKeyboardSpeed.Location = new System.Drawing.Point(634, 13);
		this.comboKeyboardSpeed.Margin = new System.Windows.Forms.Padding(5, 11, 5, 9);
		this.comboKeyboardSpeed.Name = "comboKeyboardSpeed";
		this.comboKeyboardSpeed.Size = new System.Drawing.Size(293, 40);
		this.comboKeyboardSpeed.TabIndex = 0;
		this.comboKeyboardSpeed.TabStop = false;
		this.panelXGM.Controls.Add(this.checkXGM);
		this.panelXGM.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelXGM.Location = new System.Drawing.Point(0, 265);
		this.panelXGM.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.panelXGM.Name = "panelXMG";
		this.panelXGM.Size = new System.Drawing.Size(949, 59);
		this.panelXGM.TabIndex = 45;
		this.checkXGM.AutoSize = true;
		this.checkXGM.Location = new System.Drawing.Point(5, 11);
		this.checkXGM.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkXGM.Name = "checkXMG";
		this.checkXGM.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkXGM.Size = new System.Drawing.Size(181, 42);
		this.checkXGM.TabIndex = 2;
		this.checkXGM.Text = "XG Mobile";
		this.checkXGM.UseVisualStyleBackColor = true;
		this.tableBacklight.AutoSize = true;
		this.tableBacklight.ColumnCount = 5;
		this.tableBacklight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
		this.tableBacklight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
		this.tableBacklight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
		this.tableBacklight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
		this.tableBacklight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
		this.tableBacklight.Controls.Add(this.labelBacklightKeyboard, 0, 0);
		this.tableBacklight.Controls.Add(this.checkAwake, 1, 0);
		this.tableBacklight.Controls.Add(this.checkSleep, 2, 0);
		this.tableBacklight.Controls.Add(this.checkBoot, 3, 0);
		this.tableBacklight.Controls.Add(this.checkBattery, 4, 0);
		this.tableBacklight.Controls.Add(this.labelBacklightLogo, 0, 1);
		this.tableBacklight.Controls.Add(this.checkAwakeLogo, 1, 1);
		this.tableBacklight.Controls.Add(this.checkSleepLogo, 2, 1);
		this.tableBacklight.Controls.Add(this.checkBootLogo, 3, 1);
		this.tableBacklight.Controls.Add(this.checkBatteryLogo, 4, 1);
		this.tableBacklight.Controls.Add(this.labelBacklightBar, 0, 2);
		this.tableBacklight.Controls.Add(this.checkAwakeBar, 1, 2);
		this.tableBacklight.Controls.Add(this.checkSleepBar, 2, 2);
		this.tableBacklight.Controls.Add(this.checkBootBar, 3, 2);
		this.tableBacklight.Controls.Add(this.checkBatteryBar, 4, 2);
		this.tableBacklight.Controls.Add(this.labelBacklightLid, 0, 3);
		this.tableBacklight.Controls.Add(this.checkAwakeLid, 1, 3);
		this.tableBacklight.Controls.Add(this.checkSleepLid, 2, 3);
		this.tableBacklight.Controls.Add(this.checkBootLid, 3, 3);
		this.tableBacklight.Controls.Add(this.checkBatteryLid, 4, 3);
		this.tableBacklight.Dock = System.Windows.Forms.DockStyle.Top;
		this.tableBacklight.Location = new System.Drawing.Point(0, 5);
		this.tableBacklight.Margin = new System.Windows.Forms.Padding(0);
		this.tableBacklight.Name = "tableBacklight";
		this.tableBacklight.RowCount = 4;
		this.tableBacklight.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBacklight.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBacklight.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBacklight.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableBacklight.Size = new System.Drawing.Size(949, 172);
		this.tableBacklight.TabIndex = 44;
		this.labelBacklightKeyboard.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelBacklightKeyboard.AutoSize = true;
		this.labelBacklightKeyboard.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBacklightKeyboard.Location = new System.Drawing.Point(5, 0);
		this.labelBacklightKeyboard.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelBacklightKeyboard.Name = "labelBacklightKeyboard";
		this.labelBacklightKeyboard.Padding = new System.Windows.Forms.Padding(9, 5, 7, 5);
		this.labelBacklightKeyboard.Size = new System.Drawing.Size(227, 45);
		this.labelBacklightKeyboard.TabIndex = 6;
		this.labelBacklightKeyboard.Text = "Keyboard";
		this.checkAwake.AutoSize = true;
		this.checkAwake.Location = new System.Drawing.Point(5, 45);
		this.checkAwake.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkAwake.Name = "checkAwake";
		this.checkAwake.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkAwake.Size = new System.Drawing.Size(227, 43);
		this.checkAwake.TabIndex = 1;
		this.checkAwake.Text = Asus.Properties.Strings.Awake;
		this.checkAwake.UseVisualStyleBackColor = true;
		this.checkBoot.AutoSize = true;
		this.checkBoot.Location = new System.Drawing.Point(5, 88);
		this.checkBoot.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBoot.Name = "checkBoot";
		this.checkBoot.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBoot.Size = new System.Drawing.Size(227, 43);
		this.checkBoot.TabIndex = 2;
		this.checkBoot.Text = Asus.Properties.Strings.Boot;
		this.checkBoot.UseVisualStyleBackColor = true;
		this.checkSleep.AutoSize = true;
		this.checkSleep.Location = new System.Drawing.Point(5, 131);
		this.checkSleep.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkSleep.Name = "checkSleep";
		this.checkSleep.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkSleep.Size = new System.Drawing.Size(227, 43);
		this.checkSleep.TabIndex = 3;
		this.checkSleep.Text = "Sleep";
		this.checkSleep.UseVisualStyleBackColor = true;
		this.checkBattery.AutoSize = true;
		this.checkBattery.Location = new System.Drawing.Point(5, 174);
		this.checkBattery.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBattery.Name = "checkBattery";
		this.checkBattery.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBattery.Size = new System.Drawing.Size(227, 43);
		this.checkBattery.TabIndex = 24;
		this.checkBattery.Text = "Battery";
		this.checkBattery.UseVisualStyleBackColor = true;
		this.labelBacklightLogo.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelBacklightLogo.AutoSize = true;
		this.labelBacklightLogo.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBacklightLogo.Location = new System.Drawing.Point(242, 0);
		this.labelBacklightLogo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelBacklightLogo.Name = "labelBacklightLogo";
		this.labelBacklightLogo.Padding = new System.Windows.Forms.Padding(9, 5, 7, 5);
		this.labelBacklightLogo.Size = new System.Drawing.Size(227, 45);
		this.labelBacklightLogo.TabIndex = 21;
		this.labelBacklightLogo.Text = "Logo";
		this.checkAwakeLogo.AutoSize = true;
		this.checkAwakeLogo.Location = new System.Drawing.Point(242, 45);
		this.checkAwakeLogo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkAwakeLogo.Name = "checkAwakeLogo";
		this.checkAwakeLogo.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkAwakeLogo.Size = new System.Drawing.Size(227, 43);
		this.checkAwakeLogo.TabIndex = 17;
		this.checkAwakeLogo.Text = Asus.Properties.Strings.Awake;
		this.checkAwakeLogo.UseVisualStyleBackColor = true;
		this.checkBootLogo.AutoSize = true;
		this.checkBootLogo.Location = new System.Drawing.Point(242, 88);
		this.checkBootLogo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBootLogo.Name = "checkBootLogo";
		this.checkBootLogo.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBootLogo.Size = new System.Drawing.Size(227, 43);
		this.checkBootLogo.TabIndex = 18;
		this.checkBootLogo.Text = Asus.Properties.Strings.Boot;
		this.checkBootLogo.UseVisualStyleBackColor = true;
		this.checkSleepLogo.AutoSize = true;
		this.checkSleepLogo.Location = new System.Drawing.Point(242, 131);
		this.checkSleepLogo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkSleepLogo.Name = "checkSleepLogo";
		this.checkSleepLogo.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkSleepLogo.Size = new System.Drawing.Size(227, 43);
		this.checkSleepLogo.TabIndex = 19;
		this.checkSleepLogo.Text = Asus.Properties.Strings.Sleep;
		this.checkSleepLogo.UseVisualStyleBackColor = true;
		this.checkBatteryLogo.AutoSize = true;
		this.checkBatteryLogo.Location = new System.Drawing.Point(242, 174);
		this.checkBatteryLogo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBatteryLogo.Name = "checkBatteryLogo";
		this.checkBatteryLogo.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBatteryLogo.Size = new System.Drawing.Size(227, 43);
		this.checkBatteryLogo.TabIndex = 25;
		this.checkBatteryLogo.Text = "Battery";
		this.checkBatteryLogo.UseVisualStyleBackColor = true;
		this.labelBacklightBar.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelBacklightBar.AutoSize = true;
		this.labelBacklightBar.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBacklightBar.Location = new System.Drawing.Point(479, 0);
		this.labelBacklightBar.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelBacklightBar.Name = "labelBacklightBar";
		this.labelBacklightBar.Padding = new System.Windows.Forms.Padding(9, 5, 7, 5);
		this.labelBacklightBar.Size = new System.Drawing.Size(227, 45);
		this.labelBacklightBar.TabIndex = 11;
		this.labelBacklightBar.Text = "Lightbar";
		this.checkAwakeBar.AutoSize = true;
		this.checkAwakeBar.Location = new System.Drawing.Point(479, 45);
		this.checkAwakeBar.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkAwakeBar.Name = "checkAwakeBar";
		this.checkAwakeBar.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkAwakeBar.Size = new System.Drawing.Size(227, 43);
		this.checkAwakeBar.TabIndex = 7;
		this.checkAwakeBar.Text = Asus.Properties.Strings.Awake;
		this.checkAwakeBar.UseVisualStyleBackColor = true;
		this.checkBootBar.AutoSize = true;
		this.checkBootBar.Location = new System.Drawing.Point(479, 88);
		this.checkBootBar.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBootBar.Name = "checkBootBar";
		this.checkBootBar.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBootBar.Size = new System.Drawing.Size(227, 43);
		this.checkBootBar.TabIndex = 8;
		this.checkBootBar.Text = Asus.Properties.Strings.Boot;
		this.checkBootBar.UseVisualStyleBackColor = true;
		this.checkSleepBar.AutoSize = true;
		this.checkSleepBar.Location = new System.Drawing.Point(479, 131);
		this.checkSleepBar.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkSleepBar.Name = "checkSleepBar";
		this.checkSleepBar.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkSleepBar.Size = new System.Drawing.Size(227, 43);
		this.checkSleepBar.TabIndex = 9;
		this.checkSleepBar.Text = Asus.Properties.Strings.Sleep;
		this.checkSleepBar.UseVisualStyleBackColor = true;
		this.checkBatteryBar.AutoSize = true;
		this.checkBatteryBar.Location = new System.Drawing.Point(479, 174);
		this.checkBatteryBar.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBatteryBar.Name = "checkBatteryBar";
		this.checkBatteryBar.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBatteryBar.Size = new System.Drawing.Size(227, 43);
		this.checkBatteryBar.TabIndex = 22;
		this.checkBatteryBar.Text = "Battery";
		this.checkBatteryBar.UseVisualStyleBackColor = true;
		this.labelBacklightLid.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelBacklightLid.AutoSize = true;
		this.labelBacklightLid.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBacklightLid.Location = new System.Drawing.Point(716, 0);
		this.labelBacklightLid.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelBacklightLid.Name = "labelBacklightLid";
		this.labelBacklightLid.Padding = new System.Windows.Forms.Padding(9, 5, 7, 5);
		this.labelBacklightLid.Size = new System.Drawing.Size(228, 45);
		this.labelBacklightLid.TabIndex = 16;
		this.labelBacklightLid.Text = "Lid";
		this.checkAwakeLid.AutoSize = true;
		this.checkAwakeLid.Location = new System.Drawing.Point(716, 45);
		this.checkAwakeLid.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkAwakeLid.Name = "checkAwakeLid";
		this.checkAwakeLid.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkAwakeLid.Size = new System.Drawing.Size(228, 43);
		this.checkAwakeLid.TabIndex = 12;
		this.checkAwakeLid.Text = Asus.Properties.Strings.Awake;
		this.checkAwakeLid.UseVisualStyleBackColor = true;
		this.checkBootLid.AutoSize = true;
		this.checkBootLid.Location = new System.Drawing.Point(716, 88);
		this.checkBootLid.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBootLid.Name = "checkBootLid";
		this.checkBootLid.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBootLid.Size = new System.Drawing.Size(228, 43);
		this.checkBootLid.TabIndex = 13;
		this.checkBootLid.Text = Asus.Properties.Strings.Boot;
		this.checkBootLid.UseVisualStyleBackColor = true;
		this.checkSleepLid.AutoSize = true;
		this.checkSleepLid.Location = new System.Drawing.Point(716, 131);
		this.checkSleepLid.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkSleepLid.Name = "checkSleepLid";
		this.checkSleepLid.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkSleepLid.Size = new System.Drawing.Size(228, 43);
		this.checkSleepLid.TabIndex = 14;
		this.checkSleepLid.Text = Asus.Properties.Strings.Sleep;
		this.checkSleepLid.UseVisualStyleBackColor = true;
		this.checkBatteryLid.AutoSize = true;
		this.checkBatteryLid.Location = new System.Drawing.Point(716, 174);
		this.checkBatteryLid.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.checkBatteryLid.Name = "checkBatteryLid";
		this.checkBatteryLid.Padding = new System.Windows.Forms.Padding(16, 3, 7, 3);
		this.checkBatteryLid.Size = new System.Drawing.Size(228, 43);
		this.checkBatteryLid.TabIndex = 23;
		this.checkBatteryLid.Text = "Battery";
		this.checkBatteryLid.UseVisualStyleBackColor = true;
		this.panelSettingsHeader.AutoSize = true;
		this.panelSettingsHeader.BackColor = System.Drawing.SystemColors.ControlLight;
		this.panelSettingsHeader.Controls.Add(this.pictureScan);
		this.panelSettingsHeader.Controls.Add(this.pictureLog);
		this.panelSettingsHeader.Controls.Add(this.pictureSettings);
		this.panelSettingsHeader.Controls.Add(this.labelSettings);
		this.panelSettingsHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelSettingsHeader.Location = new System.Drawing.Point(15, 956);
		this.panelSettingsHeader.Name = "panelSettingsHeader";
		this.panelSettingsHeader.Padding = new System.Windows.Forms.Padding(11, 5, 11, 5);
		this.panelSettingsHeader.Size = new System.Drawing.Size(949, 51);
		this.panelSettingsHeader.TabIndex = 45;
		this.pictureScan.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.pictureScan.BackgroundImage = Asus.Properties.Resources.icons8_search_32;
		this.pictureScan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureScan.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureScan.Location = new System.Drawing.Point(857, 11);
		this.pictureScan.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.pictureScan.Name = "pictureScan";
		this.pictureScan.Size = new System.Drawing.Size(32, 32);
		this.pictureScan.TabIndex = 13;
		this.pictureScan.TabStop = false;
		this.pictureScan.Visible = false;
		this.pictureLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.pictureLog.BackgroundImage = Asus.Properties.Resources.icons8_log_32;
		this.pictureLog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureLog.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureLog.Location = new System.Drawing.Point(897, 11);
		this.pictureLog.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.pictureLog.Name = "pictureLog";
		this.pictureLog.Size = new System.Drawing.Size(32, 32);
		this.pictureLog.TabIndex = 12;
		this.pictureLog.TabStop = false;
		this.pictureSettings.BackgroundImage = Asus.Properties.Resources.icons8_settings_32;
		this.pictureSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureSettings.Location = new System.Drawing.Point(21, 11);
		this.pictureSettings.Name = "pictureSettings";
		this.pictureSettings.Size = new System.Drawing.Size(32, 32);
		this.pictureSettings.TabIndex = 1;
		this.pictureSettings.TabStop = false;
		this.labelSettings.AutoSize = true;
		this.labelSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelSettings.Location = new System.Drawing.Point(56, 9);
		this.labelSettings.Name = "labelSettings";
		this.labelSettings.Size = new System.Drawing.Size(78, 32);
		this.labelSettings.TabIndex = 0;
		this.labelSettings.Text = "Other";
		this.panelSettings.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelSettings.AutoSize = true;
		this.panelSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelSettings.Controls.Add(this.checkAutoToggleClamshellMode);
		this.panelSettings.Controls.Add(this.checkTopmost);
		this.panelSettings.Controls.Add(this.checkNoOverdrive);
		this.panelSettings.Controls.Add(this.checkKeystoneSound);
		this.panelSettings.Controls.Add(this.checkBootSound);
		this.panelSettings.Controls.Add(this.checkUSBC);
		this.panelSettings.Controls.Add(this.checkGpuApps);
		this.panelSettings.Controls.Add(this.checkNVPlatform);
		this.panelSettings.Controls.Add(this.checkNumberPad);
		this.panelSettings.Controls.Add(this.checkStatusLed);
		this.panelSettings.Controls.Add(this.checkStandbyNetworking);
		this.panelSettings.Controls.Add(this.checkAspm);
		this.panelSettings.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelSettings.Location = new System.Drawing.Point(15, 1252);
		this.panelSettings.Name = "panelSettings";
		this.panelSettings.Padding = new System.Windows.Forms.Padding(21, 5, 11, 5);
		this.panelSettings.Size = new System.Drawing.Size(949, 472);
		this.panelSettings.TabIndex = 50;
		this.checkAutoToggleClamshellMode.AutoSize = true;
		this.checkAutoToggleClamshellMode.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkAutoToggleClamshellMode.Location = new System.Drawing.Point(21, 425);
		this.checkAutoToggleClamshellMode.Name = "checkAutoToggleClamshellMode";
		this.checkAutoToggleClamshellMode.Padding = new System.Windows.Forms.Padding(3);
		this.checkAutoToggleClamshellMode.Size = new System.Drawing.Size(917, 42);
		this.checkAutoToggleClamshellMode.TabIndex = 9;
		this.checkAutoToggleClamshellMode.Text = "Auto Toggle Clamshell Mode";
		this.checkAutoToggleClamshellMode.UseVisualStyleBackColor = true;
		this.checkTopmost.AutoSize = true;
		this.checkTopmost.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkTopmost.Location = new System.Drawing.Point(21, 341);
		this.checkTopmost.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkTopmost.Name = "checkTopmost";
		this.checkTopmost.Padding = new System.Windows.Forms.Padding(3);
		this.checkTopmost.Size = new System.Drawing.Size(917, 42);
		this.checkTopmost.TabIndex = 8;
		this.checkTopmost.Text = Asus.Properties.Strings.WindowTop;
		this.checkTopmost.UseVisualStyleBackColor = true;
		this.checkNoOverdrive.AutoSize = true;
		this.checkNoOverdrive.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkNoOverdrive.Location = new System.Drawing.Point(21, 299);
		this.checkNoOverdrive.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkNoOverdrive.Name = "checkNoOverdrive";
		this.checkNoOverdrive.Padding = new System.Windows.Forms.Padding(3);
		this.checkNoOverdrive.Size = new System.Drawing.Size(917, 42);
		this.checkNoOverdrive.TabIndex = 7;
		this.checkNoOverdrive.Text = Asus.Properties.Strings.DisableOverdrive;
		this.checkNoOverdrive.UseVisualStyleBackColor = true;
		this.checkBootSound.AutoSize = true;
		this.checkBootSound.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkBootSound.Location = new System.Drawing.Point(21, 257);
		this.checkBootSound.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkBootSound.Name = "checkBootSound";
		this.checkBootSound.Padding = new System.Windows.Forms.Padding(3);
		this.checkBootSound.Size = new System.Drawing.Size(917, 42);
		this.checkBootSound.TabIndex = 5;
		this.checkBootSound.Text = "Boot Sound";
		this.checkBootSound.UseVisualStyleBackColor = true;
		this.checkKeystoneSound.AutoSize = true;
		this.checkKeystoneSound.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkKeystoneSound.Location = new System.Drawing.Point(21, 299);
		this.checkKeystoneSound.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkKeystoneSound.Name = "checkKeystoneSound";
		this.checkKeystoneSound.Padding = new System.Windows.Forms.Padding(3);
		this.checkKeystoneSound.Size = new System.Drawing.Size(917, 42);
		this.checkKeystoneSound.TabIndex = 6;
		this.checkKeystoneSound.Text = "Keystone Sound";
		this.checkKeystoneSound.UseVisualStyleBackColor = true;
		this.checkKeystoneSound.Visible = false;
		this.checkUSBC.AutoSize = true;
		this.checkUSBC.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkUSBC.Location = new System.Drawing.Point(21, 215);
		this.checkUSBC.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkUSBC.Name = "checkUSBC";
		this.checkUSBC.Padding = new System.Windows.Forms.Padding(3);
		this.checkUSBC.Size = new System.Drawing.Size(917, 42);
		this.checkUSBC.TabIndex = 4;
		this.checkUSBC.Text = "Keep GPU disabled on USB-C charger in Optimized mode";
		this.checkUSBC.UseVisualStyleBackColor = true;
		this.checkGpuApps.AutoSize = true;
		this.checkGpuApps.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkGpuApps.Location = new System.Drawing.Point(21, 173);
		this.checkGpuApps.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkGpuApps.Name = "checkGpuApps";
		this.checkGpuApps.Padding = new System.Windows.Forms.Padding(3);
		this.checkGpuApps.Size = new System.Drawing.Size(917, 42);
		this.checkGpuApps.TabIndex = 3;
		this.checkGpuApps.Text = "Stop all apps using GPU when switching to Eco";
		this.checkGpuApps.UseVisualStyleBackColor = true;
		this.checkNVPlatform.AutoSize = true;
		this.checkNVPlatform.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkNVPlatform.Location = new System.Drawing.Point(21, 89);
		this.checkNVPlatform.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkNVPlatform.Name = "checkNVPlatform";
		this.checkNVPlatform.Padding = new System.Windows.Forms.Padding(3);
		this.checkNVPlatform.Size = new System.Drawing.Size(917, 42);
		this.checkNVPlatform.TabIndex = 2;
		this.checkNVPlatform.Text = "Stop/Start NVIDIA services based on dGPU state";
		this.checkNVPlatform.UseVisualStyleBackColor = true;
		this.checkStatusLed.AutoSize = true;
		this.checkStatusLed.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkStatusLed.Location = new System.Drawing.Point(21, 47);
		this.checkStatusLed.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkStatusLed.Name = "checkStatusLed";
		this.checkStatusLed.Padding = new System.Windows.Forms.Padding(3);
		this.checkStatusLed.Size = new System.Drawing.Size(917, 42);
		this.checkStatusLed.TabIndex = 1;
		this.checkStatusLed.Text = "LED Status Indicators";
		this.checkStatusLed.UseVisualStyleBackColor = true;
		this.checkStatusLed.Visible = false;
		this.checkNumberPad.AutoSize = true;
		this.checkNumberPad.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkNumberPad.Location = new System.Drawing.Point(21, 89);
		this.checkNumberPad.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkNumberPad.Name = "checkNumberPad";
		this.checkNumberPad.Padding = new System.Windows.Forms.Padding(3);
		this.checkNumberPad.Size = new System.Drawing.Size(917, 42);
		this.checkNumberPad.TabIndex = 10;
		this.checkNumberPad.Text = "Touchpad NumberPad";
		this.checkNumberPad.UseVisualStyleBackColor = true;
		this.checkNumberPad.Visible = false;
		this.checkAspm.AutoSize = true;
		this.checkAspm.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkAspm.Location = new System.Drawing.Point(21, 5);
		this.checkAspm.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkAspm.Name = "checkAspm";
		this.checkAspm.Padding = new System.Windows.Forms.Padding(3);
		this.checkAspm.Size = new System.Drawing.Size(917, 42);
		this.checkAspm.TabIndex = 0;
		this.checkAspm.Text = "Disable PCIe Link State Management (plugged in)";
		this.checkAspm.UseVisualStyleBackColor = true;
		this.checkAspm.Visible = true;
		this.checkStandbyNetworking.AutoSize = true;
		this.checkStandbyNetworking.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkStandbyNetworking.Location = new System.Drawing.Point(21, 47);
		this.checkStandbyNetworking.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.checkStandbyNetworking.Name = "checkStandbyNetworking";
		this.checkStandbyNetworking.Padding = new System.Windows.Forms.Padding(3);
		this.checkStandbyNetworking.Size = new System.Drawing.Size(917, 42);
		this.checkStandbyNetworking.TabIndex = 1;
		this.checkStandbyNetworking.Text = "Disable networking in Modern Standby";
		this.checkStandbyNetworking.UseVisualStyleBackColor = true;
		this.checkStandbyNetworking.Visible = true;
		this.panelPower.Controls.Add(this.numericHibernateAfter);
		this.panelPower.Controls.Add(this.labelHibernateAfter);
		this.panelPower.Controls.Add(this.pictureHibernate);
		this.panelPower.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelPower.Location = new System.Drawing.Point(15, 1724);
		this.panelPower.Name = "panelPower";
		this.panelPower.Size = new System.Drawing.Size(949, 54);
		this.panelPower.TabIndex = 4;
		this.numericHibernateAfter.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.numericHibernateAfter.Increment = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericHibernateAfter.Location = new System.Drawing.Point(776, 7);
		this.numericHibernateAfter.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.numericHibernateAfter.Maximum = new decimal(new int[4] { 3000000, 0, 0, 0 });
		this.numericHibernateAfter.Name = "numericHibernateAfter";
		this.numericHibernateAfter.Size = new System.Drawing.Size(152, 39);
		this.numericHibernateAfter.TabIndex = 1;
		this.numericHibernateAfter.Unit = "min";
		this.numericHibernateAfter.UnitFirst = false;
		this.labelHibernateAfter.AutoSize = true;
		this.labelHibernateAfter.Location = new System.Drawing.Point(59, 10);
		this.labelHibernateAfter.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.labelHibernateAfter.Name = "labelHibernateAfter";
		this.labelHibernateAfter.Size = new System.Drawing.Size(457, 32);
		this.labelHibernateAfter.TabIndex = 45;
		this.labelHibernateAfter.Text = "Minutes till Hibernation in sleep (0 - OFF)";
		this.pictureHibernate.BackgroundImage = Asus.Properties.Resources.icons8_hibernate_32;
		this.pictureHibernate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureHibernate.Location = new System.Drawing.Point(21, 10);
		this.pictureHibernate.Name = "pictureHibernate";
		this.pictureHibernate.Size = new System.Drawing.Size(32, 32);
		this.pictureHibernate.TabIndex = 22;
		this.pictureHibernate.TabStop = false;
		this.panelAPU.AutoSize = true;
		this.panelAPU.Controls.Add(this.comboAPU);
		this.panelAPU.Controls.Add(this.pictureAPUMem);
		this.panelAPU.Controls.Add(this.labelAPUMem);
		this.panelAPU.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAPU.Location = new System.Drawing.Point(15, 1135);
		this.panelAPU.Name = "panelAPU";
		this.panelAPU.Padding = new System.Windows.Forms.Padding(11, 5, 11, 0);
		this.panelAPU.Size = new System.Drawing.Size(949, 57);
		this.panelAPU.TabIndex = 46;
		this.panelAPU.Visible = false;
		this.panelAPU.Paint += new System.Windows.Forms.PaintEventHandler(panelAPU_Paint);
		this.comboAPU.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboAPU.BorderColor = System.Drawing.Color.White;
		this.comboAPU.ButtonColor = System.Drawing.SystemColors.ControlLight;
		this.comboAPU.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.comboAPU.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.comboAPU.FormattingEnabled = true;
		this.comboAPU.Items.AddRange(new object[9] { "Auto", "1G", "2G", "3G", "4G", "5G", "6G", "7G", "8G" });
		this.comboAPU.Location = new System.Drawing.Point(618, 8);
		this.comboAPU.Margin = new System.Windows.Forms.Padding(5, 11, 5, 9);
		this.comboAPU.Name = "comboAPU";
		this.comboAPU.Size = new System.Drawing.Size(309, 40);
		this.comboAPU.TabIndex = 12;
		this.comboAPU.TabStop = false;
		this.pictureAPUMem.BackgroundImage = Asus.Properties.Resources.icons8_video_48;
		this.pictureAPUMem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureAPUMem.Location = new System.Drawing.Point(21, 11);
		this.pictureAPUMem.Name = "pictureAPUMem";
		this.pictureAPUMem.Size = new System.Drawing.Size(32, 32);
		this.pictureAPUMem.TabIndex = 1;
		this.pictureAPUMem.TabStop = false;
		this.labelAPUMem.AutoSize = true;
		this.labelAPUMem.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelAPUMem.Location = new System.Drawing.Point(64, 11);
		this.labelAPUMem.Name = "labelAPUMem";
		this.labelAPUMem.Size = new System.Drawing.Size(309, 32);
		this.labelAPUMem.TabIndex = 0;
		this.labelAPUMem.Text = "Memory assigned to iGPU";
		this.panelCores.AutoSize = true;
		this.panelCores.Controls.Add(this.buttonCores);
		this.panelCores.Controls.Add(this.comboCoresP);
		this.panelCores.Controls.Add(this.comboCoresE);
		this.panelCores.Controls.Add(this.pictureCores);
		this.panelCores.Controls.Add(this.labelCores);
		this.panelCores.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelCores.Location = new System.Drawing.Point(15, 1076);
		this.panelCores.Name = "panelCores";
		this.panelCores.Padding = new System.Windows.Forms.Padding(11, 5, 11, 0);
		this.panelCores.Size = new System.Drawing.Size(949, 59);
		this.panelCores.TabIndex = 47;
		this.panelCores.Visible = false;
		this.buttonCores.Activated = false;
		this.buttonCores.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonCores.BackColor = System.Drawing.SystemColors.ButtonHighlight;
		this.buttonCores.BorderColor = System.Drawing.Color.Transparent;
		this.buttonCores.BorderRadius = 2;
		this.buttonCores.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonCores.Location = new System.Drawing.Point(831, 7);
		this.buttonCores.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.buttonCores.Name = "buttonCores";
		this.buttonCores.Secondary = false;
		this.buttonCores.Size = new System.Drawing.Size(106, 48);
		this.buttonCores.TabIndex = 20;
		this.buttonCores.Text = "Apply";
		this.buttonCores.UseVisualStyleBackColor = false;
		this.comboCoresP.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboCoresP.BorderColor = System.Drawing.Color.White;
		this.comboCoresP.ButtonColor = System.Drawing.SystemColors.ControlLight;
		this.comboCoresP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.comboCoresP.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.comboCoresP.FormattingEnabled = true;
		this.comboCoresP.Location = new System.Drawing.Point(513, 10);
		this.comboCoresP.Margin = new System.Windows.Forms.Padding(5, 11, 5, 9);
		this.comboCoresP.Name = "comboCoresP";
		this.comboCoresP.Size = new System.Drawing.Size(150, 40);
		this.comboCoresP.TabIndex = 13;
		this.comboCoresP.TabStop = false;
		this.comboCoresE.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboCoresE.BorderColor = System.Drawing.Color.White;
		this.comboCoresE.ButtonColor = System.Drawing.SystemColors.ControlLight;
		this.comboCoresE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.comboCoresE.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.comboCoresE.FormattingEnabled = true;
		this.comboCoresE.Location = new System.Drawing.Point(674, 10);
		this.comboCoresE.Margin = new System.Windows.Forms.Padding(5, 11, 5, 9);
		this.comboCoresE.Name = "comboCoresE";
		this.comboCoresE.Size = new System.Drawing.Size(150, 40);
		this.comboCoresE.TabIndex = 12;
		this.comboCoresE.TabStop = false;
		this.pictureCores.BackgroundImage = Asus.Properties.Resources.icons8_processor_32;
		this.pictureCores.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureCores.Location = new System.Drawing.Point(21, 15);
		this.pictureCores.Name = "pictureCores";
		this.pictureCores.Size = new System.Drawing.Size(32, 32);
		this.pictureCores.TabIndex = 1;
		this.pictureCores.TabStop = false;
		this.labelCores.AutoSize = true;
		this.labelCores.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelCores.Location = new System.Drawing.Point(64, 14);
		this.labelCores.Name = "labelCores";
		this.labelCores.Size = new System.Drawing.Size(299, 32);
		this.labelCores.TabIndex = 0;
		this.labelCores.Text = "CPU Cores Configuration";
		this.panelACPI.AutoSize = true;
		this.panelACPI.Controls.Add(this.textACPIParam);
		this.panelACPI.Controls.Add(this.textACPICommand);
		this.panelACPI.Controls.Add(this.buttonACPISend);
		this.panelACPI.Controls.Add(this.pictureDebug);
		this.panelACPI.Controls.Add(this.labelACPITitle);
		this.panelACPI.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelACPI.Location = new System.Drawing.Point(15, 1007);
		this.panelACPI.Name = "panelACPI";
		this.panelACPI.Padding = new System.Windows.Forms.Padding(11, 5, 11, 0);
		this.panelACPI.Size = new System.Drawing.Size(949, 69);
		this.panelACPI.TabIndex = 48;
		this.panelACPI.Visible = false;
		this.textACPIParam.Location = new System.Drawing.Point(717, 18);
		this.textACPIParam.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textACPIParam.Name = "textACPIParam";
		this.textACPIParam.PlaceholderText = "Value";
		this.textACPIParam.Size = new System.Drawing.Size(127, 39);
		this.textACPIParam.TabIndex = 22;
		this.textACPIParam.TabStop = false;
		this.textACPICommand.Location = new System.Drawing.Point(467, 18);
		this.textACPICommand.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.textACPICommand.Name = "textACPICommand";
		this.textACPICommand.PlaceholderText = "Address";
		this.textACPICommand.Size = new System.Drawing.Size(242, 39);
		this.textACPICommand.TabIndex = 21;
		this.textACPICommand.TabStop = false;
		this.buttonACPISend.Activated = false;
		this.buttonACPISend.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonACPISend.BackColor = System.Drawing.SystemColors.ButtonHighlight;
		this.buttonACPISend.BorderColor = System.Drawing.Color.Transparent;
		this.buttonACPISend.BorderRadius = 2;
		this.buttonACPISend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonACPISend.Location = new System.Drawing.Point(855, 13);
		this.buttonACPISend.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		this.buttonACPISend.Name = "buttonACPISend";
		this.buttonACPISend.Secondary = false;
		this.buttonACPISend.Size = new System.Drawing.Size(106, 46);
		this.buttonACPISend.TabIndex = 20;
		this.buttonACPISend.Text = "Send";
		this.buttonACPISend.UseVisualStyleBackColor = false;
		this.pictureDebug.BackgroundImage = Asus.Properties.Resources.icons8_heartbeat_32;
		this.pictureDebug.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureDebug.Location = new System.Drawing.Point(21, 21);
		this.pictureDebug.Name = "pictureDebug";
		this.pictureDebug.Size = new System.Drawing.Size(32, 32);
		this.pictureDebug.TabIndex = 1;
		this.pictureDebug.TabStop = false;
		this.labelACPITitle.AutoSize = true;
		this.labelACPITitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelACPITitle.Location = new System.Drawing.Point(57, 21);
		this.labelACPITitle.Name = "labelACPITitle";
		this.labelACPITitle.Size = new System.Drawing.Size(188, 32);
		this.labelACPITitle.TabIndex = 0;
		this.labelACPITitle.Text = "ACPI DEVS Test";
		this.panelOptimalBrightness.AutoSize = true;
		this.panelOptimalBrightness.Controls.Add(this.comboOptimalBrightness);
		this.panelOptimalBrightness.Controls.Add(this.pictureOptimalBrightness);
		this.panelOptimalBrightness.Controls.Add(this.labelOptimalBrightness);
		this.panelOptimalBrightness.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelOptimalBrightness.Location = new System.Drawing.Point(15, 1192);
		this.panelOptimalBrightness.Name = "panelOptimalBrightness";
		this.panelOptimalBrightness.Padding = new System.Windows.Forms.Padding(11, 5, 11, 0);
		this.panelOptimalBrightness.Size = new System.Drawing.Size(949, 60);
		this.panelOptimalBrightness.TabIndex = 49;
		this.panelOptimalBrightness.Visible = false;
		this.comboOptimalBrightness.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboOptimalBrightness.BorderColor = System.Drawing.Color.White;
		this.comboOptimalBrightness.ButtonColor = System.Drawing.SystemColors.ControlLight;
		this.comboOptimalBrightness.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.comboOptimalBrightness.Font = new System.Drawing.Font("Segoe UI", 9f);
		this.comboOptimalBrightness.FormattingEnabled = true;
		this.comboOptimalBrightness.Items.AddRange(new object[3] { "Off", "On Always", "On Battery" });
		this.comboOptimalBrightness.Location = new System.Drawing.Point(618, 11);
		this.comboOptimalBrightness.Margin = new System.Windows.Forms.Padding(5, 11, 5, 9);
		this.comboOptimalBrightness.Name = "comboOptimalBrightness";
		this.comboOptimalBrightness.Size = new System.Drawing.Size(309, 40);
		this.comboOptimalBrightness.TabIndex = 12;
		this.comboOptimalBrightness.TabStop = false;
		this.pictureOptimalBrightness.BackgroundImage = Asus.Properties.Resources.icons8_brightness_32;
		this.pictureOptimalBrightness.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureOptimalBrightness.Location = new System.Drawing.Point(21, 11);
		this.pictureOptimalBrightness.Name = "pictureOptimalBrightness";
		this.pictureOptimalBrightness.Size = new System.Drawing.Size(32, 32);
		this.pictureOptimalBrightness.TabIndex = 1;
		this.pictureOptimalBrightness.TabStop = false;
		this.labelOptimalBrightness.AutoSize = true;
		this.labelOptimalBrightness.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelOptimalBrightness.Location = new System.Drawing.Point(64, 10);
		this.labelOptimalBrightness.Name = "labelOptimalBrightness";
		this.labelOptimalBrightness.Size = new System.Drawing.Size(323, 32);
		this.labelOptimalBrightness.TabIndex = 0;
		this.labelOptimalBrightness.Text = "Optimal Display Brightness";
		base.AutoScaleDimensions = new System.Drawing.SizeF(192f, 192f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.AutoScroll = true;
		this.AutoSize = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.ClientSize = new System.Drawing.Size(1013, 1759);
		base.Controls.Add(this.panelServices);
		base.Controls.Add(this.panelPower);
		base.Controls.Add(this.panelSettings);
		base.Controls.Add(this.panelOptimalBrightness);
		base.Controls.Add(this.panelAPU);
		base.Controls.Add(this.panelCores);
		base.Controls.Add(this.panelACPI);
		base.Controls.Add(this.panelSettingsHeader);
		base.Controls.Add(this.panelBacklight);
		base.Controls.Add(this.panelBacklightHeader);
		base.Controls.Add(this.panelBindings);
		base.Controls.Add(this.panelBindingsHeader);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
		base.MaximizeBox = false;
		base.MdiChildrenMinimizedAnchorBottom = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(1033, 71);
		base.Name = "Extra";
		base.Padding = new System.Windows.Forms.Padding(15);
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "Extra Settings";
		this.panelServices.ResumeLayout(false);
		this.panelServices.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureService).EndInit();
		this.panelBindingsHeader.ResumeLayout(false);
		this.panelBindingsHeader.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBindings).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHelp).EndInit();
		this.panelBindings.ResumeLayout(false);
		this.panelBindings.PerformLayout();
		this.tableBindings.ResumeLayout(false);
		this.tableBindings.PerformLayout();
		this.panelBacklightHeader.ResumeLayout(false);
		this.panelBacklightHeader.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBacklight).EndInit();
		this.panelBacklight.ResumeLayout(false);
		this.panelBacklight.PerformLayout();
		this.panelBacklightExtra.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericBacklightPluggedTime).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericBacklightTime).EndInit();
		this.panelXGM.ResumeLayout(false);
		this.panelXGM.PerformLayout();
		this.tableBacklight.ResumeLayout(false);
		this.panelSettingsHeader.ResumeLayout(false);
		this.panelSettingsHeader.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureScan).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureLog).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureSettings).EndInit();
		this.panelSettings.ResumeLayout(false);
		this.panelSettings.PerformLayout();
		this.panelPower.ResumeLayout(false);
		this.panelPower.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericHibernateAfter).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHibernate).EndInit();
		this.panelAPU.ResumeLayout(false);
		this.panelAPU.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureAPUMem).EndInit();
		this.panelCores.ResumeLayout(false);
		this.panelCores.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureCores).EndInit();
		this.panelACPI.ResumeLayout(false);
		this.panelACPI.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureDebug).EndInit();
		this.panelOptimalBrightness.ResumeLayout(false);
		this.panelOptimalBrightness.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureOptimalBrightness).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

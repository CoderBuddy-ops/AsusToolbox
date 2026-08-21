using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Automation;
using Asus.Fan;
using Asus.Helpers;
using Asus.Mode;
using Asus.Properties;
using Asus.UI;
using Asus.USB;
using PawnIO;

namespace Asus;

public class Fans : RForm
{
	private int curIndex = -1;


	private int _kbIndex;

	private int _chartTabDirection;
	private Panel chartGPU;
	private Panel chartCPU;
	private Panel chartMid;
	private Panel chartXGM;
	private object seriesCPU = new();
	private object seriesGPU = new();
	private object seriesMid = new();
	private object seriesXGM = new();


	private static bool gpuVisible = true;

	private static bool fanRpm = true;

	private static readonly Font _axisFont = new Font("Arial", 7f);

	private const int tempMin = 20;

	private const int tempMax = 110;

	private const int fansMax = 100;


	private ModeControl modeControl = Program.modeControl;

	private FanSensorControl fanSensorControl;

	private static int gpuPowerBase = 0;

	private static bool clampFanDots = AppConfig.IsClampFanDots();

	private static readonly string[] HysteresisLabels = new string[5]
	{
		Strings.VeryLow,
		Strings.Low,
		Strings.Medium,
		Strings.High,
		Strings.VeryHigh
	};

	private IContainer? components = null;

	private Panel panelFans;

	private Panel panelSliders;

	private TableLayoutPanel tableFanCharts;


	private Label labelTip;

	private Panel panelPower;

	private Panel panelCPU;

	private Label labelCPU;

	private Label labelLeftCPU;

	private RTrackBar trackCPU;

	private Panel panelTotal;

	private Label labelTotal;

	private Label labelLeftTotal;

	private RTrackBar trackTotal;

	private Panel panelTitleCPU;

	private PictureBox pictureBoxCPU;

	private Label labelPowerLimits;

	private Panel panelGPU;

	private Panel panelGPUMemory;

	private Label labelGPUMemory;

	private Label labelGPUMemoryTitle;

	private RTrackBar trackGPUMemory;

	private Panel panelGPUCore;

	private Label labelGPUCore;

	private RTrackBar trackGPUCore;

	private Label labelGPUCoreTitle;

	private Panel panelTitleGPU;

	private PictureBox pictureGPU;

	private Label labelGPU;

	private RCheckBox checkApplyPower;

	private Panel panelGPUBoost;

	private Label labelGPUBoost;

	private Label labelGPUBoostTitle;

	private RTrackBar trackGPUBoost;

	private Panel panelGPUTemp;

	private Label labelGPUTemp;

	private Label labelGPUTempTitle;

	private RTrackBar trackGPUTemp;

	private Panel panelTitleFans;

	private Panel panelApplyFans;

	private Label labelFansResult;

	private RCheckBox checkApplyFans;

	private RButton buttonReset;

	private Label labelBoost;

	private RComboBox comboBoost;

	private PictureBox picturePerf;

	private Label labelFans;

	private Panel panelFast;

	private Label labelFast;

	private Label labelLeftFast;

	private RTrackBar trackFast;

	private Panel panelBoost;

	private RComboBox comboModes;

	private RButton buttonAdd;

	private RButton buttonRemove;

	private RButton buttonRename;

	private Panel panelUV;

	private Label labelUV;

	private Label labelLeftUV;

	private RTrackBar trackUV;

	private PictureBox pictureUV;

	private Label labelTitleUV;

	private RButton buttonApplyAdvanced;

	private Panel panelAdvancedReadLimits;

	private RButton buttonReadLimits;

	private Panel panelApplyPower;

	private Panel panelAdvanced;

	private Panel panelAdvancedApply;

	private Panel panelTitleAdvanced;

	private Panel panelUViGPU;

	private Label labelUViGPU;

	private Label labelLeftUViGPU;

	private RTrackBar trackUViGPU;

	private Panel panelNav;

	private TableLayoutPanel tableNav;

	private RButton buttonCPU;

	private RButton buttonGPU;

	private RButton buttonAdvanced;

	private Panel panelBoostTitle;

	private PictureBox pictureBoost;

	private Label labelRisky;

	private Panel panelTitleTemp;

	private PictureBox pictureTemp;

	private Label labelTempLimit;

	private Panel panelTemperature;

	private Label labelTemp;

	private Label labelLeftTemp;

	private RTrackBar trackTemp;

	private Panel panelAdvancedAlways;

	private RCheckBox checkApplyUV;

	private Panel panelPowerMode;

	private RComboBox comboPowerMode;

	private Panel panelPowerModeTItle;

	private PictureBox picturePowerMode;

	private Label labelPowerModeTitle;

	private Panel panelGPUClockLimit;

	private Label labelGPUClockLimit;

	private RTrackBar trackGPUClockLimit;

	private Label labelGPUClockLimitTitle;

	private RButton buttonCalibrate;

	private Panel panelSlow;

	private Label labelSlow;

	private Label labelLeftSlow;

	private RTrackBar trackSlow;

	private Panel panelDownload;

	private RButton buttonDownload;

	private Panel panelPawnIO;

	private Panel panelGPUPower;

	private Label labelGPUPower;

	private Label labelGPUPowerTitle;

	private RTrackBar trackGPUPower;

	private TableLayoutPanel tableLayoutModes;

	private RCheckBox checkFanClamp;

	private Panel panelHysteresis;

	private TableLayoutPanel tableHysteresis;

	private Label labelHysteresisUp;

	private RTrackBar trackHysteresisUp;

	private Label labelHysteresisDown;

	private RTrackBar trackHysteresisDown;

	private Label labelHysteresisUpValue;

	private Label labelHysteresisDownValue;

	private static bool isGPUPower => gpuPowerBase > 0;

	public Fans()
	{
		InitializeComponent();
		fanSensorControl = new FanSensorControl(this);
		comboModes.ClientSize = new Size(comboModes.Width, comboModes.Height - 4);
		Text = Strings.FansAndPower;
		labelPowerLimits.Text = Strings.PowerLimits;
		checkApplyPower.Text = Strings.ApplyPowerLimits;
		labelFans.Text = "BIOS " + Strings.FanCurves;
		labelBoost.Text = Strings.CPUBoost;
		buttonReset.Text = Strings.FactoryDefaults;
		checkApplyFans.Text = Strings.ApplyFanCurve;
		labelGPU.Text = Strings.GPUSettings;
		labelGPUCoreTitle.Text = Strings.GPUCoreClockOffset;
		labelGPUMemoryTitle.Text = Strings.GPUMemoryClockOffset;
		labelGPUBoostTitle.Text = Strings.GPUBoost;
		labelGPUTempTitle.Text = Strings.GPUTempTarget;
		labelGPUPowerTitle.Text = Strings.GPUPower;
		labelRisky.Text = Strings.UndervoltingRisky;
		buttonApplyAdvanced.Text = Strings.Apply;
		checkApplyUV.Text = Strings.AutoApply;
		buttonCalibrate.Text = Strings.Calibrate;
		checkFanClamp.Text = Strings.ClampToGrid;
		labelHysteresisUp.Text = Strings.HysteresisUp;
		labelHysteresisDown.Text = Strings.HysteresisDown;
		buttonReadLimits.Text = Strings.ReadLimits;
		buttonDownload.Text = Strings.InstallPawnIODriver;
		InitTheme(setDPI: true);
		labelTip.Visible = false;
		labelTip.BackColor = Color.Transparent;
		chartCPU.MouseMove += delegate(object? sender, MouseEventArgs e)
		{
			ChartCPU_MouseMove(sender, e, AsusFan.CPU);
		};
		chartCPU.MouseUp += ChartCPU_MouseUp;
		chartCPU.MouseLeave += ChartCPU_MouseLeave;
		chartGPU.MouseMove += delegate(object? sender, MouseEventArgs e)
		{
			ChartCPU_MouseMove(sender, e, AsusFan.GPU);
		};
		chartGPU.MouseUp += ChartCPU_MouseUp;
		chartGPU.MouseLeave += ChartCPU_MouseLeave;
		chartMid.MouseMove += delegate(object? sender, MouseEventArgs e)
		{
			ChartCPU_MouseMove(sender, e, AsusFan.Mid);
		};
		chartMid.MouseUp += ChartCPU_MouseUp;
		chartMid.MouseLeave += ChartCPU_MouseLeave;
		chartXGM.MouseMove += delegate(object? sender, MouseEventArgs e)
		{
			ChartCPU_MouseMove(sender, e, AsusFan.XGM);
		};
		chartXGM.MouseUp += ChartCPU_MouseUp;
		chartXGM.MouseLeave += ChartCPU_MouseLeave;
		chartCPU.MouseClick += ChartCPU_MouseClick;
		chartGPU.MouseClick += ChartCPU_MouseClick;
		chartMid.MouseClick += ChartCPU_MouseClick;
		chartXGM.MouseClick += ChartCPU_MouseClick;
		chartCPU.TabStop = true;
		chartGPU.TabStop = true;
		chartMid.TabStop = true;
		chartXGM.TabStop = true;
		chartCPU.PreviewKeyDown += Chart_PreviewKeyDown;
		chartGPU.PreviewKeyDown += Chart_PreviewKeyDown;
		chartMid.PreviewKeyDown += Chart_PreviewKeyDown;
		chartXGM.PreviewKeyDown += Chart_PreviewKeyDown;
		chartCPU.KeyDown += delegate(object? s, KeyEventArgs e)
		{
			Chart_KeyDown(s, e, AsusFan.CPU);
		};
		chartGPU.KeyDown += delegate(object? s, KeyEventArgs e)
		{
			Chart_KeyDown(s, e, AsusFan.GPU);
		};
		chartMid.KeyDown += delegate(object? s, KeyEventArgs e)
		{
			Chart_KeyDown(s, e, AsusFan.Mid);
		};
		chartXGM.KeyDown += delegate(object? s, KeyEventArgs e)
		{
			Chart_KeyDown(s, e, AsusFan.XGM);
		};
		chartCPU.GotFocus += delegate(object? s, EventArgs e)
		{
			Chart_GotFocus(s, AsusFan.CPU);
		};
		chartGPU.GotFocus += delegate(object? s, EventArgs e)
		{
			Chart_GotFocus(s, AsusFan.GPU);
		};
		chartMid.GotFocus += delegate(object? s, EventArgs e)
		{
			Chart_GotFocus(s, AsusFan.Mid);
		};
		chartXGM.GotFocus += delegate(object? s, EventArgs e)
		{
			Chart_GotFocus(s, AsusFan.XGM);
		};
		chartCPU.LostFocus += Chart_LostFocus;
		chartGPU.LostFocus += Chart_LostFocus;
		chartMid.LostFocus += Chart_LostFocus;
		chartXGM.LostFocus += Chart_LostFocus;
		buttonReset.Click += ButtonReset_Click;
		trackTotal.Maximum = AsusACPI.MaxTotal;
		trackTotal.Minimum = 5;
		trackSlow.Maximum = AsusACPI.MaxTotal;
		trackSlow.Minimum = 5;
		trackCPU.Maximum = AsusACPI.MaxCPU;
		trackCPU.Minimum = 5;
		trackFast.Maximum = AsusACPI.MaxTotal;
		trackFast.Minimum = 5;
		trackTotal.Scroll += TrackTotal_Scroll;
		trackSlow.Scroll += TrackSlow_Scroll;
		trackFast.Scroll += TrackFast_Scroll;
		trackCPU.Scroll += TrackCPU_Scroll;
		trackFast.MouseUp += TrackPower_MouseUp;
		trackCPU.MouseUp += TrackPower_MouseUp;
		trackTotal.MouseUp += TrackPower_MouseUp;
		trackSlow.MouseUp += TrackPower_MouseUp;
		trackFast.KeyUp += TrackPower_KeyUp;
		trackCPU.KeyUp += TrackPower_KeyUp;
		trackTotal.KeyUp += TrackPower_KeyUp;
		trackSlow.KeyUp += TrackPower_KeyUp;
		checkApplyFans.Click += CheckApplyFans_Click;
		checkApplyPower.Click += CheckApplyPower_Click;
		trackGPUClockLimit.Maximum = 3000;
		trackGPUBoost.Minimum = 5;
		trackGPUBoost.Maximum = AsusACPI.MaxGPUBoost;
		trackGPUTemp.Minimum = 75;
		trackGPUTemp.Maximum = 87;
		trackGPUPower.Minimum = AsusACPI.MinGPUPower;
		trackGPUPower.Maximum = AsusACPI.MaxGPUPower;
		trackGPUClockLimit.Scroll += trackGPUClockLimit_Scroll;
		trackGPUCore.Scroll += trackGPU_Scroll;
		trackGPUMemory.Scroll += trackGPU_Scroll;
		trackGPUBoost.Scroll += trackGPUPower_Scroll;
		trackGPUTemp.Scroll += trackGPUPower_Scroll;
		trackGPUPower.Scroll += trackGPUPower_Scroll;
		trackGPUCore.MouseUp += TrackGPUClocks_MouseUp;
		trackGPUMemory.MouseUp += TrackGPUClocks_MouseUp;
		trackGPUClockLimit.MouseUp += TrackGPUClocks_MouseUp;
		trackGPUBoost.MouseUp += TrackGPU_MouseUp;
		trackGPUTemp.MouseUp += TrackGPU_MouseUp;
		trackGPUPower.MouseUp += TrackGPU_MouseUp;
		trackHysteresisUp.Scroll += TrackHysteresis_Scroll;
		trackHysteresisDown.Scroll += TrackHysteresis_Scroll;
		trackHysteresisUp.MouseUp += TrackHysteresis_MouseUp;
		trackHysteresisDown.MouseUp += TrackHysteresis_MouseUp;
		labelFansResult.Visible = false;
		trackUV.Minimum = CpuInfo.MinCPUUV;
		trackUV.Maximum = CpuInfo.MaxCPUUV;
		trackUViGPU.Minimum = CpuInfo.MinIGPUUV;
		trackUViGPU.Maximum = CpuInfo.MaxIGPUUV;
		trackTemp.Minimum = CpuInfo.MinTemp;
		trackTemp.Maximum = CpuInfo.DefaultTemp;
		comboPowerMode.DropDownStyle = ComboBoxStyle.DropDownList;
		comboPowerMode.DataSource = new BindingSource(PowerNative.powerModes, null);
		comboPowerMode.DisplayMember = "Value";
		comboPowerMode.ValueMember = "Key";
		FillModes(contextMenu: false);
		InitAll();
		InitCPU();
		comboBoost.SelectedValueChanged += ComboBoost_Changed;
		comboPowerMode.SelectedValueChanged += ComboPowerMode_Changed;
		comboModes.SelectionChangeCommitted += ComboModes_SelectedValueChanged;
		comboModes.TextChanged += ComboModes_TextChanged;
		comboModes.KeyPress += ComboModes_KeyPress;
		base.Shown += Fans_Shown;
		buttonAdd.Click += ButtonAdd_Click;
		buttonRemove.Click += ButtonRemove_Click;
		buttonRename.Click += ButtonRename_Click;
		trackUV.Scroll += TrackUV_Scroll;
		trackUViGPU.Scroll += TrackUV_Scroll;
		trackTemp.Scroll += TrackUV_Scroll;
		buttonApplyAdvanced.Click += ButtonApplyAdvanced_Click;
		buttonReadLimits.Click += ButtonReadLimits_Click;
		buttonCPU.BorderColor = RForm.colorStandard;
		buttonGPU.BorderColor = RForm.colorTurbo;
		buttonAdvanced.BorderColor = Color.Gray;
		buttonCPU.Click += ButtonCPU_Click;
		buttonGPU.Click += ButtonGPU_Click;
		buttonAdvanced.Click += ButtonAdvanced_Click;
		checkApplyUV.Click += CheckApplyUV_Click;
		buttonCalibrate.Click += ButtonCalibrate_Click;
		buttonDownload.Click += ButtonDownload_Click;
		checkFanClamp.Checked = clampFanDots;
		checkFanClamp.Click += CheckFanClamp_Click;
		ToggleNavigation();
		if (!Program.acpi.IsSupported(1114148u))
		{
			buttonCalibrate.Visible = false;
		}
		gpuPowerBase = Program.acpi.DeviceGet(1179801u);
		panelGPUPower.Visible = isGPUPower;
		base.FormClosed += Fans_FormClosed;
		base.Activated += delegate
		{
			VisualiseAdvanced();
		};
		trackUV.AccessibleName = labelLeftUV.Text;
		trackUViGPU.AccessibleName = labelLeftUViGPU.Text;
		trackTemp.AccessibleName = labelLeftTemp.Text;
		trackGPUCore.AccessibleName = labelGPUCoreTitle.Text;
		trackGPUMemory.AccessibleName = labelGPUMemoryTitle.Text;
		trackGPUBoost.AccessibleName = labelGPUBoostTitle.Text;
		trackGPUTemp.AccessibleName = labelGPUTempTitle.Text;
		trackGPUPower.AccessibleName = labelGPUPowerTitle.Text;
		trackGPUClockLimit.AccessibleName = labelGPUClockLimitTitle.Text;
		trackHysteresisUp.AccessibleName = labelHysteresisUp.Text;
		trackHysteresisDown.AccessibleName = labelHysteresisDown.Text;
		chartCPU.AccessibleName = "CPU fan curve";
		chartGPU.AccessibleName = "GPU fan curve";
		chartMid.AccessibleName = "Mid fan curve";
		chartXGM.AccessibleName = "XG Mobile fan curve";
	}

	private void CheckFanClamp_Click(object? sender, EventArgs e)
	{
		clampFanDots = checkFanClamp.Checked;
		AppConfig.Set("fan_clamp", clampFanDots ? 1 : 0);
	}

	private void ButtonDownload_Click(object? sender, EventArgs e)
	{
		Process.Start(new ProcessStartInfo("https://pawnio.eu/")
		{
			UseShellExecute = true
		});
	}

	private void ButtonCalibrate_Click(object? sender, EventArgs e)
	{
		buttonCalibrate.Enabled = false;
		fanSensorControl.StartCalibration();
	}

	private void ChartCPU_MouseClick(object? sender, MouseEventArgs e)
	{
		// Ultralight: chart removed
	}

	private void Fans_FormClosed(object? sender, FormClosedEventArgs e)
	{
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
	}

	private void CheckApplyUV_Click(object? sender, EventArgs e)
	{
		AppConfig.SetMode("auto_uv", checkApplyUV.Checked ? 1 : 0);
		modeControl.AutoRyzen();
	}

	public void InitAll()
	{
		InitMode();
		InitFans();
		InitPower();
		InitPowerPlan();
		InitUV();
		InitGPU();
	}

	public void InitCPU()
	{
		string name = CpuInfo.Name;
		if (name.Length > 0)
		{
			Text = Strings.FansAndPower + " - " + name;
		}
	}

	public void ToggleNavigation(int index = 0)
	{
		SuspendLayout();
		buttonCPU.Activated = false;
		buttonGPU.Activated = false;
		buttonAdvanced.Activated = false;
		panelPower.Visible = false;
		panelGPU.Visible = false;
		panelAdvanced.Visible = false;
		switch (index)
		{
		case 1:
			buttonGPU.Activated = true;
			panelGPU.Visible = true;
			break;
		case 2:
			buttonAdvanced.Activated = true;
			panelAdvanced.Visible = true;
			break;
		default:
			buttonCPU.Activated = true;
			panelPower.Visible = true;
			break;
		}
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private void ButtonAdvanced_Click(object? sender, EventArgs e)
	{
		ToggleNavigation(2);
	}

	private void ButtonGPU_Click(object? sender, EventArgs e)
	{
		ToggleNavigation(1);
	}

	private void ButtonCPU_Click(object? sender, EventArgs e)
	{
		ToggleNavigation();
	}

	private void ButtonApplyAdvanced_Click(object? sender, EventArgs e)
	{
		string text = modeControl.SetRyzen(launchAsAdmin: true);
		checkApplyUV.Enabled = true;
		ShowLabelRisky(text);
	}

	private void ButtonReadLimits_Click(object? sender, EventArgs e)
	{
		ShowLabelRisky(modeControl.ReadRyzenLimits());
	}

	private void ShowLabelRisky(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			labelRisky.Text = text;
			labelRisky.Visible = true;
		}
	}

	public void InitUV()
	{
		int value = Math.Max(trackUV.Minimum, Math.Min(trackUV.Maximum, AppConfig.GetMode("cpu_uv", 0)));
		int value2 = Math.Max(trackUViGPU.Minimum, Math.Min(trackUViGPU.Maximum, AppConfig.GetMode("igpu_uv", 0)));
		int num = AppConfig.GetMode("cpu_temp");
		if (num < CpuInfo.MinTemp || num > CpuInfo.DefaultTemp)
		{
			num = CpuInfo.DefaultTemp;
		}
		RCheckBox rCheckBox = checkApplyUV;
		bool enabled = (checkApplyUV.Checked = AppConfig.IsApplyUV());
		rCheckBox.Enabled = enabled;
		trackUV.Value = value;
		trackUViGPU.Value = value2;
		trackTemp.Value = num;
		VisualiseAdvanced();
		buttonAdvanced.Visible = CpuInfo.IsAMD;
	}

	private void VisualiseAdvanced()
	{
		bool flag = ModeControl.IsPawnAvailable() || ModeControl.IsPawnInstalled();
		panelPawnIO.Visible = flag;
		panelDownload.Visible = !flag;
		if (flag)
		{
			panelTitleAdvanced.Visible = CpuInfo.IsSupportedUV();
			labelRisky.Visible = CpuInfo.IsSupportedUV();
			panelUV.Visible = CpuInfo.IsSupportedUV();
			panelUViGPU.Visible = CpuInfo.IsSupportedUViGPU();
		}
		labelUV.Text = trackUV.Value.ToString();
		labelUViGPU.Text = trackUViGPU.Value.ToString();
		labelTemp.Text = ((trackTemp.Value < CpuInfo.DefaultTemp) ? TempHelper.FormatTemp(trackTemp.Value) : "Default");
	}

	private void AdvancedScroll()
	{
		AppConfig.SetMode("auto_uv", 0);
		RCheckBox rCheckBox = checkApplyUV;
		bool enabled = (checkApplyUV.Checked = false);
		rCheckBox.Enabled = enabled;
		VisualiseAdvanced();
		AppConfig.SetMode("cpu_temp", trackTemp.Value);
		AppConfig.SetMode("cpu_uv", trackUV.Value);
		AppConfig.SetMode("igpu_uv", trackUViGPU.Value);
	}

	private void TrackUV_Scroll(object? sender, EventArgs e)
	{
		AdvancedScroll();
	}

	private void ComboModes_KeyPress(object? sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			RenameToggle();
		}
	}

	private void ComboModes_TextChanged(object? sender, EventArgs e)
	{
		if (comboModes.DropDownStyle != ComboBoxStyle.DropDownList && Modes.IsCurrentCustom())
		{
			AppConfig.SetMode("mode_name", comboModes.Text);
		}
	}

	private void RenameToggle()
	{
		if (comboModes.DropDownStyle == ComboBoxStyle.DropDownList)
		{
			comboModes.DropDownStyle = ComboBoxStyle.DropDown;
			return;
		}
		int current = Modes.GetCurrent();
		FillModes();
		comboModes.SelectedValue = current;
	}

	private void ButtonRename_Click(object? sender, EventArgs e)
	{
		RenameToggle();
	}

	private void ButtonRemove_Click(object? sender, EventArgs e)
	{
		int current = Modes.GetCurrent();
		if (Modes.IsCurrentCustom())
		{
			Modes.Remove(current);
			FillModes();
			modeControl.SetPerformanceMode(0);
		}
	}

	private void FillModes(bool contextMenu = true)
	{
		comboModes.DropDownStyle = ComboBoxStyle.DropDownList;
		comboModes.DataSource = new BindingSource(Modes.GetDictonary(), null);
		comboModes.DisplayMember = "Value";
		comboModes.ValueMember = "Key";
		if (contextMenu)
		{
			Program.settingsForm.SetContextMenu();
		}
	}

	private void ButtonAdd_Click(object? sender, EventArgs e)
	{
		int mode = Modes.Add();
		FillModes();
		modeControl.SetPerformanceMode(mode);
	}

	public void InitMode()
	{
		int current = Modes.GetCurrent();
		comboModes.SelectedValue = current;
		RButton rButton = buttonRename;
		bool visible = (buttonRemove.Visible = Modes.IsCurrentCustom());
		rButton.Visible = visible;
	}

	private void ComboModes_SelectedValueChanged(object? sender, EventArgs e)
	{
		object selectedValue = comboModes.SelectedValue;
		if (selectedValue != null && (int)selectedValue != Modes.GetCurrent())
		{
			modeControl.SetPerformanceMode((int)selectedValue);
		}
	}

	private void TrackGPU_MouseUp(object? sender, MouseEventArgs e)
	{
		modeControl.SetGPUPower();
	}

	private void TrackGPUClocks_MouseUp(object? sender, MouseEventArgs e)
	{
		modeControl.SetGPUClocks();
	}

	private void InitGPUPower()
	{
		if (!isGPUPower)
		{
			return;
		}
		int maxGPUPower = 0; // Ultralight: NvidiaSmi removed
		if (maxGPUPower > 0)
		{
			AsusACPI.MaxGPUPower = maxGPUPower - gpuPowerBase - AsusACPI.MaxGPUBoost;
			trackGPUPower.Minimum = AsusACPI.MinGPUPower;
			trackGPUPower.Maximum = AsusACPI.MaxGPUPower;
		}
		Task.Run(async delegate
		{
			await Task.Delay(TimeSpan.FromMilliseconds(200.0));
			int num = Program.acpi.DeviceGet(1179800u);
			Logger.WriteLine($"ReadGPUPower ({Modes.GetCurrentBase()}): {gpuPowerBase} + {num}");
			int gpu_power = AppConfig.GetMode("gpu_power");
			if (gpu_power < 0)
			{
				gpu_power = ((num >= 0) ? num : AsusACPI.MaxGPUPower);
			}
			Invoke(delegate
			{
				trackGPUPower.Value = Math.Max(Math.Min(gpu_power, AsusACPI.MaxGPUPower), AsusACPI.MinGPUPower);
				VisualiseGPUSettings();
			});
		});
	}

	public void InitGPU()
	{
		Task.Run(delegate
		{
			if (Program.acpi.DeviceGet(AsusACPI.GPUEco) == 1)
			{
				Invoke(delegate
				{
					bool flag2 = (buttonGPU.Visible = false);
					gpuVisible = flag2;
				});
			}			else
			{
				// Ultralight: GPU control removed
				Invoke(delegate
				{
					bool flag2 = (buttonGPU.Visible = false);
					gpuVisible = flag2;
				});
			}
		});
	}

	private void VisualiseGPUSettings()
	{
		labelGPUCore.Text = $"{trackGPUCore.Value} MHz";
		labelGPUMemory.Text = $"{trackGPUMemory.Value} MHz";
		labelGPUBoost.Text = $"{trackGPUBoost.Value}W";
		labelGPUTemp.Text = TempHelper.FormatTemp(trackGPUTemp.Value);
		if (trackGPUClockLimit.Value >= 3000)
		{
			labelGPUClockLimit.Text = "Default";
		}
		else
		{
			labelGPUClockLimit.Text = $"{trackGPUClockLimit.Value} MHz";
		}
		labelGPUPower.Text = gpuPowerBase + trackGPUPower.Value + "W";
	}

	private void VisualiseHysteresis()
	{
		labelHysteresisUpValue.Text = HysteresisLabels[trackHysteresisUp.Value - 1];
		labelHysteresisDownValue.Text = HysteresisLabels[trackHysteresisDown.Value - 1];
	}

	private void InitHysteresis()
	{
		(int, int) fanHysteresis = Program.acpi.GetFanHysteresis();
		if (fanHysteresis.Item1 < 0 || fanHysteresis.Item2 < 0)
		{
			panelHysteresis.Visible = false;
			return;
		}
		panelHysteresis.Visible = true;
		int num = AppConfig.GetMode("hysteresis_up");
		int num2 = AppConfig.GetMode("hysteresis_down");
		if (num < 0)
		{
			int num3;
			if (fanHysteresis.Item1 > 0)
			{
				(num3, _) = fanHysteresis;
			}
			else
			{
				num3 = 3;
			}
			num = num3;
		}
		if (num2 < 0)
		{
			num2 = ((fanHysteresis.Item2 > 0) ? fanHysteresis.Item2 : 3);
		}
		trackHysteresisUp.Value = Math.Clamp(num, trackHysteresisUp.Minimum, trackHysteresisUp.Maximum);
		trackHysteresisDown.Value = Math.Clamp(num2, trackHysteresisDown.Minimum, trackHysteresisDown.Maximum);
		VisualiseHysteresis();
	}

	private void TrackHysteresis_Scroll(object? sender, EventArgs e)
	{
		AppConfig.SetMode("hysteresis_up", trackHysteresisUp.Value);
		AppConfig.SetMode("hysteresis_down", trackHysteresisDown.Value);
		VisualiseHysteresis();
	}

	private void TrackHysteresis_MouseUp(object? sender, MouseEventArgs e)
	{
		Program.acpi.SetFanHysteresis(trackHysteresisUp.Value, trackHysteresisDown.Value);
	}

	private void trackGPUClockLimit_Scroll(object? sender, EventArgs e)
	{
		int value = (int)Math.Round((float)trackGPUClockLimit.Value / 5f) * 5;
		trackGPUClockLimit.Value = value;
		AppConfig.SetMode("gpu_clock_limit", value);
		VisualiseGPUSettings();
	}

	private void trackGPU_Scroll(object? sender, EventArgs e)
	{
		if (sender != null)
		{
			TrackBar obj = (TrackBar)sender;
			obj.Value = (int)Math.Round((float)obj.Value / 5f) * 5;
			AppConfig.SetMode("gpu_core", trackGPUCore.Value);
			AppConfig.SetMode("gpu_memory", trackGPUMemory.Value);
			VisualiseGPUSettings();
		}
	}

	private void trackGPUPower_Scroll(object? sender, EventArgs e)
	{
		AppConfig.SetMode("gpu_boost", trackGPUBoost.Value);
		AppConfig.SetMode("gpu_temp", trackGPUTemp.Value);
		if (isGPUPower)
		{
			AppConfig.SetMode("gpu_power", trackGPUPower.Value);
		}
		VisualiseGPUSettings();
	}

	private static string ChartYLabel(int percentage, AsusFan device, string unit = "")
	{
		if (percentage == 0)
		{
			return "OFF";
		}
		int fanMin = FanSensorControl.GetFanMin(device);
		int fanMax = FanSensorControl.GetFanMax(device);
		if (fanRpm)
		{
			return 200.0 * Math.Floor((float)(fanMin * 100 + (fanMax - fanMin) * percentage) / 200f) + unit;
		}
		return percentage + "%";
	}

	private void SetAxis(object chart, AsusFan device) { }

	private void SetChart(object chart, AsusFan device) { }

	public void FormPosition()
	{
		if (base.Height > Program.settingsForm.Height)
		{
			base.Top = Math.Max(0, Program.settingsForm.Top + Program.settingsForm.Height - base.Height);
		}
		else
		{
			Size size2 = (MinimumSize = new Size(0, Program.settingsForm.Height));
			base.Size = size2;
			base.Height = Program.settingsForm.Height;
			base.Top = Program.settingsForm.Top;
		}
		base.Left = Program.settingsForm.Left - base.Width - 5;
	}

	private void Fans_Shown(object? sender, EventArgs e)
	{
		FormPosition();
	}

	private void TrackPower_MouseUp(object? sender, MouseEventArgs e)
	{
		Task.Run(delegate
		{
			modeControl.AutoPower(launchAsAdmin: true);
		});
	}

	private void TrackPower_KeyUp(object? sender, KeyEventArgs e)
	{
		Task.Run(delegate
		{
			modeControl.AutoPower(launchAsAdmin: true);
		});
	}

	public void InitPowerPlan()
	{
		int cPUBoost = PowerNative.GetCPUBoost();
		if (cPUBoost >= 0)
		{
			comboBoost.SelectedIndex = Math.Min(cPUBoost, comboBoost.Items.Count - 1);
		}
		string powerMode = PowerNative.GetPowerMode();
		bool batterySaverStatus = PowerNative.GetBatterySaverStatus();
		comboPowerMode.Enabled = !batterySaverStatus;
		if (batterySaverStatus)
		{
			comboPowerMode.SelectedIndex = 0;
		}
		else
		{
			comboPowerMode.SelectedValue = powerMode;
		}
	}

	private void ComboPowerMode_Changed(object? sender, EventArgs e)
	{
		string text = (string)comboPowerMode.SelectedValue;
		PowerNative.SetPowerMode(text);
		if (PowerNative.GetDefaultPowerMode(Modes.GetCurrentBase()) != text)
		{
			AppConfig.SetMode("powermode", text);
		}
		else
		{
			AppConfig.RemoveMode("powermode");
		}
	}

	private void ComboBoost_Changed(object? sender, EventArgs e)
	{
		if (AppConfig.GetMode("auto_boost") != comboBoost.SelectedIndex)
		{
			PowerNative.SetCPUBoost(comboBoost.SelectedIndex);
		}
		AppConfig.SetMode("auto_boost", comboBoost.SelectedIndex);
	}

	private void CheckApplyPower_Click(object? sender, EventArgs e)
	{
		if (sender != null)
		{
			CheckBox checkBox = (CheckBox)sender;
			AppConfig.SetMode("auto_apply_power", checkBox.Checked ? 1 : 0);
			modeControl.SetPerformanceMode();
		}
	}

	private void CheckApplyFans_Click(object? sender, EventArgs e)
	{
		if (sender != null)
		{
			CheckBox checkBox = (CheckBox)sender;
			AppConfig.SetMode("auto_apply", checkBox.Checked ? 1 : 0);
			modeControl.SetPerformanceMode();
		}
	}

	public void InitAxis()
	{
		if (this == null || Text == "")
		{
			return;
		}
		Invoke(delegate
		{
			buttonCalibrate.Enabled = true;
			SetAxis(chartCPU, AsusFan.CPU);
			SetAxis(chartGPU, AsusFan.GPU);
			if (chartMid.Visible)
			{
				SetAxis(chartMid, AsusFan.Mid);
			}
		});
	}

	public void LabelFansResult(string text)
	{
		if (text.Length > 0)
		{
			Logger.WriteLine(text);
		}
		if (base.IsDisposed || !base.IsHandleCreated || Text == "")
		{
			return;
		}
		try
		{
			BeginInvoke(delegate
			{
				labelFansResult.Text = text;
				labelFansResult.Visible = text.Length > 0;
			});
		}
		catch (ObjectDisposedException)
		{
		}
	}

	public void InitPower()
	{
		bool flag = Program.acpi.IsSupported(1179808u) || CpuInfo.IsAMD;
		bool flag2 = Program.acpi.IsAllAmdPPT();
		bool flag3 = Program.acpi.IsSupported(1179841u);
		panelTotal.Visible = flag;
		panelCPU.Visible = flag2;
		Panel panel = panelApplyPower;
		bool visible = (panelTitleCPU.Visible = flag || flag2 || flag3);
		panel.Visible = visible;
		if (flag2)
		{
			labelLeftTotal.Text = "Platform (CPU + GPU)";
			labelLeftCPU.Text = "CPU";
			panelFast.Visible = false;
			panelSlow.Visible = false;
		}
		else
		{
			panelSlow.Visible = flag;
			if (CpuInfo.IsAMD)
			{
				labelLeftTotal.Text = "SPL (CPU sustained)";
				labelLeftSlow.Text = "sPPT (CPU long boost)";
				labelLeftFast.Text = "fPPT (CPU short boost)";
				panelFast.Visible = flag3;
			}
			else
			{
				labelLeftTotal.Text = "PL1 (CPU sustained)";
				labelLeftSlow.Text = "PL2 (CPU long boost)";
				panelFast.Visible = false;
			}
		}
		checkApplyPower.Checked = AppConfig.IsApplyPower();
		int num = AppConfig.GetMode("limit_total", AsusACPI.DefaultTotal);
		int num2 = AppConfig.GetMode("limit_slow", num);
		int num3 = AppConfig.GetMode("limit_fast", num);
		int num4 = AppConfig.GetMode("limit_cpu", 80);
		if (num > AsusACPI.MaxTotal)
		{
			num = AsusACPI.MaxTotal;
		}
		if (num < 5)
		{
			num = 5;
		}
		if (num4 > AsusACPI.MaxCPU)
		{
			num4 = AsusACPI.MaxCPU;
		}
		if (num4 < 5)
		{
			num4 = 5;
		}
		if (num2 > AsusACPI.MaxTotal)
		{
			num2 = AsusACPI.MaxTotal;
		}
		if (num2 < 5)
		{
			num2 = 5;
		}
		if (num3 > AsusACPI.MaxTotal)
		{
			num3 = AsusACPI.MaxTotal;
		}
		if (num3 < 5)
		{
			num3 = 5;
		}
		trackTotal.Value = num;
		trackSlow.Value = num2;
		trackCPU.Value = num4;
		trackFast.Value = num3;
		trackTotal.AccessibleName = labelLeftTotal.Text;
		trackSlow.AccessibleName = labelLeftSlow.Text;
		trackFast.AccessibleName = labelLeftFast.Text;
		trackCPU.AccessibleName = labelLeftCPU.Text;
		SavePower();
	}

	private void SavePower()
	{
		labelTotal.Text = trackTotal.Value + "W";
		labelSlow.Text = trackSlow.Value + "W";
		labelCPU.Text = trackCPU.Value + "W";
		labelFast.Text = trackFast.Value + "W";
		AppConfig.SetMode("limit_total", trackTotal.Value);
		AppConfig.SetMode("limit_slow", trackSlow.Value);
		AppConfig.SetMode("limit_cpu", trackCPU.Value);
		AppConfig.SetMode("limit_fast", trackFast.Value);
	}

	private void TrackTotal_Scroll(object? sender, EventArgs e)
	{
		if (trackTotal.Value > trackSlow.Value)
		{
			trackSlow.Value = trackTotal.Value;
		}
		if (trackTotal.Value > trackFast.Value)
		{
			trackFast.Value = trackTotal.Value;
		}
		if (trackTotal.Value < trackCPU.Value)
		{
			trackCPU.Value = trackTotal.Value;
		}
		SavePower();
	}

	private void TrackSlow_Scroll(object? sender, EventArgs e)
	{
		if (trackSlow.Value < trackTotal.Value)
		{
			trackTotal.Value = trackSlow.Value;
		}
		if (trackSlow.Value > trackFast.Value)
		{
			trackFast.Value = trackSlow.Value;
		}
		SavePower();
	}

	private void TrackFast_Scroll(object? sender, EventArgs e)
	{
		if (trackFast.Value < trackSlow.Value)
		{
			trackSlow.Value = trackFast.Value;
		}
		if (trackFast.Value < trackTotal.Value)
		{
			trackTotal.Value = trackFast.Value;
		}
		SavePower();
	}

	private void TrackCPU_Scroll(object? sender, EventArgs e)
	{
		if (trackCPU.Value > trackTotal.Value)
		{
			trackTotal.Value = trackCPU.Value;
		}
		SavePower();
	}

	public void InitFans()
	{
		int num = 2;
		if (!AsusACPI.IsEmptyCurve(Program.acpi.GetFanCurve(AsusFan.Mid)) || Program.acpi.IsMidFanSupported())
		{
			AppConfig.Set("mid_fan", 1);
			num++;
			chartMid.Visible = true;
			SetChart(chartMid, AsusFan.Mid);
			LoadProfile(seriesMid, AsusFan.Mid);
		}
		else
		{
			AppConfig.Set("mid_fan", 0);
		}
		if (Program.acpi.IsXGConnected() || XGM.IsConnected())
		{
			AppConfig.Set("xgm_fan", 1);
			num++;
			chartXGM.Visible = true;
			SetChart(chartXGM, AsusFan.XGM);
			LoadProfile(seriesXGM, AsusFan.XGM);
		}
		else
		{
			AppConfig.Set("xgm_fan", 0);
		}
		try
		{
			if (num > 2)
			{
				Size size2 = (MinimumSize = new Size(base.Size.Width, Math.Max(MinimumSize.Height, (int)(ControlHelper.GetDpiScale(this).Value * (float)(num * 200 + 100)))));
				base.Size = size2;
			}
		}
		catch (Exception)
		{
		}
		SetChart(chartCPU, AsusFan.CPU);
		SetChart(chartGPU, AsusFan.GPU);
		LoadProfile(seriesCPU, AsusFan.CPU);
		LoadProfile(seriesGPU, AsusFan.GPU);
		bool num2 = AppConfig.IsApplyPower() && AppConfig.IsFanRequired();
		bool flag = AppConfig.IsApplyFans();
		checkApplyFans.Checked = flag;
		if (num2 || flag)
		{
		}
		else
		{
		}
		InitHysteresis();
	}

	private void LoadProfile(object series, AsusFan device, bool reset = false)
	{
		// Ultralight: chart removed
	}

	private void SaveProfile(object series, AsusFan device)
	{
		// Ultralight: chart removed
	}

	private void ButtonReset_Click(object? sender, EventArgs e)
	{
		LoadProfile(seriesCPU, AsusFan.CPU, reset: true);
		LoadProfile(seriesGPU, AsusFan.GPU, reset: true);
		if (AppConfig.Is("mid_fan"))
		{
			LoadProfile(seriesMid, AsusFan.Mid, reset: true);
		}
		if (AppConfig.Is("xgm_fan"))
		{
			LoadProfile(seriesXGM, AsusFan.XGM, reset: true);
		}
		checkApplyFans.Checked = false;
		checkApplyPower.Checked = false;
		AppConfig.SetMode("auto_apply", 0);
		AppConfig.SetMode("auto_apply_power", 0);
		trackUV.Value = CpuInfo.MaxCPUUV;
		trackUViGPU.Value = CpuInfo.MaxIGPUUV;
		trackTemp.Value = CpuInfo.DefaultTemp;
		AdvancedScroll();
		AppConfig.RemoveMode("cpu_temp");
		modeControl.ResetPerformanceMode();
		InitPowerPlan();
		XGM.Reset();
		if (gpuVisible)
		{
			trackGPUClockLimit.Value = 3000;
			trackGPUCore.Value = 0;
			trackGPUMemory.Value = 0;
			trackGPUBoost.Value = AsusACPI.MaxGPUBoost;
			trackGPUTemp.Value = 87;
			AppConfig.RemoveMode("gpu_boost");
			AppConfig.RemoveMode("gpu_temp");
			AppConfig.RemoveMode("gpu_power");
			AppConfig.RemoveMode("gpu_clock_limit");
			AppConfig.RemoveMode("gpu_core");
			AppConfig.RemoveMode("gpu_memory");
			InitGPUPower();
			VisualiseGPUSettings();
			modeControl.SetGPUClocks(launchAsAdmin: true, reset: true);
			modeControl.SetGPUPower();
		}
		if (panelHysteresis.Visible)
		{
			AppConfig.RemoveMode("hysteresis_up");
			AppConfig.RemoveMode("hysteresis_down");
			InitHysteresis();
		}
	}

	private void Chart_Save()
	{
		// Ultralight: chart removed
	}

	private void ChartCPU_MouseUp(object? sender, MouseEventArgs e)
	{
		Chart_Save();
	}

	private void ChartCPU_MouseLeave(object? sender, EventArgs e)
	{
		// Ultralight: chart removed
	}

	private void Chart_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
	{
		int count = 8; // Ultralight: chart removed
		switch (e.KeyCode)
		{
		case Keys.End:
		case Keys.Home:
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
			e.IsInputKey = true;
			break;
		case Keys.Tab:
		{
			bool flag = e.Modifiers.HasFlag(Keys.Shift);
			if (flag ? (_kbIndex > 0) : (_kbIndex < count - 1))
			{
				e.IsInputKey = true;
			}
			else
			{
				_chartTabDirection = ((!flag) ? 1 : (-1));
			}
			break;
		}
		}
	}

	private void Chart_GotFocus(object? sender, AsusFan device)
	{
		// Ultralight: chart removed
	}

	private void Chart_LostFocus(object? sender, EventArgs e)
	{
		labelTip.Visible = false;
	}

	private void Chart_KeyDown(object? sender, KeyEventArgs e, AsusFan device)
	{
		// Ultralight: chart removed
	}

	private void Chart_AdjustPoint(int dx, int dy, object series, AsusFan device)
	{
		// Ultralight: chart removed
	}


	private void ChartCPU_MouseMove(object? sender, MouseEventArgs e, AsusFan device)
	{
		// Ultralight: chart removed
	}

	private void FanDragHint(bool show)
	{
		labelFansResult.Text = (show ? Strings.FanDragAll : "");
		labelFansResult.ForeColor = (show ? RForm.colorGray : RForm.colorTurbo);
		labelFansResult.Visible = show;
	}

	private void AdjustAll(double deltaX, double deltaY, object series)
	{
		// Ultralight: chart removed
	}

	private void AdjustAllLevels(int index, double curXVal, double curYVal, object series)
	{
		// Ultralight: chart removed
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
		// Ultralight: chartArea/Title removed
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Asus.Fans));
		this.panelFans = new System.Windows.Forms.Panel();
		this.checkFanClamp = new Asus.UI.RCheckBox();
		this.labelTip = new System.Windows.Forms.Label();
		this.tableFanCharts = new System.Windows.Forms.TableLayoutPanel();
		this.chartGPU = new Panel();
		this.chartCPU = new Panel();
		this.chartXGM = new Panel();
		this.chartMid = new Panel();
		this.panelTitleFans = new System.Windows.Forms.Panel();
		this.tableLayoutModes = new System.Windows.Forms.TableLayoutPanel();
		this.buttonRemove = new Asus.UI.RButton();
		this.buttonAdd = new Asus.UI.RButton();
		this.buttonRename = new Asus.UI.RButton();
		this.comboModes = new Asus.UI.RComboBox();
		this.picturePerf = new System.Windows.Forms.PictureBox();
		this.labelFans = new System.Windows.Forms.Label();
		this.panelHysteresis = new System.Windows.Forms.Panel();
		this.tableHysteresis = new System.Windows.Forms.TableLayoutPanel();
		this.labelHysteresisUp = new System.Windows.Forms.Label();
		this.trackHysteresisUp = new Asus.UI.RTrackBar();
		this.labelHysteresisDown = new System.Windows.Forms.Label();
		this.trackHysteresisDown = new Asus.UI.RTrackBar();
		this.labelHysteresisUpValue = new System.Windows.Forms.Label();
		this.labelHysteresisDownValue = new System.Windows.Forms.Label();
		this.panelApplyFans = new System.Windows.Forms.Panel();
		this.buttonCalibrate = new Asus.UI.RButton();
		this.labelFansResult = new System.Windows.Forms.Label();
		this.checkApplyFans = new Asus.UI.RCheckBox();
		this.buttonReset = new Asus.UI.RButton();
		this.comboBoost = new Asus.UI.RComboBox();
		this.panelSliders = new System.Windows.Forms.Panel();
		this.panelAdvanced = new System.Windows.Forms.Panel();
		this.panelAdvancedAlways = new System.Windows.Forms.Panel();
		this.checkApplyUV = new Asus.UI.RCheckBox();
		this.panelAdvancedApply = new System.Windows.Forms.Panel();
		this.buttonApplyAdvanced = new Asus.UI.RButton();
		this.panelAdvancedReadLimits = new System.Windows.Forms.Panel();
		this.buttonReadLimits = new Asus.UI.RButton();
		this.labelRisky = new System.Windows.Forms.Label();
		this.panelUViGPU = new System.Windows.Forms.Panel();
		this.labelUViGPU = new System.Windows.Forms.Label();
		this.labelLeftUViGPU = new System.Windows.Forms.Label();
		this.trackUViGPU = new Asus.UI.RTrackBar();
		this.panelUV = new System.Windows.Forms.Panel();
		this.labelUV = new System.Windows.Forms.Label();
		this.labelLeftUV = new System.Windows.Forms.Label();
		this.trackUV = new Asus.UI.RTrackBar();
		this.panelTitleAdvanced = new System.Windows.Forms.Panel();
		this.pictureUV = new System.Windows.Forms.PictureBox();
		this.labelTitleUV = new System.Windows.Forms.Label();
		this.panelTemperature = new System.Windows.Forms.Panel();
		this.labelTemp = new System.Windows.Forms.Label();
		this.labelLeftTemp = new System.Windows.Forms.Label();
		this.trackTemp = new Asus.UI.RTrackBar();
		this.panelTitleTemp = new System.Windows.Forms.Panel();
		this.pictureTemp = new System.Windows.Forms.PictureBox();
		this.labelTempLimit = new System.Windows.Forms.Label();
		this.panelDownload = new System.Windows.Forms.Panel();
		this.buttonDownload = new Asus.UI.RButton();
		this.panelPawnIO = new System.Windows.Forms.Panel();
		this.panelPower = new System.Windows.Forms.Panel();
		this.panelApplyPower = new System.Windows.Forms.Panel();
		this.checkApplyPower = new Asus.UI.RCheckBox();
		this.panelCPU = new System.Windows.Forms.Panel();
		this.labelCPU = new System.Windows.Forms.Label();
		this.labelLeftCPU = new System.Windows.Forms.Label();
		this.trackCPU = new Asus.UI.RTrackBar();
		this.panelFast = new System.Windows.Forms.Panel();
		this.labelFast = new System.Windows.Forms.Label();
		this.labelLeftFast = new System.Windows.Forms.Label();
		this.trackFast = new Asus.UI.RTrackBar();
		this.panelSlow = new System.Windows.Forms.Panel();
		this.labelSlow = new System.Windows.Forms.Label();
		this.labelLeftSlow = new System.Windows.Forms.Label();
		this.trackSlow = new Asus.UI.RTrackBar();
		this.panelTotal = new System.Windows.Forms.Panel();
		this.labelTotal = new System.Windows.Forms.Label();
		this.labelLeftTotal = new System.Windows.Forms.Label();
		this.trackTotal = new Asus.UI.RTrackBar();
		this.panelTitleCPU = new System.Windows.Forms.Panel();
		this.pictureBoxCPU = new System.Windows.Forms.PictureBox();
		this.labelPowerLimits = new System.Windows.Forms.Label();
		this.panelBoost = new System.Windows.Forms.Panel();
		this.panelBoostTitle = new System.Windows.Forms.Panel();
		this.pictureBoost = new System.Windows.Forms.PictureBox();
		this.labelBoost = new System.Windows.Forms.Label();
		this.panelPowerMode = new System.Windows.Forms.Panel();
		this.comboPowerMode = new Asus.UI.RComboBox();
		this.panelPowerModeTItle = new System.Windows.Forms.Panel();
		this.picturePowerMode = new System.Windows.Forms.PictureBox();
		this.labelPowerModeTitle = new System.Windows.Forms.Label();
		this.panelGPU = new System.Windows.Forms.Panel();
		this.panelGPUTemp = new System.Windows.Forms.Panel();
		this.labelGPUTemp = new System.Windows.Forms.Label();
		this.labelGPUTempTitle = new System.Windows.Forms.Label();
		this.trackGPUTemp = new Asus.UI.RTrackBar();
		this.panelGPUBoost = new System.Windows.Forms.Panel();
		this.labelGPUBoost = new System.Windows.Forms.Label();
		this.labelGPUBoostTitle = new System.Windows.Forms.Label();
		this.trackGPUBoost = new Asus.UI.RTrackBar();
		this.panelGPUPower = new System.Windows.Forms.Panel();
		this.labelGPUPower = new System.Windows.Forms.Label();
		this.labelGPUPowerTitle = new System.Windows.Forms.Label();
		this.trackGPUPower = new Asus.UI.RTrackBar();
		this.panelGPUMemory = new System.Windows.Forms.Panel();
		this.labelGPUMemory = new System.Windows.Forms.Label();
		this.labelGPUMemoryTitle = new System.Windows.Forms.Label();
		this.trackGPUMemory = new Asus.UI.RTrackBar();
		this.panelGPUCore = new System.Windows.Forms.Panel();
		this.labelGPUCore = new System.Windows.Forms.Label();
		this.trackGPUCore = new Asus.UI.RTrackBar();
		this.labelGPUCoreTitle = new System.Windows.Forms.Label();
		this.panelGPUClockLimit = new System.Windows.Forms.Panel();
		this.labelGPUClockLimit = new System.Windows.Forms.Label();
		this.trackGPUClockLimit = new Asus.UI.RTrackBar();
		this.labelGPUClockLimitTitle = new System.Windows.Forms.Label();
		this.panelTitleGPU = new System.Windows.Forms.Panel();
		this.pictureGPU = new System.Windows.Forms.PictureBox();
		this.labelGPU = new System.Windows.Forms.Label();
		this.panelNav = new System.Windows.Forms.Panel();
		this.tableNav = new System.Windows.Forms.TableLayoutPanel();
		this.buttonAdvanced = new Asus.UI.RButton();
		this.buttonGPU = new Asus.UI.RButton();
		this.buttonCPU = new Asus.UI.RButton();
		this.panelFans.SuspendLayout();
		this.tableFanCharts.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.chartGPU).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.chartCPU).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.chartXGM).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.chartMid).BeginInit();
		this.panelTitleFans.SuspendLayout();
		this.tableLayoutModes.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.picturePerf).BeginInit();
		this.panelHysteresis.SuspendLayout();
		this.tableHysteresis.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackHysteresisUp).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackHysteresisDown).BeginInit();
		this.panelApplyFans.SuspendLayout();
		this.panelSliders.SuspendLayout();
		this.panelAdvanced.SuspendLayout();
		this.panelAdvancedAlways.SuspendLayout();
		this.panelAdvancedApply.SuspendLayout();
		this.panelAdvancedReadLimits.SuspendLayout();
		this.panelUViGPU.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackUViGPU).BeginInit();
		this.panelUV.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackUV).BeginInit();
		this.panelTitleAdvanced.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureUV).BeginInit();
		this.panelTemperature.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackTemp).BeginInit();
		this.panelTitleTemp.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureTemp).BeginInit();
		this.panelDownload.SuspendLayout();
		this.panelPawnIO.SuspendLayout();
		this.panelPower.SuspendLayout();
		this.panelApplyPower.SuspendLayout();
		this.panelCPU.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackCPU).BeginInit();
		this.panelFast.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackFast).BeginInit();
		this.panelSlow.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackSlow).BeginInit();
		this.panelTotal.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackTotal).BeginInit();
		this.panelTitleCPU.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxCPU).BeginInit();
		this.panelBoost.SuspendLayout();
		this.panelBoostTitle.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBoost).BeginInit();
		this.panelPowerMode.SuspendLayout();
		this.panelPowerModeTItle.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.picturePowerMode).BeginInit();
		this.panelGPU.SuspendLayout();
		this.panelGPUTemp.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUTemp).BeginInit();
		this.panelGPUBoost.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUBoost).BeginInit();
		this.panelGPUPower.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUPower).BeginInit();
		this.panelGPUMemory.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUMemory).BeginInit();
		this.panelGPUCore.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUCore).BeginInit();
		this.panelGPUClockLimit.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUClockLimit).BeginInit();
		this.panelTitleGPU.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureGPU).BeginInit();
		this.panelNav.SuspendLayout();
		this.tableNav.SuspendLayout();
		base.SuspendLayout();
		this.panelFans.AutoSize = true;
		this.panelFans.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelFans.Controls.Add(this.checkFanClamp);
		this.panelFans.Controls.Add(this.labelTip);
		this.panelFans.Controls.Add(this.tableFanCharts);
		this.panelFans.Controls.Add(this.panelTitleFans);
		this.panelFans.Controls.Add(this.panelHysteresis);
		this.panelFans.Controls.Add(this.panelApplyFans);
		this.panelFans.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelFans.Location = new System.Drawing.Point(530, 0);
		this.panelFans.Margin = new System.Windows.Forms.Padding(0);
		this.panelFans.MinimumSize = new System.Drawing.Size(816, 0);
		this.panelFans.Name = "panelFans";
		this.panelFans.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
		this.panelFans.Size = new System.Drawing.Size(820, 1100);
		this.panelFans.TabIndex = 12;
		this.checkFanClamp.AutoSize = true;
		this.checkFanClamp.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.checkFanClamp.Location = new System.Drawing.Point(18, 80);
		this.checkFanClamp.Name = "checkFanClamp";
		this.checkFanClamp.Padding = new System.Windows.Forms.Padding(8, 1, 2, 1);
		this.checkFanClamp.Size = new System.Drawing.Size(193, 44);
		this.checkFanClamp.TabIndex = 5;
		this.checkFanClamp.TabStop = false;
		this.checkFanClamp.Text = "Clamp to Grid";
		this.checkFanClamp.UseVisualStyleBackColor = false;
		this.labelTip.AutoSize = true;
		this.labelTip.BackColor = System.Drawing.SystemColors.ControlLightLight;
		this.labelTip.Location = new System.Drawing.Point(684, 92);
		this.labelTip.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelTip.Name = "labelTip";
		this.labelTip.Padding = new System.Windows.Forms.Padding(4);
		this.labelTip.Size = new System.Drawing.Size(105, 40);
		this.labelTip.TabIndex = 2;
		this.labelTip.Text = "500,300";
		this.tableFanCharts.AutoSize = true;
		this.tableFanCharts.ColumnCount = 1;
		this.tableFanCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableFanCharts.Controls.Add(this.chartGPU, 0, 1);
		this.tableFanCharts.Controls.Add(this.chartCPU, 0, 0);
		this.tableFanCharts.Controls.Add(this.chartXGM, 0, 2);
		this.tableFanCharts.Controls.Add(this.chartMid, 0, 2);
		this.tableFanCharts.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableFanCharts.Location = new System.Drawing.Point(0, 66);
		this.tableFanCharts.Margin = new System.Windows.Forms.Padding(4);
		this.tableFanCharts.Name = "tableFanCharts";
		this.tableFanCharts.Padding = new System.Windows.Forms.Padding(10, 0, 10, 5);
		this.tableFanCharts.RowCount = 2;
		this.tableFanCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.tableFanCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.tableFanCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.tableFanCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25f));
		this.tableFanCharts.Size = new System.Drawing.Size(810, 918);
		this.tableFanCharts.TabIndex = 1;
		this.chartGPU.Dock = System.Windows.Forms.DockStyle.Fill;
		this.chartGPU.Location = new System.Drawing.Point(12, 238);
		this.chartGPU.Margin = new System.Windows.Forms.Padding(2, 10, 2, 10);
		this.chartGPU.Name = "chartGPU";
		this.chartGPU.Size = new System.Drawing.Size(786, 208);
		this.chartGPU.TabIndex = 1;
		this.chartGPU.Text = "chartGPU";
		this.chartCPU.Dock = System.Windows.Forms.DockStyle.Fill;
		this.chartCPU.Location = new System.Drawing.Point(12, 10);
		this.chartCPU.Margin = new System.Windows.Forms.Padding(2, 10, 2, 10);
		this.chartCPU.Name = "chartCPU";
		this.chartCPU.Size = new System.Drawing.Size(786, 208);
		this.chartCPU.TabIndex = 0;
		this.chartCPU.Text = "chartCPU";
		this.chartXGM.Dock = System.Windows.Forms.DockStyle.Fill;
		this.chartXGM.Location = new System.Drawing.Point(12, 694);
		this.chartXGM.Margin = new System.Windows.Forms.Padding(2, 10, 2, 10);
		this.chartXGM.Name = "chartXGM";
		this.chartXGM.Size = new System.Drawing.Size(786, 209);
		this.chartXGM.TabIndex = 3;
		this.chartXGM.Text = "chartXGM";
		this.chartXGM.Visible = false;
		this.chartMid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.chartMid.Location = new System.Drawing.Point(12, 466);
		this.chartMid.Margin = new System.Windows.Forms.Padding(2, 10, 2, 10);
		this.chartMid.Name = "chartMid";
		this.chartMid.Size = new System.Drawing.Size(786, 208);
		this.chartMid.TabIndex = 2;
		this.chartMid.Text = "chartMid";
		this.chartMid.Visible = false;
		this.panelTitleFans.Controls.Add(this.tableLayoutModes);
		this.panelTitleFans.Controls.Add(this.picturePerf);
		this.panelTitleFans.Controls.Add(this.labelFans);
		this.panelTitleFans.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTitleFans.Location = new System.Drawing.Point(0, 0);
		this.panelTitleFans.Margin = new System.Windows.Forms.Padding(4);
		this.panelTitleFans.Name = "panelTitleFans";
		this.panelTitleFans.Size = new System.Drawing.Size(810, 66);
		this.panelTitleFans.TabIndex = 0;
		this.tableLayoutModes.ColumnCount = 4;
		this.tableLayoutModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60f));
		this.tableLayoutModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60f));
		this.tableLayoutModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60f));
		this.tableLayoutModes.Controls.Add(this.buttonRemove, 0, 0);
		this.tableLayoutModes.Controls.Add(this.buttonAdd, 3, 0);
		this.tableLayoutModes.Controls.Add(this.buttonRename, 1, 0);
		this.tableLayoutModes.Controls.Add(this.comboModes, 2, 0);
		this.tableLayoutModes.Dock = System.Windows.Forms.DockStyle.Right;
		this.tableLayoutModes.Location = new System.Drawing.Point(330, 0);
		this.tableLayoutModes.Margin = new System.Windows.Forms.Padding(0);
		this.tableLayoutModes.Name = "tableLayoutModes";
		this.tableLayoutModes.Padding = new System.Windows.Forms.Padding(0, 8, 4, 10);
		this.tableLayoutModes.RowCount = 1;
		this.tableLayoutModes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutModes.Size = new System.Drawing.Size(480, 66);
		this.tableLayoutModes.TabIndex = 1;
		this.buttonRemove.Activated = false;
		this.buttonRemove.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonRemove.BorderColor = System.Drawing.Color.Transparent;
		this.buttonRemove.BorderRadius = 2;
		this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonRemove.Image = Asus.Properties.Resources.icons8_remove_64;
		this.buttonRemove.Location = new System.Drawing.Point(0, 10);
		this.buttonRemove.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
		this.buttonRemove.Name = "buttonRemove";
		this.buttonRemove.Secondary = true;
		this.buttonRemove.Size = new System.Drawing.Size(54, 46);
		this.buttonRemove.TabIndex = 0;
		this.buttonRemove.UseVisualStyleBackColor = false;
		this.buttonAdd.Activated = false;
		this.buttonAdd.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonAdd.BorderColor = System.Drawing.Color.Transparent;
		this.buttonAdd.BorderRadius = 2;
		this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonAdd.Image = Asus.Properties.Resources.icons8_add_64;
		this.buttonAdd.Location = new System.Drawing.Point(416, 10);
		this.buttonAdd.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
		this.buttonAdd.Name = "buttonAdd";
		this.buttonAdd.Secondary = true;
		this.buttonAdd.Size = new System.Drawing.Size(54, 46);
		this.buttonAdd.TabIndex = 3;
		this.buttonAdd.UseVisualStyleBackColor = false;
		this.buttonRename.Activated = false;
		this.buttonRename.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonRename.BorderColor = System.Drawing.Color.Transparent;
		this.buttonRename.BorderRadius = 2;
		this.buttonRename.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonRename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonRename.Image = Asus.Properties.Resources.icons8_edit_32;
		this.buttonRename.Location = new System.Drawing.Point(60, 10);
		this.buttonRename.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
		this.buttonRename.Name = "buttonRename";
		this.buttonRename.Secondary = true;
		this.buttonRename.Size = new System.Drawing.Size(54, 46);
		this.buttonRename.TabIndex = 1;
		this.buttonRename.UseVisualStyleBackColor = false;
		this.comboModes.BorderColor = System.Drawing.Color.White;
		this.comboModes.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboModes.Dock = System.Windows.Forms.DockStyle.Top;
		this.comboModes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.comboModes.FormattingEnabled = true;
		this.comboModes.Location = new System.Drawing.Point(120, 14);
		this.comboModes.Margin = new System.Windows.Forms.Padding(0, 3, 6, 4);
		this.comboModes.Name = "comboModes";
		this.comboModes.Size = new System.Drawing.Size(290, 40);
		this.comboModes.TabIndex = 2;
		this.picturePerf.BackgroundImage = Asus.Properties.Resources.icons8_fan_32;
		this.picturePerf.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.picturePerf.InitialImage = null;
		this.picturePerf.Location = new System.Drawing.Point(18, 18);
		this.picturePerf.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.picturePerf.Name = "picturePerf";
		this.picturePerf.Size = new System.Drawing.Size(32, 32);
		this.picturePerf.TabIndex = 41;
		this.picturePerf.TabStop = false;
		this.labelFans.AutoSize = true;
		this.labelFans.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelFans.Location = new System.Drawing.Point(53, 17);
		this.labelFans.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelFans.Name = "labelFans";
		this.labelFans.Size = new System.Drawing.Size(90, 32);
		this.labelFans.TabIndex = 40;
		this.labelFans.Text = "Profile";
		this.panelHysteresis.Controls.Add(this.tableHysteresis);
		this.panelHysteresis.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelHysteresis.Location = new System.Drawing.Point(0, 860);
		this.panelHysteresis.Margin = new System.Windows.Forms.Padding(4);
		this.panelHysteresis.Name = "panelHysteresis";
		this.panelHysteresis.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
		this.panelHysteresis.Size = new System.Drawing.Size(810, 130);
		this.panelHysteresis.TabIndex = 3;
		this.tableHysteresis.ColumnCount = 3;
		this.tableHysteresis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
		this.tableHysteresis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableHysteresis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160f));
		this.tableHysteresis.Controls.Add(this.labelHysteresisUp, 0, 0);
		this.tableHysteresis.Controls.Add(this.trackHysteresisUp, 1, 0);
		this.tableHysteresis.Controls.Add(this.labelHysteresisUpValue, 2, 0);
		this.tableHysteresis.Controls.Add(this.labelHysteresisDown, 0, 1);
		this.tableHysteresis.Controls.Add(this.trackHysteresisDown, 1, 1);
		this.tableHysteresis.Controls.Add(this.labelHysteresisDownValue, 2, 1);
		this.tableHysteresis.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableHysteresis.Location = new System.Drawing.Point(10, 5);
		this.tableHysteresis.Margin = new System.Windows.Forms.Padding(0);
		this.tableHysteresis.Name = "tableHysteresis";
		this.tableHysteresis.RowCount = 2;
		this.tableHysteresis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableHysteresis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableHysteresis.Size = new System.Drawing.Size(790, 120);
		this.tableHysteresis.TabIndex = 0;
		this.labelHysteresisUp.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelHysteresisUp.AutoSize = true;
		this.labelHysteresisUp.Location = new System.Drawing.Point(4, 6);
		this.labelHysteresisUp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelHysteresisUp.Name = "labelHysteresisUp";
		this.labelHysteresisUp.Size = new System.Drawing.Size(200, 32);
		this.labelHysteresisUp.TabIndex = 0;
		this.labelHysteresisUp.Text = "Hysteresis Up";
		this.trackHysteresisUp.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.trackHysteresisUp.LargeChange = 1;
		this.trackHysteresisUp.Location = new System.Drawing.Point(162, 2);
		this.trackHysteresisUp.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackHysteresisUp.Maximum = 5;
		this.trackHysteresisUp.Minimum = 1;
		this.trackHysteresisUp.Name = "trackHysteresisUp";
		this.trackHysteresisUp.Size = new System.Drawing.Size(500, 41);
		this.trackHysteresisUp.TabIndex = 1;
		this.trackHysteresisUp.TickFrequency = 1;
		this.trackHysteresisUp.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackHysteresisUp.Value = 1;
		this.labelHysteresisDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelHysteresisDown.AutoSize = true;
		this.labelHysteresisDown.Location = new System.Drawing.Point(4, 51);
		this.labelHysteresisDown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelHysteresisDown.Name = "labelHysteresisDown";
		this.labelHysteresisDown.Size = new System.Drawing.Size(200, 32);
		this.labelHysteresisDown.TabIndex = 2;
		this.labelHysteresisDown.Text = "Hysteresis Down";
		this.trackHysteresisDown.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.trackHysteresisDown.LargeChange = 1;
		this.trackHysteresisDown.Location = new System.Drawing.Point(162, 47);
		this.trackHysteresisDown.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackHysteresisDown.Maximum = 5;
		this.trackHysteresisDown.Minimum = 1;
		this.trackHysteresisDown.Name = "trackHysteresisDown";
		this.trackHysteresisDown.Size = new System.Drawing.Size(500, 41);
		this.trackHysteresisDown.TabIndex = 3;
		this.trackHysteresisDown.TickFrequency = 1;
		this.trackHysteresisDown.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackHysteresisDown.Value = 1;
		this.labelHysteresisUpValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelHysteresisUpValue.AutoSize = false;
		this.labelHysteresisUpValue.Location = new System.Drawing.Point(670, 6);
		this.labelHysteresisUpValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelHysteresisUpValue.Name = "labelHysteresisUpValue";
		this.labelHysteresisUpValue.Size = new System.Drawing.Size(152, 32);
		this.labelHysteresisUpValue.TabIndex = 4;
		this.labelHysteresisUpValue.Text = "Very Low";
		this.labelHysteresisUpValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.labelHysteresisDownValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.labelHysteresisDownValue.AutoSize = false;
		this.labelHysteresisDownValue.Location = new System.Drawing.Point(670, 51);
		this.labelHysteresisDownValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelHysteresisDownValue.Name = "labelHysteresisDownValue";
		this.labelHysteresisDownValue.Size = new System.Drawing.Size(152, 32);
		this.labelHysteresisDownValue.TabIndex = 5;
		this.labelHysteresisDownValue.Text = "Very Low";
		this.labelHysteresisDownValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.panelApplyFans.Controls.Add(this.buttonCalibrate);
		this.panelApplyFans.Controls.Add(this.labelFansResult);
		this.panelApplyFans.Controls.Add(this.checkApplyFans);
		this.panelApplyFans.Controls.Add(this.buttonReset);
		this.panelApplyFans.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panelApplyFans.Location = new System.Drawing.Point(0, 984);
		this.panelApplyFans.Margin = new System.Windows.Forms.Padding(4);
		this.panelApplyFans.Name = "panelApplyFans";
		this.panelApplyFans.Size = new System.Drawing.Size(810, 116);
		this.panelApplyFans.TabIndex = 4;
		this.buttonCalibrate.Activated = false;
		this.buttonCalibrate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonCalibrate.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonCalibrate.BorderColor = System.Drawing.Color.Transparent;
		this.buttonCalibrate.BorderRadius = 2;
		this.buttonCalibrate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonCalibrate.Location = new System.Drawing.Point(275, 36);
		this.buttonCalibrate.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.buttonCalibrate.Name = "buttonCalibrate";
		this.buttonCalibrate.Secondary = true;
		this.buttonCalibrate.Size = new System.Drawing.Size(141, 54);
		this.buttonCalibrate.TabIndex = 1;
		this.buttonCalibrate.Text = "Calibrate";
		this.buttonCalibrate.UseVisualStyleBackColor = false;
		this.labelFansResult.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.labelFansResult.ForeColor = System.Drawing.Color.Red;
		this.labelFansResult.Location = new System.Drawing.Point(18, 2);
		this.labelFansResult.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelFansResult.Name = "labelFansResult";
		this.labelFansResult.Size = new System.Drawing.Size(771, 32);
		this.labelFansResult.TabIndex = 3;
		this.labelFansResult.Visible = false;
		this.checkApplyFans.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.checkApplyFans.AutoSize = true;
		this.checkApplyFans.BackColor = System.Drawing.SystemColors.ControlLight;
		this.checkApplyFans.Location = new System.Drawing.Point(454, 42);
		this.checkApplyFans.Margin = new System.Windows.Forms.Padding(0);
		this.checkApplyFans.Name = "checkApplyFans";
		this.checkApplyFans.Padding = new System.Windows.Forms.Padding(16, 6, 16, 6);
		this.checkApplyFans.Size = new System.Drawing.Size(341, 48);
		this.checkApplyFans.TabIndex = 2;
		this.checkApplyFans.Text = Asus.Properties.Strings.ApplyFanCurve;
		this.checkApplyFans.UseVisualStyleBackColor = false;
		this.buttonReset.Activated = false;
		this.buttonReset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonReset.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonReset.BorderColor = System.Drawing.Color.Transparent;
		this.buttonReset.BorderRadius = 2;
		this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonReset.Location = new System.Drawing.Point(15, 36);
		this.buttonReset.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.buttonReset.Name = "buttonReset";
		this.buttonReset.Secondary = true;
		this.buttonReset.Size = new System.Drawing.Size(252, 54);
		this.buttonReset.TabIndex = 0;
		this.buttonReset.Text = Asus.Properties.Strings.FactoryDefaults;
		this.buttonReset.UseVisualStyleBackColor = false;
		this.comboBoost.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.comboBoost.BorderColor = System.Drawing.Color.White;
		this.comboBoost.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboBoost.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoost.FormattingEnabled = true;
		this.comboBoost.Items.AddRange(new object[7] { "Disabled", "Enabled", "Aggressive", "Efficient Enabled", "Efficient Aggressive", "Aggressive at Guaranteed", "Efficient at Guaranteed" });
		this.comboBoost.Location = new System.Drawing.Point(13, 12);
		this.comboBoost.Margin = new System.Windows.Forms.Padding(4);
		this.comboBoost.Name = "comboBoost";
		this.comboBoost.Size = new System.Drawing.Size(329, 40);
		this.comboBoost.TabIndex = 42;
		this.panelSliders.Controls.Add(this.panelAdvanced);
		this.panelSliders.Controls.Add(this.panelPower);
		this.panelSliders.Controls.Add(this.panelGPU);
		this.panelSliders.Controls.Add(this.panelNav);
		this.panelSliders.Dock = System.Windows.Forms.DockStyle.Left;
		this.panelSliders.Location = new System.Drawing.Point(0, 0);
		this.panelSliders.Margin = new System.Windows.Forms.Padding(0);
		this.panelSliders.MinimumSize = new System.Drawing.Size(530, 0);
		this.panelSliders.Name = "panelSliders";
		this.panelSliders.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
		this.panelSliders.Size = new System.Drawing.Size(530, 1100);
		this.panelSliders.TabIndex = 13;
		this.panelAdvanced.AutoSize = true;
		this.panelAdvanced.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelAdvanced.Controls.Add(this.panelPawnIO);
		this.panelAdvanced.Controls.Add(this.panelDownload);
		this.panelAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAdvanced.Location = new System.Drawing.Point(10, 1768);
		this.panelAdvanced.Name = "panelAdvanced";
		this.panelAdvanced.Size = new System.Drawing.Size(520, 992);
		this.panelAdvanced.TabIndex = 3;
		this.panelAdvanced.Visible = false;
		this.panelAdvancedAlways.AutoSize = true;
		this.panelAdvancedAlways.Controls.Add(this.checkApplyUV);
		this.panelAdvancedAlways.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAdvancedAlways.Location = new System.Drawing.Point(0, 931);
		this.panelAdvancedAlways.Name = "panelAdvancedAlways";
		this.panelAdvancedAlways.Padding = new System.Windows.Forms.Padding(16, 0, 16, 15);
		this.panelAdvancedAlways.Size = new System.Drawing.Size(520, 61);
		this.panelAdvancedAlways.TabIndex = 7;
		this.checkApplyUV.BackColor = System.Drawing.SystemColors.ControlLight;
		this.checkApplyUV.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkApplyUV.Enabled = false;
		this.checkApplyUV.Location = new System.Drawing.Point(16, 0);
		this.checkApplyUV.Margin = new System.Windows.Forms.Padding(15, 15, 0, 0);
		this.checkApplyUV.Name = "checkApplyUV";
		this.checkApplyUV.Padding = new System.Windows.Forms.Padding(16, 6, 16, 6);
		this.checkApplyUV.Size = new System.Drawing.Size(488, 46);
		this.checkApplyUV.TabIndex = 51;
		this.checkApplyUV.Text = "Auto Apply";
		this.checkApplyUV.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.checkApplyUV.UseVisualStyleBackColor = false;
		this.panelAdvancedApply.AutoSize = true;
		this.panelAdvancedApply.Controls.Add(this.buttonApplyAdvanced);
		this.panelAdvancedApply.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAdvancedApply.Location = new System.Drawing.Point(0, 851);
		this.panelAdvancedApply.Name = "panelAdvancedApply";
		this.panelAdvancedApply.Padding = new System.Windows.Forms.Padding(15);
		this.panelAdvancedApply.Size = new System.Drawing.Size(520, 80);
		this.panelAdvancedApply.TabIndex = 6;
		this.buttonApplyAdvanced.Activated = false;
		this.buttonApplyAdvanced.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonApplyAdvanced.BorderColor = System.Drawing.Color.Transparent;
		this.buttonApplyAdvanced.BorderRadius = 2;
		this.buttonApplyAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
		this.buttonApplyAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonApplyAdvanced.Location = new System.Drawing.Point(15, 15);
		this.buttonApplyAdvanced.Margin = new System.Windows.Forms.Padding(4, 2, 15, 15);
		this.buttonApplyAdvanced.Name = "buttonApplyAdvanced";
		this.buttonApplyAdvanced.Secondary = true;
		this.buttonApplyAdvanced.Size = new System.Drawing.Size(490, 50);
		this.buttonApplyAdvanced.TabIndex = 49;
		this.buttonApplyAdvanced.Text = "Apply";
		this.buttonApplyAdvanced.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonApplyAdvanced.UseVisualStyleBackColor = false;
		this.panelAdvancedReadLimits.AutoSize = true;
		this.panelAdvancedReadLimits.Controls.Add(this.buttonReadLimits);
		this.panelAdvancedReadLimits.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAdvancedReadLimits.Name = "panelAdvancedReadLimits";
		this.panelAdvancedReadLimits.Padding = new System.Windows.Forms.Padding(15);
		this.panelAdvancedReadLimits.Size = new System.Drawing.Size(520, 80);
		this.panelAdvancedReadLimits.TabIndex = 8;
		this.buttonReadLimits.Activated = false;
		this.buttonReadLimits.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonReadLimits.BorderColor = System.Drawing.Color.Transparent;
		this.buttonReadLimits.BorderRadius = 2;
		this.buttonReadLimits.Dock = System.Windows.Forms.DockStyle.Top;
		this.buttonReadLimits.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonReadLimits.Location = new System.Drawing.Point(15, 15);
		this.buttonReadLimits.Margin = new System.Windows.Forms.Padding(4, 2, 15, 15);
		this.buttonReadLimits.Name = "buttonReadLimits";
		this.buttonReadLimits.Secondary = true;
		this.buttonReadLimits.Size = new System.Drawing.Size(490, 50);
		this.buttonReadLimits.TabIndex = 53;
		this.buttonReadLimits.Text = "Read Limits";
		this.buttonReadLimits.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonReadLimits.UseVisualStyleBackColor = false;
		this.labelRisky.BackColor = System.Drawing.Color.IndianRed;
		this.labelRisky.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelRisky.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.labelRisky.Location = new System.Drawing.Point(0, 608);
		this.labelRisky.Margin = new System.Windows.Forms.Padding(0);
		this.labelRisky.Name = "labelRisky";
		this.labelRisky.Padding = new System.Windows.Forms.Padding(10, 10, 10, 5);
		this.labelRisky.Size = new System.Drawing.Size(520, 243);
		this.labelRisky.TabIndex = 5;
		this.labelRisky.Text = resources.GetString("labelRisky.Text");
		this.panelUViGPU.AutoSize = true;
		this.panelUViGPU.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelUViGPU.Controls.Add(this.labelUViGPU);
		this.panelUViGPU.Controls.Add(this.labelLeftUViGPU);
		this.panelUViGPU.Controls.Add(this.trackUViGPU);
		this.panelUViGPU.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelUViGPU.Location = new System.Drawing.Point(0, 484);
		this.panelUViGPU.Margin = new System.Windows.Forms.Padding(4);
		this.panelUViGPU.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelUViGPU.Name = "panelUViGPU";
		this.panelUViGPU.Size = new System.Drawing.Size(520, 124);
		this.panelUViGPU.TabIndex = 4;
		this.labelUViGPU.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelUViGPU.Location = new System.Drawing.Point(347, 9);
		this.labelUViGPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelUViGPU.Name = "labelUViGPU";
		this.labelUViGPU.Size = new System.Drawing.Size(148, 32);
		this.labelUViGPU.TabIndex = 13;
		this.labelUViGPU.Text = "UV";
		this.labelUViGPU.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftUViGPU.AutoSize = true;
		this.labelLeftUViGPU.Location = new System.Drawing.Point(10, 10);
		this.labelLeftUViGPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftUViGPU.Name = "labelLeftUViGPU";
		this.labelLeftUViGPU.Size = new System.Drawing.Size(65, 32);
		this.labelLeftUViGPU.TabIndex = 12;
		this.labelLeftUViGPU.Text = "iGPU";
		this.trackUViGPU.Location = new System.Drawing.Point(6, 48);
		this.trackUViGPU.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackUViGPU.Maximum = 0;
		this.trackUViGPU.Minimum = -40;
		this.trackUViGPU.Name = "trackUViGPU";
		this.trackUViGPU.Size = new System.Drawing.Size(508, 90);
		this.trackUViGPU.TabIndex = 11;
		this.trackUViGPU.TickFrequency = 5;
		this.trackUViGPU.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.panelUV.AutoSize = true;
		this.panelUV.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelUV.Controls.Add(this.labelUV);
		this.panelUV.Controls.Add(this.labelLeftUV);
		this.panelUV.Controls.Add(this.trackUV);
		this.panelUV.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelUV.Location = new System.Drawing.Point(0, 360);
		this.panelUV.Margin = new System.Windows.Forms.Padding(4);
		this.panelUV.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelUV.Name = "panelUV";
		this.panelUV.Size = new System.Drawing.Size(520, 124);
		this.panelUV.TabIndex = 3;
		this.labelUV.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelUV.Location = new System.Drawing.Point(347, 13);
		this.labelUV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelUV.Name = "labelUV";
		this.labelUV.Size = new System.Drawing.Size(148, 32);
		this.labelUV.TabIndex = 13;
		this.labelUV.Text = "UV";
		this.labelUV.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftUV.AutoSize = true;
		this.labelLeftUV.Location = new System.Drawing.Point(10, 10);
		this.labelLeftUV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftUV.Name = "labelLeftUV";
		this.labelLeftUV.Size = new System.Drawing.Size(58, 32);
		this.labelLeftUV.TabIndex = 12;
		this.labelLeftUV.Text = "CPU";
		this.trackUV.Location = new System.Drawing.Point(6, 48);
		this.trackUV.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackUV.Maximum = 0;
		this.trackUV.Minimum = -40;
		this.trackUV.Name = "trackUV";
		this.trackUV.Size = new System.Drawing.Size(508, 90);
		this.trackUV.TabIndex = 11;
		this.trackUV.TickFrequency = 5;
		this.trackUV.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.panelTitleAdvanced.Controls.Add(this.pictureUV);
		this.panelTitleAdvanced.Controls.Add(this.labelTitleUV);
		this.panelTitleAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTitleAdvanced.Location = new System.Drawing.Point(0, 294);
		this.panelTitleAdvanced.Name = "panelTitleAdvanced";
		this.panelTitleAdvanced.Size = new System.Drawing.Size(520, 66);
		this.panelTitleAdvanced.TabIndex = 2;
		this.pictureUV.BackgroundImage = Asus.Properties.Resources.icons8_voltage_32;
		this.pictureUV.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureUV.InitialImage = null;
		this.pictureUV.Location = new System.Drawing.Point(10, 18);
		this.pictureUV.Margin = new System.Windows.Forms.Padding(4, 2, 4, 10);
		this.pictureUV.Name = "pictureUV";
		this.pictureUV.Size = new System.Drawing.Size(32, 32);
		this.pictureUV.TabIndex = 48;
		this.pictureUV.TabStop = false;
		this.labelTitleUV.AutoSize = true;
		this.labelTitleUV.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelTitleUV.Location = new System.Drawing.Point(43, 17);
		this.labelTitleUV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelTitleUV.Name = "labelTitleUV";
		this.labelTitleUV.Size = new System.Drawing.Size(166, 32);
		this.labelTitleUV.TabIndex = 47;
		this.labelTitleUV.Text = "Undervolting";
		this.panelTemperature.AutoSize = true;
		this.panelTemperature.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelTemperature.Controls.Add(this.labelTemp);
		this.panelTemperature.Controls.Add(this.labelLeftTemp);
		this.panelTemperature.Controls.Add(this.trackTemp);
		this.panelTemperature.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTemperature.Location = new System.Drawing.Point(0, 170);
		this.panelTemperature.Margin = new System.Windows.Forms.Padding(4);
		this.panelTemperature.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelTemperature.Name = "panelTemperature";
		this.panelTemperature.Size = new System.Drawing.Size(520, 124);
		this.panelTemperature.TabIndex = 1;
		this.labelTemp.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelTemp.Location = new System.Drawing.Point(347, 13);
		this.labelTemp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelTemp.Name = "labelTemp";
		this.labelTemp.Size = new System.Drawing.Size(148, 32);
		this.labelTemp.TabIndex = 13;
		this.labelTemp.Text = "T";
		this.labelTemp.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftTemp.AutoSize = true;
		this.labelLeftTemp.Location = new System.Drawing.Point(10, 10);
		this.labelLeftTemp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftTemp.Name = "labelLeftTemp";
		this.labelLeftTemp.Size = new System.Drawing.Size(183, 32);
		this.labelLeftTemp.TabIndex = 12;
		this.labelLeftTemp.Text = "CPU Temp Limit";
		this.trackTemp.Location = new System.Drawing.Point(6, 48);
		this.trackTemp.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackTemp.Maximum = 0;
		this.trackTemp.Minimum = -40;
		this.trackTemp.Name = "trackTemp";
		this.trackTemp.Size = new System.Drawing.Size(508, 90);
		this.trackTemp.TabIndex = 11;
		this.trackTemp.TickFrequency = 5;
		this.trackTemp.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.panelTitleTemp.Controls.Add(this.pictureTemp);
		this.panelTitleTemp.Controls.Add(this.labelTempLimit);
		this.panelTitleTemp.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTitleTemp.Location = new System.Drawing.Point(0, 104);
		this.panelTitleTemp.Name = "panelTitleTemp";
		this.panelTitleTemp.Size = new System.Drawing.Size(520, 66);
		this.panelTitleTemp.TabIndex = 0;
		this.pictureTemp.BackgroundImage = Asus.Properties.Resources.icons8_temperature_32;
		this.pictureTemp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureTemp.InitialImage = null;
		this.pictureTemp.Location = new System.Drawing.Point(10, 18);
		this.pictureTemp.Margin = new System.Windows.Forms.Padding(4, 2, 4, 10);
		this.pictureTemp.Name = "pictureTemp";
		this.pictureTemp.Size = new System.Drawing.Size(32, 32);
		this.pictureTemp.TabIndex = 48;
		this.pictureTemp.TabStop = false;
		this.labelTempLimit.AutoSize = true;
		this.labelTempLimit.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelTempLimit.Location = new System.Drawing.Point(46, 17);
		this.labelTempLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelTempLimit.Name = "labelTempLimit";
		this.labelTempLimit.Size = new System.Drawing.Size(140, 32);
		this.labelTempLimit.TabIndex = 47;
		this.labelTempLimit.Text = "Temp Limit";
		this.panelPawnIO.AutoSize = true;
		this.panelPawnIO.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelPawnIO.Controls.Add(this.panelAdvancedReadLimits);
		this.panelPawnIO.Controls.Add(this.panelAdvancedAlways);
		this.panelPawnIO.Controls.Add(this.panelAdvancedApply);
		this.panelPawnIO.Controls.Add(this.labelRisky);
		this.panelPawnIO.Controls.Add(this.panelUViGPU);
		this.panelPawnIO.Controls.Add(this.panelUV);
		this.panelPawnIO.Controls.Add(this.panelTitleAdvanced);
		this.panelPawnIO.Controls.Add(this.panelTemperature);
		this.panelPawnIO.Controls.Add(this.panelTitleTemp);
		this.panelPawnIO.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelPawnIO.Name = "panelPawnIO";
		this.panelPawnIO.TabIndex = 1;
		this.panelDownload.AutoSize = true;
		this.panelDownload.Controls.Add(this.buttonDownload);
		this.panelDownload.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelDownload.Location = new System.Drawing.Point(0, 0);
		this.panelDownload.Name = "panelDownload";
		this.panelDownload.Padding = new System.Windows.Forms.Padding(20);
		this.panelDownload.Size = new System.Drawing.Size(520, 104);
		this.panelDownload.TabIndex = 0;
		this.panelDownload.Visible = false;
		this.buttonDownload.Activated = false;
		this.buttonDownload.AutoSize = true;
		this.buttonDownload.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.buttonDownload.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonDownload.BorderColor = System.Drawing.Color.Transparent;
		this.buttonDownload.BorderRadius = 2;
		this.buttonDownload.Dock = System.Windows.Forms.DockStyle.Top;
		this.buttonDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonDownload.Location = new System.Drawing.Point(20, 20);
		this.buttonDownload.Margin = new System.Windows.Forms.Padding(20);
		this.buttonDownload.Name = "buttonDownload";
		this.buttonDownload.Padding = new System.Windows.Forms.Padding(10);
		this.buttonDownload.Secondary = true;
		this.buttonDownload.Size = new System.Drawing.Size(480, 64);
		this.buttonDownload.TabIndex = 19;
		this.buttonDownload.Text = "Install PawnIO Driver (pawnio.eu)";
		this.buttonDownload.UseVisualStyleBackColor = false;
		this.panelPower.AutoSize = true;
		this.panelPower.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelPower.Controls.Add(this.panelApplyPower);
		this.panelPower.Controls.Add(this.panelCPU);
		this.panelPower.Controls.Add(this.panelFast);
		this.panelPower.Controls.Add(this.panelSlow);
		this.panelPower.Controls.Add(this.panelTotal);
		this.panelPower.Controls.Add(this.panelTitleCPU);
		this.panelPower.Controls.Add(this.panelBoost);
		this.panelPower.Controls.Add(this.panelBoostTitle);
		this.panelPower.Controls.Add(this.panelPowerMode);
		this.panelPower.Controls.Add(this.panelPowerModeTItle);
		this.panelPower.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelPower.Location = new System.Drawing.Point(10, 888);
		this.panelPower.Margin = new System.Windows.Forms.Padding(4);
		this.panelPower.Name = "panelPower";
		this.panelPower.Size = new System.Drawing.Size(520, 880);
		this.panelPower.TabIndex = 2;
		this.panelApplyPower.AutoSize = true;
		this.panelApplyPower.Controls.Add(this.checkApplyPower);
		this.panelApplyPower.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelApplyPower.Location = new System.Drawing.Point(0, 804);
		this.panelApplyPower.Name = "panelApplyPower";
		this.panelApplyPower.Padding = new System.Windows.Forms.Padding(15);
		this.panelApplyPower.Size = new System.Drawing.Size(520, 76);
		this.panelApplyPower.TabIndex = 9;
		this.checkApplyPower.BackColor = System.Drawing.SystemColors.ControlLight;
		this.checkApplyPower.Dock = System.Windows.Forms.DockStyle.Top;
		this.checkApplyPower.Location = new System.Drawing.Point(15, 15);
		this.checkApplyPower.Margin = new System.Windows.Forms.Padding(0);
		this.checkApplyPower.Name = "checkApplyPower";
		this.checkApplyPower.Padding = new System.Windows.Forms.Padding(16, 6, 16, 6);
		this.checkApplyPower.Size = new System.Drawing.Size(490, 46);
		this.checkApplyPower.TabIndex = 45;
		this.checkApplyPower.Text = "Apply Power Limits";
		this.checkApplyPower.UseVisualStyleBackColor = false;
		this.panelCPU.AutoSize = true;
		this.panelCPU.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelCPU.Controls.Add(this.labelCPU);
		this.panelCPU.Controls.Add(this.labelLeftCPU);
		this.panelCPU.Controls.Add(this.trackCPU);
		this.panelCPU.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelCPU.Location = new System.Drawing.Point(0, 680);
		this.panelCPU.Margin = new System.Windows.Forms.Padding(4);
		this.panelCPU.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelCPU.Name = "panelCPU";
		this.panelCPU.Size = new System.Drawing.Size(520, 124);
		this.panelCPU.TabIndex = 8;
		this.labelCPU.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelCPU.Location = new System.Drawing.Point(398, 8);
		this.labelCPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelCPU.Name = "labelCPU";
		this.labelCPU.Size = new System.Drawing.Size(116, 32);
		this.labelCPU.TabIndex = 13;
		this.labelCPU.Text = "CPU";
		this.labelCPU.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftCPU.AutoSize = true;
		this.labelLeftCPU.Location = new System.Drawing.Point(10, 8);
		this.labelLeftCPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftCPU.Name = "labelLeftCPU";
		this.labelLeftCPU.Size = new System.Drawing.Size(58, 32);
		this.labelLeftCPU.TabIndex = 12;
		this.labelLeftCPU.Text = "CPU";
		this.trackCPU.Location = new System.Drawing.Point(6, 44);
		this.trackCPU.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackCPU.Maximum = 85;
		this.trackCPU.Minimum = 5;
		this.trackCPU.Name = "trackCPU";
		this.trackCPU.Size = new System.Drawing.Size(508, 90);
		this.trackCPU.TabIndex = 11;
		this.trackCPU.TickFrequency = 5;
		this.trackCPU.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackCPU.Value = 80;
		this.panelFast.AutoSize = true;
		this.panelFast.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelFast.Controls.Add(this.labelFast);
		this.panelFast.Controls.Add(this.labelLeftFast);
		this.panelFast.Controls.Add(this.trackFast);
		this.panelFast.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelFast.Location = new System.Drawing.Point(0, 556);
		this.panelFast.Margin = new System.Windows.Forms.Padding(4);
		this.panelFast.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelFast.Name = "panelFast";
		this.panelFast.Size = new System.Drawing.Size(520, 124);
		this.panelFast.TabIndex = 7;
		this.labelFast.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelFast.Location = new System.Drawing.Point(396, 8);
		this.labelFast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelFast.Name = "labelFast";
		this.labelFast.Size = new System.Drawing.Size(114, 32);
		this.labelFast.TabIndex = 13;
		this.labelFast.Text = "FPPT";
		this.labelFast.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftFast.AutoSize = true;
		this.labelLeftFast.Location = new System.Drawing.Point(10, 8);
		this.labelLeftFast.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftFast.Name = "labelLeftFast";
		this.labelLeftFast.Size = new System.Drawing.Size(65, 32);
		this.labelLeftFast.TabIndex = 12;
		this.labelLeftFast.Text = "FPPT";
		this.trackFast.Location = new System.Drawing.Point(6, 48);
		this.trackFast.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackFast.Maximum = 85;
		this.trackFast.Minimum = 5;
		this.trackFast.Name = "trackFast";
		this.trackFast.Size = new System.Drawing.Size(508, 90);
		this.trackFast.TabIndex = 11;
		this.trackFast.TickFrequency = 5;
		this.trackFast.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackFast.Value = 80;
		this.panelSlow.AutoSize = true;
		this.panelSlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelSlow.Controls.Add(this.labelSlow);
		this.panelSlow.Controls.Add(this.labelLeftSlow);
		this.panelSlow.Controls.Add(this.trackSlow);
		this.panelSlow.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelSlow.Location = new System.Drawing.Point(0, 432);
		this.panelSlow.Margin = new System.Windows.Forms.Padding(4);
		this.panelSlow.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelSlow.Name = "panelSlow";
		this.panelSlow.Size = new System.Drawing.Size(520, 124);
		this.panelSlow.TabIndex = 6;
		this.labelSlow.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelSlow.Location = new System.Drawing.Point(396, 10);
		this.labelSlow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelSlow.Name = "labelSlow";
		this.labelSlow.Size = new System.Drawing.Size(116, 32);
		this.labelSlow.TabIndex = 12;
		this.labelSlow.Text = "SPPT";
		this.labelSlow.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftSlow.AutoSize = true;
		this.labelLeftSlow.Location = new System.Drawing.Point(10, 10);
		this.labelLeftSlow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftSlow.Name = "labelLeftSlow";
		this.labelLeftSlow.Size = new System.Drawing.Size(66, 32);
		this.labelLeftSlow.TabIndex = 11;
		this.labelLeftSlow.Text = "SPPT";
		this.trackSlow.Location = new System.Drawing.Point(6, 48);
		this.trackSlow.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackSlow.Maximum = 180;
		this.trackSlow.Minimum = 10;
		this.trackSlow.Name = "trackSlow";
		this.trackSlow.Size = new System.Drawing.Size(508, 90);
		this.trackSlow.TabIndex = 10;
		this.trackSlow.TickFrequency = 5;
		this.trackSlow.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackSlow.Value = 125;
		this.panelTotal.AutoSize = true;
		this.panelTotal.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelTotal.Controls.Add(this.labelTotal);
		this.panelTotal.Controls.Add(this.labelLeftTotal);
		this.panelTotal.Controls.Add(this.trackTotal);
		this.panelTotal.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTotal.Location = new System.Drawing.Point(0, 308);
		this.panelTotal.Margin = new System.Windows.Forms.Padding(4);
		this.panelTotal.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelTotal.Name = "panelTotal";
		this.panelTotal.Size = new System.Drawing.Size(520, 124);
		this.panelTotal.TabIndex = 5;
		this.labelTotal.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelTotal.Location = new System.Drawing.Point(396, 10);
		this.labelTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelTotal.Name = "labelTotal";
		this.labelTotal.Size = new System.Drawing.Size(116, 32);
		this.labelTotal.TabIndex = 12;
		this.labelTotal.Text = "SPL";
		this.labelTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelLeftTotal.AutoSize = true;
		this.labelLeftTotal.Location = new System.Drawing.Point(10, 10);
		this.labelLeftTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelLeftTotal.Name = "labelLeftTotal";
		this.labelLeftTotal.Size = new System.Drawing.Size(51, 32);
		this.labelLeftTotal.TabIndex = 11;
		this.labelLeftTotal.Text = "SPL";
		this.trackTotal.Location = new System.Drawing.Point(6, 48);
		this.trackTotal.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackTotal.Maximum = 180;
		this.trackTotal.Minimum = 10;
		this.trackTotal.Name = "trackTotal";
		this.trackTotal.Size = new System.Drawing.Size(508, 90);
		this.trackTotal.TabIndex = 10;
		this.trackTotal.TickFrequency = 5;
		this.trackTotal.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackTotal.Value = 125;
		this.panelTitleCPU.AutoSize = true;
		this.panelTitleCPU.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelTitleCPU.Controls.Add(this.pictureBoxCPU);
		this.panelTitleCPU.Controls.Add(this.labelPowerLimits);
		this.panelTitleCPU.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTitleCPU.Location = new System.Drawing.Point(0, 248);
		this.panelTitleCPU.Margin = new System.Windows.Forms.Padding(4);
		this.panelTitleCPU.Name = "panelTitleCPU";
		this.panelTitleCPU.Size = new System.Drawing.Size(520, 60);
		this.panelTitleCPU.TabIndex = 4;
		this.pictureBoxCPU.BackgroundImage = Asus.Properties.Resources.icons8_processor_32;
		this.pictureBoxCPU.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBoxCPU.InitialImage = null;
		this.pictureBoxCPU.Location = new System.Drawing.Point(10, 18);
		this.pictureBoxCPU.Margin = new System.Windows.Forms.Padding(4, 2, 4, 10);
		this.pictureBoxCPU.Name = "pictureBoxCPU";
		this.pictureBoxCPU.Size = new System.Drawing.Size(32, 32);
		this.pictureBoxCPU.TabIndex = 40;
		this.pictureBoxCPU.TabStop = false;
		this.labelPowerLimits.AutoSize = true;
		this.labelPowerLimits.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelPowerLimits.Location = new System.Drawing.Point(46, 16);
		this.labelPowerLimits.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelPowerLimits.Name = "labelPowerLimits";
		this.labelPowerLimits.Size = new System.Drawing.Size(160, 32);
		this.labelPowerLimits.TabIndex = 39;
		this.labelPowerLimits.Text = "Power Limits";
		this.panelBoost.Controls.Add(this.comboBoost);
		this.panelBoost.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBoost.Location = new System.Drawing.Point(0, 184);
		this.panelBoost.Margin = new System.Windows.Forms.Padding(4);
		this.panelBoost.Name = "panelBoost";
		this.panelBoost.Size = new System.Drawing.Size(520, 64);
		this.panelBoost.TabIndex = 3;
		this.panelBoostTitle.AutoSize = true;
		this.panelBoostTitle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelBoostTitle.Controls.Add(this.pictureBoost);
		this.panelBoostTitle.Controls.Add(this.labelBoost);
		this.panelBoostTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBoostTitle.Location = new System.Drawing.Point(0, 124);
		this.panelBoostTitle.Margin = new System.Windows.Forms.Padding(4);
		this.panelBoostTitle.Name = "panelBoostTitle";
		this.panelBoostTitle.Size = new System.Drawing.Size(520, 60);
		this.panelBoostTitle.TabIndex = 2;
		this.pictureBoost.BackgroundImage = Asus.Properties.Resources.icons8_rocket_32;
		this.pictureBoost.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureBoost.InitialImage = null;
		this.pictureBoost.Location = new System.Drawing.Point(10, 18);
		this.pictureBoost.Margin = new System.Windows.Forms.Padding(4, 2, 4, 10);
		this.pictureBoost.Name = "pictureBoost";
		this.pictureBoost.Size = new System.Drawing.Size(32, 32);
		this.pictureBoost.TabIndex = 40;
		this.pictureBoost.TabStop = false;
		this.labelBoost.AutoSize = true;
		this.labelBoost.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBoost.Location = new System.Drawing.Point(46, 18);
		this.labelBoost.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelBoost.Name = "labelBoost";
		this.labelBoost.Size = new System.Drawing.Size(133, 32);
		this.labelBoost.TabIndex = 39;
		this.labelBoost.Text = "CPU Boost";
		this.panelPowerMode.Controls.Add(this.comboPowerMode);
		this.panelPowerMode.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelPowerMode.Location = new System.Drawing.Point(0, 60);
		this.panelPowerMode.Margin = new System.Windows.Forms.Padding(4);
		this.panelPowerMode.Name = "panelPowerMode";
		this.panelPowerMode.Size = new System.Drawing.Size(520, 64);
		this.panelPowerMode.TabIndex = 1;
		this.comboPowerMode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.comboPowerMode.BorderColor = System.Drawing.Color.White;
		this.comboPowerMode.ButtonColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.comboPowerMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboPowerMode.FormattingEnabled = true;
		this.comboPowerMode.Items.AddRange(new object[7] { "Disabled", "Enabled", "Aggressive", "Efficient Enabled", "Efficient Aggressive", "Aggressive at Guaranteed", "Efficient at Guaranteed" });
		this.comboPowerMode.Location = new System.Drawing.Point(13, 12);
		this.comboPowerMode.Margin = new System.Windows.Forms.Padding(4);
		this.comboPowerMode.Name = "comboPowerMode";
		this.comboPowerMode.Size = new System.Drawing.Size(329, 40);
		this.comboPowerMode.TabIndex = 42;
		this.panelPowerModeTItle.AutoSize = true;
		this.panelPowerModeTItle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelPowerModeTItle.Controls.Add(this.picturePowerMode);
		this.panelPowerModeTItle.Controls.Add(this.labelPowerModeTitle);
		this.panelPowerModeTItle.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelPowerModeTItle.Location = new System.Drawing.Point(0, 0);
		this.panelPowerModeTItle.Margin = new System.Windows.Forms.Padding(4);
		this.panelPowerModeTItle.Name = "panelPowerModeTItle";
		this.panelPowerModeTItle.Size = new System.Drawing.Size(520, 60);
		this.panelPowerModeTItle.TabIndex = 0;
		this.picturePowerMode.BackgroundImage = Asus.Properties.Resources.icons8_gauge_32;
		this.picturePowerMode.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.picturePowerMode.InitialImage = null;
		this.picturePowerMode.Location = new System.Drawing.Point(10, 18);
		this.picturePowerMode.Margin = new System.Windows.Forms.Padding(4, 2, 4, 10);
		this.picturePowerMode.Name = "picturePowerMode";
		this.picturePowerMode.Size = new System.Drawing.Size(32, 32);
		this.picturePowerMode.TabIndex = 40;
		this.picturePowerMode.TabStop = false;
		this.labelPowerModeTitle.AutoSize = true;
		this.labelPowerModeTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelPowerModeTitle.Location = new System.Drawing.Point(46, 18);
		this.labelPowerModeTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelPowerModeTitle.Name = "labelPowerModeTitle";
		this.labelPowerModeTitle.Size = new System.Drawing.Size(271, 32);
		this.labelPowerModeTitle.TabIndex = 39;
		this.labelPowerModeTitle.Text = "Windows Power Mode";
		this.panelGPU.AutoSize = true;
		this.panelGPU.Controls.Add(this.panelGPUTemp);
		this.panelGPU.Controls.Add(this.panelGPUBoost);
		this.panelGPU.Controls.Add(this.panelGPUPower);
		this.panelGPU.Controls.Add(this.panelGPUMemory);
		this.panelGPU.Controls.Add(this.panelGPUCore);
		this.panelGPU.Controls.Add(this.panelGPUClockLimit);
		this.panelGPU.Controls.Add(this.panelTitleGPU);
		this.panelGPU.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPU.Location = new System.Drawing.Point(10, 66);
		this.panelGPU.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPU.Name = "panelGPU";
		this.panelGPU.Padding = new System.Windows.Forms.Padding(0, 0, 0, 18);
		this.panelGPU.Size = new System.Drawing.Size(520, 822);
		this.panelGPU.TabIndex = 1;
		this.panelGPU.Visible = false;
		this.panelGPUTemp.AutoSize = true;
		this.panelGPUTemp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelGPUTemp.Controls.Add(this.labelGPUTemp);
		this.panelGPUTemp.Controls.Add(this.labelGPUTempTitle);
		this.panelGPUTemp.Controls.Add(this.trackGPUTemp);
		this.panelGPUTemp.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPUTemp.Location = new System.Drawing.Point(0, 680);
		this.panelGPUTemp.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPUTemp.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelGPUTemp.Name = "panelGPUTemp";
		this.panelGPUTemp.Size = new System.Drawing.Size(520, 124);
		this.panelGPUTemp.TabIndex = 6;
		this.labelGPUTemp.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPUTemp.Location = new System.Drawing.Point(378, 14);
		this.labelGPUTemp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUTemp.Name = "labelGPUTemp";
		this.labelGPUTemp.Size = new System.Drawing.Size(124, 32);
		this.labelGPUTemp.TabIndex = 44;
		this.labelGPUTemp.Text = "87C";
		this.labelGPUTemp.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelGPUTempTitle.AutoSize = true;
		this.labelGPUTempTitle.Location = new System.Drawing.Point(10, 14);
		this.labelGPUTempTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUTempTitle.Name = "labelGPUTempTitle";
		this.labelGPUTempTitle.Size = new System.Drawing.Size(173, 32);
		this.labelGPUTempTitle.TabIndex = 43;
		this.labelGPUTempTitle.Text = "Thermal Target";
		this.trackGPUTemp.Location = new System.Drawing.Point(6, 56);
		this.trackGPUTemp.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackGPUTemp.Maximum = 87;
		this.trackGPUTemp.Minimum = 75;
		this.trackGPUTemp.Name = "trackGPUTemp";
		this.trackGPUTemp.Size = new System.Drawing.Size(496, 90);
		this.trackGPUTemp.TabIndex = 42;
		this.trackGPUTemp.TickFrequency = 5;
		this.trackGPUTemp.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackGPUTemp.Value = 87;
		this.panelGPUBoost.AutoSize = true;
		this.panelGPUBoost.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelGPUBoost.Controls.Add(this.labelGPUBoost);
		this.panelGPUBoost.Controls.Add(this.labelGPUBoostTitle);
		this.panelGPUBoost.Controls.Add(this.trackGPUBoost);
		this.panelGPUBoost.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPUBoost.Location = new System.Drawing.Point(0, 556);
		this.panelGPUBoost.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPUBoost.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelGPUBoost.Name = "panelGPUBoost";
		this.panelGPUBoost.Size = new System.Drawing.Size(520, 124);
		this.panelGPUBoost.TabIndex = 5;
		this.labelGPUBoost.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPUBoost.Location = new System.Drawing.Point(374, 14);
		this.labelGPUBoost.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUBoost.Name = "labelGPUBoost";
		this.labelGPUBoost.Size = new System.Drawing.Size(124, 32);
		this.labelGPUBoost.TabIndex = 44;
		this.labelGPUBoost.Text = "25W";
		this.labelGPUBoost.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelGPUBoostTitle.AutoSize = true;
		this.labelGPUBoostTitle.Location = new System.Drawing.Point(10, 14);
		this.labelGPUBoostTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUBoostTitle.Name = "labelGPUBoostTitle";
		this.labelGPUBoostTitle.Size = new System.Drawing.Size(174, 32);
		this.labelGPUBoostTitle.TabIndex = 43;
		this.labelGPUBoostTitle.Text = "Dynamic Boost";
		this.trackGPUBoost.Location = new System.Drawing.Point(6, 48);
		this.trackGPUBoost.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackGPUBoost.Maximum = 25;
		this.trackGPUBoost.Minimum = 5;
		this.trackGPUBoost.Name = "trackGPUBoost";
		this.trackGPUBoost.Size = new System.Drawing.Size(496, 90);
		this.trackGPUBoost.TabIndex = 42;
		this.trackGPUBoost.TickFrequency = 5;
		this.trackGPUBoost.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackGPUBoost.Value = 25;
		this.panelGPUPower.AutoSize = true;
		this.panelGPUPower.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelGPUPower.Controls.Add(this.labelGPUPower);
		this.panelGPUPower.Controls.Add(this.labelGPUPowerTitle);
		this.panelGPUPower.Controls.Add(this.trackGPUPower);
		this.panelGPUPower.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPUPower.Location = new System.Drawing.Point(0, 432);
		this.panelGPUPower.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPUPower.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelGPUPower.Name = "panelGPUPower";
		this.panelGPUPower.Size = new System.Drawing.Size(520, 124);
		this.panelGPUPower.TabIndex = 4;
		this.panelGPUPower.Visible = false;
		this.labelGPUPower.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPUPower.Location = new System.Drawing.Point(374, 14);
		this.labelGPUPower.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUPower.Name = "labelGPUPower";
		this.labelGPUPower.Size = new System.Drawing.Size(124, 32);
		this.labelGPUPower.TabIndex = 44;
		this.labelGPUPower.Text = "105W";
		this.labelGPUPower.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelGPUPowerTitle.AutoSize = true;
		this.labelGPUPowerTitle.Location = new System.Drawing.Point(10, 14);
		this.labelGPUPowerTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUPowerTitle.Name = "labelGPUPowerTitle";
		this.labelGPUPowerTitle.Size = new System.Drawing.Size(130, 32);
		this.labelGPUPowerTitle.TabIndex = 43;
		this.labelGPUPowerTitle.Text = "GPU Power";
		this.trackGPUPower.Location = new System.Drawing.Point(6, 48);
		this.trackGPUPower.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackGPUPower.Maximum = 25;
		this.trackGPUPower.Minimum = 5;
		this.trackGPUPower.Name = "trackGPUPower";
		this.trackGPUPower.Size = new System.Drawing.Size(496, 90);
		this.trackGPUPower.TabIndex = 42;
		this.trackGPUPower.TickFrequency = 5;
		this.trackGPUPower.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackGPUPower.Value = 25;
		this.panelGPUMemory.AutoSize = true;
		this.panelGPUMemory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelGPUMemory.Controls.Add(this.labelGPUMemory);
		this.panelGPUMemory.Controls.Add(this.labelGPUMemoryTitle);
		this.panelGPUMemory.Controls.Add(this.trackGPUMemory);
		this.panelGPUMemory.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPUMemory.Location = new System.Drawing.Point(0, 308);
		this.panelGPUMemory.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPUMemory.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelGPUMemory.Name = "panelGPUMemory";
		this.panelGPUMemory.Size = new System.Drawing.Size(520, 124);
		this.panelGPUMemory.TabIndex = 3;
		this.labelGPUMemory.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPUMemory.Location = new System.Drawing.Point(344, 14);
		this.labelGPUMemory.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUMemory.Name = "labelGPUMemory";
		this.labelGPUMemory.Size = new System.Drawing.Size(160, 32);
		this.labelGPUMemory.TabIndex = 44;
		this.labelGPUMemory.Text = "2000 MHz";
		this.labelGPUMemory.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelGPUMemoryTitle.AutoSize = true;
		this.labelGPUMemoryTitle.Location = new System.Drawing.Point(10, 14);
		this.labelGPUMemoryTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUMemoryTitle.Name = "labelGPUMemoryTitle";
		this.labelGPUMemoryTitle.Size = new System.Drawing.Size(241, 32);
		this.labelGPUMemoryTitle.TabIndex = 43;
		this.labelGPUMemoryTitle.Text = "Memory Clock Offset";
		this.trackGPUMemory.LargeChange = 100;
		this.trackGPUMemory.Location = new System.Drawing.Point(6, 48);
		this.trackGPUMemory.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackGPUMemory.Maximum = 300;
		this.trackGPUMemory.Name = "trackGPUMemory";
		this.trackGPUMemory.Size = new System.Drawing.Size(496, 90);
		this.trackGPUMemory.SmallChange = 10;
		this.trackGPUMemory.TabIndex = 42;
		this.trackGPUMemory.TickFrequency = 50;
		this.trackGPUMemory.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.panelGPUCore.AutoSize = true;
		this.panelGPUCore.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelGPUCore.Controls.Add(this.labelGPUCore);
		this.panelGPUCore.Controls.Add(this.trackGPUCore);
		this.panelGPUCore.Controls.Add(this.labelGPUCoreTitle);
		this.panelGPUCore.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPUCore.Location = new System.Drawing.Point(0, 184);
		this.panelGPUCore.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPUCore.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelGPUCore.Name = "panelGPUCore";
		this.panelGPUCore.Size = new System.Drawing.Size(520, 124);
		this.panelGPUCore.TabIndex = 2;
		this.labelGPUCore.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPUCore.Location = new System.Drawing.Point(326, 16);
		this.labelGPUCore.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUCore.Name = "labelGPUCore";
		this.labelGPUCore.Size = new System.Drawing.Size(176, 32);
		this.labelGPUCore.TabIndex = 29;
		this.labelGPUCore.Text = "1500 MHz";
		this.labelGPUCore.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.trackGPUCore.LargeChange = 100;
		this.trackGPUCore.Location = new System.Drawing.Point(6, 48);
		this.trackGPUCore.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackGPUCore.Maximum = 300;
		this.trackGPUCore.Name = "trackGPUCore";
		this.trackGPUCore.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.trackGPUCore.Size = new System.Drawing.Size(496, 90);
		this.trackGPUCore.SmallChange = 10;
		this.trackGPUCore.TabIndex = 18;
		this.trackGPUCore.TickFrequency = 50;
		this.trackGPUCore.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.labelGPUCoreTitle.AutoSize = true;
		this.labelGPUCoreTitle.Location = new System.Drawing.Point(10, 16);
		this.labelGPUCoreTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUCoreTitle.Name = "labelGPUCoreTitle";
		this.labelGPUCoreTitle.Size = new System.Drawing.Size(201, 32);
		this.labelGPUCoreTitle.TabIndex = 17;
		this.labelGPUCoreTitle.Text = "Core Clock Offset";
		this.panelGPUClockLimit.AutoSize = true;
		this.panelGPUClockLimit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelGPUClockLimit.Controls.Add(this.labelGPUClockLimit);
		this.panelGPUClockLimit.Controls.Add(this.trackGPUClockLimit);
		this.panelGPUClockLimit.Controls.Add(this.labelGPUClockLimitTitle);
		this.panelGPUClockLimit.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelGPUClockLimit.Location = new System.Drawing.Point(0, 60);
		this.panelGPUClockLimit.Margin = new System.Windows.Forms.Padding(4);
		this.panelGPUClockLimit.MaximumSize = new System.Drawing.Size(0, 124);
		this.panelGPUClockLimit.Name = "panelGPUClockLimit";
		this.panelGPUClockLimit.Size = new System.Drawing.Size(520, 124);
		this.panelGPUClockLimit.TabIndex = 1;
		this.labelGPUClockLimit.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPUClockLimit.Location = new System.Drawing.Point(326, 16);
		this.labelGPUClockLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUClockLimit.Name = "labelGPUClockLimit";
		this.labelGPUClockLimit.Size = new System.Drawing.Size(176, 32);
		this.labelGPUClockLimit.TabIndex = 29;
		this.labelGPUClockLimit.Text = "1500 MHz";
		this.labelGPUClockLimit.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.trackGPUClockLimit.LargeChange = 100;
		this.trackGPUClockLimit.Location = new System.Drawing.Point(6, 48);
		this.trackGPUClockLimit.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.trackGPUClockLimit.Maximum = 3000;
		this.trackGPUClockLimit.Name = "trackGPUClockLimit";
		this.trackGPUClockLimit.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.trackGPUClockLimit.Size = new System.Drawing.Size(496, 90);
		this.trackGPUClockLimit.SmallChange = 10;
		this.trackGPUClockLimit.TabIndex = 18;
		this.trackGPUClockLimit.TickFrequency = 50;
		this.trackGPUClockLimit.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.labelGPUClockLimitTitle.AutoSize = true;
		this.labelGPUClockLimitTitle.Location = new System.Drawing.Point(10, 16);
		this.labelGPUClockLimitTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPUClockLimitTitle.Name = "labelGPUClockLimitTitle";
		this.labelGPUClockLimitTitle.Size = new System.Drawing.Size(188, 32);
		this.labelGPUClockLimitTitle.TabIndex = 17;
		this.labelGPUClockLimitTitle.Text = "Core Clock Limit";
		this.panelTitleGPU.AutoSize = true;
		this.panelTitleGPU.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelTitleGPU.Controls.Add(this.pictureGPU);
		this.panelTitleGPU.Controls.Add(this.labelGPU);
		this.panelTitleGPU.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTitleGPU.Location = new System.Drawing.Point(0, 0);
		this.panelTitleGPU.Margin = new System.Windows.Forms.Padding(4);
		this.panelTitleGPU.Name = "panelTitleGPU";
		this.panelTitleGPU.Size = new System.Drawing.Size(520, 60);
		this.panelTitleGPU.TabIndex = 0;
		this.pictureGPU.BackgroundImage = Asus.Properties.Resources.icons8_video_card_32;
		this.pictureGPU.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.pictureGPU.ErrorImage = null;
		this.pictureGPU.InitialImage = null;
		this.pictureGPU.Location = new System.Drawing.Point(10, 18);
		this.pictureGPU.Margin = new System.Windows.Forms.Padding(4, 2, 4, 10);
		this.pictureGPU.Name = "pictureGPU";
		this.pictureGPU.Size = new System.Drawing.Size(32, 32);
		this.pictureGPU.TabIndex = 41;
		this.pictureGPU.TabStop = false;
		this.labelGPU.AutoSize = true;
		this.labelGPU.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelGPU.Location = new System.Drawing.Point(45, 17);
		this.labelGPU.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.labelGPU.Name = "labelGPU";
		this.labelGPU.Size = new System.Drawing.Size(162, 32);
		this.labelGPU.TabIndex = 40;
		this.labelGPU.Text = "GPU Settings";
		this.panelNav.AutoSize = true;
		this.panelNav.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelNav.Controls.Add(this.tableNav);
		this.panelNav.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelNav.Location = new System.Drawing.Point(10, 0);
		this.panelNav.Margin = new System.Windows.Forms.Padding(4);
		this.panelNav.Name = "panelNav";
		this.panelNav.Size = new System.Drawing.Size(520, 66);
		this.panelNav.TabIndex = 0;
		this.tableNav.ColumnCount = 3;
		this.tableNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333332f));
		this.tableNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333332f));
		this.tableNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333332f));
		this.tableNav.Controls.Add(this.buttonAdvanced, 0, 0);
		this.tableNav.Controls.Add(this.buttonGPU, 0, 0);
		this.tableNav.Controls.Add(this.buttonCPU, 0, 0);
		this.tableNav.Dock = System.Windows.Forms.DockStyle.Top;
		this.tableNav.Location = new System.Drawing.Point(0, 0);
		this.tableNav.MinimumSize = new System.Drawing.Size(0, 62);
		this.tableNav.Name = "tableNav";
		this.tableNav.Padding = new System.Windows.Forms.Padding(0, 3, 0, 1);
		this.tableNav.RowCount = 1;
		this.tableNav.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableNav.Size = new System.Drawing.Size(520, 66);
		this.tableNav.TabIndex = 42;
		this.buttonAdvanced.Activated = false;
		this.buttonAdvanced.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonAdvanced.BorderColor = System.Drawing.Color.Transparent;
		this.buttonAdvanced.BorderRadius = 2;
		this.buttonAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonAdvanced.Location = new System.Drawing.Point(350, 5);
		this.buttonAdvanced.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.buttonAdvanced.Name = "buttonAdvanced";
		this.buttonAdvanced.Secondary = true;
		this.buttonAdvanced.Size = new System.Drawing.Size(166, 58);
		this.buttonAdvanced.TabIndex = 2;
		this.buttonAdvanced.Text = "Advanced";
		this.buttonAdvanced.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonAdvanced.UseVisualStyleBackColor = false;
		this.buttonGPU.Activated = false;
		this.buttonGPU.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonGPU.BorderColor = System.Drawing.Color.Transparent;
		this.buttonGPU.BorderRadius = 2;
		this.buttonGPU.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonGPU.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonGPU.Location = new System.Drawing.Point(177, 5);
		this.buttonGPU.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.buttonGPU.Name = "buttonGPU";
		this.buttonGPU.Secondary = true;
		this.buttonGPU.Size = new System.Drawing.Size(165, 58);
		this.buttonGPU.TabIndex = 1;
		this.buttonGPU.Text = "GPU";
		this.buttonGPU.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonGPU.UseVisualStyleBackColor = false;
		this.buttonCPU.Activated = false;
		this.buttonCPU.BackColor = System.Drawing.SystemColors.ControlLight;
		this.buttonCPU.BorderColor = System.Drawing.Color.Transparent;
		this.buttonCPU.BorderRadius = 2;
		this.buttonCPU.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonCPU.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonCPU.Location = new System.Drawing.Point(4, 5);
		this.buttonCPU.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.buttonCPU.Name = "buttonCPU";
		this.buttonCPU.Secondary = true;
		this.buttonCPU.Size = new System.Drawing.Size(165, 58);
		this.buttonCPU.TabIndex = 0;
		this.buttonCPU.Text = "CPU";
		this.buttonCPU.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.buttonCPU.UseVisualStyleBackColor = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(192f, 192f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.AutoSize = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.ClientSize = new System.Drawing.Size(1350, 1100);
		base.Controls.Add(this.panelFans);
		base.Controls.Add(this.panelSliders);
		base.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(26, 1100);
		base.Name = "Fans";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "Fans and Power";
		this.panelFans.ResumeLayout(false);
		this.panelFans.PerformLayout();
		this.tableFanCharts.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.chartGPU).EndInit();
		((System.ComponentModel.ISupportInitialize)this.chartCPU).EndInit();
		((System.ComponentModel.ISupportInitialize)this.chartXGM).EndInit();
		((System.ComponentModel.ISupportInitialize)this.chartMid).EndInit();
		this.panelTitleFans.ResumeLayout(false);
		this.panelTitleFans.PerformLayout();
		this.tableLayoutModes.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.picturePerf).EndInit();
		this.panelHysteresis.ResumeLayout(false);
		this.panelHysteresis.PerformLayout();
		this.tableHysteresis.ResumeLayout(false);
		this.tableHysteresis.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackHysteresisUp).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackHysteresisDown).EndInit();
		this.panelApplyFans.ResumeLayout(false);
		this.panelApplyFans.PerformLayout();
		this.panelSliders.ResumeLayout(false);
		this.panelSliders.PerformLayout();
		this.panelAdvanced.ResumeLayout(false);
		this.panelAdvanced.PerformLayout();
		this.panelAdvancedAlways.ResumeLayout(false);
		this.panelAdvancedApply.ResumeLayout(false);
		this.panelAdvancedReadLimits.ResumeLayout(false);
		this.panelUViGPU.ResumeLayout(false);
		this.panelUViGPU.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackUViGPU).EndInit();
		this.panelUV.ResumeLayout(false);
		this.panelUV.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackUV).EndInit();
		this.panelTitleAdvanced.ResumeLayout(false);
		this.panelTitleAdvanced.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureUV).EndInit();
		this.panelTemperature.ResumeLayout(false);
		this.panelTemperature.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackTemp).EndInit();
		this.panelTitleTemp.ResumeLayout(false);
		this.panelTitleTemp.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureTemp).EndInit();
		this.panelDownload.ResumeLayout(false);
		this.panelDownload.PerformLayout();
		this.panelPawnIO.ResumeLayout(false);
		this.panelPawnIO.PerformLayout();
		this.panelPower.ResumeLayout(false);
		this.panelPower.PerformLayout();
		this.panelApplyPower.ResumeLayout(false);
		this.panelCPU.ResumeLayout(false);
		this.panelCPU.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackCPU).EndInit();
		this.panelFast.ResumeLayout(false);
		this.panelFast.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackFast).EndInit();
		this.panelSlow.ResumeLayout(false);
		this.panelSlow.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackSlow).EndInit();
		this.panelTotal.ResumeLayout(false);
		this.panelTotal.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackTotal).EndInit();
		this.panelTitleCPU.ResumeLayout(false);
		this.panelTitleCPU.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBoxCPU).EndInit();
		this.panelBoost.ResumeLayout(false);
		this.panelBoostTitle.ResumeLayout(false);
		this.panelBoostTitle.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBoost).EndInit();
		this.panelPowerMode.ResumeLayout(false);
		this.panelPowerModeTItle.ResumeLayout(false);
		this.panelPowerModeTItle.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.picturePowerMode).EndInit();
		this.panelGPU.ResumeLayout(false);
		this.panelGPU.PerformLayout();
		this.panelGPUTemp.ResumeLayout(false);
		this.panelGPUTemp.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUTemp).EndInit();
		this.panelGPUBoost.ResumeLayout(false);
		this.panelGPUBoost.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUBoost).EndInit();
		this.panelGPUPower.ResumeLayout(false);
		this.panelGPUPower.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUPower).EndInit();
		this.panelGPUMemory.ResumeLayout(false);
		this.panelGPUMemory.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUMemory).EndInit();
		this.panelGPUCore.ResumeLayout(false);
		this.panelGPUCore.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUCore).EndInit();
		this.panelGPUClockLimit.ResumeLayout(false);
		this.panelGPUClockLimit.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackGPUClockLimit).EndInit();
		this.panelTitleGPU.ResumeLayout(false);
		this.panelTitleGPU.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureGPU).EndInit();
		this.panelNav.ResumeLayout(false);
		this.tableNav.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

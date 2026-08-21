using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.Automation;
using Asus.AutoUpdate;
using Asus.Battery;
using Asus.Display;
using Asus.Fan;
using Asus.Helpers;
using Asus.Input;
using Asus.Mode;
using Asus.Properties;
using Asus.UI;
using Asus.USB;

namespace Asus;

public class SettingsForm : RForm
{
	private ContextMenuStrip contextMenuStrip = new CustomContextMenu();

	private ToolStripMenuItem menuEco;

	private ToolStripMenuItem menuStandard;

	private ToolStripMenuItem menuUltimate;

	private ToolStripMenuItem menuOptimized;

	private DonateControl donateControl;

	public object? gpuControl; // Ultralight: GPU control removed

	public AutoUpdateControl updateControl;

	public static System.Timers.Timer sensorTimer = null;

	private static readonly bool sensorsAlways = AppConfig.Is("sensors_always");

	private readonly System.Windows.Forms.Timer batteryTimer = new System.Windows.Forms.Timer
	{
		Interval = 200
	};

	public Fans? fansForm;

	public Extra? extraForm;

	public Updates? updatesForm;


	public About? aboutForm;

	private static long lastRefresh;

	private static long lastBatteryRefresh;

	private static long lastLostFocus;

	private bool isGpuSection = true;

	private bool isMuxGpu = true;

	private bool batteryMouseOver;

	private bool batteryFullMouseOver;

	private bool sliderGammaIgnore;

	private bool activateCheck;

	private (int, bool, bool)? lastIcon;

	private bool isDark = RForm.CheckSystemDarkModeStatus();

	private IContainer components;

	private Panel panelMatrix;

	private Panel panelBattery;

	private Panel panelFooter;



	private CheckBox checkStartup;

	private Panel panelPerformance;

	private TableLayoutPanel tablePerf;

	private RButton buttonTurbo;

	private RButton buttonBalanced;

	private RButton buttonSilent;

	private Panel panelGPU;

	private TableLayoutPanel tableGPU;

	private RButton buttonXGM;

	private RButton buttonUltimate;

	private RButton buttonStandard;

	private RButton buttonEco;

	private Panel panelScreen;

	private TableLayoutPanel tableScreen;

	private RButton buttonScreenAuto;

	private RButton button60Hz;

	private Panel panelKeyboard;

	private TableLayoutPanel tableLayoutMatrix;

	private RComboBox comboMatrixRunning;

	private RComboBox comboMatrix;

	private TableLayoutPanel tableLayoutKeyboard;

	private RComboBox comboKeyboard;

	private RButton button120Hz;

	private RButton buttonOptimized;

	private Label labelTipGPU;

	private Label labelTipScreen;

	private RButton buttonMiniled;

	private RButton buttonMatrix;

	private RButton buttonKeyboardColor; // Ultralight: RColorButton removed




	private Slider sliderBattery;

	private Panel panelGPUTitle;

	private PictureBox pictureGPU;

	private ToolTip toolTip;

	private Label labelGPU;

	private Label labelGPUFan;

	private Panel panelCPUTitle;

	private CheckBox checkAutoMode;

	private PictureBox picturePerf;

	private Label labelPerf;

	private Label labelCPUFan;

	private Panel panelScreenTitle;

	private Label labelMidFan;

	private PictureBox pictureScreen;

	private Label labelSreen;

	private Panel panelKeyboardTitle;

	private PictureBox pictureKeyboard;

	private Label labelKeyboard;

	private Panel panelMatrixTitle;

	private PictureBox pictureMatrix;

	private Label labelMatrix;

	private Panel panelBatteryTitle;

	private Label labelBattery;

	private PictureBox pictureBattery;

	private Label labelBatteryTitle;

	// Header strip (reference design: logo + status + close, single compact window).
	private Panel panelHeader;
	private Panel panelHeaderRight;
	private PictureBox pictureLogo;
	private PictureBox pictureHeartbeat;
	private Label labelHeaderTitle;
	private Label labelHeaderStatus;
	private Label labelHeaderCPU;
	private Label labelHeaderFan;
	private RButton buttonHeaderClose;

	// Battery card quick action buttons and layout


	// AI Auto card controls
	private Panel panelAiAuto;
	private Panel panelAiAutoTitle;
	private PictureBox pictureAiAuto;
	private Label labelAiAutoTitle;
	private RButton buttonAiAutoToggle;
	private Label labelAiAutoStatus;
	// Quick controls strip (Fn Lock, Energy Saver)
	private Panel panelQuickControls;
	private TableLayoutPanel tableQuickControls;


	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

	private Panel panelStartup;

	private RButton buttonStopGPU;

	private TableLayoutPanel tableButtons;

	private Panel panelPeripherals;

	private TableLayoutPanel tableLayoutPeripherals;

	private RButton buttonPeripheral2;

	private RButton buttonPeripheral3;

	private RButton buttonPeripheral1;

	private RButton buttonKeyboard;

	private RButton buttonUpdates;

	private Label labelCharge;

	private RButton buttonFnLock;

	private RButton buttonBatteryFull;

	private Panel panelAlly;

	private TableLayoutPanel tableLayoutAlly;

	private RButton buttonControllerMode;

	private Panel panelAllyTitle;

	private Label labelAlly;

	private PictureBox pictureAlly;

	private RButton buttonBacklight;

	private TableLayoutPanel tableAMD;

	private RButton buttonFPS;

	private RButton buttonController;

	private RButton buttonOverlay;

	private Panel panelGamma;

	private Slider sliderGamma;

	private Panel panelGammaTitle;

	private Label labelGamma;

	private PictureBox pictureGamma;

	private Label labelGammaTitle;

	private TableLayoutPanel tableVisual;

	private RComboBox comboVisual;

	private RComboBox comboGamut;

	private RComboBox comboColorTemp;

	private RButton buttonInstallColor;

	private Label labelVisual;

	private RButton buttonFHD;

	private RButton buttonAutoTDP;

	private Label labelBacklight;

	private Panel panelVersion;

	private Label labelVersion;

	private RBadgeButton buttonDonate;

	private RButton buttonEnergySaver;

	private RButton buttonAmdOled;

	private RButton buttonArmoury;

	private RButton buttonHDRControl;

	private Panel panelRearLight;

	private TableLayoutPanel tableLayoutRearLight;

	private RButton buttonRearColor; // Ultralight: RColorButton removed

	private RComboBox comboRearLight;

	private Panel panelRearLightTitle;

	private PictureBox pictureRearLight;

	private Label labelRearLight;

	public SettingsForm()
	{
		InitializeComponent();
		CreateHeader();
		InitTheme(setDPI: true);
		// gpuControl removed for ultralight build
		updateControl = new AutoUpdateControl(this);
		buttonSilent.Text = "Silent";
		buttonBalanced.Text = "Balanced";
		buttonTurbo.Text = "Performance";
		buttonEco.Text = Strings.EcoMode;
		buttonUltimate.Text = Strings.UltimateMode;
		buttonStandard.Text = Strings.StandardMode;
		buttonOptimized.Text = Strings.Optimized;
		buttonStopGPU.Text = Strings.StopGPUApps;
		buttonScreenAuto.Text = "Auto";
		button60Hz.Text = "60Hz";
		button120Hz.Text = "Display";
		buttonMiniled.Text = Strings.Multizone;
		buttonKeyboardColor.Text = "Color \u2588";
		buttonKeyboard.Text = "\u2699 Extra";
		labelPerf.Text = "Mode: Balanced";
		labelGPU.Text = Strings.GPUMode;
		labelSreen.Text = "Laptop Screen";
		labelBatteryTitle.Text = "Battery Charge Limit";
		checkStartup.Text = Strings.RunOnStartup;

		buttonUpdates.Text = "\u2699 Updates";

		buttonDonate.Text = Strings.Donate;
		sliderBattery.AccessibleName = Strings.BatteryChargeLimit;

		buttonUpdates.AccessibleName = Strings.BiosAndDriverUpdates;
		panelPerformance.AccessibleName = Strings.PerformanceMode;
		buttonSilent.AccessibleName = Strings.Silent;
		buttonBalanced.AccessibleName = Strings.Balanced;
		buttonTurbo.AccessibleName = Strings.Turbo;
		panelGPU.AccessibleName = Strings.GPUMode;
		panelScreen.AccessibleName = Strings.LaptopScreen;
		buttonScreenAuto.AccessibleName = Strings.AutoMode;
		base.FormClosing += SettingsForm_FormClosing;
		base.Deactivate += SettingsForm_LostFocus;
		base.Activated += SettingsForm_Focused;
		base.LocationChanged += delegate
		{
			if (Visible && WindowState == FormWindowState.Normal)
			{
				SaveWindowPosition();
			}
		};

		// Card rounded background painting
		panelPerformance.Paint += (s, e) => ControlHelper.PaintCard(panelPerformance, e.Graphics, 8);
		panelBattery.Paint += (s, e) => ControlHelper.PaintCard(panelBattery, e.Graphics, 8);
		panelScreen.Paint += (s, e) => ControlHelper.PaintCard(panelScreen, e.Graphics, 8);
		panelAiAuto.Paint += (s, e) => ControlHelper.PaintCard(panelAiAuto, e.Graphics, 8);
		panelQuickControls.Paint += (s, e) => ControlHelper.PaintCard(panelQuickControls, e.Graphics, 8);

		// Performance mode clicks disable AI Auto (manual override)
		buttonSilent.Click += (s, e) => { if (checkAutoMode.Checked) checkAutoMode.Checked = false; ButtonSilent_Click(s, e); };
		buttonBalanced.Click += (s, e) => { if (checkAutoMode.Checked) checkAutoMode.Checked = false; ButtonBalanced_Click(s, e); };
		buttonTurbo.Click += (s, e) => { if (checkAutoMode.Checked) checkAutoMode.Checked = false; ButtonTurbo_Click(s, e); };

		buttonEco.Click += ButtonEco_Click;
		buttonStandard.Click += ButtonStandard_Click;
		buttonUltimate.Click += ButtonUltimate_Click;
		buttonOptimized.Click += ButtonOptimized_Click;
		buttonStopGPU.Click += ButtonStopGPU_Click;
		pictureGPU.Click += PictureGPU_Click;

		// Quick controls strip buttons
		buttonFnLock.Click += ButtonFnLock_Click;
		buttonEnergySaver.Click += ButtonEnergySaver_Click;


		buttonAiAutoToggle.Click += delegate
		{
			ToggleAiAuto();
			VisualiseAiAuto();
		};


		base.VisibleChanged += SettingsForm_VisibleChanged;
		button60Hz.Click += Button60Hz_Click;
		button120Hz.Click += Button120Hz_Click;
		buttonScreenAuto.Click += ButtonScreenAuto_Click;
		buttonMiniled.Click += ButtonMiniled_Click;
		buttonFHD.Click += ButtonFHD_Click;
		buttonHDRControl.Click += ButtonHDRControl_Click;

		labelCPUFan.Click += LabelCPUFan_Click;
		labelGPUFan.Click += LabelCPUFan_Click;
		checkStartup.Checked = Startup.IsScheduled();
		checkStartup.CheckedChanged += CheckStartup_CheckedChanged;
		labelVersion.Click += LabelVersion_Click;
		labelVersion.ForeColor = Color.FromArgb(128, Color.Gray);
		buttonUpdates.Click += ButtonUpdates_Click;
		sliderBattery.MouseUp += SliderBattery_MouseUp;
		sliderBattery.KeyUp += SliderBattery_KeyUp;
		sliderBattery.ValueChanged += SliderBattery_ValueChanged;
		batteryTimer.Tick += delegate
		{
			batteryTimer.Stop();
			BatteryControl.SetBatteryChargeLimit(sliderBattery.Value);
		};
		if (AppConfig.IsChargeLimit6080())
		{
			sliderBattery.supportedValues = new List<int> { 60, 65, 70, 75, 80, 100 };
		}
		sensorTimer = new System.Timers.Timer(AppConfig.Is("ai_auto_mode") ? (AutoModeControl.IntervalSeconds * 1000) : AppConfig.Get("sensor_timer", 1000));
		sensorTimer.Elapsed += OnTimedEvent;
		sensorTimer.Enabled = sensorsAlways;
		labelCharge.MouseEnter += PanelBattery_MouseEnter;
		labelCharge.MouseLeave += PanelBattery_MouseLeave;
		labelBattery.Click += LabelBattery_Click;
		buttonBatteryFull.MouseEnter += ButtonBatteryFull_MouseEnter;
		buttonBatteryFull.MouseLeave += ButtonBatteryFull_MouseLeave;
		buttonBatteryFull.Click += ButtonBatteryFull_Click;
		buttonOverlay.Click += ButtonOverlay_Click;
		buttonOverlay.BorderColor = RForm.colorStandard;
		Text = "Asus " + AppConfig.GetModelShort();
		try { Icon = Properties.Resources.standard; } catch {}
		base.TopMost = AppConfig.Is("topmost");
		base.Resize += SettingsForm_Resize;
		VisualiseFnLock();
		VisualiseEnergySaver();
		VisualiseBattery(sliderBattery.Value);
		VisualiseAiAuto();
		labelVisual.Click += LabelVisual_Click;
		labelCharge.Click += LabelCharge_Click;
		donateControl = new DonateControl(this, buttonDonate);
		donateControl.Init();
		labelBacklight.ForeColor = RForm.colorStandard;
		labelBacklight.Click += LabelBacklight_Click;
		panelPerformance.Focus();
		InitVisual();
	}

	public override bool InitTheme(bool setDPI = false)
	{
		bool changed = base.InitTheme(setDPI);
		StyleHeader();
		return changed;
	}

	// ---- Header strip (reference design) ----

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		// Single compact window: no OS caption, rounded corners, one close control.
		FormBorderStyle = FormBorderStyle.None;
		Region = ControlHelper.CreateRoundedRegion(Width, Height, 12);
		RestoreWindowPosition();
		FixHeaderLayout();
	}

	private void CreateHeader()
	{
		panelHeader = new Panel();
		pictureLogo = new PictureBox();
		pictureHeartbeat = new PictureBox();
		labelHeaderTitle = new Label();
		labelHeaderStatus = new Label();
		labelHeaderCPU = new Label();
		labelHeaderFan = new Label();
		buttonHeaderClose = new RButton();

		panelHeader.SuspendLayout();

		panelHeader.Name = "panelHeader";
		panelHeader.Dock = DockStyle.Top;
		panelHeader.Height = 42;
		panelHeader.Padding = new Padding(8, 0, 8, 0);

		panelHeaderRight = new Panel
		{
			Dock = DockStyle.Right,
			Width = 220,
			BackColor = Color.Transparent
		};

		buttonHeaderClose.Text = "\u2715";
		buttonHeaderClose.Borderless = true;
		buttonHeaderClose.BorderRadius = 4;
		buttonHeaderClose.Size = new Size(24, 24);
		buttonHeaderClose.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		buttonHeaderClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		buttonHeaderClose.Click += delegate
		{
			HideAll();
		};

		labelHeaderCPU.Text = "CPU: --\u00b0C";
		labelHeaderCPU.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
		labelHeaderCPU.AutoSize = true;
		labelHeaderCPU.Anchor = AnchorStyles.Top | AnchorStyles.Right;

		labelHeaderFan.Text = "Fan: -- RPM";
		labelHeaderFan.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
		labelHeaderFan.AutoSize = true;
		labelHeaderFan.Anchor = AnchorStyles.Top | AnchorStyles.Right;

		panelHeaderRight.Controls.Add(buttonHeaderClose);
		panelHeaderRight.Controls.Add(labelHeaderCPU);
		panelHeaderRight.Controls.Add(labelHeaderFan);

		try
		{
			pictureLogo.Image = Properties.Resources.standard.ToBitmap();
		}
		catch
		{
			// Never block startup on a missing logo asset.
		}
		pictureLogo.SizeMode = PictureBoxSizeMode.Zoom;
		pictureLogo.Location = new Point(8, 11);
		pictureLogo.Size = new Size(20, 20);
		pictureLogo.Cursor = Cursors.Hand;
		pictureLogo.MouseDown += Header_MouseDown;

		try
		{
			pictureHeartbeat.Image = ControlHelper.TintImage(Properties.Resources.icons8_heartbeat_32, RForm.colorAccent);
		}
		catch
		{
		}
		pictureHeartbeat.SizeMode = PictureBoxSizeMode.Zoom;
		pictureHeartbeat.Location = new Point(32, 13);
		pictureHeartbeat.Size = new Size(16, 16);
		pictureHeartbeat.MouseDown += Header_MouseDown;

		labelHeaderTitle.Text = "System Status";
		labelHeaderTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
		labelHeaderTitle.Location = new Point(52, 11);
		labelHeaderTitle.AutoSize = true;
		labelHeaderTitle.MouseDown += Header_MouseDown;

		panelHeader.Controls.Add(pictureLogo);
		panelHeader.Controls.Add(pictureHeartbeat);
		panelHeader.Controls.Add(labelHeaderTitle);
		panelHeader.Controls.Add(panelHeaderRight);
		panelHeader.MouseDown += Header_MouseDown;

		// Docked last so the header sits at the very top of the compact window.
		Controls.Add(panelHeader);
		panelHeader.ResumeLayout(false);
		panelHeader.PerformLayout();

		// Red section markers on every section title (reference accent language).
		AddSectionMarker(panelCPUTitle);
		AddSectionMarker(panelBatteryTitle);
		AddSectionMarker(panelScreenTitle);
		AddSectionMarker(panelAiAutoTitle);
	}

	private void StyleHeader()
	{
		if (panelHeader == null) return;
		panelHeader.BackColor = RForm.formBack;
		labelHeaderTitle.ForeColor = RForm.foreMain;
		labelHeaderCPU.ForeColor = Color.FromArgb(255, 180, 184, 192);
		labelHeaderFan.ForeColor = Color.FromArgb(255, 180, 184, 192);
		buttonHeaderClose.BackColor = Color.Transparent;
		buttonHeaderClose.ForeColor = Color.FromArgb(255, 160, 164, 172);
		buttonHeaderClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 220, 36, 44);
	}

	private void FixHeaderLayout()
	{
		// Deterministic right-side layout, computed from actual sizes after form layout.
		if (panelHeaderRight == null || buttonHeaderClose == null || labelHeaderFan == null || labelHeaderCPU == null) return;
		int closeX = panelHeaderRight.Width - buttonHeaderClose.Width - 4;
		buttonHeaderClose.Location = new Point(closeX, (panelHeaderRight.Height - buttonHeaderClose.Height) / 2);
		int fanX = closeX - labelHeaderFan.Width - 8;
		int cpuX = fanX - labelHeaderCPU.Width - 8;
		labelHeaderFan.Location = new Point(fanX, (panelHeaderRight.Height - labelHeaderFan.Height) / 2);
		labelHeaderCPU.Location = new Point(cpuX, (panelHeaderRight.Height - labelHeaderCPU.Height) / 2);
	}

	private void AddSectionMarker(Panel titlePanel)
	{
		if (titlePanel == null) return;
		var marker = new Panel
		{
			BackColor = RForm.colorAccent,
			Size = new Size(3, 14),
			TabStop = false
		};
		titlePanel.Controls.Add(marker);
		marker.BringToFront();
		marker.Location = new Point(2, (titlePanel.Height - marker.Height) / 2);
	}

	private void Header_MouseDown(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && WindowState == FormWindowState.Normal)
		{
			ReleaseCapture();
			SendMessage(Handle, 0x00A1, new IntPtr(2), IntPtr.Zero); // WM_NCLBUTTONDOWN, HTCAPTION
		}
	}

	private void ButtonArmoury_Click(object? sender, EventArgs e)
	{
		if (MessageBox.Show(this, "Armoury Crate is active, download official uninstaller app?", "Armoury Crate", MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			AsusService.RunArmouryUninstaller();
		}
	}

	private void ButtonAmdOled_Click(object? sender, EventArgs e)
	{
		AmdDisplay.RunAdrenaline();
		activateCheck = true;
	}

	private void LabelBattery_Click(object? sender, EventArgs e)
	{
		HardwareControl.chargeWatt = !HardwareControl.chargeWatt;
		RefreshSensors(force: true);
	}



	private void LabelBacklight_Click(object? sender, EventArgs e)
	{
		if (AppConfig.IsDynamicLighting() && DynamicLightingHelper.IsEnabled())
		{
			DynamicLightingHelper.OpenSettings();
		}
	}

	private void ButtonFHD_Click(object? sender, EventArgs e)
	{
		ScreenControl.ToogleFHD();
	}

	private void ButtonHDRControl_Click(object? sender, EventArgs e)
	{
		ScreenControl.ToogleHDRControl();
	}

	private void SliderBattery_ValueChanged(object? sender, EventArgs e)
	{
		VisualiseBatteryTitle(sliderBattery.Value);
	}

	private void SliderBattery_KeyUp(object? sender, KeyEventArgs e)
	{
		batteryTimer.Stop();
		batteryTimer.Start();
	}

	private void SliderBattery_MouseUp(object? sender, MouseEventArgs e)
	{
		batteryTimer.Stop();
		batteryTimer.Start();
	}

	private void LabelCharge_Click(object? sender, EventArgs e)
	{
		BatteryControl.BatteryReport();
	}

	private void LabelVisual_Click(object? sender, EventArgs e)
	{
		labelVisual.Visible = false;
		VisualControl.forceVisual = true;
	}

	public void InitVisual()
	{
		if (AppConfig.Is("hide_visual"))
		{
			return;
		}
		if (AppConfig.IsOLED())
		{
			panelGamma.Visible = true;
			sliderGamma.Visible = true;
			labelGammaTitle.Text = Strings.FlickerFreeDimming + " / " + Strings.VisualMode;
			VisualiseBrightness();
			sliderGamma.ValueChanged += SliderGamma_ValueChanged;
			sliderGamma.MouseUp += SliderGamma_ValueChanged;
		}
		else
		{
			labelGammaTitle.Text = Strings.VisualMode;
		}
		Dictionary<SplendidGamut, string> gamutModes = VisualControl.GetGamutModes();
		if (gamutModes.Count > 0)
		{
			tableVisual.ColumnCount = 3;
			buttonInstallColor.Visible = false;
			panelGamma.Visible = true;
			tableVisual.Visible = true;
			SplendidCommand splendidCommand = (SplendidCommand)AppConfig.Get("visual", (int)VisualControl.GetDefaultVisualMode());
			int num = AppConfig.Get("color_temp", 50);
			comboVisual.DropDownStyle = ComboBoxStyle.DropDownList;
			comboVisual.DataSource = new BindingSource(VisualControl.GetVisualModes(), null);
			comboVisual.DisplayMember = "Value";
			comboVisual.ValueMember = "Key";
			comboVisual.SelectedValue = splendidCommand;
			comboColorTemp.DropDownStyle = ComboBoxStyle.DropDownList;
			comboColorTemp.DataSource = new BindingSource(VisualControl.GetTemperatures(), null);
			comboColorTemp.DisplayMember = "Value";
			comboColorTemp.ValueMember = "Key";
			comboColorTemp.SelectedValue = num;
			VisualControl.SetVisual(splendidCommand, num, init: true);
			comboVisual.SelectedValueChanged += ComboVisual_SelectedValueChanged;
			comboVisual.Visible = true;
			VisualiseDisabled();
			comboColorTemp.SelectedValueChanged += ComboVisual_SelectedValueChanged;
			comboColorTemp.Visible = true;
			if (gamutModes.Count > 1)
			{
				comboGamut.DropDownStyle = ComboBoxStyle.DropDownList;
				comboGamut.DataSource = new BindingSource(gamutModes, null);
				comboGamut.DisplayMember = "Value";
				comboGamut.ValueMember = "Key";
				comboGamut.SelectedValue = (SplendidGamut)AppConfig.Get("gamut", (int)VisualControl.GetDefaultGamut());
				comboGamut.SelectedValueChanged += ComboGamut_SelectedValueChanged;
				comboGamut.Visible = true;
			}
		}
		else if (ColorProfileHelper.ProfileExists())
		{
			tableVisual.ColumnCount = 2;
			buttonInstallColor.Text = Strings.DownloadColorProfiles;
			buttonInstallColor.Visible = true;
			buttonInstallColor.Click += ButtonInstallColorProfile_Click;
			panelGamma.Visible = true;
			tableVisual.Visible = true;
		}
	}

	public void CycleVisualMode(int delta)
	{
		if (comboVisual.Items.Count < 1)
		{
			return;
		}
		if (delta > 0)
		{
			if (comboVisual.SelectedIndex < comboVisual.Items.Count - 1)
			{
				comboVisual.SelectedIndex++;
			}
			else
			{
				comboVisual.SelectedIndex = 0;
			}
		}
		else if (comboVisual.SelectedIndex > 0)
		{
			comboVisual.SelectedIndex--;
		}
		else
		{
			comboVisual.SelectedIndex = comboVisual.Items.Count - 1;
		}
		Program.toast.RunToast(comboVisual.GetItemText(comboVisual.SelectedItem), ToastIcon.BrightnessUp);
	}

	private async void ButtonInstallColorProfile_Click(object? sender, EventArgs e)
	{
		await ColorProfileHelper.InstallProfile();
		InitVisual();
	}

	private void ComboGamut_SelectedValueChanged(object? sender, EventArgs e)
	{
		VisualControl.SetGamut((int)comboGamut.SelectedValue);
	}

	private void ComboVisual_SelectedValueChanged(object? sender, EventArgs e)
	{
		VisualControl.SetVisual((SplendidCommand)comboVisual.SelectedValue, (int)comboColorTemp.SelectedValue);
		VisualiseDisabled();
	}

	public void VisualiseBrightness()
	{
		if (base.InvokeRequired)
		{
			Invoke(VisualiseBrightness);
			return;
		}
		sliderGammaIgnore = true;
		sliderGamma.Value = VisualControl.GetBrightness();
		labelGamma.Text = sliderGamma.Value + "%";
		sliderGammaIgnore = false;
	}

	public void VisualiseAmdOled(bool status = false)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseAmdOled(status);
			});
		}
		else
		{
			buttonAmdOled.Visible = status;
		}
	}

	public void VisualiseArmoury(bool status = false)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseArmoury(status);
			});
		}
		else
		{
			buttonArmoury.Visible = status;
		}
	}

	public void VisualiseDisabled()
	{
		RComboBox rComboBox = comboGamut;
		bool enabled = (comboColorTemp.Enabled = AppConfig.Get("visual") != 18);
		rComboBox.Enabled = enabled;
	}

	public void VisualiseGamut()
	{
		if (base.InvokeRequired)
		{
			Invoke(VisualiseGamut);
		}
		else if (comboGamut.Items.Count > 0)
		{
			comboGamut.SelectedIndex = 0;
		}
	}

	private void SliderGamma_ValueChanged(object? sender, EventArgs e)
	{
		if (!sliderGammaIgnore)
		{
			VisualControl.SetBrightness(sliderGamma.Value);
		}
	}

	private void ButtonOverlay_Click(object? sender, EventArgs e)
	{
		ToggleOverlay();
	}

	public void VisualiseBacklight(int backlight)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseBacklight(backlight);
			});
		}
		else
		{
			buttonBacklight.Text = Math.Round((double)backlight * 33.33) + "%";
		}
	}

	public void VisualiseFPSLimit(int limit)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseFPSLimit(limit);
			});
		}
		else
		{
			buttonFPS.Text = "FPS Limit " + ((limit > 0 && limit <= 120) ? ((object)limit) : "OFF");
		}
	}

	public void VisualiseAutoTDP(bool status)
	{
		Logger.WriteLine($"Auto TDP: {status}");
		buttonAutoTDP.Activated = status;
	}

	private void SettingsForm_Focused(object? sender, EventArgs e)
	{
		if (activateCheck)
		{
			buttonAmdOled.Visible = AmdDisplay.IsOledPowerOptimization();
			activateCheck = false;
		}
	}

	private void SettingsForm_LostFocus(object? sender, EventArgs e)
	{
		lastLostFocus = DateTimeOffset.Now.ToUnixTimeMilliseconds();
	}

	private void ButtonBatteryFull_Click(object? sender, EventArgs e)
	{
		BatteryControl.ToggleBatteryLimitFull();
	}

	private void ButtonBatteryFull_MouseLeave(object? sender, EventArgs e)
	{
		batteryFullMouseOver = false;
		RefreshSensors(force: true);
	}

	private void ButtonBatteryFull_MouseEnter(object? sender, EventArgs e)
	{
		batteryFullMouseOver = true;
		labelCharge.Text = Strings.BatteryLimitFull;
	}

	private void SettingsForm_Resize(object? sender, EventArgs e)
	{
		if (base.WindowState != 0)
		{
			base.WindowState = FormWindowState.Normal;
			return;
		}
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;
		if (base.Left < workingArea.Left)
		{
			base.Left = workingArea.Left;
		}
		if (base.Top < workingArea.Top)
		{
			base.Top = workingArea.Top;
		}
		if (base.Right > workingArea.Right)
		{
			base.Left = workingArea.Right - base.Width;
		}
		if (base.Bottom > workingArea.Bottom)
		{
			base.Top = workingArea.Bottom - base.Height;
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		MaximumSize = base.Size;
		MinimumSize = base.Size;
	}

	private void PanelBattery_MouseEnter(object? sender, EventArgs e)
	{
		batteryMouseOver = true;
		ShowBatteryWear();
	}

	private void PanelBattery_MouseLeave(object? sender, EventArgs e)
	{
		batteryMouseOver = false;
		RefreshSensors(force: true);
	}

	private void ShowBatteryWear()
	{
		if (lastBatteryRefresh == 0L || Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastBatteryRefresh) > 900000)
		{
			lastBatteryRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			HardwareControl.RefreshBatteryHealth();
		}
		if (HardwareControl.batteryHealth != -1m)
		{
			labelCharge.Text = Strings.BatteryHealth + ": " + Math.Round(HardwareControl.batteryHealth, 1) + "%";
		}
	}

	private void SettingsForm_VisibleChanged(object? sender, EventArgs e)
	{
		ApplyAutoModeTimer();
		if (base.Visible)
		{
			updateControl.CheckForUpdates();
		}
	}

	private void ButtonUpdates_Click(object? sender, EventArgs e)
	{
		if (updatesForm == null || updatesForm.Text == "")
		{
			updatesForm = new Updates();
			AddOwnedForm(updatesForm);
		}
		if (updatesForm.Visible)
		{
			updatesForm.Close();
		}
		else
		{
			updatesForm.Show();
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 536 && m.WParam == 4)
		{
			Logger.WriteLine("System Suspend");
			Program.modeControl.SleepReset();
			m.Result = 1;
		}
		if (m.Msg == 536 && m.WParam == 18)
		{
			Logger.WriteLine("System Resume");
			BatteryControl.AutoBattery();
			m.Result = 1;
		}
		if (m.Msg == 536 && m.WParam == 32787)
		{
			NativeMethods.POWERBROADCAST_SETTING pOWERBROADCAST_SETTING = (NativeMethods.POWERBROADCAST_SETTING)m.GetLParam(typeof(NativeMethods.POWERBROADCAST_SETTING));
			if (pOWERBROADCAST_SETTING.PowerSetting == NativeMethods.PowerSettingGuid.LIDSWITCH_STATE_CHANGE)
			{
				switch (pOWERBROADCAST_SETTING.Data)
				{
				case 0:
					Logger.WriteLine("Lid Closed");
					BatteryControl.AutoBattery();
					InputDispatcher.lidClose = true;
					Aura.ApplyBrightness(0, "Lid");
					break;
				case 1:
					Logger.WriteLine("Lid Open");
					InputDispatcher.InitFNLock();
					InputDispatcher.lidClose = false;
					Aura.ApplyBrightness(InputDispatcher.GetBacklight(), "Lid");
					break;
				}
			}
			else if (pOWERBROADCAST_SETTING.PowerSetting == NativeMethods.PowerSettingGuid.EnergySaverStatus)
			{
				Logger.WriteLine("Battery Saver: " + pOWERBROADCAST_SETTING.Data);
				buttonEnergySaver.Visible = pOWERBROADCAST_SETTING.Data != 0;
			}
			else
			{
				switch (pOWERBROADCAST_SETTING.Data)
				{
				case 0:
					Logger.WriteLine("Monitor Power Off");
					Aura.SleepBrightness();
					XGM.NotifyShutdown();
					break;
				case 1:
					Logger.WriteLine("Monitor Power On");
					if (!Program.SetAutoModes(powerChanged: false, init: false, wakeup: true))
					{
						BatteryControl.AutoBattery();
					}
					break;
				case 2:
					Logger.WriteLine("Monitor Dimmed");
					break;
				}
			}
			m.Result = 1;
		}
		if (m.Msg == Program.WM_TASKBARCREATED)
		{
			Logger.WriteLine("Taskbar created, re-creating tray icon");
			if (Program.trayIcon != null)
			{
				Program.trayIcon.Visible = true;
			}
		}
		try
		{
			base.WndProc(ref m);
		}
		catch (Exception)
		{
		}
	}

	public void SetContextMenu()
	{
		int current = Modes.GetCurrent();
		foreach (ToolStripItem item in contextMenuStrip.Items.Cast<ToolStripItem>().ToList())
		{
			if (item is ToolStripMenuItem toolStripMenuItem)
			{
				toolStripMenuItem.Dispose();
			}
		}
		contextMenuStrip.Items.Clear();
		contextMenuStrip.ShowCheckMargin = true;
		contextMenuStrip.ImageScalingSize = new Size(16, 16);
		contextMenuStrip.ShowImageMargin = false;
		Padding margin = new Padding(5, 5, 5, 5);
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(Strings.PerformanceMode);
		toolStripMenuItem2.Margin = margin;
		toolStripMenuItem2.Enabled = false;
		contextMenuStrip.Items.Add(toolStripMenuItem2);
		foreach (KeyValuePair<int, string> mode in Modes.GetDictonary())
		{
			ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem(mode.Value);
			toolStripMenuItem3.Tag = mode.Key;
			toolStripMenuItem3.Click += delegate
			{
				Program.modeControl.SetPerformanceMode(mode.Key);
			};
			toolStripMenuItem3.Margin = margin;
			toolStripMenuItem3.Checked = mode.Key == current;
			contextMenuStrip.Items.Add(toolStripMenuItem3);
		}
		contextMenuStrip.Items.Add("-");
		ToolStripMenuItem openAsus = new ToolStripMenuItem(Strings.OpenAsus);
		openAsus.Margin = margin;
		openAsus.Click += delegate
		{
			Program.SettingsToggle(trayClick: true);
		};
		contextMenuStrip.Items.Add(openAsus);
		ToolStripMenuItem aiAuto = new ToolStripMenuItem(Strings.AiAuto);
		aiAuto.Margin = margin;
		aiAuto.Checked = AutoModeControl.IsEnabled;
		aiAuto.Click += delegate
		{
			Program.settingsForm.ToggleAiAuto();
		};
		contextMenuStrip.Items.Add(aiAuto);
		ToolStripMenuItem batteryTitle = new ToolStripMenuItem(Strings.BatteryChargeLimit);
		batteryTitle.Margin = margin;
		batteryTitle.Enabled = false;
		contextMenuStrip.Items.Add(batteryTitle);
		foreach (int limit in new int[] { 60, 80, 100 })
		{
			ToolStripMenuItem menuBattery = new ToolStripMenuItem(limit + "%");
			menuBattery.Margin = margin;
			menuBattery.Checked = sliderBattery.Value == limit;
			menuBattery.Click += delegate
			{
				sliderBattery.Value = limit;
				BatteryControl.SetBatteryChargeLimit(limit);
			};
			contextMenuStrip.Items.Add(menuBattery);
		}
		ToolStripMenuItem checkUpdates = new ToolStripMenuItem(Strings.CheckForUpdates);
		checkUpdates.Margin = margin;
		checkUpdates.Click += delegate
		{
			updateControl.Update();
		};
		contextMenuStrip.Items.Add(checkUpdates);
		contextMenuStrip.Items.Add("-");
		if (isGpuSection)
		{
			ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem(Strings.GPUMode);
			toolStripMenuItem4.Margin = margin;
			toolStripMenuItem4.Enabled = false;
			contextMenuStrip.Items.Add(toolStripMenuItem4);
			menuEco = new ToolStripMenuItem(Strings.EcoMode);
			menuEco.Click += ButtonEco_Click;
			menuEco.Margin = margin;
			menuEco.Checked = buttonEco.Activated;
			contextMenuStrip.Items.Add(menuEco);
			menuStandard = new ToolStripMenuItem(Strings.StandardMode);
			menuStandard.Click += ButtonStandard_Click;
			menuStandard.Margin = margin;
			menuStandard.Checked = buttonStandard.Activated;
			contextMenuStrip.Items.Add(menuStandard);
			menuUltimate = new ToolStripMenuItem(Strings.UltimateMode);
			menuUltimate.Click += ButtonUltimate_Click;
			menuUltimate.Margin = margin;
			menuUltimate.Checked = buttonUltimate.Activated;
			menuUltimate.Visible = isMuxGpu;
			contextMenuStrip.Items.Add(menuUltimate);
			menuOptimized = new ToolStripMenuItem(Strings.Optimized);
			menuOptimized.Click += ButtonOptimized_Click;
			menuOptimized.Margin = margin;
			menuOptimized.Checked = buttonOptimized.Activated;
			contextMenuStrip.Items.Add(menuOptimized);
			contextMenuStrip.Items.Add("-");
		}
		ToolStripMenuItem bwIcon = new ToolStripMenuItem(Strings.BWTrayIcon);
		bwIcon.Margin = margin;
		bwIcon.Checked = AppConfig.IsBWIcon();
		bwIcon.Click += delegate
		{
			bwIcon.Checked = !bwIcon.Checked;
			AppConfig.Set("bw_icon", bwIcon.Checked ? 1 : 0);
			VisualiseIcon();
		};
		contextMenuStrip.Items.Add(bwIcon);
		contextMenuStrip.Items.Add("-");
		ToolStripMenuItem toolStripMenuItem7 = new ToolStripMenuItem("About");
		toolStripMenuItem7.Click += ButtonAbout_Click;
		toolStripMenuItem7.Margin = margin;
		contextMenuStrip.Items.Add(toolStripMenuItem7);
		ToolStripMenuItem toolStripMenuItem8 = new ToolStripMenuItem(Strings.Quit);
		toolStripMenuItem8.Click += ButtonQuit_Click;
		toolStripMenuItem8.Margin = margin;
		contextMenuStrip.Items.Add(toolStripMenuItem8);
		contextMenuStrip.Renderer = new CustomMenuRenderer();
		InitContextMenuTheme();
		if (Program.trayIcon != null)
		{
			Program.trayIcon.ContextMenuStrip = contextMenuStrip;
		}
	}

	public void InitContextMenuTheme()
	{
		if (contextMenuStrip != null)
		{
			contextMenuStrip.BackColor = BackColor;
			contextMenuStrip.ForeColor = ForeColor;
		}
		donateControl?.ApplyTheme();
	}

	private void ButtonXGM_Click(object? sender, EventArgs e)
	{
		// Ultralight: XGM removed
	}

	private void ButtonAbout_Click(object? sender, EventArgs e)
	{
		if (aboutForm == null || aboutForm.Text == "")
		{
			aboutForm = new About();
			AddOwnedForm(aboutForm);
		}
		if (aboutForm.Visible)
		{
			aboutForm.Close();
		}
		else
		{
			aboutForm.Show();
		}
	}

	public void SetVersionLabel(string label, bool update = false)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				labelVersion.Text = label;
				if (update)
				{
					labelVersion.ForeColor = RForm.colorTurbo;
				}
			});
		}
		else
		{
			labelVersion.Text = label;
			if (update)
			{
				labelVersion.ForeColor = RForm.colorTurbo;
			}
		}
	}

	private void LabelVersion_Click(object? sender, EventArgs e)
	{
		updateControl.Update();
	}

	private static void OnTimedEvent(object? source, ElapsedEventArgs? e)
	{
		Program.settingsForm.RefreshSensors();
	}

	private void ButtonFHD_MouseHover(object? sender, EventArgs e)
	{
		labelTipScreen.Text = "Switch to " + ((buttonFHD.Text == "FHD") ? "UHD" : "FHD") + " Mode";
	}

	private void Button120Hz_MouseHover(object? sender, EventArgs e)
	{
		labelTipScreen.Text = Strings.MaxRefreshTooltip;
	}

	private void Button60Hz_MouseHover(object? sender, EventArgs e)
	{
		labelTipScreen.Text = Strings.MinRefreshTooltip.Replace("60", ScreenControl.MIN_RATE.ToString());
	}

	private void ButtonScreen_MouseLeave(object? sender, EventArgs e)
	{
		labelTipScreen.Text = "";
	}

	private void ButtonScreenAuto_MouseHover(object? sender, EventArgs e)
	{
		labelTipScreen.Text = Strings.AutoRefreshTooltip.Replace("60", ScreenControl.MIN_RATE.ToString());
	}

	private void ButtonUltimate_MouseHover(object? sender, EventArgs e)
	{
		labelTipGPU.Text = Strings.UltimateGPUTooltip;
	}

	private void ButtonStandard_MouseHover(object? sender, EventArgs e)
	{
		labelTipGPU.Text = Strings.StandardGPUTooltip;
	}

	private void ButtonEco_MouseHover(object? sender, EventArgs e)
	{
		labelTipGPU.Text = Strings.EcoGPUTooltip;
	}

	private void ButtonOptimized_MouseHover(object? sender, EventArgs e)
	{
		labelTipGPU.Text = Strings.OptimizedGPUTooltip;
	}

	private void ButtonGPU_MouseLeave(object? sender, EventArgs e)
	{
		labelTipGPU.Text = "";
	}

	private void ButtonXGM_MouseMove(object? sender, MouseEventArgs e)
	{
		if (sender != null)
		{
			TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)sender;
			if (buttonXGM.Visible)
			{
				labelTipGPU.Text = (buttonXGM.Bounds.Contains(tableLayoutPanel.PointToClient(System.Windows.Forms.Cursor.Position)) ? "XGMobile toggle works only in Standard mode" : "");
			}
		}
	}

	private void ButtonScreenAuto_Click(object? sender, EventArgs e)
	{
		ScreenControl.SetAutoRefresh(1);
		ScreenControl.AutoScreen();
	}

	private void CheckStartup_CheckedChanged(object? sender, EventArgs e)
	{
		if (sender != null)
		{
			if (((CheckBox)sender).Checked)
			{
				Startup.Schedule();
			}
			else
			{
				Startup.UnSchedule();
			}
		}
	}

	private void LabelCPUFan_Click(object? sender, EventArgs e)
	{
		FanSensorControl.fanRpm = !FanSensorControl.fanRpm;
		RefreshSensors(force: true);
	}

	private void ButtonKeyboardColor2_Click(object? sender, EventArgs e)
	{
		SetColorPicker("aura_color2", Aura.Color2);
	}

	private void ButtonKeyboard_Click(object? sender, EventArgs e)
	{
		if (extraForm == null || extraForm.Text == "")
		{
			extraForm = new Extra();
			AddOwnedForm(extraForm);
		}
		if (extraForm.Visible)
		{
			extraForm.Close();
		}
		else
		{
			extraForm.Show();
		}
	}

	public void FansInit()
	{
		if (fansForm != null && !(fansForm.Text == ""))
		{
			Invoke(fansForm.InitAll);
		}
	}

	public void GPUInit()
	{
		if (fansForm != null && !(fansForm.Text == ""))
		{
			Invoke(fansForm.InitGPU);
		}
	}

	public void FansToggle(int index = 0)
	{
		if (fansForm == null || fansForm.Text == "")
		{
			fansForm = new Fans();
			AddOwnedForm(fansForm);
		}
		if (fansForm.Visible)
		{
			fansForm.Close();
			return;
		}
		fansForm.FormPosition();
		fansForm.Show();
		fansForm.ToggleNavigation(index);
	}

	private void SetColorPicker(string colorField, Color initial)
	{
		// Ultralight: color picker removed
	}

	private void ButtonKeyboardColor_Click(object? sender, EventArgs e)
	{
		SetColorPicker("aura_color", Aura.Color1);
	}

	private void ButtonRearColor_Click(object? sender, EventArgs e)
	{
		SetColorPicker("rear_color", Aura.RearColor);
	}

	private void ComboRearLight_SelectedValueChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("rear_mode", (int)comboRearLight.SelectedValue);
		SetAura();
	}

	public void InitRearLight()
	{
		panelRearLight.Visible = false;
	}

	public void InitAura()
	{
		comboKeyboard.DropDownStyle = ComboBoxStyle.DropDownList;
		if (!Aura.IsBacklightDetected)
		{
			Aura.Init();
		}
		Aura.Mode = (AuraMode)AppConfig.Get("aura_mode");
		Aura.Speed = (AuraSpeed)AppConfig.Get("aura_speed");
		Aura.SetColor(AppConfig.Get("aura_color"));
		Aura.SetColor2(AppConfig.Get("aura_color2"));
		comboKeyboard.DataSource = new BindingSource(Aura.GetModes(), null);
		comboKeyboard.DisplayMember = "Value";
		comboKeyboard.ValueMember = "Key";
		comboKeyboard.SelectedValue = Aura.Mode;
		comboKeyboard.SelectedValueChanged += ComboKeyboard_SelectedValueChanged;
		if (Aura.isWhite)
		{
			buttonKeyboardColor.Visible = false;
		}
		VisualiseAura();
		InitRearLight();
	}

	public void SetAura()
	{
		Task.Run(delegate
		{
			Aura.ApplyAura();
			VisualiseAura();
		});
	}

	private void _VisualiseAura()
	{
		// Ultralight: color swatch removed
		if (AppConfig.IsDynamicLighting() && DynamicLightingHelper.IsEnabled() && !AppConfig.IsDynamicLightingOnly())
		{
			labelBacklight.Cursor = Cursors.Hand;
			labelBacklight.Text = Strings.DisableDynamicLighting;
		}
		else if (Aura.Mode == AuraMode.AMBIENT)
		{
			labelBacklight.Cursor = Cursors.Default;
			labelBacklight.Text = Strings.AmbientModeResources;
		}
		else
		{
			labelBacklight.Cursor = Cursors.Default;
			labelBacklight.Text = "";
		}
	}

	public void VisualiseAura()
	{
		if (base.InvokeRequired)
		{
			Invoke(_VisualiseAura);
		}
		else
		{
			_VisualiseAura();
		}
	}

	public void CycleAuraMode(int delta)
	{
		if (delta > 0)
		{
			if (comboKeyboard.SelectedIndex < comboKeyboard.Items.Count - 1)
			{
				comboKeyboard.SelectedIndex++;
			}
			else
			{
				comboKeyboard.SelectedIndex = 0;
			}
		}
		else if (comboKeyboard.SelectedIndex > 0)
		{
			comboKeyboard.SelectedIndex--;
		}
		else
		{
			comboKeyboard.SelectedIndex = comboKeyboard.Items.Count - 1;
		}
		Program.toast.RunToast(comboKeyboard.GetItemText(comboKeyboard.SelectedItem), ToastIcon.BacklightUp);
	}

	private void ComboKeyboard_SelectedValueChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("aura_mode", (int)comboKeyboard.SelectedValue);
		SetAura();
	}

	private void Button120Hz_Click(object? sender, EventArgs e)
	{
		ScreenControl.SetAutoRefresh(0);
		ScreenControl.SetScreen(1000, 1);
	}

	private void Button60Hz_Click(object? sender, EventArgs e)
	{
		ScreenControl.SetAutoRefresh(0);
		ScreenControl.SetScreen(ScreenControl.MIN_RATE, 0);
	}

	private void ButtonMiniled_Click(object? sender, EventArgs e)
	{
		ScreenControl.ToogleMiniled();
	}

	public void VisualiseScreen(bool screenEnabled, bool screenAuto, int frequency, int maxFrequency, int overdrive, bool overdriveSetting, int miniled1, int miniled2, bool hdr, bool acm, int fhd, int hdrControl)
	{
		bool flag = hdr || acm;
		ButtonEnabled(button60Hz, screenEnabled);
		ButtonEnabled(button120Hz, screenEnabled);
		ButtonEnabled(buttonScreenAuto, screenEnabled);
		ButtonEnabled(buttonMiniled, screenEnabled);
		labelSreen.Text = "Laptop Screen";
		labelMidFan.Text = frequency > 0 ? (frequency + "Hz") : "60Hz";
		panelScreen.AccessibleName = labelSreen.Text + ": " + labelMidFan.Text;
		
		button60Hz.Activated = false;
		button60Hz.BorderColor = Color.Transparent;
		button60Hz.ForeColor = RForm.foreMain;

		button120Hz.Activated = false;
		button120Hz.BorderColor = Color.Transparent;
		button120Hz.ForeColor = RForm.foreMain;

		buttonScreenAuto.Activated = false;
		buttonScreenAuto.BorderColor = Color.Transparent;
		buttonScreenAuto.ForeColor = RForm.foreMain;

		if (screenAuto)
		{
			buttonScreenAuto.Activated = true;
			buttonScreenAuto.BorderColor = RForm.colorAccent;
			buttonScreenAuto.ForeColor = RForm.colorAccent;
		}
		else if (frequency == ScreenControl.MIN_RATE)
		{
			button60Hz.Activated = true;
			button60Hz.BorderColor = RForm.colorAccent;
			button60Hz.ForeColor = RForm.colorAccent;
		}
		else if (frequency > ScreenControl.MIN_RATE)
		{
			button120Hz.Activated = true;
			button120Hz.BorderColor = RForm.colorAccent;
			button120Hz.ForeColor = RForm.colorAccent;
		}
		button60Hz.Text = ScreenControl.MIN_RATE + "Hz";
		if (maxFrequency > ScreenControl.MIN_RATE)
		{
			button120Hz.Text = maxFrequency + "Hz" + (overdriveSetting ? " + OD" : "");
		}
		else
		{
			button120Hz.Text = "Display";
		}
		panelScreen.Visible = true;
		tableScreen.Visible = true;
		if (fhd >= 0)
		{
			buttonFHD.Visible = true;
			buttonFHD.Text = ((fhd > 0) ? "FHD" : "UHD");
		}
		bool flag2 = hdr && hdrControl >= 0;
		if (miniled1 >= 0)
		{
			buttonMiniled.Visible = !flag2;
			buttonMiniled.Enabled = !hdr;
			buttonMiniled.Activated = miniled1 == 1 || hdr;
		}
		else if (miniled2 >= 0)
		{
			buttonMiniled.Visible = !flag2;
			buttonMiniled.Enabled = !hdr;
			if (hdr)
			{
				miniled2 = 1;
			}
			switch (miniled2)
			{
			case 0:
				buttonMiniled.Text = Strings.Multizone;
				buttonMiniled.BorderColor = RForm.colorStandard;
				buttonMiniled.Activated = true;
				break;
			case 1:
				buttonMiniled.Text = Strings.MultizoneStrong;
				buttonMiniled.BorderColor = RForm.colorTurbo;
				buttonMiniled.Activated = true;
				break;
			case 2:
				buttonMiniled.Text = Strings.OneZone;
				buttonMiniled.BorderColor = RForm.colorStandard;
				buttonMiniled.Activated = false;
				break;
			}
		}
		else
		{
			buttonMiniled.Visible = false;
		}
		if (flag2)
		{
			buttonHDRControl.Visible = true;
			buttonHDRControl.Activated = hdrControl > 0;
			buttonHDRControl.BorderColor = RForm.colorTurbo;
		}
		else
		{
			buttonHDRControl.Visible = false;
		}
		if (flag)
		{
			labelVisual.Text = Strings.VisualModesHDR;
		}
		if (!screenEnabled)
		{
			labelVisual.Text = Strings.VisualModesScreen;
		}
		if (!screenEnabled || flag)
		{
			labelVisual.Location = tableVisual.Location;
			labelVisual.Width = tableVisual.Width;
			labelVisual.Height = tableVisual.Height;
			labelVisual.Visible = true;
		}
		else
		{
			labelVisual.Visible = false;
		}
	}

	private void ButtonQuit_Click(object? sender, EventArgs e)
	{
		AsusLampArray.Release();
		Close();
		Program.trayIcon.Visible = false;
		Application.Exit();
	}

	public void HideAll()
	{
		SaveWindowPosition();
		Hide();
		if (fansForm != null && fansForm.Text != "")
		{
			fansForm.Close();
		}
		if (extraForm != null && extraForm.Text != "")
		{
			extraForm.Close();
		}
		if (updatesForm != null && updatesForm.Text != "")
		{
			updatesForm.Close();
		}
		MemoryHelper.TrimAfter(null, null);
	}

	public void SaveWindowPosition()
	{
		if (base.WindowState == FormWindowState.Normal)
		{
			AppConfig.Set("win_x", base.Left);
			AppConfig.Set("win_y", base.Top);
		}
	}

	public void RestoreWindowPosition()
	{
		int x = AppConfig.Get("win_x", -100000);
		int y = AppConfig.Get("win_y", -100000);
		Screen screen = Screen.PrimaryScreen ?? Screen.FromControl(this);
		Rectangle workArea = screen.WorkingArea;

		if (x == -100000 || y == -100000)
		{
			// First launch: place compactly in the lower right corner near the taskbar/tray
			base.Location = new Point(workArea.Right - base.Width - 16, workArea.Bottom - base.Height - 16);
			return;
		}

		bool isVisible = false;
		Rectangle windowRect = new Rectangle(x, y, base.Width, base.Height);
		foreach (Screen scr in Screen.AllScreens)
		{
			Rectangle intersection = Rectangle.Intersect(scr.WorkingArea, windowRect);
			if (intersection.Width >= 80 && intersection.Height >= 80)
			{
				isVisible = true;
				break;
			}
		}

		if (isVisible)
		{
			base.Location = new Point(x, y);
		}
		else
		{
			// Off-screen recovery: reposition safely to lower right corner
			base.Location = new Point(workArea.Right - base.Width - 16, workArea.Bottom - base.Height - 16);
		}
	}

	public void ShowAll()
	{
		Activate();
		base.TopMost = true;
		base.TopMost = AppConfig.Is("topmost");
	}

	public bool HasAnyFocus(bool lostFocusCheck = false)
	{
		if ((fansForm == null || !fansForm.ContainsFocus) && (extraForm == null || !extraForm.ContainsFocus) && (updatesForm == null || !updatesForm.ContainsFocus) && !base.ContainsFocus)
		{
			if (lostFocusCheck)
			{
				return Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastLostFocus) < 300;
			}
			return false;
		}
		return true;
	}

	private void SettingsForm_FormClosing(object? sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			HideAll();
		}
	}

	private void ButtonUltimate_Click(object? sender, EventArgs e) { }

	private void ButtonStandard_Click(object? sender, EventArgs e) { }

	private void ButtonEco_Click(object? sender, EventArgs e) { }

	private void ButtonOptimized_Click(object? sender, EventArgs e)
	{
		AppConfig.Set("gpu_auto", (AppConfig.Get("gpu_auto") != 1) ? 1 : 0);
	}

	private void ButtonStopGPU_Click(object? sender, EventArgs e)
	{
	}

	public async void RefreshSensors(bool force = false)
	{
		int num = ((!base.Visible && sensorsAlways) ? 6000 : 2000);
		if (!force && Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastRefresh) < num)
		{
			return;
		}
		lastRefresh = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		string cpuTemp = "";
		string gpuTemp = "";
		string cpuFan = "";
		string gpuFan = "";
		string midFan = "";
		string battery = "";
		string charge = "";
		await Task.Run(delegate
		{
			HardwareControl.ReadSensors();
		});
		if (HardwareControl.cpuTemp > 0f)
		{
			cpuTemp = ": " + TempHelper.FormatTemp(HardwareControl.cpuTemp.Value);
		}
		if (HardwareControl.batteryCapacity > 0m)
		{
			charge = Strings.BatteryCharge + ": " + HardwareControl.batteryCharge;
		}
		decimal? batteryRate = HardwareControl.batteryRate;
		if ((batteryRate.GetValueOrDefault() < default(decimal)) & batteryRate.HasValue)
		{
			battery = Strings.Discharging + ": " + Math.Round(-HardwareControl.batteryRate.Value, 1) + "W";
		}
		else
		{
			batteryRate = HardwareControl.batteryRate;
			if ((batteryRate.GetValueOrDefault() > default(decimal)) & batteryRate.HasValue)
			{
				battery = Strings.Charging + ": " + Math.Round(HardwareControl.batteryRate.Value, 1) + "W";
			}
		}
		if (HardwareControl.gpuTemp > 0f)
		{
			gpuTemp = ": " + TempHelper.FormatTemp(HardwareControl.gpuTemp.Value);
		}
		if (HardwareControl.cpuFan != null)
		{
			cpuFan = Strings.FanSpeed + ": " + HardwareControl.cpuFan;
		}
		if (HardwareControl.gpuFan != null)
		{
			gpuFan = Strings.FanSpeed + ": " + HardwareControl.gpuFan;
		}
		if (HardwareControl.midFan != null)
		{
			midFan = Strings.FanSpeed + ": " + HardwareControl.midFan;
		}
		string text = "CPU" + cpuTemp + " " + cpuFan;
		if (gpuTemp.Length > 0)
		{
			text = text + "\nGPU" + gpuTemp + " " + gpuFan;
		}
		if (battery.Length > 0)
		{
			text = text + "\n" + battery;
		}
		if (Program.settingsForm.IsHandleCreated)
		{
			Program.settingsForm.BeginInvoke(delegate
			{
				labelCPUFan.Text = "CPU" + cpuTemp + "  " + cpuFan;
				labelGPUFan.Text = "GPU" + gpuTemp + "  " + gpuFan;
				if (labelHeaderCPU != null && labelHeaderFan != null)
				{
					labelHeaderCPU.Text = "CPU: " + (HardwareControl.cpuTemp > 0f ? TempHelper.FormatTemp(HardwareControl.cpuTemp.Value) : "—");
					labelHeaderFan.Text = "Fan: " + (HardwareControl.cpuFan != null ? HardwareControl.cpuFan.ToString() : "—");
					FixHeaderLayout(); // keep CPU/Fan right-aligned as text width changes
				}
				if (HardwareControl.gpuFan != null && AppConfig.NoGpu())
				{
					labelMidFan.Text = "GPU" + gpuTemp + " " + gpuFan;
				}
				if (HardwareControl.midFan != null)
				{
					labelMidFan.Text = "Mid " + midFan;
				}
				labelBattery.Text = battery;
				if (!batteryMouseOver && !batteryFullMouseOver)
				{
					labelCharge.Text = charge;
				}
			});
		}
		if (Program.trayIcon != null)
		{
			Program.trayIcon.Text = text;
		}
		if (!AutoModeControl.IsEnabled || !(HardwareControl.cpuTemp > 0f))
		{
			return;
		}
		int num2 = AppConfig.Get("performance_mode");
		AutoDecision decision = AutoModeEngine.Shared.Evaluate(HardwareControl.cpuTemp.Value, HardwareControl.GetCPUUsage(), Program.currentSource == Program.PowerSource.Battery, num2);
		AutoModeEngine.LastDecision = decision;
		if (num2 != decision.TargetMode && Program.settingsForm.IsHandleCreated)
		{
			Program.settingsForm.BeginInvoke(delegate
			{
				Program.modeControl.SetPerformanceMode(decision.TargetMode);
			});
		}
		int suggestedIntervalMs = AutoModeEngine.GetSuggestedIntervalMs(decision.Workload);
		if (sensorTimer.Interval != (double)suggestedIntervalMs)
		{
			sensorTimer.Interval = suggestedIntervalMs;
		}
	}

	private void CheckAutoMode_CheckedChanged(object? sender, EventArgs e)
	{
		AppConfig.Set("ai_auto_mode", checkAutoMode.Checked ? 1 : 0);
		ApplyAutoModeTimer();
		if (checkAutoMode.Checked)
		{
			RefreshSensors(force: true);
		}
	}

	public void ApplyAutoModeTimer()
	{
		sensorTimer.Interval = (AutoModeControl.IsEnabled ? (AutoModeControl.IntervalSeconds * 1000) : AppConfig.Get("sensor_timer", 1000));
		sensorTimer.Enabled = base.Visible || sensorsAlways || AutoModeControl.IsEnabled;
	}

	/// <summary>Toggles AI Auto from the tray; the checkbox handler applies and persists it.</summary>
	public void ToggleAiAuto()
	{
		checkAutoMode.Checked = !checkAutoMode.Checked;
	}


	public void LabelFansResult(string text)
	{
		if (fansForm != null && !fansForm.IsDisposed && fansForm.Text != "")
		{
			fansForm.LabelFansResult(text);
		}
	}

	public void ToggleOverlay(bool fromHotkey = false)
	{
		bool flag = !AppConfig.IsOverlay();
		AppConfig.Set("overlay", flag ? 1 : 0);
		Logger.WriteLine("Overlay " + (flag ? "On" : "Off") + (AppConfig.IsOverlayGameOnly() ? " (game only)" : ""));
		if (flag)
		{
			// Ultralight: overlay removed
		}
		SetContextMenu();
	}

	public void ToggleOverlayGameOnly()
	{
		SetContextMenu();
	}

	public void ShowMode(int mode)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseMode(mode);
			});
		}
		else
		{
			VisualiseMode(mode);
		}
	}

	protected void VisualiseMode(int mode)
	{
		buttonSilent.Activated = (mode == 2);
		buttonSilent.BorderColor = (mode == 2) ? RForm.colorAccent : Color.Transparent;
		buttonSilent.ForeColor = (mode == 2) ? RForm.colorAccent : RForm.foreMain;

		buttonBalanced.Activated = (mode == 0);
		buttonBalanced.BorderColor = (mode == 0) ? RForm.colorAccent : Color.Transparent;
		buttonBalanced.ForeColor = (mode == 0) ? RForm.colorAccent : RForm.foreMain;

		buttonTurbo.Activated = (mode == 1);
		buttonTurbo.BorderColor = (mode == 1) ? RForm.colorAccent : Color.Transparent;
		buttonTurbo.ForeColor = (mode == 1) ? RForm.colorAccent : RForm.foreMain;

		string currentName = Modes.GetName(mode);
		labelPerf.Text = "Mode: " + currentName;
		panelPerformance.AccessibleName = labelPerf.Text;

		foreach (object item in contextMenuStrip.Items)
		{
			if (item is ToolStripMenuItem { Tag: not null } toolStripMenuItem)
			{
				toolStripMenuItem.Checked = (int)toolStripMenuItem.Tag == mode;
			}
		}
	}

	public void SetModeLabel(string modeText)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				labelPerf.Text = modeText;
				panelPerformance.AccessibleName = labelPerf.Text;
			});
		}
		else
		{
			labelPerf.Text = modeText;
			panelPerformance.AccessibleName = labelPerf.Text;
		}
	}

	public void VisualizeXGM(int GPUMode = -1)
	{
		bool connected = Program.acpi.IsXGConnected();
		int activated = (connected ? Program.acpi.DeviceGet(589849u) : (-1));
		Invoke(delegate
		{
			VisualizeXGM(connected, activated, GPUMode);
		});
	}

	private void VisualizeXGM(bool connected, int activated, int GPUMode)
	{
		RButton rButton = buttonXGM;
		bool enabled = (buttonXGM.Visible = connected);
		rButton.Enabled = enabled;
		if (connected)
		{
			if (GPUMode != -1)
			{
				ButtonEnabled(buttonXGM, AppConfig.IsAMDiGPU() || GPUMode != 0);
			}
			Logger.WriteLine("XGM Activated flag: " + activated);
			buttonXGM.Activated = activated == 1;
			if (activated == 1)
			{
				ButtonEnabled(buttonOptimized, enabled: false);
				ButtonEnabled(buttonEco, enabled: false);
				ButtonEnabled(buttonStandard, enabled: false);
				ButtonEnabled(buttonUltimate, enabled: false);
			}
			else
			{
				ButtonEnabled(buttonOptimized, enabled: true);
				ButtonEnabled(buttonEco, enabled: true);
				ButtonEnabled(buttonStandard, enabled: true);
				ButtonEnabled(buttonUltimate, enabled: true);
			}
		}
	}

	public void VisualiseGPUButtons(bool eco = true, bool ultimate = true)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseGPUButtons(eco, ultimate);
			});
			return;
		}
		isMuxGpu = ultimate;
		if (!eco)
		{
			ToolStripMenuItem toolStripMenuItem = menuEco;
			bool visible = (buttonEco.Visible = false);
			toolStripMenuItem.Visible = visible;
			ToolStripMenuItem toolStripMenuItem2 = menuOptimized;
			visible = (buttonOptimized.Visible = false);
			toolStripMenuItem2.Visible = visible;
			buttonStopGPU.Visible = true;
			tableGPU.ColumnCount = 3;
			tableScreen.ColumnCount = 3;
		}
		else
		{
			buttonStopGPU.Visible = false;
		}
		if (!ultimate)
		{
			ToolStripMenuItem toolStripMenuItem3 = menuUltimate;
			bool visible = (buttonUltimate.Visible = false);
			toolStripMenuItem3.Visible = visible;
			tableGPU.ColumnCount = 3;
			tableScreen.ColumnCount = 3;
		}
	}

	public void HideGPUModes(bool gpuExists)
	{
		isGpuSection = false;
		buttonEco.Visible = false;
		buttonStandard.Visible = false;
		buttonUltimate.Visible = false;
		buttonOptimized.Visible = false;
		buttonStopGPU.Visible = true;
		tableGPU.ColumnCount = 0;
		SetContextMenu();
		panelGPU.Visible = gpuExists;
	}

	public void LockGPUModes(string text = null)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				LockGPUModes(text);
			});
			return;
		}
		if (text == null)
		{
			text = Strings.GPUMode + ": " + Strings.GPUChanging + " ...";
		}
		ButtonEnabled(buttonOptimized, enabled: false);
		ButtonEnabled(buttonEco, enabled: false);
		ButtonEnabled(buttonStandard, enabled: false);
		ButtonEnabled(buttonUltimate, enabled: false);
		ButtonEnabled(buttonXGM, enabled: false);
		labelGPU.Text = text;
	}

	public void VisualiseGPUMode(int GPUMode = -1)
	{
		// Ultralight: GPU mode visualization removed
	}

	public void VisualiseIcon(bool themeChange = false)
	{
		if (Program.trayIcon == null)
		{
			return;
		}
		if (themeChange)
		{
			isDark = RForm.CheckSystemDarkModeStatus();
		}
		int num = AppConfig.Get("gpu_mode");
		bool flag = AppConfig.IsBWIcon();
		(int, bool, bool)? tuple = lastIcon;
		int num2 = num;
		bool flag2 = isDark;
		bool flag3 = flag;
		bool hasValue = tuple.HasValue;
		if (hasValue)
		{
			if (!hasValue)
			{
				return;
			}
			(int, bool, bool) valueOrDefault = tuple.GetValueOrDefault();
			if (valueOrDefault.Item1 == num2 && valueOrDefault.Item2 == flag2 && valueOrDefault.Item3 == flag3)
			{
				return;
			}
		}
		lastIcon = (num, isDark, flag);
		Icon icon = num switch
		{
			0 => (!flag) ? Resources.eco : (isDark ? Resources.light_eco : Resources.dark_eco), 
			2 => (!flag) ? Resources.ultimate : (isDark ? Resources.light_standard : Resources.dark_standard), 
			_ => (!flag) ? Resources.standard : (isDark ? Resources.light_standard : Resources.dark_standard), 
		};
		Icon? icon2 = Program.trayIcon.Icon;
		Program.trayIcon.Icon = icon;
		icon2?.Dispose();
	}

	private void PictureGPU_Click(object? sender, EventArgs e)
	{
		// Ultralight: GPU control removed
	}

	private void ButtonSilent_Click(object? sender, EventArgs e)
	{
		Program.modeControl.SetPerformanceMode(2);
	}

	private void ButtonBalanced_Click(object? sender, EventArgs e)
	{
		Program.modeControl.SetPerformanceMode(0);
	}

	private void ButtonTurbo_Click(object? sender, EventArgs e)
	{
		Program.modeControl.SetPerformanceMode(1);
	}

	public void ButtonEnabled(RButton but, bool enabled)
	{
		but.Enabled = enabled;
		but.BackColor = (but.Enabled ? Color.FromArgb(255, but.BackColor) : Color.FromArgb(100, but.BackColor));
	}

	public void VisualiseBatteryTitle(int limit)
	{
		labelBatteryTitle.Text = "Battery Charge Limit";
		if (labelBattery != null)
		{
			labelBattery.Text = "Limit: " + limit + "%";
		}
	}

	public void VisualiseBattery(int limit)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseBattery(limit);
			});
			return;
		}
		VisualiseBatteryTitle(limit);
		sliderBattery.Value = limit;
		sliderBattery.AccessibleName = Strings.BatteryChargeLimit + ": " + limit + "%";

		VisualiseBatteryFull();
	}

	public void VisualiseBatteryFull()
	{
		if (base.InvokeRequired)
		{
			Invoke(VisualiseBatteryFull);
		}
		else if (BatteryControl.chargeFull)
		{
			buttonBatteryFull.BackColor = RForm.colorAccent;
			buttonBatteryFull.ForeColor = Color.White;
			buttonBatteryFull.AccessibleName = Strings.BatteryChargeLimit + "100% on";
		}
		else
		{
			buttonBatteryFull.BackColor = RForm.buttonSecond;
			buttonBatteryFull.ForeColor = SystemColors.ControlDark;
			buttonBatteryFull.AccessibleName = Strings.BatteryChargeLimit + "100% off";
		}
	}

	public void UpdateKeyboardLabel()
	{
		labelKeyboard.Text = Strings.LaptopKeyboard;
	}

	public void VisualiseAudio(double level)
	{
		if (base.InvokeRequired)
		{
			Invoke(delegate
			{
				VisualiseAudio(level);
			});
		}
		else
		{
			int count = (int)Math.Round(level / 2.0);
			string text = new string('|', count);
			labelMatrix.Text = "Slash Lighting: " + text;
		}
	}

	public void VisualiseFnLock()
	{
		if (base.InvokeRequired)
		{
			Invoke(VisualiseFnLock);
			return;
		}
		if (buttonFnLock == null) return;
		bool isFn = AppConfig.Is("fn_lock");
		buttonFnLock.Activated = isFn;
		buttonFnLock.BorderColor = isFn ? RForm.colorAccent : Color.Transparent;
		buttonFnLock.ForeColor = isFn ? RForm.colorAccent : RForm.foreMain;
		buttonFnLock.Text = isFn ? "Fn-Lock: ON" : "Fn-Lock: OFF";
		buttonFnLock.AccessibleName = isFn ? "Fn-Lock on" : "Fn-Lock off";
	}

	public void VisualiseEnergySaver()
	{
		if (base.InvokeRequired)
		{
			Invoke(VisualiseEnergySaver);
			return;
		}
		if (buttonEnergySaver == null) return;
		bool isSaver = BatteryControl.IsEnergySaver();
		buttonEnergySaver.Activated = isSaver;
		buttonEnergySaver.BorderColor = isSaver ? RForm.colorAccent : Color.Transparent;
		buttonEnergySaver.ForeColor = isSaver ? RForm.colorAccent : RForm.foreMain;
		buttonEnergySaver.Text = isSaver ? "Energy Saver: ON" : "Energy Saver: OFF";
		buttonEnergySaver.AccessibleName = isSaver ? "Energy Saver on" : "Energy Saver off";


	}

	public void VisualiseAiAuto()
	{
		if (base.InvokeRequired)
		{
			Invoke(VisualiseAiAuto);
			return;
		}
		if (buttonAiAutoToggle == null) return;
		bool isEnabled = AutoModeControl.IsEnabled;
		buttonAiAutoToggle.Activated = isEnabled;
		buttonAiAutoToggle.BorderColor = isEnabled ? RForm.colorAccent : Color.Transparent;
		buttonAiAutoToggle.ForeColor = isEnabled ? RForm.colorAccent : RForm.foreMain;
		buttonAiAutoToggle.Text = isEnabled ? "ON" : "OFF";
		if (labelAiAutoStatus != null)
		{
			if (!isEnabled)
			{
				labelAiAutoStatus.Text = "Manual mode active \u2022 Click to enable Auto";
				labelAiAutoStatus.ForeColor = SystemColors.ControlDark;
			}
			else
			{
				string reason = AutoModeEngine.LastDecision.Reason;
				if (string.IsNullOrEmpty(reason) || reason == "No sensor data")
				{
					labelAiAutoStatus.Text = Modes.GetCurrentName() + " \u2022 Normal workload";
				}
				else
				{
					labelAiAutoStatus.Text = Modes.GetCurrentName() + " \u2022 " + reason;
				}
				labelAiAutoStatus.ForeColor = RForm.foreMain;
			}
		}
	}

	private void ButtonFnLock_Click(object? sender, EventArgs e)
	{
		InputDispatcher.ToggleFnLock();
		VisualiseFnLock();
	}

	private void ButtonEnergySaver_Click(object? sender, EventArgs e)
	{
		BatteryControl.ToggleEnergySaver();
	}

	private void ButtonDrivers_Click(object? sender, EventArgs e)
	{
		ButtonUpdates_Click(sender, e);
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
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);

		// Performance Card
		this.panelPerformance = new System.Windows.Forms.Panel();
		this.panelCPUTitle = new System.Windows.Forms.Panel();
		this.picturePerf = new System.Windows.Forms.PictureBox();
		this.labelPerf = new System.Windows.Forms.Label();
		this.labelCPUFan = new System.Windows.Forms.Label();
		this.checkAutoMode = new System.Windows.Forms.CheckBox();

		this.tablePerf = new System.Windows.Forms.TableLayoutPanel();
		this.buttonSilent = new Asus.UI.RButton();
		this.buttonBalanced = new Asus.UI.RButton();
		this.buttonTurbo = new Asus.UI.RButton();

		// Battery Card
		this.panelBattery = new System.Windows.Forms.Panel();
		this.panelBatteryTitle = new System.Windows.Forms.Panel();
		this.pictureBattery = new System.Windows.Forms.PictureBox();
		this.labelBatteryTitle = new System.Windows.Forms.Label();
		this.labelBattery = new System.Windows.Forms.Label();

		this.buttonBatteryFull = new Asus.UI.RButton();
		this.sliderBattery = new Asus.UI.Slider();

		// Laptop Screen Card
		this.panelScreen = new System.Windows.Forms.Panel();
		this.panelScreenTitle = new System.Windows.Forms.Panel();
		this.pictureScreen = new System.Windows.Forms.PictureBox();
		this.labelSreen = new System.Windows.Forms.Label();
		this.labelMidFan = new System.Windows.Forms.Label();
		this.labelTipScreen = new System.Windows.Forms.Label();
		this.tableScreen = new System.Windows.Forms.TableLayoutPanel();
		this.buttonScreenAuto = new Asus.UI.RButton();
		this.button60Hz = new Asus.UI.RButton();
		this.button120Hz = new Asus.UI.RButton();
		this.buttonMiniled = new Asus.UI.RButton();
		this.buttonFHD = new Asus.UI.RButton();
		this.buttonHDRControl = new Asus.UI.RButton();

		// AI Auto Card (Full Width)
		this.panelAiAuto = new System.Windows.Forms.Panel();
		this.panelAiAutoTitle = new System.Windows.Forms.Panel();
		this.pictureAiAuto = new System.Windows.Forms.PictureBox();
		this.labelAiAutoTitle = new System.Windows.Forms.Label();
		this.buttonAiAutoToggle = new Asus.UI.RButton();
		this.labelAiAutoStatus = new System.Windows.Forms.Label();

		// Quick Controls Strip
		this.panelQuickControls = new System.Windows.Forms.Panel();
		this.tableQuickControls = new System.Windows.Forms.TableLayoutPanel();
		this.buttonFnLock = new Asus.UI.RButton();
		this.buttonEnergySaver = new Asus.UI.RButton();

		// Footer Strip
		this.panelFooter = new System.Windows.Forms.Panel();
		this.tableButtons = new System.Windows.Forms.TableLayoutPanel();
		this.checkStartup = new System.Windows.Forms.CheckBox();
		this.buttonUpdates = new Asus.UI.RButton();

		this.buttonDonate = new Asus.UI.RBadgeButton();
		this.labelVersion = new System.Windows.Forms.Label();

		// Background Hidden Controls (Retained for binary compatibility / helper access)
		this.panelGamma = new System.Windows.Forms.Panel();
		this.panelGammaTitle = new System.Windows.Forms.Panel();
		this.pictureGamma = new System.Windows.Forms.PictureBox();
		this.labelGammaTitle = new System.Windows.Forms.Label();
		this.labelGamma = new System.Windows.Forms.Label();
		this.labelVisual = new System.Windows.Forms.Label();
		this.sliderGamma = new Asus.UI.Slider();
		this.tableVisual = new System.Windows.Forms.TableLayoutPanel();
		this.comboVisual = new Asus.UI.RComboBox();
		this.comboColorTemp = new Asus.UI.RComboBox();
		this.comboGamut = new Asus.UI.RComboBox();
		this.buttonInstallColor = new Asus.UI.RButton();
		this.panelKeyboard = new System.Windows.Forms.Panel();
		this.panelKeyboardTitle = new System.Windows.Forms.Panel();
		this.pictureKeyboard = new System.Windows.Forms.PictureBox();
		this.labelKeyboard = new System.Windows.Forms.Label();
		this.labelBacklight = new System.Windows.Forms.Label();
		this.tableLayoutKeyboard = new System.Windows.Forms.TableLayoutPanel();
		this.comboKeyboard = new Asus.UI.RComboBox();
		this.buttonKeyboardColor = new Asus.UI.RButton();
		this.buttonKeyboard = new Asus.UI.RButton();
		this.panelGPU = new System.Windows.Forms.Panel();
		this.panelGPUTitle = new System.Windows.Forms.Panel();
		this.pictureGPU = new System.Windows.Forms.PictureBox();
		this.labelGPU = new System.Windows.Forms.Label();
		this.labelGPUFan = new System.Windows.Forms.Label();
		this.labelTipGPU = new System.Windows.Forms.Label();
		this.tableGPU = new System.Windows.Forms.TableLayoutPanel();
		this.buttonStopGPU = new Asus.UI.RButton();
		this.buttonEco = new Asus.UI.RButton();
		this.buttonStandard = new Asus.UI.RButton();
		this.buttonUltimate = new Asus.UI.RButton();
		this.buttonOptimized = new Asus.UI.RButton();
		this.buttonXGM = new Asus.UI.RButton();
		this.tableAMD = new System.Windows.Forms.TableLayoutPanel();
		this.buttonFPS = new Asus.UI.RButton();
		this.buttonOverlay = new Asus.UI.RButton();
		this.buttonAutoTDP = new Asus.UI.RButton();
		this.panelMatrix = new System.Windows.Forms.Panel();
		this.panelMatrixTitle = new System.Windows.Forms.Panel();
		this.pictureMatrix = new System.Windows.Forms.PictureBox();
		this.labelMatrix = new System.Windows.Forms.Label();
		this.tableLayoutMatrix = new System.Windows.Forms.TableLayoutPanel();
		this.comboMatrix = new Asus.UI.RComboBox();
		this.comboMatrixRunning = new Asus.UI.RComboBox();
		this.buttonMatrix = new Asus.UI.RButton();
		this.panelRearLight = new System.Windows.Forms.Panel();
		this.panelRearLightTitle = new System.Windows.Forms.Panel();
		this.pictureRearLight = new System.Windows.Forms.PictureBox();
		this.labelRearLight = new System.Windows.Forms.Label();
		this.tableLayoutRearLight = new System.Windows.Forms.TableLayoutPanel();
		this.comboRearLight = new Asus.UI.RComboBox();
		this.buttonRearColor = new Asus.UI.RButton();
		this.panelStartup = new System.Windows.Forms.Panel();
		this.labelCharge = new System.Windows.Forms.Label();
		this.panelPeripherals = new System.Windows.Forms.Panel();
		this.tableLayoutPeripherals = new System.Windows.Forms.TableLayoutPanel();
		this.buttonPeripheral1 = new Asus.UI.RButton();
		this.buttonPeripheral2 = new Asus.UI.RButton();
		this.buttonPeripheral3 = new Asus.UI.RButton();
		this.panelAlly = new System.Windows.Forms.Panel();
		this.panelAllyTitle = new System.Windows.Forms.Panel();
		this.pictureAlly = new System.Windows.Forms.PictureBox();
		this.labelAlly = new System.Windows.Forms.Label();
		this.tableLayoutAlly = new System.Windows.Forms.TableLayoutPanel();
		this.buttonController = new Asus.UI.RButton();
		this.buttonBacklight = new Asus.UI.RButton();
		this.buttonControllerMode = new Asus.UI.RButton();
		this.panelVersion = new System.Windows.Forms.Panel();
		this.buttonAmdOled = new Asus.UI.RButton();
		this.buttonArmoury = new Asus.UI.RButton();

		this.SuspendLayout();

		// --- Panel 1: Performance Mode Card ---
		this.panelPerformance.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelPerformance.AutoSize = false;
		this.panelPerformance.Controls.Add(this.tablePerf);
		this.panelPerformance.Controls.Add(this.panelCPUTitle);
		this.panelPerformance.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelPerformance.Height = 80;
		this.panelPerformance.Location = new System.Drawing.Point(10, 42);
		this.panelPerformance.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
		this.panelPerformance.Name = "panelPerformance";
		this.panelPerformance.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
		this.panelPerformance.TabIndex = 0;

		this.panelCPUTitle.Controls.Add(this.picturePerf);
		this.panelCPUTitle.Controls.Add(this.labelPerf);
		this.panelCPUTitle.Controls.Add(this.labelCPUFan);
		this.panelCPUTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelCPUTitle.Height = 20;
		this.panelCPUTitle.Location = new System.Drawing.Point(10, 6);
		this.panelCPUTitle.Margin = new System.Windows.Forms.Padding(0);
		this.panelCPUTitle.Name = "panelCPUTitle";

		try { this.picturePerf.Image = ControlHelper.TintImage(Asus.Properties.Resources.icons8_gauge_32, RForm.colorAccent); } catch {}
		this.picturePerf.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.picturePerf.Location = new System.Drawing.Point(0, 2);
		this.picturePerf.Size = new System.Drawing.Size(16, 16);
		this.picturePerf.TabStop = false;

		this.labelPerf.AutoSize = true;
		this.labelPerf.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelPerf.Location = new System.Drawing.Point(22, 1);
		this.labelPerf.Name = "labelPerf";
		this.labelPerf.Text = "Mode: Balanced";

		this.labelCPUFan.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelCPUFan.Location = new System.Drawing.Point(220, 2);
		this.labelCPUFan.Size = new System.Drawing.Size(200, 18);
		this.labelCPUFan.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelCPUFan.Visible = false;

		this.tablePerf.ColumnCount = 3;
		this.tablePerf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33f));
		this.tablePerf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33f));
		this.tablePerf.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33f));
		this.tablePerf.Controls.Add(this.buttonSilent, 0, 0);
		this.tablePerf.Controls.Add(this.buttonBalanced, 1, 0);
		this.tablePerf.Controls.Add(this.buttonTurbo, 2, 0);
		this.tablePerf.Dock = System.Windows.Forms.DockStyle.Top;
		this.tablePerf.Height = 44;
		this.tablePerf.Location = new System.Drawing.Point(10, 26);
		this.tablePerf.Margin = new System.Windows.Forms.Padding(0);
		this.tablePerf.RowCount = 1;
		this.tablePerf.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

		this.buttonSilent.Activated = false;
		this.buttonSilent.BackColor = System.Drawing.Color.FromArgb(26, 29, 35);
		this.buttonSilent.BorderColor = System.Drawing.Color.Transparent;
		this.buttonSilent.BorderRadius = 5;
		this.buttonSilent.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonSilent.FlatAppearance.BorderSize = 0;
		this.buttonSilent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonSilent.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.buttonSilent.ForeColor = System.Drawing.Color.FromArgb(215, 218, 224);
		this.buttonSilent.Image = Asus.Properties.Resources.icons8_bicycle_48__1_;
		this.buttonSilent.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.buttonSilent.Location = new System.Drawing.Point(2, 2);
		this.buttonSilent.Margin = new System.Windows.Forms.Padding(2);
		this.buttonSilent.Name = "buttonSilent";
		this.buttonSilent.Text = "Silent";
		this.buttonSilent.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;

		this.buttonBalanced.Activated = false;
		this.buttonBalanced.BackColor = System.Drawing.Color.FromArgb(26, 29, 35);
		this.buttonBalanced.BorderColor = System.Drawing.Color.Transparent;
		this.buttonBalanced.BorderRadius = 5;
		this.buttonBalanced.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonBalanced.FlatAppearance.BorderSize = 0;
		this.buttonBalanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonBalanced.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.buttonBalanced.ForeColor = System.Drawing.Color.FromArgb(215, 218, 224);
		this.buttonBalanced.Image = Asus.Properties.Resources.icons8_fiat_500_48;
		this.buttonBalanced.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.buttonBalanced.Location = new System.Drawing.Point(2, 2);
		this.buttonBalanced.Margin = new System.Windows.Forms.Padding(2);
		this.buttonBalanced.Name = "buttonBalanced";
		this.buttonBalanced.Text = "Balanced";
		this.buttonBalanced.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;

		this.buttonTurbo.Activated = false;
		this.buttonTurbo.BackColor = System.Drawing.Color.FromArgb(26, 29, 35);
		this.buttonTurbo.BorderColor = System.Drawing.Color.Transparent;
		this.buttonTurbo.BorderRadius = 5;
		this.buttonTurbo.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonTurbo.FlatAppearance.BorderSize = 0;
		this.buttonTurbo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonTurbo.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.buttonTurbo.ForeColor = System.Drawing.Color.FromArgb(215, 218, 224);
		this.buttonTurbo.Image = Asus.Properties.Resources.icons8_rocket_48;
		this.buttonTurbo.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.buttonTurbo.Location = new System.Drawing.Point(2, 2);
		this.buttonTurbo.Margin = new System.Windows.Forms.Padding(2);
		this.buttonTurbo.Name = "buttonTurbo";
		this.buttonTurbo.Text = "Performance";
		this.buttonTurbo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;

		// --- Panel 2: Battery Charge Limit Card ---
		this.panelBattery.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelBattery.AutoSize = false;
		this.panelBattery.Controls.Add(this.sliderBattery);
		this.panelBattery.Controls.Add(this.panelBatteryTitle);
		this.panelBattery.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBattery.Height = 48;
		this.panelBattery.Location = new System.Drawing.Point(10, 128);
		this.panelBattery.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
		this.panelBattery.Name = "panelBattery";
		this.panelBattery.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
		this.panelBattery.TabIndex = 1;

		this.panelBatteryTitle.Controls.Add(this.pictureBattery);
		this.panelBatteryTitle.Controls.Add(this.labelBatteryTitle);
		this.panelBatteryTitle.Controls.Add(this.labelBattery);
		this.panelBatteryTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelBatteryTitle.Height = 20;
		this.panelBatteryTitle.Location = new System.Drawing.Point(10, 6);
		this.panelBatteryTitle.Margin = new System.Windows.Forms.Padding(0);
		this.panelBatteryTitle.Name = "panelBatteryTitle";

		try { this.pictureBattery.Image = ControlHelper.TintImage(Asus.Properties.Resources.icons8_charging_battery_32, RForm.colorAccent); } catch {}
		this.pictureBattery.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBattery.Location = new System.Drawing.Point(0, 2);
		this.pictureBattery.Size = new System.Drawing.Size(16, 16);
		this.pictureBattery.TabStop = false;

		this.labelBatteryTitle.AutoSize = true;
		this.labelBatteryTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelBatteryTitle.Location = new System.Drawing.Point(22, 1);
		this.labelBatteryTitle.Name = "labelBatteryTitle";
		this.labelBatteryTitle.Text = "Battery Charge Limit";

		this.labelBattery.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelBattery.Font = new System.Drawing.Font("Segoe UI", 8.5f);
		this.labelBattery.ForeColor = System.Drawing.Color.FromArgb(160, 164, 174);
		this.labelBattery.Location = new System.Drawing.Point(220, 2);
		this.labelBattery.Size = new System.Drawing.Size(200, 18);
		this.labelBattery.Text = "Limit: 80%";
		this.labelBattery.TextAlign = System.Drawing.ContentAlignment.TopRight;


		this.sliderBattery.Dock = System.Windows.Forms.DockStyle.Top;
		this.sliderBattery.Height = 18;
		this.sliderBattery.Location = new System.Drawing.Point(10, 54);
		this.sliderBattery.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
		this.sliderBattery.Max = 100;
		this.sliderBattery.Min = 40;
		this.sliderBattery.Name = "sliderBattery";
		this.sliderBattery.Step = 5;
		this.sliderBattery.Value = 80;
		this.sliderBattery.accentColor = RForm.colorAccent;

		// --- Panel 3: Laptop Screen Card ---
		this.panelScreen.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelScreen.AutoSize = false;
		this.panelScreen.Controls.Add(this.tableScreen);
		this.panelScreen.Controls.Add(this.panelScreenTitle);
		this.panelScreen.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelScreen.Height = 60;
		this.panelScreen.Location = new System.Drawing.Point(10, 218);
		this.panelScreen.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
		this.panelScreen.Name = "panelScreen";
		this.panelScreen.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
		this.panelScreen.TabIndex = 2;

		this.panelScreenTitle.Controls.Add(this.pictureScreen);
		this.panelScreenTitle.Controls.Add(this.labelSreen);
		this.panelScreenTitle.Controls.Add(this.labelMidFan);
		this.panelScreenTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelScreenTitle.Height = 20;
		this.panelScreenTitle.Location = new System.Drawing.Point(10, 6);
		this.panelScreenTitle.Margin = new System.Windows.Forms.Padding(0);
		this.panelScreenTitle.Name = "panelScreenTitle";

		try { this.pictureScreen.Image = ControlHelper.TintImage(Asus.Properties.Resources.icons8_laptop_32, RForm.colorAccent); } catch {}
		this.pictureScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureScreen.Location = new System.Drawing.Point(0, 2);
		this.pictureScreen.Size = new System.Drawing.Size(16, 16);
		this.pictureScreen.TabStop = false;

		this.labelSreen.AutoSize = true;
		this.labelSreen.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelSreen.Location = new System.Drawing.Point(22, 1);
		this.labelSreen.Name = "labelSreen";
		this.labelSreen.Text = "Laptop Screen";

		this.labelMidFan.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelMidFan.Location = new System.Drawing.Point(220, 2);
		this.labelMidFan.Size = new System.Drawing.Size(200, 18);
		this.labelMidFan.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.labelMidFan.Font = new System.Drawing.Font("Segoe UI", 8.5f);
		this.labelMidFan.ForeColor = System.Drawing.Color.FromArgb(160, 164, 174);
		this.labelMidFan.Text = "60Hz";

		this.tableScreen.ColumnCount = 3;
		this.tableScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333f));
		this.tableScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333f));
		this.tableScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333f));
		this.tableScreen.Controls.Add(this.buttonScreenAuto, 0, 0);
		this.tableScreen.Controls.Add(this.button60Hz, 1, 0);
		this.tableScreen.Controls.Add(this.button120Hz, 2, 0);
		this.tableScreen.Dock = System.Windows.Forms.DockStyle.Top;
		this.tableScreen.Height = 28;
		this.tableScreen.Location = new System.Drawing.Point(10, 26);
		this.tableScreen.Margin = new System.Windows.Forms.Padding(0);
		this.tableScreen.RowCount = 1;
		this.tableScreen.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

		this.buttonScreenAuto.Activated = false;
		this.buttonScreenAuto.BackColor = System.Drawing.Color.FromArgb(26, 29, 35);
		this.buttonScreenAuto.BorderColor = System.Drawing.Color.Transparent;
		this.buttonScreenAuto.BorderRadius = 4;
		this.buttonScreenAuto.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonScreenAuto.FlatAppearance.BorderSize = 0;
		this.buttonScreenAuto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonScreenAuto.Font = new System.Drawing.Font("Segoe UI", 8.5f);
		this.buttonScreenAuto.ForeColor = System.Drawing.Color.FromArgb(215, 218, 224);
		this.buttonScreenAuto.Location = new System.Drawing.Point(2, 2);
		this.buttonScreenAuto.Margin = new System.Windows.Forms.Padding(2);
		this.buttonScreenAuto.Text = "Auto";

		this.button60Hz.Activated = false;
		this.button60Hz.BackColor = System.Drawing.Color.FromArgb(26, 29, 35);
		this.button60Hz.BorderColor = System.Drawing.Color.Transparent;
		this.button60Hz.BorderRadius = 4;
		this.button60Hz.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button60Hz.FlatAppearance.BorderSize = 0;
		this.button60Hz.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button60Hz.Font = new System.Drawing.Font("Segoe UI", 8.5f);
		this.button60Hz.ForeColor = System.Drawing.Color.FromArgb(215, 218, 224);
		this.button60Hz.Location = new System.Drawing.Point(2, 2);
		this.button60Hz.Margin = new System.Windows.Forms.Padding(2);
		this.button60Hz.Text = "60Hz";

		this.button120Hz.Activated = false;
		this.button120Hz.BackColor = System.Drawing.Color.FromArgb(26, 29, 35);
		this.button120Hz.BorderColor = System.Drawing.Color.Transparent;
		this.button120Hz.BorderRadius = 4;
		this.button120Hz.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button120Hz.FlatAppearance.BorderSize = 0;
		this.button120Hz.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button120Hz.Font = new System.Drawing.Font("Segoe UI", 8.5f);
		this.button120Hz.ForeColor = System.Drawing.Color.FromArgb(215, 218, 224);
		this.button120Hz.Location = new System.Drawing.Point(2, 2);
		this.button120Hz.Margin = new System.Windows.Forms.Padding(2);
		this.button120Hz.Text = "Display";

		// --- Panel 4: AI Auto Card (Full Width) ---
		this.panelAiAuto.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelAiAuto.AutoSize = false;
		this.panelAiAuto.Controls.Add(this.labelAiAutoStatus);
		this.panelAiAuto.Controls.Add(this.panelAiAutoTitle);
		this.panelAiAuto.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAiAuto.Height = 46;
		this.panelAiAuto.Location = new System.Drawing.Point(10, 284);
		this.panelAiAuto.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
		this.panelAiAuto.Name = "panelAiAuto";
		this.panelAiAuto.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
		this.panelAiAuto.TabIndex = 3;

		this.panelAiAutoTitle.Controls.Add(this.pictureAiAuto);
		this.panelAiAutoTitle.Controls.Add(this.labelAiAutoTitle);
		this.panelAiAutoTitle.Controls.Add(this.buttonAiAutoToggle);
		this.panelAiAutoTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelAiAutoTitle.Height = 20;
		this.panelAiAutoTitle.Location = new System.Drawing.Point(10, 6);
		this.panelAiAutoTitle.Margin = new System.Windows.Forms.Padding(0);
		this.panelAiAutoTitle.Name = "panelAiAutoTitle";

		try { this.pictureAiAuto.Image = ControlHelper.TintImage(Asus.Properties.Resources.icons8_automation_32, RForm.colorAccent); } catch {}
		this.pictureAiAuto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureAiAuto.Location = new System.Drawing.Point(0, 2);
		this.pictureAiAuto.Size = new System.Drawing.Size(16, 16);
		this.pictureAiAuto.TabStop = false;

		this.labelAiAutoTitle.AutoSize = true;
		this.labelAiAutoTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelAiAutoTitle.Location = new System.Drawing.Point(22, 1);
		this.labelAiAutoTitle.Name = "labelAiAutoTitle";
		this.labelAiAutoTitle.Text = "AI Auto";

		this.buttonAiAutoToggle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.buttonAiAutoToggle.Size = new System.Drawing.Size(46, 18);
		this.buttonAiAutoToggle.Location = new System.Drawing.Point(374, 1);
		this.buttonAiAutoToggle.Font = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold);
		this.buttonAiAutoToggle.BorderRadius = 4;
		this.buttonAiAutoToggle.Text = "ON";

		this.labelAiAutoStatus.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelAiAutoStatus.Height = 16;
		this.labelAiAutoStatus.Font = new System.Drawing.Font("Segoe UI", 7.5f);
		this.labelAiAutoStatus.ForeColor = System.Drawing.Color.FromArgb(160, 164, 174);
		this.labelAiAutoStatus.Location = new System.Drawing.Point(10, 26);
		this.labelAiAutoStatus.Text = "Balanced \u2022 Light workload";
		// --- Panel 5: Quick Controls Strip ---
		this.panelQuickControls.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
		this.panelQuickControls.AutoSize = false;
		this.panelQuickControls.Controls.Add(this.tableQuickControls);
		this.panelQuickControls.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelQuickControls.Height = 34;
		this.panelQuickControls.Location = new System.Drawing.Point(10, 368);
		this.panelQuickControls.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
		this.panelQuickControls.Name = "panelQuickControls";
		this.panelQuickControls.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
		this.panelQuickControls.TabIndex = 4;

		this.tableQuickControls.ColumnCount = 2;
		this.tableQuickControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableQuickControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableQuickControls.Controls.Add(this.buttonFnLock, 0, 0);
		this.tableQuickControls.Controls.Add(this.buttonEnergySaver, 1, 0);
		this.tableQuickControls.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableQuickControls.Location = new System.Drawing.Point(10, 2);
		this.tableQuickControls.Margin = new System.Windows.Forms.Padding(0);
		this.tableQuickControls.RowCount = 1;
		this.tableQuickControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

		this.buttonFnLock.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonFnLock.BorderRadius = 4;
		this.buttonFnLock.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.buttonFnLock.Location = new System.Drawing.Point(2, 2);
		this.buttonFnLock.Margin = new System.Windows.Forms.Padding(2);
		this.buttonFnLock.Text = "Fn-Lock: OFF";

		this.buttonEnergySaver.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonEnergySaver.BorderRadius = 4;
		this.buttonEnergySaver.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.buttonEnergySaver.Location = new System.Drawing.Point(2, 2);
		this.buttonEnergySaver.Margin = new System.Windows.Forms.Padding(2);
		this.buttonEnergySaver.Text = "Energy Saver: OFF";

		// --- Panel 6: Footer Strip ---
		this.panelFooter.AutoSize = false;
		this.panelFooter.Controls.Add(this.tableButtons);
		this.panelFooter.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelFooter.Height = 32;
		this.panelFooter.Location = new System.Drawing.Point(10, 408);
		this.panelFooter.Margin = new System.Windows.Forms.Padding(0);
		this.panelFooter.Name = "panelFooter";
		this.panelFooter.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
		this.panelFooter.TabIndex = 5;

		this.tableButtons.ColumnCount = 2;
		this.tableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableButtons.Controls.Add(this.checkStartup, 0, 0);
		this.tableButtons.Controls.Add(this.buttonUpdates, 1, 0);

		this.tableButtons.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableButtons.Location = new System.Drawing.Point(4, 2);
		this.tableButtons.Margin = new System.Windows.Forms.Padding(0);
		this.tableButtons.RowCount = 1;
		this.tableButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

		this.checkStartup.Dock = System.Windows.Forms.DockStyle.Fill;
		this.checkStartup.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkStartup.Text = Asus.Properties.Strings.RunOnStartup;

		this.buttonUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonUpdates.BorderRadius = 4;
		this.buttonUpdates.Font = new System.Drawing.Font("Segoe UI", 7.5f);
		this.buttonUpdates.Location = new System.Drawing.Point(2, 2);
		this.buttonUpdates.Margin = new System.Windows.Forms.Padding(2);
		this.buttonUpdates.Text = "\u2699 Updates";



		// Hidden components visibility
		this.panelGPU.Visible = false;
		this.panelGamma.Visible = false;
		this.panelKeyboard.Visible = false;
		this.panelMatrix.Visible = false;
		this.panelRearLight.Visible = false;
		this.panelPeripherals.Visible = false;
		this.panelAlly.Visible = false;
		this.panelStartup.Visible = false;
		this.panelVersion.Visible = false;

		// Form properties
		base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.AutoScroll = false;
		base.ClientSize = new System.Drawing.Size(460, 380);
		base.MinimumSize = new System.Drawing.Size(440, 380);
		base.MaximumSize = new System.Drawing.Size(500, 480);

		// Controls added in reverse order for Dock = Top
		base.Controls.Add(this.panelFooter);
		base.Controls.Add(this.panelQuickControls);
		base.Controls.Add(this.panelAiAuto);
		base.Controls.Add(this.panelScreen);
		base.Controls.Add(this.panelBattery);
		base.Controls.Add(this.panelPerformance);

		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MdiChildrenMinimizedAnchorBottom = false;
		base.MinimizeBox = false;
		base.Name = "SettingsForm";
		base.Padding = new System.Windows.Forms.Padding(10, 0, 10, 6);
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "Asus";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

using Asus.Display;
using Asus.Helpers;
using Asus.Mode;
using Asus.USB;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;

namespace Asus.Input
{

    public class InputDispatcher
    {
        System.Timers.Timer timer = new System.Timers.Timer(AppConfig.Get("keyboard_timeout_refresh", 1000));
        public static bool backlightActivity = true;
        public static bool lidClose = false;
        public static bool tentMode = false;
        private static bool? _fnLock = null;
        private static string? _asusPath = null;

        private static long lastSleep;

        public static Keys keyProfile = (Keys)AppConfig.Get("keybind_profile", (int)Keys.F5);
        public static Keys keyApp = (Keys)AppConfig.Get("keybind_app", (int)Keys.F12);

        public static Keys keyProfile0 = (Keys)AppConfig.Get("keybind_profile_0", (int)Keys.F17);
        public static Keys keyProfile1 = (Keys)AppConfig.Get("keybind_profile_1", (int)Keys.F18);
        public static Keys keyProfile2 = (Keys)AppConfig.Get("keybind_profile_2", (int)Keys.F16);
        public static Keys keyProfile3 = (Keys)AppConfig.Get("keybind_profile_3", (int)Keys.F19);
        public static Keys keyProfile4 = (Keys)AppConfig.Get("keybind_profile_4", (int)Keys.F20);
        public static Keys keyXGM = (Keys)AppConfig.Get("keybind_xgm", (int)Keys.F21);
        public static Keys keyOverlay = (Keys)AppConfig.Get("keybind_overlay", (int)Keys.O);

        public static ModifierKeys keyModifier = GetModifierKeys("modifier_keybind", ModifierKeys.Shift | ModifierKeys.Control);
        public static ModifierKeys keyModifierAlt = GetModifierKeys("modifier_keybind_alt", ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt);

        static ModeControl modeControl = Program.modeControl;

        KeyboardListener listener;
        KeyboardHook hook = new KeyboardHook();

        public InputDispatcher()
        {

            byte[] result = Program.acpi.DeviceInit();
            Debug.WriteLine($"Init: {BitConverter.ToString(result)}");

            Program.acpi.SubscribeToEvents(WatcherEventArrived);
            //Task.Run(Program.acpi.RunListener);

            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(KeyPressed);

            MKeyControl.ApplyAll();
            RegisterKeys();

            timer.Elapsed += Timer_Elapsed;

        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (GetBacklight() == 0) return;

            TimeSpan iddle = NativeMethods.GetIdleTime();
            int kb_timeout;

            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                kb_timeout = AppConfig.Get("keyboard_ac_timeout", 0);
            else
                kb_timeout = AppConfig.Get("keyboard_timeout", 60);

            if (kb_timeout == 0) return;

            if (backlightActivity && iddle.TotalSeconds > kb_timeout)
            {
                backlightActivity = false;
                Aura.ApplyBrightness(0, "Timeout");
            }

            if (!backlightActivity && iddle.TotalSeconds < kb_timeout)
            {
                backlightActivity = true;
                SetBacklightAuto();
            }

            //Logger.WriteLine("Iddle: " + iddle.TotalSeconds);
        }

        public void Init()
        {
            if (listener is not null) listener.Dispose();

            Program.acpi.DeviceInit();
            MKeyControl.ApplyAll();

            if (!AsusService.IsAsusOptimizationRunning())
            {
                Program.acpi.DeviceGet(AsusACPI.CameraShutter);
                listener = new KeyboardListener(HandleEvent);
                InitCamera();
            }
            else
            {
                Logger.WriteLine("Optimization service is running");
            }

            InitBacklightTimer();
            MuteLEDInit();
        }

        public static void InitFNLock()
        {
            if (!IsHardwareFnLock()) return;
            AsusHid.InitInput();
            HardwareFnLock(AppConfig.Is("fn_lock"));
        }

        public void InitBacklightTimer()
        {
            timer.Enabled = AppConfig.Get("keyboard_timeout") > 0 && SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online ||
                            AppConfig.Get("keyboard_ac_timeout") > 0 && SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
        }

        private static ModifierKeys GetModifierKeys(string configKey, ModifierKeys defaultModifiers)
        {
            string configValue = AppConfig.GetString(configKey, "");
                
            if (string.IsNullOrWhiteSpace(configValue))
                return defaultModifiers;

            ModifierKeys modifiers = ModifierKeys.None;
            HashSet<string> keys = new HashSet<string>(configValue.Split('-'), StringComparer.OrdinalIgnoreCase);

            if (keys.Contains("win")) modifiers |= ModifierKeys.Win;
            if (keys.Contains("shift")) modifiers |= ModifierKeys.Shift;
            if (keys.Contains("control")) modifiers |= ModifierKeys.Control;
            if (keys.Contains("alt")) modifiers |= ModifierKeys.Alt;

            return modifiers;
        }

        public void RegisterKeys()
        {
            hook.UnregisterAll();

            string actionM1 = AppConfig.GetString("m1");
            string actionM2 = AppConfig.GetString("m2");

            if (keyProfile != Keys.None)
            {
                hook.RegisterHotKey(keyModifier, keyProfile);
                hook.RegisterHotKey(keyModifierAlt, keyProfile);
            }

            if (keyApp != Keys.None) hook.RegisterHotKey(keyModifier, keyApp);

            if (!AppConfig.Is("skip_hotkeys"))
            {
                if (AppConfig.IsVivoZenbook() && AppConfig.IsOLED())
                {
                    hook.RegisterHotKey(keyModifierAlt, Keys.F7);
                    hook.RegisterHotKey(keyModifierAlt, Keys.F8);
                }

                hook.RegisterHotKey(keyModifierAlt, Keys.F13);

                hook.RegisterHotKey(keyModifierAlt, Keys.F14);
                hook.RegisterHotKey(keyModifierAlt, Keys.F15);

                hook.RegisterHotKey(keyModifierAlt, keyProfile0);
                hook.RegisterHotKey(keyModifierAlt, keyProfile1);
                hook.RegisterHotKey(keyModifierAlt, keyProfile2);
                hook.RegisterHotKey(keyModifierAlt, keyProfile3);
                hook.RegisterHotKey(keyModifierAlt, keyProfile4);
                hook.RegisterHotKey(keyModifierAlt, keyXGM);

                hook.RegisterHotKey(ModifierKeys.Control, Keys.VolumeDown);
                hook.RegisterHotKey(ModifierKeys.Control, Keys.VolumeUp);
                hook.RegisterHotKey(ModifierKeys.Shift, Keys.VolumeDown);
                hook.RegisterHotKey(ModifierKeys.Shift, Keys.VolumeUp);
                hook.RegisterHotKey(keyModifier, Keys.F20);
            }

            if (keyOverlay != Keys.None) hook.RegisterHotKey(keyModifierAlt, keyOverlay);

            if (!AppConfig.IsVivoZenPro())
            {
                if (actionM1 is not null && actionM1.Length > 0 && !MKeyControl.IsFirmware("m1")) hook.RegisterHotKey(ModifierKeys.NoRepeat, Keys.VolumeDown);
                if (actionM2 is not null && actionM2.Length > 0 && !MKeyControl.IsFirmware("m2")) hook.RegisterHotKey(ModifierKeys.NoRepeat, Keys.VolumeUp);
            }

            // FN-Lock group

            if (AppConfig.Is("fn_lock") && !IsHardwareFnLock())
                for (Keys i = Keys.F1; i <= Keys.F11; i++) hook.RegisterHotKey(ModifierKeys.None, i);



        }


        public static int[] ParseHexValues(string input)
        {
            string pattern = @"\b(0x[0-9A-Fa-f]{1,2}|[0-9A-Fa-f]{1,2})\b";

            if (!Regex.IsMatch(input, $"^{pattern}(\\s+{pattern})*$")) return new int[0];

            MatchCollection matches = Regex.Matches(input, pattern);

            int[] hexValues = new int[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                string hexValueStr = matches[i].Value;
                int hexValue = int.Parse(hexValueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    ? hexValueStr.Substring(2)
                    : hexValueStr, System.Globalization.NumberStyles.HexNumber);

                hexValues[i] = hexValue;
            }

            return hexValues;
        }


        static void RunKeyCommand(string command, bool launchOnNoKeys = true)
        {
            int[] hexKeys = new int[0];
            try { hexKeys = ParseHexValues(command); } catch { }

            switch (hexKeys.Length)
            {
                case 1:
                    KeyboardHook.KeyPress((Keys)hexKeys[0]);
                    break;
                case 2:
                    KeyboardHook.KeyKeyPress((Keys)hexKeys[0], (Keys)hexKeys[1]);
                    break;
                case 3:
                    KeyboardHook.KeyKeyKeyPress((Keys)hexKeys[0], (Keys)hexKeys[1], (Keys)hexKeys[2]);
                    break;
                case 4:
                    KeyboardHook.KeyKeyKeyKeyPress((Keys)hexKeys[0], (Keys)hexKeys[1], (Keys)hexKeys[2], (Keys)hexKeys[3]);
                    break;
                default:
                    if (launchOnNoKeys && !string.IsNullOrWhiteSpace(command)) LaunchProcess(command);
                    break;
            }
        }

        static void CustomKey(string configKey = "m3")
        {
            RunKeyCommand(AppConfig.GetString(configKey + "_custom"));
        }


        static void SetBrightness(bool up, bool hotkey = false)
        {
            if (AppConfig.SwappedBrightness() && !hotkey) up = !up;

            int step = AppConfig.Get("brightness_step", 10);
            if (step != 10)
            {
                Program.toast.RunToast(ScreenBrightness.Adjust(up ? step : -step) + "%", up ? ToastIcon.BrightnessUp : ToastIcon.BrightnessDown);
                return;
            }

            Program.acpi.DeviceSet(AsusACPI.UniversalControl, up ? AsusACPI.Brightness_Up : AsusACPI.Brightness_Down, "Brightness");

        }

        static void SetBrightnessDimming(int delta)
        {
            int brightness = VisualControl.SetBrightness(delta: delta);
            if (brightness >= 0)
                Program.toast.RunToast(brightness + "%", (delta < 0) ? ToastIcon.BrightnessDown : ToastIcon.BrightnessUp);
        }

        public void KeyPressed(object sender, KeyPressedEventArgs e)
        {

            Logger.WriteLine(e.Key.ToString() + " " + e.Modifier.ToString());

            if (e.Modifier == ModifierKeys.None)
            {
                if (AppConfig.NoMKeys())
                {
                    switch (e.Key)
                    {
                        case Keys.F2:
                            KeyboardHook.KeyPress(Keys.VolumeDown);
                            return;
                        case Keys.F3:
                            KeyboardHook.KeyPress(Keys.VolumeUp);
                            return;
                        case Keys.F4:
                            ToggleMic();
                            return;
                    }
                }


                switch (e.Key)
                {
                    case Keys.F1:
                        KeyboardHook.KeyPress(Keys.VolumeMute);
                        break;
                    case Keys.F2:
                        SetBacklight(-1, true);
                        break;
                    case Keys.F3:
                        SetBacklight(1, true);
                        break;
                    case Keys.F4:
                        KeyProcess("fnf4");
                        break;
                    case Keys.F5:
                        KeyProcess("fnf5");
                        break;
                    case Keys.F6:
                        KeyboardHook.KeyPress(Keys.Snapshot);
                        break;
                    case Keys.F7:
                        SetBrightness(false);
                        break;
                    case Keys.F8:
                        SetBrightness(true);
                        break;
                    case Keys.F9:
                        KeyboardHook.KeyKeyPress(Keys.LWin, Keys.P);
                        break;
                    case Keys.F10:
                        ToggleTouchpadEvent(true);
                        break;
                    case Keys.F11:
                        SleepEvent();
                        break;
                    case Keys.VolumeDown:
                        KeyProcess("m1");
                        break;
                    case Keys.VolumeUp:
                        KeyProcess("m2");
                        break;
                    case Keys.Left:
                        KeyboardHook.KeyPress(Keys.Home);
                        break;
                    case Keys.Right:
                        KeyboardHook.KeyPress(Keys.End);
                        break;
                    case Keys.Up:
                        KeyboardHook.KeyPress(Keys.PageUp);
                        break;
                    case Keys.Down:
                        KeyboardHook.KeyPress(Keys.PageDown);
                        break;
                    default:
                        break;
                }

            }

            if (e.Modifier == keyModifier)
            {
                if (e.Key == keyProfile) modeControl.CyclePerformanceMode();
                if (e.Key == keyApp) Program.SettingsToggle();
                if (e.Key == Keys.F20) ToggleMic();
            }

            if (e.Modifier == keyModifierAlt)
            {
                if (e.Key == keyProfile) modeControl.CyclePerformanceMode(true);

                if (e.Key == keyProfile0) modeControl.SetPerformanceMode(0, true);
                if (e.Key == keyProfile1) modeControl.SetPerformanceMode(1, true);
                if (e.Key == keyProfile2) modeControl.SetPerformanceMode(2, true);
                if (e.Key == keyProfile3) modeControl.SetPerformanceMode(3, true);
                if (e.Key == keyProfile4) modeControl.SetPerformanceMode(4, true);
                if (e.Key == keyOverlay) Program.settingsForm.BeginInvoke(() => Program.settingsForm.ToggleOverlay(true));

                switch (e.Key)
                {
                    case Keys.F1:
                        SetBrightness(false);
                        break;
                    case Keys.F2:
                        SetBrightness(true);
                        break;
                    case Keys.F3:
                        break;
                    case Keys.F4:
                        Program.SettingsToggle();
                        break;
                    case Keys.F6:
                        ToggleTouchScreen();
                        break;
                    case Keys.F7:
                        SetBrightnessDimming(-10);
                        break;
                    case Keys.F8:
                        SetBrightnessDimming(10);
                        break;
                    case Keys.F13:
                        ToggleScreenRate();
                        break;
                    case Keys.F14:
                        Program.toast.RunToast(Properties.Strings.EcoMode);
                        break;
                    case Keys.F15:
                        Program.toast.RunToast(Properties.Strings.StandardMode);
                        break;
                }
            }

            if (e.Modifier == (ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Keys.VolumeDown:
                        // Screen brightness down on CTRL+VolDown
                        SetBrightness(false);
                        break;
                    case Keys.VolumeUp:
                        // Screen brightness up on CTRL+VolUp
                        SetBrightness(true);
                        break;
                }
            }

            if (e.Modifier == (ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Keys.VolumeDown:
                        // Keyboard backlight down on SHIFT+VolDown
                        SetBacklight(-1);
                        break;
                    case Keys.VolumeUp:
                        // Keyboard backlight up on SHIFT+VolUp
                        SetBacklight(1);
                        break;
                }
            }
        }


        public static void KeyProcess(string name = "m3")
        {
            if (name == "m4" && Control.ModifierKeys == (Keys.Control | Keys.Shift | Keys.Alt))
            {
                Thread.Sleep(3000);
                if ((User32.GetAsyncKeyState(0x11) & User32.GetAsyncKeyState(0x10) & User32.GetAsyncKeyState(0x12) & 0x8000) != 0)
                {
                    Program.acpi.DeviceSet(AsusACPI.GPUMux, 1, "MUX hybrid recovery");
                    Process.Start(new ProcessStartInfo("shutdown", "/r /t 1") { CreateNoWindow = true, UseShellExecute = false });
                }
                return;
            }

            string action = AppConfig.GetString(name);

            if (action is null || action.Length <= 1)
            {
                if (name == "m4")
                    action = "asus";
                if (name == "m5")
                    action = "performance";
                if (name == "fnf4")
                    action = "aura";
                if (name == "fnf5")
                    action = "performance";
                if (name == "m3" && !AsusService.IsAsusOptimizationRunning())
                    action = "micmute";
                if (name == "fnc")
                    action = "fnlock";
                if (name == "fnv")
                    action = "visual";
                if (name == "fne")
                    action = "calculator";
            }

            switch (action)
            {
                case "mute":
                    KeyboardHook.KeyPress(Keys.VolumeMute);
                    break;
                case "volume_down":
                    KeyboardHook.KeyPress(Keys.VolumeDown);
                    break;
                case "volume_up":
                    KeyboardHook.KeyPress(Keys.VolumeUp);
                    break;
                case "backlight_down":
                    SetBacklight(-1);
                    break;
                case "backlight_up":
                    SetBacklight(1);
                    break;
                case "play":
                    KeyboardHook.KeyPress(Keys.MediaPlayPause);
                    break;
                case "screenshot":
                    KeyboardHook.KeyPress(Keys.Snapshot);
                    break;
                case "lock":
                    Logger.WriteLine("Screen lock");
                    NativeMethods.LockScreen();
                    break;
                case "screen":
                    Logger.WriteLine("Screen off toggle");
                    NativeMethods.TurnOffScreen();
                    break;
                case "miniled":
                    if (ScreenCCD.GetHDRStatus()) return;
                    string miniledName = ScreenControl.ToogleMiniled();
                    Program.toast.RunToast(miniledName, miniledName == Properties.Strings.OneZone ? ToastIcon.BrightnessDown : ToastIcon.BrightnessUp);
                    break;
                case "aura":
                    Program.settingsForm.BeginInvoke(Program.settingsForm.CycleAuraMode, Control.ModifierKeys == Keys.Shift ? -1 : 1);
                    break;
                case "visual":
                    Program.settingsForm.BeginInvoke(Program.settingsForm.CycleVisualMode, Control.ModifierKeys == Keys.Shift ? -1 : 1);
                    break;
                case "performance":
                    modeControl.CyclePerformanceMode(Control.ModifierKeys == Keys.Shift);
                    break;
                case "asus":
                    try
                    {
                        Program.settingsForm.BeginInvoke(delegate
                        {
                            Program.SettingsToggle();
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    break;
                case "fnlock":
                    ToggleFnLock();
                    break;
                case "overlay":
                    Program.settingsForm.BeginInvoke(() => Program.settingsForm.ToggleOverlay(true));
                    break;
                case "micmute":
                    ToggleMic();
                    break;
                case "brightness_up":
                    SetBrightness(true);
                    break;
                case "brightness_down":
                    SetBrightness(false);
                    break;
                case "custom":
                    CustomKey(name);
                    break;
                case "calculator":
                    LaunchProcess("calc");
                    break;

                case "touchscreen":
                    ToggleTouchScreen();
                    break;
                default:
                    break;
            }
        }


        static void MuteLED()
        {
            Thread.Sleep(500);
            Program.acpi.DeviceSet(AsusACPI.SoundMuteLed, false ? 1 : 0, "SoundLed");
        }

        static void ToggleTouchScreen()
        {
            var status = !TouchscreenHelper.GetStatus();
            Logger.WriteLine("Touchscreen status: " + status);
            if (status is not null)
            {
                Program.toast.RunToast(Properties.Strings.Touchscreen + " " + ((bool)status ? Properties.Strings.On : Properties.Strings.Off), ToastIcon.Touchpad);
                TouchscreenHelper.ToggleTouchscreen((bool)status);
            }
        }

        static void ToggleMic()
        {
            bool muteStatus = false;
            Program.toast.RunToast(muteStatus ? Properties.Strings.Muted : Properties.Strings.Unmuted, muteStatus ? ToastIcon.MicrophoneMute : ToastIcon.Microphone);
            if (AppConfig.IsVivoZenbook()) Program.acpi.DeviceSet(AsusACPI.MicMuteLed, muteStatus ? 1 : 0, "MicmuteLed");
        }

        static void MuteLEDInit()
        {
            if (!AppConfig.IsVivoZenbook()) return;
            if (Program.acpi.IsSupported(AsusACPI.MicMuteLed)) Program.acpi.DeviceSet(AsusACPI.MicMuteLed, false ? 1 : 0, "MicmuteLedInit");
            if (Program.acpi.IsSupported(AsusACPI.SoundMuteLed)) Program.acpi.DeviceSet(AsusACPI.SoundMuteLed, false ? 1 : 0, "SoundLedInit");
        }

        static bool GetTouchpadState()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PrecisionTouchPad\Status", false))
            {
                Logger.WriteLine("Touchpad status:" + key?.GetValue("Enabled")?.ToString());
                return key?.GetValue("Enabled")?.ToString() == "1";
            }
        }

        static void ToggleTouchpadEvent(bool hotkey = false)
        {
            if (hotkey || !AppConfig.IsHardwareTouchpadToggle()) ToggleTouchpad();
            Thread.Sleep(200);
            Program.toast.RunToast(GetTouchpadState() ? Properties.Strings.On : Properties.Strings.Off, ToastIcon.Touchpad);
        }

        static void ToggleTouchpad()
        {
            if (AppConfig.IsROG())
            {
                AsusHid.WriteInput([AsusHid.INPUT_ID, 0xF4, 0x6B], "USB Touchpad");
            } else
            {
                KeyboardHook.KeyKeyKeyPress(Keys.LWin, Keys.LControlKey, Keys.F24, 50, 50);
            }

        }

        static void SleepEvent()
        {
            if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastSleep) < 1000) return;
            lastSleep = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Program.acpi.DeviceSet(AsusACPI.UniversalControl, AsusACPI.KB_Sleep, "Sleep");
        }

        public static void ToggleArrowLock()
        {
            int arLock = AppConfig.Is("arrow_lock") ? 0 : 1;
            AppConfig.Set("arrow_lock", arLock);

            Program.settingsForm.BeginInvoke(Program.inputDispatcher.RegisterKeys);
            Program.toast.RunToast("Arrow-Lock " + (arLock == 1 ? Properties.Strings.On : Properties.Strings.Off), ToastIcon.FnLock);
        }

        public static bool IsHardwareFnLock()
        {
            if (AppConfig.IsHardwareFnLock()) return true;
            if (_fnLock is null)
            {
                var fnLockStatus = Program.acpi.DeviceGet(AsusACPI.FnLock);
                Logger.WriteLine("FnLock Support: " + fnLockStatus);
                _fnLock = fnLockStatus >= 0;
            }
            return (bool)_fnLock;
        }

        public static void HardwareFnLock(bool fnLock)
        {
            Program.acpi.DeviceSet(AsusACPI.FnLock, fnLock ? 1 : 0, "FnLock");
        }

        public static void ToggleFnLock()
        {
            bool fnLock = !AppConfig.Is("fn_lock");
            AppConfig.Set("fn_lock", fnLock ? 1 : 0);

            if (IsHardwareFnLock())
                HardwareFnLock(fnLock);
            else
                Program.settingsForm.BeginInvoke(Program.inputDispatcher.RegisterKeys);

            Program.settingsForm.BeginInvoke(Program.settingsForm.VisualiseFnLock);

            Program.toast.RunToast(fnLock ? Properties.Strings.FnLockOn : Properties.Strings.FnLockOff, ToastIcon.FnLock);
        }

        public static void ToggleWinLock()
        {
            Program.toast.RunToast(Properties.Strings.WinLockToggle);
        }

        public static void SetSlateMode(int status)
        {
            try
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\PriorityControl", "ConvertibleSlateMode", status, RegistryValueKind.DWord);
                Logger.WriteLine("Setting ConvertibleSlateMode : " + status);
            } catch (Exception ex)
            {
                Logger.WriteLine("Can't set ConvertibleSlateMode: " + ex.Message);
            }
        }

        public static void TabletMode()
        {
            if (AppConfig.Is("disable_tablet")) return;

            bool touchpadState = GetTouchpadState();
            bool tabletState = Program.acpi.DeviceGet(AsusACPI.TabletState) > 0;
            int slateState = Program.acpi.DeviceGet(AsusACPI.SlateMode);

            Logger.WriteLine($"Tablet: {tabletState} | SlateMode: {slateState} | Touchpad: {touchpadState}");

            if (slateState >= 0) SetSlateMode(slateState);
            if (tabletState && touchpadState || !tabletState && !touchpadState) ToggleTouchpad();
        }

        static int GetTentState()
        {
            var tentState = Program.acpi.DeviceGet(AsusACPI.TentState);
            // TentState is sticky on some convertibles (e.g. ProArt PX13); cross-check TabletState.
            if (tentState > 0 && Program.acpi.DeviceGet(AsusACPI.TabletState) == AsusACPI.Tablet_Notebook) tentState = 0;
            Logger.WriteLine($"Tent: {tentState}");
            return tentState;
        }

        public static void TentMode()
        {
            var tentState = GetTentState();
            if (tentState < 0) return;
            tentMode = tentState > 0;
            Aura.ApplyBrightness(tentMode ? 0 : GetBacklight(), "Tent");
        }

        static void HandleEvent(int EventID)
        {
            string carrier = MKeyControl.CarrierSlot(EventID);
            if (carrier is not null)
            {
                KeyProcess(carrier);
                return;
            }

            // All devices use the same HID key-codes, so we can process them all the same.
            {
                switch (EventID)
                {
                    case 95:     // Z13 Side button
                        KeyProcess("m4");
                        return;
                    case 134:     // FN + F12 ON OLD DEVICES
                    case 139:     // ProArt F12
                        KeyProcess("m4");
                        return;
                    case 124:    // M3
                        KeyProcess("m3");
                        return;
                    case 56:    // M4 / Rog button
                        KeyProcess("m4");
                        return;
                    case 55:    // Arconym
                        KeyProcess("m6");
                        return;
                    case 181:    // FN + Numpad Enter
                        KeyProcess("fne");
                        return;
                    case 93:    // GoPro key
                    case 174:   // FN+F5
                    case 153:   // FN+F5 OLD MODELS
                        modeControl.CyclePerformanceMode(Control.ModifierKeys == Keys.Shift);
                        return;
                    case 178:   // FN+LEFT ARROW / FN + F4
                        Program.settingsForm.BeginInvoke(Program.settingsForm.CycleAuraMode, -1);
                        return;
                    case 179:   // FN+F4
                        KeyProcess("fnf4");
                        return;
                    case 138:   // Fn + V
                        KeyProcess("fnv");
                        return;
                    case 158:   // Fn + C
                        KeyProcess("fnc");
                        return;
                    case 189: // Tablet mode
                        AutoKeyboard();
                        return;
                    case 197: // FN+F2
                        SetBacklight(-1);
                        return;
                    case 196: // FN+F3
                        SetBacklight(1);
                        return;
                    case 199: // ON Z13 - FN+F11 - cycles backlight
                        SetBacklight(4);
                        return;
                    case 46: // Fn + F4 Vivobook Brightness down
                        if (Control.ModifierKeys == Keys.Control && AppConfig.IsOLED())
                        {
                            SetBrightnessDimming(-10);
                        }
                        break;
                    case 47: // Fn + F5 Vivobook Brightness up
                        if (Control.ModifierKeys == Keys.Control && AppConfig.IsOLED())
                        {
                            SetBrightnessDimming(10);
                        }
                        break;
                }
            }

            if (!AsusService.IsAsusOptimizationRunning())
                HandleOptimizationEvent(EventID);

        }

        // Asus Optimization service Events 
        static void HandleOptimizationEvent(int EventID)
        {
            switch (EventID)
            {
                case 16: // FN+F7
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        SetBacklight(-1);
                    }
                    else if (Control.ModifierKeys == Keys.Control && AppConfig.IsOLED())
                    {
                        SetBrightnessDimming(-10);
                    }
                    else
                    {
                        SetBrightness(false, true);
                    }
                    break;
                case 32: // FN+F8
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        SetBacklight(1);
                    }
                    else if (Control.ModifierKeys == Keys.Control && AppConfig.IsOLED())
                    {
                        SetBrightnessDimming(10);
                    }
                    else
                    {
                        SetBrightness(true, true);
                    }
                    break;
                case 133: // Camera Toggle
                    ToggleCamera();
                    break;
                case 107: // FN+F10
                    ToggleTouchpadEvent();
                    break;
                case 108: // FN+F11
                    SleepEvent();
                    break;

                case 51:    // Fn+F6 on old TUFs
                case 53:    // Fn+F6 on GA-502DU model
                    NativeMethods.TurnOffScreen();
                    return;
                case 126:    // Fn+F8 emojis popup
                    KeyboardHook.KeyKeyPress(Keys.LWin, Keys.OemSemicolon);
                    return;
                case 78:    // Fn + ESC
                    ToggleFnLock();
                    return;
                case 79:    // Fn + Win
                    ToggleWinLock();
                    return;
                case 75:    // Fn + Arrow Lock
                    ToggleArrowLock();
                    return;
                case 136:    // FN + F12
                    Program.acpi.DeviceSet(AsusACPI.UniversalControl, AsusACPI.Airplane, "Airplane");
                    return;
                case 50:
                    // Sound Mute Event
                    MuteLED();
                    return;
                case 157:   // Zenbook DUO FN+F
                    modeControl.CyclePerformanceMode(Control.ModifierKeys == Keys.Shift);
                    return;
                case 250:
                    // Tent Mode
                    TentMode();
                    return;
            }
        }


        public static int GetBacklight()
        {
            int backlight_power = AppConfig.Get("keyboard_brightness", 1);
            int backlight_battery = AppConfig.Get("keyboard_brightness_ac", 1);
            bool onBattery = SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online;

            int backlight;

            //backlight = onBattery ? Math.Min(backlight_battery, backlight_power) : Math.Max(backlight_battery, backlight_power);
            backlight = onBattery ? backlight_battery : backlight_power;

            return Math.Max(Math.Min(3, backlight), 0);
        }

        public static void AutoKeyboard()
        {
            if (AppConfig.HasTabletMode()) TabletMode();
            if (lidClose)
            {
                Logger.WriteLine("Skipping Backlight Init: Lid Closed");
                return;
            }

            if (tentMode)
            {
                tentMode = GetTentState() > 0;
                if (tentMode)
                {
                    Logger.WriteLine("Skipping Backlight Init: Tent Mode");
                    return;
                }
            }

            Aura.Init();

            if (!AppConfig.Is("skip_aura"))
            {
                Aura.ApplyPower();
                SetBacklightAuto();
                Aura.ApplyAura();
            } else
            {
                Logger.WriteLine("Skipping Aura");
            }
        }


        public static void SetBacklightAuto()
        {
            if (lidClose || tentMode) return;
            Aura.ApplyBrightness(GetBacklight(), "Auto");
            backlightActivity = true;
        }

        public static void StartupBacklight()
        {
            Aura.DirectBrightness(GetBacklight(), "Startup");
        }

        public static void SetBacklight(int delta, bool force = false)
        {
            int backlight_power = AppConfig.Get("keyboard_brightness", 1);
            int backlight_battery = AppConfig.Get("keyboard_brightness_ac", 1);
            bool onBattery = SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online;

            int backlight = onBattery ? backlight_battery : backlight_power;
            int backlightMax = AppConfig.Get("max_brightness", 3);

            if (delta > backlightMax)
                backlight = ++backlight % (backlightMax + 1);
            else
                backlight = Math.Max(Math.Min(backlightMax, backlight + delta), 0);

            if (onBattery)
                AppConfig.Set("keyboard_brightness_ac", backlight);
            else
                AppConfig.Set("keyboard_brightness", backlight);

            var extraForm = Program.settingsForm.extraForm;
            if (extraForm != null && extraForm.Text != "") extraForm.VisualiseBacklight(backlight);

            if (force || !AsusService.IsAsusOptimizationRunning())
            {
                Aura.ApplyBrightness(backlight, "HotKey");
            }

            if (!AsusService.IsOSDRunning())
            {
                string[] backlightNames = new string[] { Properties.Strings.BacklightOff, Properties.Strings.BacklightLow, Properties.Strings.BacklightMid, Properties.Strings.BacklightMax };
                Program.toast.RunToast(backlightNames[backlight], delta > 0 ? ToastIcon.BacklightUp : ToastIcon.BacklightDown);
            }

        }

        public static void ToggleScreenRate()
        {
            AppConfig.Set("screen_auto", 0);
            ScreenControl.ToggleScreenRate();
        }


        private static string GetAsusPath()
        {
            if (_asusPath == null)
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher(@"Select * from Win32_SystemDriver WHERE Name='ATKWMIACPIIO'"))
                    {
                        foreach (var driver in searcher.Get())
                        {
                            string path = driver["PathName"].ToString();
                            _asusPath = Path.GetDirectoryName(path);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex.Message);
                }
            }

            return _asusPath;
        }

        public static void ToggleCamera()
        {
            int cameraShutter = Program.acpi.DeviceGet(AsusACPI.CameraShutter);
            Logger.WriteLine("Camera Shutter status: " + cameraShutter);

            int state = cameraShutter & 1;
            int feature = cameraShutter & ~1;

            switch (feature)
            {
                case 0x00000:
                    Program.acpi.DeviceSet(AsusACPI.CameraShutter, state ^ 1,
                        state == 0 ? "CameraShutterOn" : "CameraShutterOff");
                    Program.toast.RunToast(state == 0 ? "Camera Off" : "Camera On");
                    break;
                case 0x40000:
                    Program.toast.RunToast(state == 0 ? "Camera Off" : "Camera On");
                    break;
                case 0xC0000:
                    SetCamera(state ^ 1);
                    break;
                case 0x100000:
                    Program.acpi.DeviceSet(AsusACPI.CameraShutter, 4 | state, "CameraShutter");
                    Program.toast.RunToast(state == 0 ? "Camera On" : "Camera Off");
                    break;
                default:
                    SetCamera(2);
                    break;
            }
        }

        private static void SetCamera(int status, bool toast = true)
        {
            string asusPath = GetAsusPath();

            var cameraStatus = AppConfig.Get("camera_status");
            if (status == 2 && cameraStatus >= 0) status = cameraStatus > 0 ? 0 : 1;

            var result = ProcessHelper.RunCMD($"{asusPath}\\AsusHotkey.exe", $"-MFCameraCommand {status} 1 0", asusPath);
            var cameraLedStatus = Program.acpi.DeviceGet(AsusACPI.CameraLed);
            Logger.WriteLine("Camera LED: " + cameraLedStatus);
            AppConfig.Set("camera_status", status);
            if (toast)
            {
                string statusText = cameraLedStatus switch
                {
                    0 => "On",
                    1 => "Off",
                    _ => status switch
                    {
                        0 => "On",
                        1 => "Off",
                        _ => "Toggled"
                    }
                };
                Program.toast.RunToast($"Camera {statusText}");
            }
        }

        private static void InitCamera()
        {
            var cameraStatus = AppConfig.Get("camera_status");
            if (cameraStatus >= 0) SetCamera(cameraStatus, false);
        }

        public static void SetStatusLED(bool status)
        {
            Program.acpi.DeviceSet(AsusACPI.StatusLed, status ? 7 : 0, "StatusLED");
        }

        public static void InitStatusLed()
        {
            if (AppConfig.IsAutoStatusLed()) SetStatusLED(true);
        }

        public static void ShutdownStatusLed()
        {
            if (AppConfig.IsAutoStatusLed()) SetStatusLED(false);
        }

        static void LaunchProcess(string command = "")
        {
            if (string.IsNullOrEmpty(command)) return;
            try
            {
                RestrictedProcessHelper.RunAsRestrictedUser(command);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Failed to run: {command} {ex.Message}");
            }
        }

        static void WatcherEventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if (e.NewEvent is null) return;
                int EventID = int.Parse(e.NewEvent["EventID"].ToString());
                Logger.WriteLine("WMI event " + EventID);
                if (AppConfig.NoWMI()) return;

                if (EventID == 123) Program.OnChargerEvent();
                if (EventID == 186 || EventID == 194) Program.settingsForm.VisualizeXGM();

                HandleEvent(EventID);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("WMI event error: " + ex.Message);
            }
        }
    }
}

using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Asus.UI
{
    public class RForm : Form
    {

        public static Color colorEco = Color.FromArgb(255, 6, 180, 138);
        public static Color colorStandard = Color.FromArgb(255, 58, 174, 239);
        public static Color colorTurbo = Color.FromArgb(255, 255, 32, 32);
        public static Color colorCustom = Color.FromArgb(255, 255, 128, 0);
        public static Color colorGray = Color.FromArgb(255, 168, 168, 168);

        // Asus brand accent (logo red) — used for the active/selected state
        // and the header close control, per the reference design.
        public static Color colorAccent = Color.FromArgb(255, 178, 34, 34);


        public static Color buttonMain;
        public static Color buttonSecond;

        public static Color formBack;
        public static Color foreMain;
        public static Color borderMain;
        public static Color borderSecond;
        public static Color chartMain;
        public static Color chartGrid;

        public static bool flatTheme = false;

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool CheckSystemDarkModeStatus();

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#135")]
        private static extern int SetPreferredAppMode(int preferredAppMode);

        [DllImport("UXTheme.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(nint hWnd, string pszSubAppName, string? pszSubIdList);

        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(nint hwnd, int attr, int[] attrValue, int attrSize);

        public bool darkTheme = false;
        private bool themeInitialized = false;
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                parms.ClassStyle &= ~0x00020000;
                return parms;
            }
        }
        public static void InitColors(bool darkTheme)
        {
            // Unset or "flat" => modern flat surfaces; "classic" keeps the gradient look.
            string? theme = AppConfig.GetString("theme");
            flatTheme = theme is null || theme.Equals("flat", StringComparison.OrdinalIgnoreCase);

            if (darkTheme)
            {
                // Reference charcoal palette with precision Asus red accent.
                buttonMain = Color.FromArgb(255, 26, 29, 35);
                buttonSecond = Color.FromArgb(255, 20, 22, 26);

                formBack = Color.FromArgb(255, 14, 15, 18);
                foreMain = Color.FromArgb(255, 235, 238, 242);
                borderMain = Color.FromArgb(255, 38, 42, 50);
                borderSecond = Color.FromArgb(255, 32, 35, 42);

                chartMain = Color.FromArgb(255, 20, 22, 26);
                chartGrid = Color.FromArgb(255, 42, 46, 54);
            }
            else
            {
                buttonMain = SystemColors.ControlLightLight;
                buttonSecond = SystemColors.ControlLight;

                formBack = SystemColors.Control;
                foreMain = SystemColors.ControlText;
                borderMain = Color.FromArgb(255, 220, 220, 220);
                borderSecond = Color.FromArgb(255, 215, 215, 215);

                chartMain = SystemColors.ControlLightLight;
                chartGrid = Color.LightGray;
            }
        }

        private static bool IsDarkTheme()
        {
            string? uiMode = AppConfig.GetString("ui_mode");

            // Default to dark when no mode is configured.
            if (uiMode is null)
            {
                return true;
            }

            if (uiMode.ToLower() == "dark")
            {
                return true;
            }

            if (uiMode.ToLower() == "light")
            {
                return false;
            }

            if (uiMode.ToLower() == "windows")
            {
                return CheckSystemDarkModeStatus();
            }

            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var registryValueObject = key?.GetValue("AppsUseLightTheme");

            if (registryValueObject == null) return false;
            return (int)registryValueObject <= 0;
        }

        public virtual bool InitTheme(bool setDPI = false)
        {
            bool newDarkTheme = IsDarkTheme();
            bool changed = darkTheme != newDarkTheme;
            bool firstInit = !themeInitialized;
            darkTheme = newDarkTheme;
            themeInitialized = true;

            InitColors(darkTheme);

            if (setDPI)
                ControlHelper.Resize(this);

            if (changed || firstInit)
            {
                DwmSetWindowAttribute(Handle, 20, new[] { darkTheme ? 1 : 0 }, 4);
                SetPreferredAppMode(darkTheme ? 1 : 0); 
                SetWindowTheme(Handle, darkTheme ? "DarkMode_Explorer" : "Explorer", null);
                ControlHelper.Adjust(this, changed);
                this.Invalidate();
            }


            return changed;

        }

    }
}

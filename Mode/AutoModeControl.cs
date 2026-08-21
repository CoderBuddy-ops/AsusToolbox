namespace Asus.Mode
{
    /// <summary>
    /// "AI Auto" performance mode: adapts the BIOS performance mode to CPU
    /// temperature. Thresholds, hysteresis and the polling interval are
    /// user-configurable (see the AI Auto settings page).
    /// </summary>
    public static class AutoModeControl
    {
        public const int DEFAULT_TEMP_HIGH = 75; // switch to High Power (Turbo) above this
        public const int DEFAULT_TEMP_LOW = 55;  // switch to Low Power (Silent) below this
        public const int DEFAULT_HYSTERESIS = 5; // °C of hysteresis around each threshold
        public const int DEFAULT_INTERVAL_SECONDS = 1;

        public static int TempHigh => AppConfig.Get("ai_auto_temp_high", DEFAULT_TEMP_HIGH);
        public static int TempLow => AppConfig.Get("ai_auto_temp_low", DEFAULT_TEMP_LOW);
        public static int Hysteresis => AppConfig.Get("ai_auto_hysteresis", DEFAULT_HYSTERESIS);
        public static int IntervalSeconds => Math.Max(1, AppConfig.Get("ai_auto_interval", DEFAULT_INTERVAL_SECONDS));

        public static bool IsEnabled => AppConfig.Is("ai_auto_mode");

        /// <summary>
        /// Picks the target performance mode for the given CPU temperature and
        /// current mode. Above the high threshold goes to High Power (Turbo),
        /// below the low threshold goes to Low Power (Silent), and Standard
        /// (Balanced) in between - with hysteresis so the mode doesn't flap
        /// when temperature hovers right at a threshold.
        /// </summary>
        public static int GetTargetMode(float cpuTemp, int currentMode)
        {
            if (cpuTemp <= 0) return currentMode;

            int target;
            if (cpuTemp > TempHigh)
                target = AsusACPI.PerformanceTurbo;
            else if (cpuTemp < TempLow)
                target = AsusACPI.PerformanceSilent;
            else
                target = AsusACPI.PerformanceBalanced;

            // Hysteresis: once in a hot/cold mode, stay in it a little longer
            // before dropping back to Standard (with hysteresis 0 this exactly
            // matches the plain threshold logic).
            if (target == AsusACPI.PerformanceBalanced)
            {
                if (currentMode == AsusACPI.PerformanceTurbo && cpuTemp > TempHigh - Hysteresis)
                    return AsusACPI.PerformanceTurbo;
                if (currentMode == AsusACPI.PerformanceSilent && cpuTemp < TempLow + Hysteresis)
                    return AsusACPI.PerformanceSilent;
            }

            return target;
        }
    }
}

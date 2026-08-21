namespace Asus.Mode
{
    /// <summary>High-level AI Auto behaviour profiles presented in the UI.</summary>
    public enum AutoBehavior
    {
        Conservative = 0,
        Adaptive = 1,
        Aggressive = 2
    }

    /// <summary>
    /// Concrete engine settings for an AI Auto behaviour profile. The mapping
    /// is pure so it is unit-testable without touching config or hardware.
    /// </summary>
    public sealed record AutoBehaviorPreset(int TempHigh, int TempLow, int Hysteresis, int CooldownSamples)
    {
        /// <summary>The default (shipped) profile — "Adaptive".</summary>
        public static readonly AutoBehaviorPreset AdaptiveDefaults = new(75, 55, 5, 3);

        /// <summary>Maps a behaviour profile to concrete engine settings.</summary>
        public static AutoBehaviorPreset For(AutoBehavior behavior) => behavior switch
        {
            // Cooler-first: upgrade late, only on sustained load, more hysteresis.
            AutoBehavior.Conservative => new AutoBehaviorPreset(82, 58, 6, 5),
            // Performance-first: reacts quickly, less hysteresis, fast cooldown.
            AutoBehavior.Aggressive => new AutoBehaviorPreset(68, 52, 3, 2),
            _ => AdaptiveDefaults,
        };

        /// <summary>Applies the preset to app configuration.</summary>
        public void Apply()
        {
            AppConfig.Set("ai_auto_temp_high", TempHigh);
            AppConfig.Set("ai_auto_temp_low", TempLow);
            AppConfig.Set("ai_auto_hysteresis", Hysteresis);
            AppConfig.Set("ai_auto_cooldown", CooldownSamples);
        }
    }
}

namespace Asus.Mode
{
    public enum WorkloadLevel
    {
        Idle = 0,
        Light = 1,
        Moderate = 2,
        Heavy = 3
    }

    public readonly record struct AutoDecision(int TargetMode, WorkloadLevel Workload, string Reason)
    {
        public static readonly AutoDecision NoData = new(AsusACPI.PerformanceBalanced, WorkloadLevel.Idle, "No sensor data");
    }

    /// <summary>
    /// The local, rule-based "AI Auto" decision engine. Deterministic and
    /// explainable. Classifies the current workload from CPU load + temperature,
    /// applies hysteresis and a cooldown streak before upgrading to Performance,
    /// and always defers to thermal safety.
    /// </summary>
    public sealed class AutoModeEngine
    {
        private int _heavyStreak;

        // Hardcoded optimal thresholds to eliminate tuning UI
        private const int TempHigh = 75;
        private const int TempLow = 55;
        private const int ThermalLimit = 90;
        private const int CooldownSamples = 3;
        private const int HeavyLoadPercent = 70;
        private const int LightLoadPercent = 20;

        public static AutoModeEngine Shared { get; } = new AutoModeEngine();
        public static AutoDecision LastDecision { get; set; } = AutoDecision.NoData;

        public static WorkloadLevel ClassifyLoad(float cpuLoad)
        {
            if (cpuLoad <= LightLoadPercent) return WorkloadLevel.Light;
            if (cpuLoad >= HeavyLoadPercent) return WorkloadLevel.Heavy;
            return WorkloadLevel.Moderate;
        }

        public static int GetSuggestedIntervalMs(WorkloadLevel workload) => workload switch
        {
            WorkloadLevel.Idle => 5000,
            WorkloadLevel.Light => 3000,
            WorkloadLevel.Moderate => 2000,
            _ => 1000,
        };

        public void Reset() => _heavyStreak = 0;

        public AutoDecision Evaluate(float cpuTemp, float? cpuLoad, bool onBattery, int currentMode)
        {
            if (cpuTemp <= 0 || cpuLoad is null || cpuLoad < 0 || float.IsNaN(cpuTemp) || float.IsNaN(cpuLoad.Value))
            {
                _heavyStreak = 0;
                return AutoDecision.NoData with { TargetMode = currentMode };
            }

            WorkloadLevel workload = ClassifyLoad(cpuLoad.Value);
            bool sustainedHeavy = workload == WorkloadLevel.Heavy;
            _heavyStreak = sustainedHeavy ? _heavyStreak + 1 : 0;

            if (cpuTemp >= ThermalLimit)
                return new AutoDecision(AsusACPI.PerformanceTurbo, workload, $"Thermal safety — CPU {cpuTemp:0}°C");

            if (cpuTemp > TempHigh)
                return new AutoDecision(AsusACPI.PerformanceTurbo, workload, $"High CPU temperature ({cpuTemp:0}°C)");

            if (onBattery && workload <= WorkloadLevel.Light)
                return new AutoDecision(AsusACPI.PerformanceSilent, workload, "On battery, light workload");

            if (sustainedHeavy && _heavyStreak >= CooldownSamples)
                return new AutoDecision(AsusACPI.PerformanceTurbo, workload, "Sustained high CPU load");

            if (cpuTemp < TempLow)
                return new AutoDecision(AsusACPI.PerformanceSilent, workload, $"Low CPU temperature ({cpuTemp:0}°C)");

            string reason = workload switch
            {
                WorkloadLevel.Idle => "Idle",
                WorkloadLevel.Light => "Light workload",
                WorkloadLevel.Moderate => "Moderate workload",
                _ => "Heavy workload (warming up)",
            };
            return new AutoDecision(AsusACPI.PerformanceBalanced, workload, reason);
        }
    }
}

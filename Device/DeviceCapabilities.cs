using PawnIO;

namespace Asus.Device
{
    /// <summary>
    /// Capability matrix for the detected ASUS machine.
    ///
    /// The app is built specifically for ASUS Vivobook 15 laptops. Every control
    /// the UI shows should be gated on a capability from this matrix so the app
    /// never presents a control the connected firmware cannot honour.
    ///
    /// Detection rules (documented, not magic):
    ///  - Firmware-probed endpoints use <see cref="AsusACPI.IsSupported"/> which
    ///    reads the ACPI endpoint and caches the result (endpoint returns &gt;= 0
    ///    means "present").
    ///  - CPU vendor comes from the CPU brand string (<see cref="CpuInfo.IsAMD"/>).
    ///  - Keyboard backlight is driven over USB-HID (not ACPI), so there is no
    ///    ACPI endpoint to probe; it is inferred from the Vivobook model family.
    ///  - Microphone noise cancellation is not discoverable through the current
    ///    ACPI interface, so it stays false unless a probe is added.
    /// </summary>
    public sealed class DeviceCapabilities
    {
        public bool IsAsus { get; init; }
        public bool IsVivobook { get; init; }
        /// <summary>True only when the exact validated model (X1504ZA) is detected.</summary>
        public bool IsValidatedModel { get; init; }
        /// <summary>The exact model token (e.g. "X1504ZA") or empty when unknown.</summary>
        public string ExactModel { get; init; } = "";
        public bool IsIntelCpu { get; init; }
        public bool IsAmdCpu { get; init; }
        public bool ACPI_Connected { get; init; }
        /// <summary>EC firmware version; null when Windows reports it invalid/unavailable.</summary>
        public string? ECVersion { get; init; }

        public bool SupportsFanControl { get; init; }
        public bool SupportsFanCurve { get; init; }
        public bool SupportsPerformanceModes { get; init; }
        public bool SupportsChargeLimit { get; init; }
        public bool SupportsKeyboardBacklight { get; init; }
        public bool SupportsPowerLimits { get; init; }
        public bool SupportsStatusLed { get; init; }
        public bool SupportsDisplayOverdrive { get; init; }
        public bool SupportsMicrophoneNoiseCancellation { get; init; }

        /// <summary>Detects capabilities for the current machine.</summary>
        public static DeviceCapabilities Detect()
            => Detect(Program.acpi, AppConfig.IsVivoZenbook(), AppConfig.IsX1504ZA(), CpuInfo.IsAMD,
                      ecVersion: AppConfig.GetECVersion());

        /// <summary>
        /// Detects capabilities from an ACPI probe. Pure and injectable so tests
        /// can build any capability combination without touching hardware.
        /// </summary>
        public static DeviceCapabilities Detect(IAsusACPI? acpi, bool isVivobookModel, bool isAmdCpu)
            => Detect(acpi, isVivobookModel, false, isAmdCpu);

        /// <summary>
        /// Detects capabilities from an ACPI probe. Pure and injectable so tests
        /// can build any capability combination without touching hardware.
        /// </summary>
        public static DeviceCapabilities Detect(IAsusACPI? acpi, bool isVivobookModel, bool isValidatedModel, bool isAmdCpu,
            string? ecVersion = null)
        {
            bool connected = acpi?.IsConnected() == true;
            bool supportsCpuFan = acpi?.IsSupported(AsusACPI.CPU_Fan) == true;
            bool supportsMidFan = acpi?.IsMidFanSupported() == true;
            bool supportsModes = acpi?.IsSupported(AsusACPI.PerformanceMode) == true
                              || acpi?.IsSupported(AsusACPI.VivoBookMode) == true;
            bool supportsCharge = acpi?.IsSupported(AsusACPI.BatteryLimit) == true;
            bool supportsStatusLed = acpi?.IsSupported(AsusACPI.StatusLed) == true;
            bool supportsOverdrive = acpi?.IsSupported(AsusACPI.ScreenOverdrive) == true;
            bool supportsPowerLimits = acpi?.IsSupported((uint)AsusACPI.PPT_APUA0) == true;

            return new DeviceCapabilities
            {
                IsAsus = isVivobookModel || connected,
                IsVivobook = isVivobookModel,
                IsValidatedModel = isValidatedModel,
                ExactModel = isValidatedModel ? "X1504ZA" : "",
                IsIntelCpu = !isAmdCpu,
                IsAmdCpu = isAmdCpu,
                ACPI_Connected = connected,
                ECVersion = ecVersion,
                SupportsFanControl = supportsCpuFan || supportsMidFan,
                SupportsFanCurve = supportsCpuFan,
                SupportsPerformanceModes = supportsModes,
                SupportsChargeLimit = supportsCharge,
                // Backlight is USB-HID driven; no ACPI endpoint exists to probe.
                SupportsKeyboardBacklight = isVivobookModel,
                SupportsPowerLimits = supportsPowerLimits && isAmdCpu,
                SupportsStatusLed = supportsStatusLed,
                SupportsDisplayOverdrive = supportsOverdrive,
                SupportsMicrophoneNoiseCancellation = false, // no probe available yet
            };
        }

        public string Summary()
        {
            string cpu = IsIntelCpu ? "Intel" : IsAmdCpu ? "AMD" : "unknown";
            return $"{(IsVivobook ? "Vivobook" : IsAsus ? "ASUS" : "Unknown")} | {cpu} CPU | " +
                   $"fan={(SupportsFanControl ? "y" : "n")} curve={(SupportsFanCurve ? "y" : "n")} " +
                   $"modes={(SupportsPerformanceModes ? "y" : "n")} charge={(SupportsChargeLimit ? "y" : "n")} " +
                   $"backlight={(SupportsKeyboardBacklight ? "y" : "n")} powerLimits={(SupportsPowerLimits ? "y" : "n")} " +
                   $"statusLed={(SupportsStatusLed ? "y" : "n")} overdrive={(SupportsDisplayOverdrive ? "y" : "n")}";
        }
    }
}

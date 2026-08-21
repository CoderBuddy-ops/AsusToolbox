namespace Asus.Helpers
{
    /// <summary>Result of the pre-BIOS-update safety gate.</summary>
    public readonly record struct BiosSafetyCheck(bool Safe, string Reason)
    {
        public static readonly BiosSafetyCheck Ok = new(true, "");
    }

    /// <summary>
    /// Safety gate for BIOS updates on the X1504ZA. Pure and testable — the UI
    /// asks these questions and only proceeds to the official installer after
    /// every check passes and the user explicitly confirms. BIOS flashing is
    /// NEVER silent: the app only hands the user to the official ASUS installer.
    /// </summary>
    public static class BiosSafety
    {
        /// <summary>The exact validated model for this build.</summary>
        public const string TargetModel = "X1504ZA";

        /// <summary>Minimum battery percentage required before a BIOS update may proceed.</summary>
        public const int MinBatteryPercent = 30;

        /// <summary>
        /// Verifies the machine identity matches the X1504ZA target. Uses the
        /// same token-exact matching as the device-validation layer.
        /// </summary>
        public static BiosSafetyCheck VerifyModel(string? model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return new BiosSafetyCheck(false, "Machine model could not be determined.");
            return AppConfig.ModelMatches(model, TargetModel)
                ? BiosSafetyCheck.Ok
                : new BiosSafetyCheck(false, $"This update is for {TargetModel}, but the detected model is \"{model}\". Aborting.");
        }

        /// <summary>Verifies AC power and battery level are safe for a firmware update.</summary>
        public static BiosSafetyCheck VerifyPower(bool onAc, decimal batteryPercent)
        {
            if (!onAc)
                return new BiosSafetyCheck(false, "AC power is not connected. Connect the charger before updating the BIOS.");

            if (batteryPercent > 0 && batteryPercent < MinBatteryPercent)
                return new BiosSafetyCheck(false, $"Battery is at {batteryPercent:0}% — below the required {MinBatteryPercent}%. Charge before updating.");

            return BiosSafetyCheck.Ok;
        }

        /// <summary>Human-readable confirmation message listing verified preconditions.</summary>
        public static string ConfirmationMessage(bool modelOk, bool onAc, decimal batteryPercent)
        {
            string batt = batteryPercent > 0 ? $"{batteryPercent:0}%" : "unknown";
            return "BIOS UPDATE — critical firmware operation\n" +
                   "\n" +
                   $"  Model match (X1504ZA)   {(modelOk ? "✓" : "✗")}\n" +
                   $"  AC power connected      {(onAc ? "✓" : "✗")}\n" +
                   $"  Battery level           {batt}\n" +
                   "\n" +
                   "You will be taken to the official ASUS download page.\n" +
                   "Install the BIOS only through the official installer.\n" +
                   "Do not disconnect power or close the laptop during flashing.\n" +
                   "\nProceed?";
        }
    }
}

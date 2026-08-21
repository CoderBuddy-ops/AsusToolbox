using Xunit;

// The app relies heavily on static state (Program.acpi, AppConfig, ModeControl
// timers/tasks), so test classes must not run concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

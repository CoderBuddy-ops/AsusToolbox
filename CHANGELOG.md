# Changelog

All notable changes to AsusToolbox are documented here.

## [1.0.0] — 2026-08-21

### Initial Release

Ultralight ASUS laptop control tool — fan control, performance modes, battery protection, and keyboard backlight in a single 4.3 MB executable.

---

### ⚡ Performance Modes

- Silent, Balanced, and Turbo modes with BIOS-level fan curves
- Automatic mode switching on battery vs. AC power
- Windows Power Plan integration per mode
- System tray icon with quick-switch menu

### 🌀 Fan Control

- Automatic fan management per performance mode
- BIOS default fan curves (same as Armoury Crate)
- CPU and GPU temperature monitoring via ACPI

### 🔋 Battery Protection

- Configurable charge limit (50%–100%)
- ACPI write confirmation
- Automatic mode behavior on battery

### 🎨 Keyboard Backlight

- Full Aura RGB control (static, breathing, strobing, rainbow)
- Per-zone and per-key lighting on supported models
- Backlight brightness adjustment

### 🖥️ Display Control

- Screen brightness control
- HDR toggle
- Refresh rate management
- Clamshell mode support

### 🔧 Hotkeys & Input

- FN-key shortcut handling
- FN-Lock toggle
- Custom hotkey support
- System tray icon for quick access

---

### 🗑️ What Was Removed (vs Original)

These features were stripped to achieve the 4.3 MB ultralight footprint.

#### GPU Mode Control — Removed

- **What:** AMD and NVIDIA GPU switching (Eco/Standard/Ultimate modes)
- **Why:** Required NvAPIWrapper.Net (NVIDIA), ADL2 (AMD), and complex driver API management. Added ~200K lines of GPU-specific code and a heavy NuGet dependency.
- **Impact:** Users needing GPU mode switching should use G-Helper or Armoury Crate.

#### Hardware Overlay & FPS Monitor — Removed

- **What:** Real-time in-game overlay showing FPS, CPU/GPU temperature, usage, and power
- **Why:** Required Windows ETW (Event Tracing for Windows) providers and continuous background monitoring. Added complexity and elevated permission requirements.
- **Impact:** Users wanting an FPS overlay can use MSI Afterburner, Rivatuner, or similar tools.

#### Audio Visualizer — Removed

- **What:** FFT-based audio visualization for keyboard backlight (music-reactive lighting)
- **Why:** Required FftSharp (FFT analysis) and NAudio.Wasapi (audio capture) — two heavy NuGet packages that added significant binary size and runtime overhead.
- **Impact:** Keyboard backlight still works; audio-reactive modes are not available.

#### Fan Curve Charts — Removed

- **What:** Interactive chart editor for custom fan curves
- **Why:** Required WinForms.DataVisualization — the heaviest UI dependency in the project. Added ~200KB to the binary and pulled in a large charting framework.
- **Impact:** Fan curves are still managed via BIOS defaults and ACPI. Custom fan curve editing is not available in the UI.

#### Color Picker Controls — Removed

- **What:** Custom color picker widgets (RColorPicker, RColorButton) for keyboard backlight color selection
- **Why:** Tied to the chart and overlay features. Simplified the UI by removing custom color picker controls.
- **Impact:** Keyboard backlight colors are set via preset values.

---

### 📦 NuGet Packages Removed

| Package | Version | Reason |
|---|---|---|
| `NvAPIWrapper.Net` | 0.8.1.101 | NVIDIA GPU control APIs — removed with GPU mode feature |
| `FftSharp` | 2.2.0 | FFT analysis for audio visualizer — removed with audio feature |
| `NAudio.Wasapi` | 2.3.0 | Audio capture for visualizer — removed with audio feature |
| `WinForms.DataVisualization` | 1.10.2 | Chart controls for fan curve editor — removed with chart feature |

### 📁 Source Files Removed

| File | Feature |
|---|---|
| `Gpu/AMD/*` | AMD GPU control |
| `Gpu/NVidia/*` | NVIDIA GPU control |
| `Gpu/GPUModeControl.cs` | GPU mode switching logic |
| `Gpu/IGpuControl.cs` | GPU control interface |
| `NvidiaSmi.cs` | NVIDIA SMI wrapper |
| `NvmlHelper.cs` | NVIDIA Management Library helper |
| `Overlay/HardwareOverlay.cs` | In-game overlay |
| `Overlay/EtwFpsMonitor.cs` | ETW-based FPS tracking |
| `Helpers/Audio.cs` | Audio capture |
| `Helpers/AudioVisualizer.cs` | FFT audio visualization |
| `UI/RChart.cs` | Custom chart control |
| `UI/RColorPicker.cs` | Custom color picker |
| `UI/RColorButton.cs` | Custom color button |

---

### 📊 Build Results

| Metric | Value |
|---|---|
| Executable size | 4.3 MB (framework-dependent) |
| Self-contained size | 149 MB (includes .NET 8 runtime) |
| RAM at idle | 87 MB working set |
| Threads | 28 |
| Build warnings | 0 |
| Background services | 0 |

---

### 🛠️ Build Fix

- **Issue:** SDK-style `.csproj` was auto-including `.cs` files from `tools/LogoGenerator/obj/`, causing duplicate assembly attribute errors.
- **Fix:** Added `<Compile Remove="tools\**" />` to the project file to exclude tool source files from compilation.

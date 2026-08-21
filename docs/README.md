# AsusToolbox

Ultralight control tool for ASUS laptops.

## Features

- 🌀 Fan Control (Silent / Balanced / Turbo)
- ⚡ Performance Mode Switching
- 🔋 Battery Charge Limit (50%–100%)
- 🎨 Keyboard Backlight (Aura RGB)
- 🖥️ Display & Brightness Control
- 🔧 Hotkeys & System Tray

## Download

[Latest Release](https://github.com/CoderBuddy-ops/AsusToolbox/releases/latest)

## Build

```bash
git clone https://github.com/CoderBuddy-ops/AsusToolbox.git
cd AsusToolbox
dotnet publish Asus.csproj -c Release -p:Platform=x64 -p:PublishSingleFile=true -p:SelfContained=false -r:win-x64
```

## Supported Models

ROG Zephyrus G14/G15/G16, M16, X13/X16, Z13, DUO, TUF Series, Strix/Scar Series, ProArt, Vivobook, Zenbook, Expertbook, ROG Ally/Ally X, and more.

## Credits

Built on top of [g-helper](https://github.com/seerge/g-helper) by [serge](https://github.com/serge).

## License

See [LICENSE](https://github.com/CoderBuddy-ops/AsusToolbox/blob/main/LICENSE).

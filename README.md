# QoL Captain of Industry

`QoL Captain of Industry` is a `Captain of Industry` `Update 4` mod with sandbox tools, quality-of-life features, resource cheats, storage controls, terrain tools, and vehicle cap increases.

GitHub: <https://github.com/LT-Kev/QoL-Captain-of-Industry>

## Captain of Industry QoL Mod for Update 4

This `Captain of Industry` mod is built for `Update 4` and focuses on practical QoL tools for sandbox play. It adds an in-game `F8` panel for game speed, vehicle limits, storage behavior, terrain actions, weather control, research helpers, and resource cheats.

## Features

- `F8` QoL window with tabs for general tools, vehicles, terrain, storage and weather
- Extra game speed controls beyond vanilla `x3`, including `x5`, `x10` and `x15`
- Vehicle cap increase tools
- Storage helpers, including global storage modes, selected-storage controls, and a per-storage inspector button
- Terrain helpers for instant mining, instant dumping, terrain surface conversion, trees, reserve refills, and selected mine tower scope
- Research, instant build, unity, workers, power, computing, weather and source/sink helpers

## Download

- Direct DLL download:
  <https://raw.githubusercontent.com/LT-Kev/QoL-Captain-of-Industry/main/downloads/QoLCaptainOfIndustry.dll>
- Direct ZIP download:
  <https://github.com/LT-Kev/QoL-Captain-of-Industry/raw/main/downloads/QoLCaptainOfIndustry_0.1.0.zip>
- `ZIP` is recommended for a fresh install because it contains the full mod folder structure.
- `DLL` is useful when you only want to update the compiled mod file in an existing install.

## Install

1. Enable modding in Captain of Industry.
2. Download the `ZIP` and extract it into `%APPDATA%\Captain of Industry\Mods`.
3. Or replace the existing `QoLCaptainOfIndustry.dll` with the direct `DLL` download if the mod folder already exists.
4. Or build the project locally and let the post-build step deploy it automatically.
5. Enable the mod in-game and load or reopen your save.

## Build

Example PowerShell build command:

```powershell
$env:COI_ROOT='B:\SteamLibrary\steamapps\common\Captain of Industry'
dotnet build '.\QoLCaptainOfIndustry.csproj' -c Release
```

The build output is copied into:

```text
%APPDATA%\Captain of Industry\Mods\QoLCaptainOfIndustry
```

## Usage

- Press `F8` to open the in-game window
- Use `coi_qol_help` in the in-game console to list commands

Useful commands:

- `coi_qol_set_game_speed 5`
- `coi_qol_add_vehicle_limit 20`
- `coi_qol_set_storage_mode full`
- `coi_qol_instant_dump dirt`
- `coi_qol_refill_groundwater`

## Included Tools

- `General`: research, instant build, unity, workers, maintenance, power, computing and extended game speed
- `Vehicles`: vehicle cap overview and quick limit increases
- `Terrain`: instant mine, instant dump, terrain conversion, tree helpers and reserve refills
- `Storage`: global storage modes, product fill helpers and per-storage inspector action
- `Weather`: fast weather switching

## Notes

- The project was rebuilt around the current Update 4 mod API and favors compatibility over fancy legacy UI hooks.
- The current GUI uses IMGUI for stability and simple deployment.
- The terrain tab currently focuses on the stable Update 4 actions and intentionally avoids some of the older brittle hooks.

## License

This project is licensed under the `MIT` License. See [LICENSE](./LICENSE).

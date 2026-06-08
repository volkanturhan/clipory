# Clipory

**English | [Türkçe](README.tr.md)**

A lightweight Windows clipboard history manager.

Clipory lives quietly in your system tray and remembers everything you copy.
Press a hotkey to bring up your recent clips, pick one, and it's pasted straight
into whatever app you're working in — no more losing something because you
copied one more thing.

<p align="center">
  <img src="docs/screenshot.png" alt="Clipory popup" width="360" />
</p>

## Features

- **Clipboard history** — keeps your most recent copied text items.
- **Quick recall** — global hotkey (`Ctrl + Shift + V`) opens a searchable list.
- **Paste back instantly** — pick an item and it's pasted into the active app.
- **Favourites** — pin the clips you reuse; they stay on top and are never dropped.
- **Survives restarts** — your history (and pins) are saved and restored.
- **Start with Windows** — optional, toggled from the tray menu.
- **English & Turkish** — switch the interface language from the tray.
- **Stays out of the way** — runs from the system tray, no taskbar clutter.
- **Private by design** — everything stays on your machine; nothing is uploaded.

## Download

> **Note:** Clipory isn't published yet — the links below go live once the
> first release is created.

Grab one from the [latest release](https://github.com/volkanturhan/Clipory/releases/latest):

| Build | Size | Requirements |
| --- | --- | --- |
| **[Clipory.exe](https://github.com/volkanturhan/Clipory/releases/latest/download/Clipory.exe)** | ~68 MB | None — just run it |
| **[Clipory-lite.exe](https://github.com/volkanturhan/Clipory/releases/latest/download/Clipory-lite.exe)** | ~0.4 MB | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime) — Windows offers to install it on first run if it's missing |

Not sure which? Pick **Clipory.exe** — it runs on any Windows PC with nothing
else to install. **Clipory-lite.exe** is tiny but needs the .NET 8 runtime.

The first time you run it, Windows SmartScreen may warn about an unknown
publisher: click **More info → Run anyway**.

## How to use

1. Launch Clipory — it starts quietly in the system tray.
2. Copy things as you normally would; Clipory remembers them.
3. Press **`Ctrl + Shift + V`** to open the popup over whatever app you're in.
4. Start typing to filter, move with **↑ / ↓**, and press **Enter** (or
   double-click) to paste the chosen clip back into that app.
5. **Right-click** a clip (or **Ctrl + P**) to pin it; **Del** removes one.
6. **Esc** or clicking away closes the popup.

Right-click the tray icon for **Open**, **Clear history**, **Start with
Windows**, and **Quit**.

## Where your data lives

History is stored locally at `%APPDATA%\Clipory\history.json` and never leaves
your machine. Use **Clear history** in the tray menu to wipe it (pinned clips are
kept); pinned items can be removed individually from the popup.

## Build from source

```bash
# Run it
dotnet run --project Clipory/Clipory.csproj

# Build the shareable single-file exe (output: dist/win-x64/Clipory.exe)
pwsh tools/publish.ps1
```

## Tech

- C# / WPF on .NET 8 (Windows)
- No third-party dependencies

## License

MIT — see [LICENSE](LICENSE).

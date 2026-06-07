# ClipStack

A lightweight Windows clipboard history manager.

ClipStack lives quietly in your system tray and remembers everything you copy.
Press a hotkey to bring up your recent clips, pick one, and it's pasted straight
into whatever app you're working in. No more "I copied something else and lost
the thing I needed."

## Features

- **Clipboard history** — keeps your most recent copied text items.
- **Quick recall** — global hotkey (`Ctrl + Shift + V`) opens a searchable list.
- **Paste back instantly** — pick an item and it's pasted into the active app.
- **Stays out of the way** — runs from the system tray, no taskbar clutter.
- **Private by design** — everything stays on your machine; nothing is uploaded.

## How to use

1. Launch ClipStack — it starts quietly in the system tray.
2. Copy things as you normally would; ClipStack remembers them.
3. Press **`Ctrl + Shift + V`** to open the popup over whatever app you're in.
4. Start typing to filter, move with **↑ / ↓**, and press **Enter** (or
   double-click) to paste the chosen clip back into that app.
5. **Esc** or clicking away closes the popup. Right-click the tray icon to open
   the history or quit.

## Status

Working and usable. Built step by step — see the commit history.

Planned next: a branded tray icon, "start with Windows" option, pinned/favourite
clips, and in-app language support.

## Tech

- C# / WPF on .NET 8 (Windows)

## Building

```bash
dotnet build ClipStack.slnx
dotnet run --project ClipStack/ClipStack.csproj
```

## License

MIT — see [LICENSE](LICENSE).

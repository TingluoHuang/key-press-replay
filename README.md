# Key Press Replay

A Windows console application that reads a JSON config file and replays keyboard input in a loop. It uses the Win32 `SendInput` API and runs as Administrator so it can send key presses to elevated (admin) windows.

## Requirements

- Windows 10 / 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or download the self-contained release — no SDK needed)

## Build

```bash
dotnet build
```

### Publish self-contained single-file exe

```bash
dotnet publish KeyPressReplay.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

### Run tests

```bash
dotnet test
```

## Usage

1. Download the latest release zip from [GitHub Releases](../../releases) — or build from source.
2. Edit `config.json` (placed next to the exe) to define your key sequence.
3. Double-click the exe — Windows will prompt for admin elevation (UAC).
4. Switch to your target application within the initial delay window.
5. Press `Ctrl+C` in the console to stop at any time.

You can also pass a custom config path:

```
KeyPressReplay.exe my-other-config.json
```

## Config Format

```jsonc
{
  // 0 = infinite loop, or set a positive number for finite repeats
  "loopCount": 0,

  // Delay in ms before the first key press (time to switch to target window)
  "initialDelayMs": 3000,

  // Ordered key-press actions
  // "holdMs" = how long to hold the key down before releasing (0 = instant tap)
  "actions": [
    { "key": "F5",           "holdMs": 0,    "waitAfterMs": 2000 },
    { "key": "Tab",          "holdMs": 0,    "waitAfterMs": 500  },
    { "key": "Enter",        "holdMs": 0,    "waitAfterMs": 1000 },
    { "key": "Ctrl+S",       "holdMs": 0,    "waitAfterMs": 500  },
    { "key": "A",            "holdMs": 5000, "waitAfterMs": 300  },
    { "key": "Ctrl+Shift+S", "holdMs": 0,    "waitAfterMs": 1000 }
  ]
}
```

### Action Properties

| Property | Default | Description |
|---|---|---|
| `key` | *(required)* | Key or combo to press (e.g. `"A"`, `"Ctrl+S"`, `"F5"`) |
| `holdMs` | `0` | Milliseconds to hold the key down before releasing. `0` = instant tap. |
| `waitAfterMs` | `500` | Milliseconds to wait after the key press before the next action. |

### Supported Key Names

| Category | Examples |
|---|---|
| Letters | `A` – `Z` |
| Digits | `0` – `9` |
| Function keys | `F1` – `F12` |
| Modifiers | `Ctrl`, `Alt`, `Shift`, `Win` (and L/R variants) |
| Navigation | `Enter`, `Tab`, `Space`, `Backspace`, `Delete`, `Escape` |
| Arrows | `Left`, `Up`, `Right`, `Down` |
| Page keys | `Home`, `End`, `PageUp`, `PageDown` |
| Combos | `Ctrl+C`, `Alt+F4`, `Ctrl+Shift+S` |
| Numpad | `Num0`–`Num9`, `NumAdd`, `NumSubtract`, etc. |
| Punctuation | `;`, `=`, `,`, `-`, `.`, `/`, `` ` ``, `[`, `]`, `\`, `'` |

## CI/CD

The GitHub Actions workflow (`.github/workflows/ci.yml`) runs on every push/PR:

1. **Build & Test** — restores, builds, and runs all unit tests.
2. **Publish & Release** — triggered by pushing a version tag (e.g. `v1.0.0`):
   - Publishes a self-contained single-file exe for `win-x64`.
   - Creates a GitHub Release with the zip attached.

### Creating a release

```bash
git tag v1.0.0
git push origin v1.0.0
```

## How It Works

- The app embeds an **application manifest** (`app.manifest`) that requests `requireAdministrator`, so Windows will show a UAC prompt on launch.
- Key presses are sent using the Win32 **`SendInput`** API via P/Invoke, which can target any foreground window including elevated processes.
- The `holdMs` property controls how long a key is held down — useful for applications that distinguish between a tap and a long press.
- The config supports **JSON comments** and trailing commas, so you can annotate your config freely.
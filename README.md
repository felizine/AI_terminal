# AI Terminal

A context-driven personal assistant with a graphical Avalonia desktop shell. This repository contains the Phase 1 foundation: an adaptive chat workspace, a read-only daily briefing, local conversation persistence, and secure credential abstractions.

Phase 1 does **not** connect to a language model or external accounts. The chat responder explicitly identifies itself as an offline shell so it cannot be mistaken for a real assistant response.

## What is implemented

- **Adaptive desktop shell** — centered chat, notification rail, and a workspace that opens on request. Layouts adapt at wide, medium, and narrow widths; the chat and workspace share the screen on wide displays and swap on narrow ones.
- **Local conversation history** — SQLite storage with WAL mode, restore of the most recent conversation, streaming text updates, cancellation, and error handling.
- **Workspace samples** — Today, Tasks, Schedule, and Settings views use clearly labeled read-only sample data to validate the interaction model.
- **Secure credentials** — API keys and future OAuth secrets are delegated to the Linux Secret Service through `secret-tool`. No secret is stored in SQLite or configuration files.
- **Accessibility baseline** — keyboard send/submit, accessible names on icon buttons, high-contrast dark palette, and an explicit reduced-motion mode that disables animated indicators.
- **Composition and observability** — dependency injection, JSON/environment configuration, structured console logging, controlled startup, and clean shutdown.
- **Tests** — unit tests for conversation orchestration and SQLite persistence, plus credential-key validation.

## Requirements

- .NET 10 SDK (the solution targets `net10.0`; `global.json` permits rolling forward to a newer SDK)
- Linux with an X11 or Wayland session
- `libsecret` and `secret-tool` for system keyring access (optional; the app falls back to a clearly labeled unavailable store)
- Inter font is bundled through `Avalonia.Fonts.Inter`

On Arch Linux:

```bash
sudo pacman -S dotnet-sdk libsecret
```

Install a .NET 10 SDK if your repository offers one; otherwise use the Nix shell or the official .NET installer.

If only a newer .NET runtime is installed while targeting `net10.0`, framework-dependent debug runs need runtime roll-forward:

```bash
DOTNET_ROLL_FORWARD=Major dotnet run --project src/AITerminal.Desktop/AITerminal.Desktop.csproj
```

Self-contained publishes do not need a system runtime.

## Build and run

```bash
dotnet restore AI_terminal.sln
dotnet build AI_terminal.sln -c Debug
DOTNET_ROLL_FORWARD=Major dotnet run --project src/AITerminal.Desktop/AITerminal.Desktop.csproj
```

## Test

```bash
DOTNET_ROLL_FORWARD=Major dotnet test AI_terminal.sln -c Debug
```

Current test coverage is 9 tests across the Core and Infrastructure projects.

## Publish for Linux

```bash
dotnet publish src/AITerminal.Desktop/AITerminal.Desktop.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -o artifacts/linux-x64

./artifacts/linux-x64/AITerminal
```

The publish output is intentionally ignored by Git.

## Configuration

Configuration is read from `src/AITerminal.Desktop/appsettings.json` (copied next to the binary) and from environment variables prefixed with `AITERMINAL_`. Use double underscores for nested keys.

| Setting | Default | Purpose |
| --- | --- | --- |
| `Storage:DataDirectory` | `~/.local/share/AITerminal` | Directory for `ai-terminal.db` |
| `Accessibility:ReduceMotion` | `false` | Hide animated response indicators |
| `Logging:LogLevel:Default` | `Information` | Minimum console log level |

Example:

```bash
AITERMINAL_Storage__DataDirectory="$HOME/.local/share/AITerminal-dev" \
AITERMINAL_Accessibility__ReduceMotion=true \
DOTNET_ROLL_FORWARD=Major \
dotnet run --project src/AITerminal.Desktop/AITerminal.Desktop.csproj
```

Conversation history is stored in SQLite at `<DataDirectory>/ai-terminal.db`. Delete that directory to reset local history. Credentials are never written there.

## Solution structure

```text
AI_terminal.sln
├── src/
│   ├── AITerminal.Desktop/          # Entry point, DI composition, configuration, logging
│   ├── AITerminal.UI/               # Avalonia views, styles, view models
│   ├── AITerminal.Core/             # Conversation models and application services
│   └── AITerminal.Infrastructure/   # SQLite, secret-tool, offline responder
├── tests/
│   ├── AITerminal.Core.Tests/
│   └── AITerminal.Infrastructure.Tests/
├── PLAN.md                          # Product vision and delivery roadmap
└── shell.nix                        # Nix development environment
```

Dependency direction: `Desktop → UI + Infrastructure → Core`. UI and Infrastructure do not depend on each other.

## Phase 1 boundaries

The following are deliberately absent and belong to later phases:

- Real model providers (OpenAI-compatible and Ollama) and tool calling
- Google Calendar, Gmail, or any external account connection
- Task editing, calendar writes, or any external side effect
- Attachment upload, voice input, web research, and remote workers
- Settings forms for providers and connectors

Every future external write must show a preview and require explicit approval.

## Notes

- The product name is not final. `AI_terminal` remains the internal repository and project identity.
- The code targets .NET 10 LTS and Avalonia 12.
- Native 2D charts (ScottPlot) and HTML 3D artifacts are planned for Phase 8.

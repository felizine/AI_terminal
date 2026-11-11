# AI Terminal — Personal Assistant Application Plan

## 1. Product vision

Build a cross-platform AI personal assistant for executives, general managers, and personal productivity. Despite the current `AI_terminal` project name, the product will be a **graphical desktop and mobile application**, not a terminal emulator.

The application will be chat-centered and context-driven. Its default interface will remain simple, while relevant graphical workspaces appear when needed for tasks, calendars, email, projects, reports, charts, approvals, and remote jobs.

The assistant will eventually:

- Communicate through text and voice.
- Use local STT/TTS models on CPU, with GPU acceleration when available.
- Connect to hosted, neocloud, OpenAI-compatible, and local LLM providers.
- Perform cited web research.
- Manage email, tasks, and schedules, starting with Google Workspace.
- Detect scheduling conflicts and propose visual, explainable alternatives.
- Delegate approved work to another computer and continue long-running projects.
- Generate reports and native 2D and interactive 3D visualizations.
- Support proactive briefings, follow-ups, project coordination, and other personal-assistant workflows.

## 2. Current project assessment

The repository already contains:

- Avalonia 12.0.5.
- .NET 10 target framework.
- CommunityToolkit.Mvvm.
- A basic MVVM structure with one window, one view model, and a placeholder `TerminalService`.
- An optional Nix development shell.
- A clean Git working tree at the time of assessment.

The development machine has .NET SDK 11.0.100 installed. The application should initially remain on .NET 10 LTS.

There are currently no application tests, AI integrations, persistence, external-service connectors, or voice capabilities.

Avalonia is appropriate for the cross-platform goal. Separate packages will still be published for each operating system and architecture, while most UI and business logic remains shared. Arch Linux is currently an upstream Tier 3 platform, so testing on the development machine is particularly important.

## 3. Core design principles

1. **Chat-centered, not chat-only** — conversation is the primary command interface, but structured graphical UI is used when it communicates better than text.
2. **Stable shell, adaptive workspace** — navigation and chat remain predictable; only the relevant content workspace changes.
3. **Progressive disclosure** — show a compact summary first and a complete workspace only when the user asks for detail.
4. **Constrained generative UI** — the model selects and populates trusted UI components; it does not generate arbitrary AXAML or executable UI code.
5. **Approval before side effects** — reads may run automatically, but every write or external side effect requires explicit approval initially.
6. **Provider independence** — core assistant logic must not depend on one model vendor.
7. **Local-first where practical** — local storage and voice processing, with clear indicators when content is sent to a cloud provider.
8. **Accessibility and fallback** — information must remain understandable without animation, color, or GPU acceleration.
9. **Modular monolith first** — avoid microservices and multi-agent complexity until real workflows require them.

## 4. User experience

### 4.1 Default interface

The default screen contains:

- A central chat area and composer.
- A compact notification area containing:
  - Upcoming calendar events.
  - Important emails.
  - Today's tasks.
  - Project progress.
  - Remote-job status.
- Access to settings and connected services.

On desktop, notifications can occupy a narrow right-hand rail. On mobile, they can appear as compact cards or a notification inbox.

The application should not open as a dense, permanent dashboard.

### 4.2 Adaptive workspace

A user request can cause a relevant component to appear:

| Context | Initial component | Detailed workspace |
| --- | --- | --- |
| Daily planning | `TodaySummary` | Tasks, calendar, conflicts, and focus time |
| Email | `EmailDigest` | Threads, drafts, and follow-ups |
| Projects | `ProjectProgress` | Milestones, blockers, activity, and artifacts |
| Rescheduling | `RescheduleProposal` | Timeline, conflicts, alternatives, and approval |
| Research | `ResearchSummary` | Sources, findings, comparisons, and reports |
| Data analysis | `ChartArtifact` | Interactive chart and report workspace |
| Remote work | `JobStatus` | Logs, diffs, tests, artifacts, and controls |

On wide desktop screens, chat can narrow while the workspace opens beside it. On mobile, the detailed workspace becomes a full-screen page, with chat available through navigation or a bottom sheet.

### 4.3 Example: daily planning

User:

> What are my tasks and plans for today?

The assistant displays a short response and a compact summary:

```text
Today
├── 3 meetings
├── 5 tasks — 2 high priority
├── 1 schedule conflict
└── 2 hours available for focused work

[View schedule] [View tasks]
```

If the user asks for detail, the summary expands into a complete task and calendar workspace.

### 4.4 Example: rescheduling with a conflict

User:

> Move my project-planning task to Friday at 2 PM.

The assistant checks the calendar but does not immediately modify it. It presents a proposal:

```text
Project planning
Thursday 10:00 ────────────→ Friday 14:00
                                  │
                                  ▼
                       Conflict: Weekly review
                       Friday 13:30–14:30

Suggested alternatives:
✓ Friday 15:00–16:00
✓ Monday 09:00–10:00
✓ Move Weekly review instead

[Apply Friday 15:00] [Choose another time] [Cancel]
```

The UI can animate the transition from the old date to the requested date, highlight the conflicting region, and reveal alternatives. It must also provide icons and textual explanations so the conflict is understandable without color or motion.

Only an explicit approval applies the selected change.

## 5. Constrained generative UI architecture

The assistant should return structured UI concepts rather than arbitrary interface markup. Initial component contracts include:

- `TodaySummary`
- `TaskList`
- `ScheduleTimeline`
- `ScheduleConflict`
- `RescheduleProposal`
- `EmailDigest`
- `ProjectProgress`
- `ResearchSummary`
- `ChartArtifact`
- `JobStatus`
- `ApprovalCard`

Avalonia maps these contracts to predefined views and view models. The model can choose the relevant component and provide data, while the application controls layout, behavior, validation, permissions, and accessibility.

Benefits:

- Consistent visual design.
- Predictable cross-platform behavior.
- Testable interactions.
- Reduced risk from malformed model output.
- No execution of model-generated UI code.

## 6. Proposed solution structure

```text
AI_terminal.sln
├── src/
│   ├── AITerminal.Desktop/          # Linux/Windows/macOS entry point
│   ├── AITerminal.UI/               # Shared Avalonia views and view models
│   ├── AITerminal.Core/             # Conversations, tasks, tools, and policies
│   └── AITerminal.Infrastructure/   # LLM, Google, search, voice, and SQLite
├── tests/
│   ├── AITerminal.Core.Tests/
│   └── AITerminal.Infrastructure.Tests/
└── src/AITerminal.Worker/           # Added later for remote execution
```

The existing `AI_terminal` name can remain as an internal repository or project name. A public product name can be selected independently; an immediate rename is not required.

## 7. Technical direction

| Area | Initial choice |
| --- | --- |
| UI | Avalonia and CommunityToolkit.Mvvm |
| Runtime | .NET 10 LTS |
| LLM abstraction | `Microsoft.Extensions.AI.IChatClient` |
| Initial providers | Configurable OpenAI-compatible endpoint and Ollama |
| Complex workflows | Microsoft Agent Framework only when required |
| External tool ecosystem | Official MCP C# SDK behind local security policies |
| Persistence | SQLite |
| Secrets | Linux Secret Service/keyring, then other OS credential stores |
| STT | `whisper.cpp`, English, CPU baseline with Vulkan/GPU option |
| TTS | `sherpa-onnx` with an English local model |
| 2D charts | ScottPlot for Avalonia |
| 3D charts | Interactive Plotly HTML artifacts initially |
| Remote execution | Authenticated, non-root worker with explicit job capabilities |

OpenAI-compatible provider settings should include endpoint, API key, model, streaming support, and tool-calling capability. Native provider adapters can be added when a service has incompatible or specialized features.

## 8. Delivery roadmap

### Phase 1 — Foundation and adaptive shell

- Split the desktop host from shared Avalonia UI.
- Add dependency injection, configuration, structured logging, and controlled shutdown.
- Build the chat, notification rail, and dynamic workspace.
- Add responsive layouts for wide, medium, and narrow screens.
- Add reduced-motion behavior and accessible contrast.
- Add SQLite persistence and OS-backed secret storage.
- Add test projects and verify Linux debug and published builds.

### Phase 2 — First vertical slice: Today, tasks, and schedule

A vertical slice is a small feature completed through every application layer, from user interaction to services and back to the GUI.

The first vertical slice is now **Today, tasks, and schedule**, rather than generic chat, web, and Google.

1. Add streaming chat with OpenAI-compatible and Ollama providers.
2. Add local tasks and read-only Google Calendar and Gmail.
3. Implement the `TodaySummary` compact component.
4. Implement detailed task and calendar workspaces.
5. Detect task/calendar conflicts.
6. Implement `RescheduleProposal` with alternatives and visual explanation.
7. Require approval before applying a calendar or task change.

First milestone example:

> Prepare me for today.

The application shows today's meetings, important email, tasks, conflicts, pending follow-ups, and suggested focus blocks. It does not modify external data without approval.

### Phase 3 — Tool and approval system

Classify tools by risk:

- **Read:** search, read calendar, read selected email.
- **Draft:** prepare emails, events, reports, or code changes without applying them.
- **Write:** create drafts, add events, or modify approved files.
- **High impact:** send email, delete data, run commands, deploy, or make purchases.

Include:

- Exact argument and content previews.
- Audit history.
- Cancellation, timeouts, and controlled retries.
- Workspace and domain allowlists.
- No credentials in model context.
- Treatment of email and web content as untrusted data, never as higher-priority instructions.

### Phase 4 — Email and project workspaces

- Email digest and thread summaries.
- Reply drafting in the user's style.
- Separate approval for creating a draft and sending it.
- Project milestones, owners, blockers, and progress.
- Commitment detection and follow-up tracking.

### Phase 5 — Cited web research

- Pluggable search providers such as SearXNG, Brave, or Tavily.
- Page retrieval with timeout, size, redirect, and content-type limits.
- Source citations and clear separation of evidence and model conclusions.
- Browser automation only when normal retrieval is insufficient.
- Research summaries, comparisons, and report artifacts.

### Phase 6 — English local voice

- Push-to-talk before always-on wake-word detection.
- CPU-first local STT and TTS.
- Optional Vulkan/GPU acceleration where useful and available.
- Editable transcript before submission.
- Interruptible spoken responses.
- Managed model downloads, storage locations, and integrity checks.
- Replaceable STT/TTS backends for benchmarking.

GPU acceleration is generally more valuable for STT than lightweight English TTS.

### Phase 7 — Remote-machine project worker

- Install a small worker on the target computer.
- Run it as a non-root user.
- Use authenticated HTTPS, optionally through an SSH tunnel initially.
- Restrict jobs to approved project directories and capabilities.
- Send structured jobs rather than unrestricted model-generated shell strings.
- Stream status, logs, diffs, test results, and artifacts.
- Persist job state so future requests continue the same project.
- Require approval for writes, dependency installation, publishing, and destructive operations.

### Phase 8 — Reports, charts, and visual polish

- Generate typed chart specifications rather than arbitrary UI code.
- Render 2D charts natively using ScottPlot.
- Generate interactive Plotly HTML for 3D scatter, surface, and mesh charts.
- Support Markdown/HTML reports, then PDF export.
- Include sources, timestamps, units, and assumptions.
- Add hardware-accelerated transitions and effects without making them necessary for comprehension.

### Phase 9 — Additional platforms

1. Stabilize Arch Linux desktop.
2. Add Windows and macOS desktop hosts.
3. Add Android/iOS with platform-specific voice, notification, and credential services.
4. Consider a browser version only after deciding which capabilities require a backend.

## 9. Security and privacy requirements

- Use minimum necessary OAuth scopes, beginning with read-only Google access.
- Use Google desktop OAuth with PKCE and loopback redirect.
- Store tokens and provider keys in OS credential storage, not source control or plaintext SQLite.
- Never expose OAuth tokens or API keys to the model.
- Show whether a request uses local or cloud inference.
- Redact secrets and sensitive content from logs.
- Require explicit approval for every external side effect initially.
- Treat MCP servers as separately trusted integrations, not automatically safe tool sources.
- Constrain web retrieval and remote workers to prevent prompt injection and unauthorized actions.
- Keep payments, purchases, legal commitments, and bookings as preview-and-confirm operations.

## 10. Responsive design and graphics validation

Test at minimum:

- Narrow mobile-like width.
- Approximately 800 px, 1280 px, and 1920 px desktop widths.
- 100%, 150%, and 200% display scaling.
- Multiple monitors and window restoration.
- X11/XWayland on Arch first; evaluate native Wayland separately when upgrading Avalonia.
- Keyboard navigation and reduced-motion mode.
- Software-rendering fallback where practical.

Avalonia can use accelerated rendering on supported systems, but advanced 3D or specialized graphical workspaces may require a dedicated rendering component later.

## 11. Testing strategy

- Unit-test task, scheduling, conflict-detection, and approval policies.
- Use fake model providers and connectors for deterministic tests.
- Test UI component contracts with valid and malformed model output.
- Integration-test Google against a dedicated test account after mocked HTTP tests.
- Test provider switching without changing core logic.
- Verify that no write tool executes without recorded approval.
- Test offline Ollama behavior and clear errors for unavailable online services.
- Perform Arch Linux smoke tests for startup, chat, window resizing, persistence, and publishing.

## 12. Additional assistant capabilities

Prioritized after the core workflows:

1. Daily and weekly executive briefings.
2. Meeting agendas, participant research, transcription, minutes, decisions, and action items.
3. Commitment and unanswered-follow-up tracking.
4. Local-file and Google Drive knowledge search with citations.
5. Project coordination, milestones, blockers, and recurring status reports.
6. Vendor, competitor, product, and hiring comparisons.
7. Notification prioritization and focus-time protection.
8. Contact/CRM history and relationship reminders.
9. Expense receipt collection and report preparation.
10. Subscription, renewal, bill, and document-deadline reminders.
11. Travel research, itineraries, and time-zone planning.
12. Event planning, reservations, gifts, and personal appointments.
13. Draft documents, presentations, memos, and standard operating procedures.
14. Recurring scheduled workflows with run history.

## 13. Immediate implementation target

The next implementation increment should contain only:

1. The adaptive application shell.
2. Responsive chat and dynamic workspace behavior.
3. Streaming provider settings and chat.
4. Local tasks plus read-only Google Calendar/Gmail.
5. The daily-summary snippet and detailed task/calendar workspace.
6. Conflict-aware reschedule proposals with explicit approval.

Web research, voice, remote execution, and advanced graphics should follow after this workflow is reliable.

## 14. Research references

- [Avalonia supported platforms](https://docs.avaloniaui.net/docs/supported-platforms)
- [Avalonia responsive layouts](https://docs.avaloniaui.net/docs/layout/responsive-layouts)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai)
- [Official MCP C# SDK](https://csharp.sdk.modelcontextprotocol.io/v2/concepts/getting-started.html)
- [Google OAuth for desktop applications](https://developers.google.com/identity/protocols/oauth2/native-app)
- [whisper.cpp](https://github.com/ggml-org/whisper.cpp)
- [sherpa-onnx](https://github.com/k2-fsa/sherpa-onnx)
- [ScottPlot Avalonia package](https://www.nuget.org/packages/ScottPlot.Avalonia)
- [Plotly.NET 3D charts](https://plotly.net/3D-charts/3d-scatter-plots.html)
- [Executive-assistant responsibilities](https://hr.unc.edu/wp-content/uploads/sites/1483/2026/04/position-description-template-executive-assistant.pdf)

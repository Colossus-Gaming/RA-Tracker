# Tasks

This file tracks feature requests, bug fixes, and research tasks for the RetroAchievements Layout Manager. Each story follows a consistent format to prevent duplicate work and maintain traceability to documentation and code.

## How This File Works

- **Stories** start with "As a user..." phrasing and get translated into concrete tasks below.
- **Status** uses: `backlog`, `in-progress`, `blocked`, `done`.
- **Attempts** log what was tried and the outcome, so work is never repeated.
- **Docs** and **Code** columns link to the relevant reference material and source files.

---

## Active Stories

### STORY-001: Subset Achievement Notifications

> As a user, I want to see different notifications for Core vs Bonus vs Specialty vs Exclusive achievements so I can visually distinguish which set an unlock belongs to during a stream.

**Status:** backlog
**Created:** 2026-02-27
**Plan:** [subset-notifications-plan.md](docs/plans/subset-notifications-plan.md)

| Phase | Task | Status | Docs | Code | Attempts |
|-------|------|--------|------|------|----------|
| 1 | Extend Achievement model with SetType, SetId, SetName | backlog | [achievement-sets (v2)](docs/v2/achievement-sets.md), [achievements (v2)](docs/v2/achievements.md) | `Models/Achievement.cs`, `Models/AchievementSet.cs` | — |
| 2 | Add sub-set notification properties to AlertsViewModel | backlog | [player-achievement-sets (v2)](docs/v2/player-achievement-sets.md) | `ViewModels/AlertsViewModel.cs` | — |
| 3 | Update AlertsOverlay for sub-set handling | backlog | — | `Views/AlertsOverlay.xaml`, `Views/AlertsOverlay.xaml.cs` | — |
| 4 | Update MainViewModel event routing | backlog | — | `ViewModels/MainViewModel.cs` | — |
| 5 | Add AppSettings persistence for sub-set prefs | backlog | — | `Models/AppSettings.cs` | — |
| 6 | Wire MainWindow to AlertsOverlay | backlog | — | `MainWindow.xaml.cs` | — |
| 7 | Add UI for sub-set notification settings | backlog | — | `MainWindow.xaml` | — |

**Notes:**
- V2 API now provides `PlayerAchievementSet` resource for per-subset progress — see [v2-status](docs/guides/v2-status.md).
- Query pattern: `GET /api/v2/users/{user}/player-achievement-sets?filter[gameId]={id}&include=achievementSet`
- Feature is opt-in (disabled by default). Achievements without SetType default to Core.

---

### STORY-002: Documentation Consolidation

> As a user, I want organized, non-redundant API documentation so I can quickly find endpoint details and integration guidance.

**Status:** done
**Created:** 2026-02-27
**Completed:** 2026-02-27

| Task | Status | Docs | Attempts |
|------|--------|------|----------|
| Consolidate overlapping nav/index files into single README | done | [docs/README.md](docs/README.md) | Merged INDEX.md, 00_bundle_readme.md, getting-started.md, api-integration.md into one file. Removed 4 files, moved 3 guides into docs/guides/ with clean names. |
| Research and document v2 API | done | [v2/README.md](docs/v2/README.md), [v2-status](docs/guides/v2-status.md) | Researched via RAWeb PRs and discussions. Created 11 resource docs in docs/v2/. Updated v2-status.md to reflect production status. |

---

### STORY-003: Replace Placeholder Text with Icons

> As a user, I want the ?? text in all of the controls to be changed to icons that match the button or frame they're a part of.

**Status:** done
**Created:** 2026-02-27
**Completed:** 2026-02-27

Used **Segoe MDL2 Assets** (zero-dependency, built into Windows 10/11) for all icons. Navigation buttons (Start/Stop/Back/Prev/Next) use standard Unicode symbols (▶ ■ ← ◀) that render in any font. Section headers use `<Run FontFamily="Segoe MDL2 Assets"/>` for inline icons. Overlay buttons use `<StackPanel>` with icon + text `<TextBlock>` elements. IconButton style was updated with `FontFamily="Segoe MDL2 Assets"` so all gear buttons render automatically.

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Choose icon approach | done | — | Segoe MDL2 Assets (zero-dep, built into Windows). No NuGet needed. |
| Replace dashboard section headers | done | `MainWindow.xaml` | User Info (E77B), Current Game (E7FC), Current Focus (E81D), Overlay Windows (E78B) |
| Replace overlay button grid | done | `MainWindow.xaml` | 8 main buttons with StackPanel icons + 8 gear IconButtons (E713) |
| Replace settings page headers | done | `MainWindow.xaml` | All 9 page headers use matching MDL2 icons via Run elements |
| Replace settings section headers | done | `MainWindow.xaml` | Window Size (E740), Badge (E7C1), Font (E8D2), Visibility (E890), Container (E71D), Separator (E738), Display (E762) |
| Replace navigation buttons | done | `MainWindow.xaml` | Start=▶, Stop=■, Back=←, Prev=◀, Next=▶ (standard Unicode) |
| Replace test buttons | done | `MainWindow.xaml` | Core star (E735), Bonus star (E734), bell (EA8F) for test alerts |
| Replace action buttons | done | `MainWindow.xaml` | Folder icon (E8B7) for stream-labels button |
| Replace achievement list locked indicator | done | `Views/AchievementListOverlay.xaml` | Lock emoji (U+1F512) / checkmark (U+2714) |

---

### STORY-004: RA Guides Integration

> As a user, I want to have a "RA Guides" link for the game/set I'm working on so that it's easier to follow. It should also have its own viewer on the main control (separate tab) so that I can view the guide and have a link in the Tracker.

**Status:** done
**Created:** 2026-02-27
**Completed:** 2026-02-27

Added `Microsoft.Web.WebView2` (1.0.3800.47) NuGet package. Created a full-page GuidesPage with WebView2 browser, header bar (back, refresh, open-in-browser buttons), and game title display. Dashboard's Current Game card now has "View Guide" and "Open in browser" buttons (visible when a game is loaded). WebView2 auto-navigates to `https://retroachievements.org/game/{gameId}` when CurrentGame changes while the Guides page is visible. No separate ViewModel needed — URL construction is handled in code-behind via `GetGuideUrl()`.

| Task | Status | Docs | Code | Attempts |
|------|--------|------|------|----------|
| Add WebView2 NuGet package | done | — | `*.csproj` | Added Microsoft.Web.WebView2 1.0.3800.47 via `dotnet add package` |
| Add Guides page to MainWindow | done | — | `MainWindow.xaml` GuidesPage Grid | Full page with header bar + WebView2 control |
| URL construction for game guides | done | — | `MainWindow.xaml.cs` GetGuideUrl() | Inline in code-behind; no separate ViewModel needed |
| Embed WebView2 control | done | — | `MainWindow.xaml` wv2:WebView2 | Lazy init via EnsureCoreWebView2Async on first navigate |
| Add "View Guide" button to Dashboard | done | — | `MainWindow.xaml` Current Game card | View Guide + Open in browser buttons, visible when HasGameInfo |
| Auto-navigate on game change | done | — | `MainWindow.xaml.cs` OnViewModelPropertyChanged | Subscribes to PropertyChanged, updates when CurrentGame changes and GuidesPage is visible |
| External browser fallback | done | — | `MainWindow.xaml.cs` OpenGuideInBrowser_Click | Process.Start with UseShellExecute=true |

---

### STORY-005: Session-Based V2 Auth (Cloudflare Bypass)

> As a user, I want the app to automatically log me into RetroAchievements behind the scenes so V2 API calls work through Cloudflare, with V1 fallback if V2 is blocked.

**Status:** done
**Created:** 2026-02-27
**Completed:** 2026-03-01

WebView2-based login flow: starts minimized, auto-fills credentials via JavaScript, auto-submits the form, extracts session + cf_clearance cookies, navigates to /settings to extract the API key from Inertia.js page props. Falls back to visible window if auto-login fails. V2Client uses session cookies for Cloudflare bypass + X-API-Key header for API auth. ServiceFactory creates session-aware or API-key-only services. UI shows V2 active (green) or V1 fallback (gold) status indicator.

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Behind-the-scenes WebView2 login | done | `Views/LoginWindow.xaml`, `Views/LoginWindow.xaml.cs` | Starts minimized, auto-fills + auto-submits. 15s timeout shows visible window. |
| Cookie extraction + sharing | done | `Views/LoginWindow.xaml.cs`, `Services/SessionService.cs` | Extracts from WebView2 CookieManager → CookieContainer. Shares User-Agent for cf_clearance. |
| API key extraction from settings page | done | `Views/LoginWindow.xaml.cs` | Navigates to /settings, extracts from Inertia.js `data-page` JSON props (3 fallback methods). |
| V2Client session cookie support | done | `Http/V2/V2Client.cs` | CookieContainer constructor, shared User-Agent, dual auth (cookies + API key header). |
| ServiceFactory session awareness | done | `Services/ServiceFactory.cs` | `HasSessionAuth` property, cookie constructor for V2Client. |
| V2 primary / V1 fallback with UI indicators | done | `ViewModels/MainViewModel.cs`, `MainWindow.xaml` | ApiStatusText, IsUsingV1Fallback properties. Green/gold status indicator. |
| Fix header layout (cut-off text) | done | `MainWindow.xaml` | Two-row header: credentials top, status bottom. Narrower input widths. |
| Hook up all overlay labels during polling | done | `MainWindow.xaml.cs` | `OnViewModelPropertyChanged` + `PushGameDataToOverlays()` pushes to all open overlays. |
| Password save/restore with DPAPI | done | `Models/AppSettings.cs`, `Services/SettingsService.cs`, `MainWindow.xaml.cs` | Encrypted password storage, Remember checkbox, auto-restore on launch. |
| Diagnostic logging throughout | done | `App.xaml.cs`, `ViewModels/MainViewModel.cs`, `Services/AchievementTrackingService.cs`, `Views/LoginWindow.xaml.cs` | `Debug.WriteLine` with tagged prefixes: [App], [MainViewModel], [LoginWindow], [Settings], etc. |

---

### STORY-006: FlaUI Integration Tests

> As a user, I want automated integration tests that launch the app, interact with UI, and capture diagnostic logs so development feedback is faster.

**Status:** done
**Created:** 2026-03-01
**Completed:** 2026-03-01

FlaUI.UIA3 tests that launch the WPF app, interact via Windows UI Automation (no cursor/focus stealing), and capture `Debug.WriteLine` output to a log file via `--log-to-file` CLI argument. AutomationProperties.AutomationId added to key XAML elements for reliable element discovery.

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Add `--log-to-file` CLI arg to App.xaml.cs | done | `App.xaml.cs` | TextWriterTraceListener with FileShare.Read so tests can read while app writes. |
| Add AutomationProperties to key XAML elements | done | `MainWindow.xaml` | UsernameTextBox, PasswordBox, StartStopButton, SessionStatusText, ApiStatusText, TestV2Button. |
| Add FlaUI.UIA3 NuGet package | done | `Retro Achievement Tracker.Tests.csproj` | FlaUI.UIA3 4.0.0 |
| Create AppLaunchTests integration test suite | done | `IntegrationTests/AppLaunchTests.cs` | 3 tests: launch + elements, credentials + start, session status. All pass (19s). |

---

## Backlog

_New stories are added here when captured from "As a user..." requests. Move to Active Stories when work begins._

---

## Completed

_Stories move here when all tasks are done._

- **STORY-002** — Documentation Consolidation (2026-02-27)
- **STORY-003** — Replace Placeholder Text with Icons (2026-02-27)
- **STORY-004** — RA Guides Integration (2026-02-27)
- **STORY-005** — Session-Based V2 Auth (2026-03-01)
- **STORY-006** — FlaUI Integration Tests (2026-03-01)

---

## Conventions

- **Story IDs** increment sequentially: STORY-001, STORY-002, etc.
- **Code paths** are relative to `Retro Achievement Tracker.WPF/` unless otherwise noted.
- **Docs paths** are relative to the repo root.
- **Attempts** should always record: what was tried, what happened, and whether it worked. This prevents re-trying failed approaches.
- When a task is blocked, add a note explaining what's blocking it and what unblocks it.

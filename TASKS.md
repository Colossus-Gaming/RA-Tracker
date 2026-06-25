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

**Status:** in-progress
**Created:** 2026-02-27
**Updated:** 2026-06-24
**Plan:** [subset-notifications-plan.md](docs/plans/subset-notifications-plan.md)

| Phase | Task | Status | Docs | Code | Attempts |
|-------|------|--------|------|------|----------|
| 1 | Extend Achievement model with SetType, SetId, SetName | done | [achievement-sets (v2)](docs/v2/achievement-sets.md), [achievements (v2)](docs/v2/achievements.md) | `Models/Achievement.cs`, `Models/AchievementSet.cs` | Model already carried the fields. |
| 2 | Tag achievements with set membership in the API mapping pipeline | done | [v2-status](docs/guides/v2-status.md) | `Services/V2ProgressService.cs` | 2026-05-29: `MapToUserGameProgress` now reads each set's achievements, tags SetId/SetType/SetName, and falls back to a flat list when no sets are returned. |
| 3 | Group tagged achievements into real per-set lists (remove core-only stub) | done | — | `Services/AchievementTrackingService.cs` | 2026-05-29: `CreateGameInfoFromProgress` groups by SetId; removed `IsAchievementInSet` stub that dumped everything into Core. |
| 4 | Per-subset notification routing (set name shown on Focus/Alerts) | done | — | `ViewModels/FocusViewModel.cs`, `ViewModels/MainViewModel.cs` | Set name flows to Focus/Alerts via `SelectedSetName`; test alert commands cover Core + subset. |
| 5 | Distinct visual treatment per set type in AlertsOverlay | done | — | `ViewModels/AlertsViewModel.cs`, `Views/AlertsOverlay.xaml` | 2026-06-24: `AlertsViewModel` now carries `SetType`/`SetName` and exposes `EffectiveBorderColor` (per-set accent: Bonus=amethyst, Specialty=sky, Exclusive=red, Challenge=amber) + a corner `SetBadge` ("BONUS"/"SPECIALTY"/…). Core/Unknown keep the standard look; mastery clears any stale subset accent. 10 new tests in `ViewModelTests/AlertsViewModelSubsetTests.cs`; 301 unit tests pass. |
| 6 | AppSettings persistence for sub-set notification prefs + settings UI | backlog | — | `Models/AppSettings.cs`, `MainWindow.xaml` | Opt-in per-set toggles + per-set accent overrides not yet added. Phase-5 accents are currently hard-coded defaults; phase 6 should make them configurable and add an opt-in toggle. |

**Notes:**
- **API delta research (2026-06-24):** Re-surveyed the v2 API since the 2026-05-29 verification (see [v2/README.md](docs/v2/README.md) timeline). Net for subsets: **no structural change required** — phase 5 shipped with the existing data. New since baseline: `PlayerAchievement` ([#4633](https://github.com/RetroAchievements/RAWeb/pull/4633), merged 2026-03-26) adds per-set *unlock* filtering (`filter[achievementSetId]`, `filter[unlockedFrom/To]`) → see STORY-011. The core gap is **still open**: no per-set achievement *definition* listing (`/games/{id}/achievements`, `/achievement-sets/{id}/achievements`, `filter[achievementSetId]` on the bare `/achievements` index all 404/400). `AchievementSetVersion` ([#4979](https://github.com/RetroAchievements/RAWeb/pull/4979)) is OPEN — watch for merge.
- The blocking gap (no per-achievement set membership through the pipeline) is now resolved; achievements carry `SetId`/`SetType`/`SetName` end-to-end on the V2 path.
- **Public-API reality (2026-05-29 research):** the **v1** Web API exposes *no* subset model — subsets are separate game IDs linked via a "Subsets" hub. Subset tracking therefore depends on the session-gated **v2** path; on V1 fallback a game collapses to a single (Core) set. See [v2-status](docs/guides/v2-status.md).
- Query pattern (v2): `GET /api/v2/users/{user}/player-achievement-sets?filter[gameId]={id}&include=achievementSet`
- Verify the live v2 response shape with env-var creds + API logging (see STORY-007); the mapper is defensive about field names because the v2 contract is not publicly published.

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

### STORY-007: Environment-Variable Credentials

> As a developer, I want to supply my username, Web API key, and password via environment variables so I can keep testing v2 without re-entering credentials in the UI.

**Status:** done
**Created:** 2026-05-29
**Completed:** 2026-05-29

Reads `RA_USERNAME`, `RA_API_KEY`, `RA_PASSWORD`. When a variable is set (non-empty), it takes precedence over the settings file; values are kept in memory only and never persisted. Used by both the app and the test suite.

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Add `EnvironmentCredentials` helper | done | `Services/EnvironmentCredentials.cs` | RA_USERNAME / RA_API_KEY / RA_PASSWORD, trimmed, blank treated as unset. |
| Apply env override in ViewModel load | done | `ViewModels/MainViewModel.cs` | `ApplyEnvironmentCredentialOverrides()` runs inside the settings-loading phase (no persistence). |
| Populate PasswordBox from env-provided password | done | `MainWindow.xaml.cs` | `RestorePasswordToPasswordBox` prefers the ViewModel password (env or settings). |
| Tests | done | `ServiceTests/EnvironmentCredentialsTests.cs`, `ViewModelTests/MainViewModelEnvironmentTests.cs` | Save/restore process env vars; assert precedence + CanStart. |

---

### STORY-008: Test Suite Overhaul

> As a developer, I want the unit tests to exercise the real app code (not duplicate stand-ins) and to all pass, so I can develop confidently.

**Status:** done
**Created:** 2026-05-29
**Completed:** 2026-05-29

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Fix 6 stale failing tests | done | `V2ApiTests/ServiceFactoryTests.cs`, `V2ApiTests/ProgressServiceTests.cs`, `ViewModelTests/MainViewModelMultiSetTests.cs` | FeatureFlag defaults now V2-on; API key optional; mastery via list fallback; clean-construct ViewModel. |
| Delete legacy duplicate-model tests | done | (removed) `ModelsAndServicesTests.cs` | It tested fake copies of the models + nonexistent `CredentialProtector`/`NotificationRequest`. |
| Add real-model + service + converter tests | done | `ModelTests/*`, `ServiceTests/*`, `ConverterTests/*`, `V2ApiTests/V2ProgressServiceSubsetTests.cs` | Subset grouping, env creds, DPAPI encryption, converters, progress fallback. |
| Add `MainViewModel(bool loadSampleData)` for deterministic VM tests | done | `ViewModels/MainViewModel.cs` | Tests skip placeholder sample data. |

**Baseline:** 278 unit tests pass (`Category!=Integration`); FlaUI launch smoke test passes.

---

### STORY-009: Make Live Data Actually Flow (v1 mapping + window UX)

> As a user, I want the app to actually populate with my real data and behave like normal windows, because nothing was wiring up.

**Status:** done
**Created:** 2026-05-29
**Completed:** 2026-05-29

Live testing revealed two blockers. **(1) Every `/api/v2/*` endpoint returns 404** — v2 isn't publicly deployed — so the app relies entirely on v1. **(2)** The v1 client deserialized JSON straight into domain models whose names didn't match (`GameID`≠`Id`, `User`≠`UserName`), and game-progress returns achievements as a dictionary — producing a blank username and current game id `0`, which broke the whole poll.

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Diagnose via automated launch→login→poll→log harness | done | settings `autoStart`+`enableApiLogging` | Confirmed v2 404 on all endpoints; v1 returned game id 0 / blank user. |
| Build v1 DTO + mapper layer (real field names, badge URLs, dict achievements) | done | `Http/V1/V1ApiModels.cs` | DTOs match captured `docs/testing/*.json`. |
| Rewire V1 client/service through the mapper | done | `Services/HybridProgressService.cs` | Now: `RetroS3xual` / game `10268` / overlays populated. |
| Window UX: taskbar + movable + not force-topmost | done | `Views/*Overlay.xaml(.cs)` | `ShowInTaskbar=True`; drag ungated on Focus/Alerts; removed Topmost↔PositionMode coupling. |
| Immediate first poll + clear placeholder + auto-launch on manual Start | done | `ViewModels/MainViewModel.cs`, `MainWindow.xaml.cs` | |
| Tests for v1 mapping | done | `V1ApiTests/V1MapperTests.cs` | 7 tests against real shapes. |

**Note:** the v2 404s in this story were later traced to the wrong host/prefix — see STORY-010. v2 is live.

---

### STORY-010: Wire Up the Real v2 API (api. subdomain)

> As a user, I want the app to actually use the live v2 API, not just fall back to v1.

**Status:** done
**Created:** 2026-05-29
**Completed:** 2026-05-29

Investigated RAWeb source ([`app/Api/RouteServiceProvider.php`](https://github.com/RetroAchievements/RAWeb/blob/master/app/Api/RouteServiceProvider.php)) + live probing. The "v2 is 404/DOA" conclusion was wrong — the app was calling the wrong host/prefix.

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Fix base URL to `api.retroachievements.org/v2` | done | `Http/V2/V2Client.cs` | Was `retroachievements.org/api/v2` (404). Now 200. |
| Fix routes: `player-games`, `player-achievements`; drop `/progress`; drop nested `game.system` include | done | `Services/V2ProgressService.cs` | Verified live: user/recent/current-game all 200. |
| Read game id from `game` relationship (player-games id is a record id) | done | `V2ResourceMapper`/`V2ProgressService` | Fixed "current game 7557786 → 10268". |
| Map `player-achievements` shape (`unlockedAt`, achievement relationship) + user `pointsHardcore` | done | `Services/V2ProgressService.cs`, `Http/V2/Mappers/V2ResourceMapper.cs` | |
| Hybrid: v2 for user/recent/current-game/sets, v1 for full per-game achievement list | done | `Services/V2ProgressService.cs` (returns null when no achievements → v1 fallback) | |
| Add response-body diagnostic logging; update tests + docs | done | `Http/V2/V2Client.cs`, `V2ApiTests/*`, `docs/v2/*`, `docs/guides/v2-status.md` | 285 tests pass. |

**Follow-on (2026-05-29, same day):** Probed RAWeb shapes to make this efficient.
- Discovered `/v2/games/{id}/achievements` and `/v2/achievement-sets/{id}/achievements` both **404** — v2 has no single-call source for "all achievements of a game." v1 `GetGameInfoAndUserProgress` is therefore the efficient per-game achievement list.
- Rewired `HybridProgressService.GetUserGameProgressAsync` to **v1 (achievement list) + v2 `player-achievement-sets` (subset aggregates) in parallel**; removed the wasteful v2 `player-games?filter[gameId]` call (always empty achievements).
- Added `V2ProgressService.GetPlayerAchievementSetsAsync` which maps `setContext.type` → SetType and the included `achievement-set.achievementsPublished` → totals.
- Fixed v2 ISO timestamp parsing to honor `Z` (`DateTimeStyles.RoundtripKind`).
- Added a reusable v2 shape probe: `--probe-v2 "<paths;...>"` (no login required).
- 287 tests pass.

**Multiset wiring completed (same day, after further probing):** Discovered `filter[achievementSetId]` is **rejected (400)** — but `/v2/achievements?filter[gameId]&include=achievementSet&page[size]=100` returns every game achievement with its set membership in **one call**. `V2ProgressService.GetGameAchievementsWithSetsAsync` uses it; `HybridProgressService` phase 2 now makes **one** call regardless of non-core set count (was N), filters to non-core, and applies unlock dates from the phase-1 unlocks map. Achievements end up tagged with `SetId`/`SetType`/`SetName`; the existing `CreateGameInfoFromProgress` grouping then populates `GameInfo.AchievementSets` for the multi-set UI. Net: **4 calls single-set, 5 calls multiset (independent of set count).** 288 tests pass.

---

### STORY-011: Per-Set Incremental Unlock Feed (v2 PlayerAchievement)

> As a streamer, I want the tracker to detect new unlocks per subset efficiently so multiset games stay responsive and correctly attributed without re-pulling the whole game's unlocks each poll.

**Status:** in-progress
**Created:** 2026-06-24
**Updated:** 2026-06-24
**Source:** API delta research 2026-06-24 — `PlayerAchievement` ([#4633](https://github.com/RetroAchievements/RAWeb/pull/4633), merged 2026-03-26).

**Why now:** `#4633` is the first v2 lever for *per-set unlock* scoping. Turned out to be a **correctness fix**, not just an optimization: subset achievements were showing 0 earned because their unlocks were never fetched (see root cause below).

| Task | Status | Docs | Code | Attempts |
|------|--------|------|------|----------|
| Live-confirm `#4633` shape + auth (own account) | done | [v2/README.md](docs/v2/README.md) | — | 2026-06-24 `--probe-v2`: `player-achievements?filter[achievementSetId]=7890` → **8 unlocks** (works with web API key, own account). **Key finding:** `filter[gameId]=11270 & filter[achievementSetId]=7890` → **0** — subset achievements belong to a different gameId, so `filter[gameId]` does **not** return subset unlocks. That was the root cause of subsets reading 0/39. |
| Fetch + apply per-set unlocks (the bug fix) | done | — | `Services/V2ProgressService.cs`, `Services/HybridProgressService.cs` | Added `GetPlayerUnlocksForSetAsync` (refactored shared `GetPlayerUnlocksAsync(filterKey,…)`). Phase 2 now fetches each engaged non-core set's unlocks by `achievementSetId` (parallel) and merges them into the unlock map before tagging. FF VIII now shows 8/39 for "No Junction, No Level Up" (was 0/39); log: "applied 15 unlocks (from 2 engaged set(s))". |
| Gate per-set drill-down to avoid fan-out | done | — | `Services/HybridProgressService.cs` | Gated on `EarnedAchievements > 0` from the 1-call `player-achievement-sets` aggregate, so only *engaged* non-core sets are queried (untouched subsets cost 0 calls). |
| Incremental since-cursor (`filter[unlockedFrom]`) feed | backlog | — | `Services/V2ProgressService.cs` | Optimization only: current impl re-fetches each engaged set's full unlock list on a full refresh. A `unlockedFrom={lastSeen}` cursor would shrink payloads — but `unlockedFrom` boundary semantics (inclusive/exclusive, hardcore vs softcore) still need live confirmation before use as a polling cursor. |

**Notes:**
- **Root cause (verified live 2026-06-24):** `player-achievements?filter[gameId]` returns only base-game unlocks; subset achievements have a different gameId, so their unlocks must be fetched by `filter[achievementSetId]`. Don't "fix" this by widening the gameId query — it can't return them.
- The achievement-scoped variant filters out unranked users and excludes hub/event achievements, so keep `PlayerAchievementSet` for per-set completion %; use `#4633` only for the per-achievement unlock feed.

---

### STORY-012: Dashboard Subset UX

> As a user, I want the dashboard to clearly show which achievement set I'm tracking, with a working set selector, the right badge, and balanced layout, so multiset games are easy to read at a glance.

**Status:** in-progress
**Created:** 2026-06-24

| Task | Status | Code | Attempts |
|------|--------|------|----------|
| Set dropdown always visible in Current Game panel | done | `ViewModels/MainViewModel.cs`, `MainWindow.xaml` | Populate `AvailableAchievementSets` for single-set games too (synthesize Core if no set metadata); visibility gated on new `HasAchievementSets`; `AchievementSet.DisplayName`/`TypeLabel` for blank core titles. |
| Fix set/stats mismatch on restore | done | `ViewModels/MainViewModel.cs` | `UpdateAvailableAchievementSets` set the dropdown selection but not `CurrentGame.SelectedSet`, so a restored non-core set showed Core's numbers. Now syncs `CurrentGame.SelectedSet`. |
| Set-type badge (CORE/BONUS/…) in Current Game panel | done | `Converters/SetTypeToAccentBrushConverter.cs`, `Converters/AchievementSetVisuals.cs`, `MainWindow.xaml` | Colored pill next to the dropdown. Colors centralized in `AchievementSetVisuals` (shared with the Alerts badge); `AlertsViewModel` refactored to use it. |
| Subset badge replaces game badge when non-core selected | done | `Models/AchievementSet.cs`, `Services/*`, `ViewModels/MainViewModel.cs`, `MainWindow.xaml` | Plumbed set `BadgeUrl` (API `achievement-sets[].badgeUrl`) through `AchievementSetProgress` → `AchievementSet`; new `CurrentGameBadgeUri` shows the subset's badge for non-core, else game badge. |
| Focus badge image in Current Focus panel | done | `MainWindow.xaml` | Added the achievement badge (bound to existing `FocusBadgeUri`) left of the focus title/description. |
| Dashboard spacing (uniform 20px gutters) | done | `MainWindow.xaml` | Measured live (pixel scan): outer 33px vs center 10px → uniform 20px (card `Margin` 10 + dashboard grid `Margin` 0). Window 790 high, bottom row `MinHeight=250`. |

**Notes:**
- **DEBUG toggle in place:** `AchievementTrackingService.DebugForceGameId` is set to `11270` (Final Fantasy VIII) in `App.OnStartup` to pin the tracked game for subset testing. It defaults to `0` (normal "currently playing"); tests leave it at 0. **Remove the `App.OnStartup` line to restore live tracking.**
- Focus overlay window sizing review is the next task (compare against the legacy project's focus window).

---

## Backlog

_New stories are added here when captured from "As a user..." requests. Move to Active Stories when work begins._

- ~~Distinct per-set-type visual treatment in AlertsOverlay (STORY-001 phase 5).~~ **Done 2026-06-24.**
- Per-set notification opt-in toggles + settings UI, incl. configurable per-set accent colors (STORY-001 phase 6).
- STORY-011 incremental since-cursor (`filter[unlockedFrom]`) unlock feed — optimization, after live cursor-semantics confirmation.
- Remove the `DebugForceGameId = 11270` hardcode in `App.OnStartup` when subset testing is finished (STORY-012).
- Watch `AchievementSetVersion` ([#4979](https://github.com/RetroAchievements/RAWeb/pull/4979), OPEN) — when merged, read latest version per set (`page[size]=1`) to re-sync set definitions only on version change.
- Optional v1 subset fallback via "Subsets hub + per-game-ID aggregation" (only if a stable public v2 contract does not materialize).

---

## Completed

_Stories move here when all tasks are done._

- **STORY-002** — Documentation Consolidation (2026-02-27)
- **STORY-003** — Replace Placeholder Text with Icons (2026-02-27)
- **STORY-004** — RA Guides Integration (2026-02-27)
- **STORY-005** — Session-Based V2 Auth (2026-03-01)
- **STORY-006** — FlaUI Integration Tests (2026-03-01)
- **STORY-007** — Environment-Variable Credentials (2026-05-29)
- **STORY-008** — Test Suite Overhaul (2026-05-29)
- **STORY-009** — Make Live Data Actually Flow / v1 mapping + window UX (2026-05-29)
- **STORY-010** — Wire Up the Real v2 API / api. subdomain (2026-05-29)

---

## Conventions

- **Story IDs** increment sequentially: STORY-001, STORY-002, etc.
- **Code paths** are relative to `Retro Achievement Tracker.WPF/` unless otherwise noted.
- **Docs paths** are relative to the repo root.
- **Attempts** should always record: what was tried, what happened, and whether it worked. This prevents re-trying failed approaches.
- When a task is blocked, add a note explaining what's blocking it and what unblocks it.

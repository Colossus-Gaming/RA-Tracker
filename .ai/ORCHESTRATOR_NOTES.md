# Orchestrator Notes

Context for Claude Code to stay consistent across sessions.

## Repo Summary

WPF (.NET 8) app that tracks RetroAchievements progress and shows overlay windows for OBS streaming. Session-based V2 API auth via WebView2 with V1 fallback.

## Canonical Commands

| Action | Command |
|--------|---------|
| Build | `dotnet build "Retro Achievement Tracker.WPF/Retro Achievement Tracker.WPF.csproj" -c Debug` |
| Unit tests | `dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj" --filter "Category!=Integration"` |
| Integration tests | `dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj" --filter "Category=Integration"` |
| All tests | `dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj"` |
| Run app | `dotnet run --project "Retro Achievement Tracker.WPF/Retro Achievement Tracker.WPF.csproj"` |
| Run app with logging | `"Retro Achievement Tracker.WPF/bin/Debug/net8.0-windows/RATracker.WPF.exe" --log-to-file <path>` |

## Credentials via Environment Variables

For dev/testing, set these so the app and tests don't need the UI. When set (non-empty), they **override** the settings file and are never persisted.

| Variable | Purpose |
|----------|---------|
| `RA_USERNAME` | RetroAchievements username |
| `RA_API_KEY` | Web API key (control panel → Keys) |
| `RA_PASSWORD` | Account password (used for the WebView2 session login that bypasses Cloudflare for v2) |

PowerShell example: `$env:RA_USERNAME="me"; $env:RA_API_KEY="..."; $env:RA_PASSWORD="..."` then run the app. Read by `Services/EnvironmentCredentials.cs`, applied in `MainViewModel.ApplyEnvironmentCredentialOverrides()`.

## Known Gotchas

- **Platform**: Windows only (WPF). Shell is bash in VSCode terminal but PowerShell for `.ps1` scripts.
- **WebView2**: LoginWindow requires Edge WebView2 Runtime. Integration tests need a display (no headless).
- **File locking**: If build fails with "file locked by RATracker.WPF", kill the process first: `taskkill //F //IM RATracker.WPF.exe`
- **CRLF warnings**: Git shows LF→CRLF warnings on some files; harmless.
- **Password storage**: Uses DPAPI (Windows-only). Tests that click Start will trigger a real login against retroachievements.org using saved credentials.
- **V1 code freeze**: Only the V2 client is actively developed. The V1 fallback lives in `Services/HybridProgressService.cs` (`V1ProgressService` / `V1ApiClient`).
- **FlaUI tests run sequentially**: `[NonParallelizable]` attribute required; each test launches/kills the app. `App_FillCredentials_And_ClickStart` can trigger a **real login** if credentials are saved — prefer `App_Launches_And_MainWindow_Appears` for a safe launch smoke test.
- **Env-var tests are `[NonParallelizable]`**: they mutate process environment variables and restore them in `[TearDown]`.

## V2 API Reality (2026-05-29) — LIVE on api. subdomain

An earlier note said "v2 is 404/DOA." **Wrong** — it was the wrong host/prefix. Confirmed via RAWeb source + live in-app requests:

- **Base: `https://api.retroachievements.org/v2`** (api. subdomain, bare `/v2`). `retroachievements.org/api/v2` 404s.
- **Auth:** `X-API-Key` header + `Accept: application/vnd.api+json`. The api subdomain returns clean JSON (no Cloudflare challenge) — v2 does NOT need the WebView2 session.
- Routes **dasherized**, **no `/progress`**: user `/v2/users/{idOrName}`; current game `/v2/users/{u}/player-games?include=game&sort=-lastPlayedAt`; recent unlocks `/v2/users/{u}/player-achievements?include=achievement`; per-game progress `/v2/users/{u}/player-games?filter[gameId]={id}&include=game,achievementSets,playerAchievementSets`.
- `player-games` id is a record id (game id is in `relationships.game`); nested includes (`game.system`) 400.
- **Working hybrid (confirmed by live probe):** v2 for user / recently-played / current-game / recent unlocks / subset progress aggregates; **v1 `GetGameInfoAndUserProgress` for the full per-game achievement list** (the only single-call source — `/v2/games/{id}/achievements` and `/v2/achievement-sets/{id}/achievements` both 404).
- **Per-game progress flow** (`HybridProgressService.GetUserGameProgressAsync`):
  - **Phase 1, parallel:** v1 `GetGameInfoAndUserProgress` + v2 `games/{id}?include=achievementSets` + v2 `player-achievement-sets?filter[gameId]` + v2 `player-achievements?filter[gameId]&page[size]=100`. Total 4 calls.
  - **Phase 2 (multiset only, 1 call):** v2 `achievements?filter[gameId]&include=achievementSet&page[size]=100` → every achievement tagged with its set; non-core ones added to progress with unlock dates applied from the phase-1 unlocks map. Single-set games skip phase 2 entirely.
  - Achievements end up tagged with `SetId`/`SetType`/`SetName`; `CreateGameInfoFromProgress` groups them so `GameInfo.AchievementSets` populates for the multi-set UI.
- **Don't try:** `/v2/games/{id}/achievements` 404, `/v2/achievement-sets/{id}/achievements` 404, `include=achievements` on `/v2/achievement-sets/{id}` 400, `filter[achievementSetId]` on `/v2/achievements` 400. The only working "all game achievements with set membership" route is `filter[gameId]+include=achievementSet`.
- **Diagnostic probes** (both run off the UI thread, use the saved/env API key — no WebView2 login):
  - `RATracker.WPF.exe --probe-v2 "<path1>;<path2>" --log-to-file <out>` — raw GET against any v2 path, logs the body (`[V2BODY]`).
  - `RATracker.WPF.exe --probe-game "<id1>;<id2>" --log-to-file <out>` — runs the full HybridProgressService flow for each game id and prints set breakdown + grouped achievements (`[GameProbe]`).
- **Live-verified multiset matrix** (`docs/guides/v2-status.md`): SMB 1446 (4 sets), Sonic 1 (2), Zelda ALttP 355 (3), **SM64 10003 (6 sets, 506 achievements — exercises pagination)**, Castlevania SOTN 11240 (1), Dragster 14402 (1). All five types — Core/Bonus/Specialty/Exclusive/Challenge — observed.
- **Challenge type**: added to `AchievementSetType` after discovering SM64's "A Button Challenge" / "Speedrun Showcase" sets resolved to `Unknown`. Their `types[]` carry `"type":"challenge"`.

## Automated verification harness

To test the full launch→login→poll chain without manual interaction:
1. Save credentials (or set `RA_*` env vars). Set `autoStart: true` and `enableApiLogging: true` in `%APPDATA%/RATracker/settings.json`.
2. Launch `RATracker.WPF.exe --log-to-file <path>`; the saved password drives the WebView2 auto-login, and the first poll fires immediately.
3. Read `<path>` to inspect `[V2API]` request/response lines and `[AchievementTrackingService]` flow. Stop the process to flush the log.

## V1 mapping (Http/V1)

`Http/V1/V1ApiModels.cs` holds DTOs that match real v1 field names (`GameID`, `User`, `BadgeName`, achievements-as-dictionary) + `V1Mapper`. The old code deserialized v1 JSON straight into domain models, which silently produced blank usernames and game id 0. Always go through `V1Mapper`. Covered by `V1ApiTests/V1MapperTests.cs`.

## Current State (2026-05-29)

- **Live data flow works on v1:** poll populates user info (username/rank/points), current game, and pushes to overlays. Verified via the automated harness (`User info loaded: RetroS3xual` / `Currently playing game ID 10268` / `Pushing game data to open overlays: Gauntlet Legends`).
- **Window management fixed:** overlays are normal taskbar windows, movable (drag anywhere), and not force-topmost; the main tracker is always reachable.
- **285 unit tests pass** (added v1 mapper + subset + env + converter coverage).
- Session-based V2 auth working (STORY-005), but v2 endpoints 404 (see reality check above) so v1 fallback carries the app.
- **Subset tracking pipeline implemented (STORY-001 phases 1-4):** achievements are tagged with `SetId`/`SetType`/`SetName` in `V2ProgressService`, and `AchievementTrackingService` groups them into real per-set lists (the old core-only stub is gone). Remaining: per-set-type visuals + opt-in settings UI.
- **Environment-variable credentials (STORY-007):** `RA_USERNAME` / `RA_API_KEY` / `RA_PASSWORD`.
- **Tests (STORY-008):** legacy duplicate-model test file removed; real-model + subset + env + converter tests added. **278 unit tests pass**, 0 failing. FlaUI launch smoke test passes. Build is warning-clean.
- `UserGameProgress.EarnedAchievements` / `TotalAchievements` now fall back to the achievement-list counts when not explicitly set (fixes mastery detection when only the list is populated).

## Task Tracker

All work items: `TASKS.md`

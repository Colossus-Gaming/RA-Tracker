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

## Known Gotchas

- **Platform**: Windows only (WPF). Shell is bash in VSCode terminal but PowerShell for `.ps1` scripts.
- **WebView2**: LoginWindow requires Edge WebView2 Runtime. Integration tests need a display (no headless).
- **File locking**: If build fails with "file locked by RATracker.WPF", kill the process first: `taskkill //F //IM RATracker.WPF.exe`
- **CRLF warnings**: Git shows LF→CRLF warnings on some files; harmless.
- **Password storage**: Uses DPAPI (Windows-only). Tests that click Start will trigger a real login against retroachievements.org using saved credentials.
- **V1 code freeze**: Do not modify V1 API client (`Http/RetroAchievementAPIClient.cs` in legacy) — only V2 client is active.
- **FlaUI tests run sequentially**: `[NonParallelizable]` attribute required; each test launches/kills the app.

## Current State (2026-03-01)

- Session-based V2 auth working (STORY-005)
- FlaUI integration tests working, 3/3 pass (STORY-006)
- STORY-001 (Subset notifications) is backlog
- 2 pre-existing nullable warnings in HybridProgressService.cs (non-blocking)

## Task Tracker

All work items: `TASKS.md`

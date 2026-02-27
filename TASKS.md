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

## Backlog

_New stories are added here when captured from "As a user..." requests. Move to Active Stories when work begins._

---

## Completed

_Stories move here when all tasks are done._

- **STORY-002** — Documentation Consolidation (2026-02-27)

---

## Conventions

- **Story IDs** increment sequentially: STORY-001, STORY-002, etc.
- **Code paths** are relative to `Retro Achievement Tracker.WPF/` unless otherwise noted.
- **Docs paths** are relative to the repo root.
- **Attempts** should always record: what was tried, what happened, and whether it worked. This prevents re-trying failed approaches.
- When a task is blocked, add a note explaining what's blocking it and what unblocks it.

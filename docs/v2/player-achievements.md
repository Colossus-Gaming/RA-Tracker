# Player Achievements

`GET /v2/users/{user}/player-achievements` — a user's individual achievement unlocks
`GET /v2/achievements/{achievement}/player-achievements` — unlocks of a single achievement (across users)

**Status: merged & live** — [#4633](https://github.com/RetroAchievements/RAWeb/pull/4633) (merged 2026-03-26).

The per-unlock resource: one row per (user, achievement) unlock. This is how v2 exposes **which** achievements a user has unlocked and **when** — the granular unlock feed that v1 only offered via `API_GetUserRecentAchievements` / `API_GetGameInfoAndUserProgress`.

## Query Parameters

**Filters:** `filter[gameId]`, `filter[achievementSetId]`, `filter[unlockedFrom]`, `filter[unlockedTo]`
**Includes:** `achievement`, `game`
**Sort:** default `-unlockedAt`

> **Verified live (2026-06-25) — subset gotcha:** subset achievements belong to a **different gameId** than the base game, so `filter[gameId]={baseGameId}` returns **only the core (base-set) unlocks**. To get a subset's unlocks you must use `filter[achievementSetId]={setId}`. This app relies on that in `HybridProgressService` (one filtered call per engaged non-core set).

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `unlockedAt` | string (ISO 8601) | Softcore unlock timestamp |
| `unlockedHardcoreAt` | string (ISO 8601), nullable | Hardcore unlock timestamp (null if not earned in hardcore) |

The achievement id is in `relationships.achievement.data.id` (not an attribute).

## Relationships

| Relationship | Description |
|-------------|-------------|
| `achievement` | The unlocked achievement (include to get title/points/badge) |
| `game` | The game the achievement belongs to |

## Notes

- The achievement-scoped route (`/v2/achievements/{id}/player-achievements`) filters out unranked users and excludes hub/event achievements — use `player-achievement-sets` for authoritative per-set completion counts, and this resource for the per-achievement unlock feed.
- Field names verified in-app; see [../guides/v2-status.md](../guides/v2-status.md).

## Source

- [PR #4633](https://github.com/RetroAchievements/RAWeb/pull/4633) (merged 2026-03-26)

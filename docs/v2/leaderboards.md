# Leaderboards

`GET /api/v2/leaderboards` — list leaderboards
`GET /api/v2/leaderboards/{id}` — get a single leaderboard

**V1 equivalents:** `API_GetGameLeaderboards.php` ([v1](../v1/get-game-leaderboards.md)), `API_GetLeaderboardEntries.php` ([v1](../v1/get-leaderboard-entries.md))

## Query Parameters

**Filters:** `filter[id]`, `filter[gameId]`, `filter[state]` (comma-separated: `active`, `inactive`)

**Sorting:** `sort=orderColumn`

**Includes:** `game`, `developer`

**Index exclusions:** Hidden leaderboards (`orderColumn < 0`) and leaderboards from Hub/Event games are excluded from the index but accessible directly via show.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `title` | string | Leaderboard title |
| `description` | string | Leaderboard description |
| `format` | string | Score display format |
| `rankAsc` | boolean | Whether lower scores rank higher |
| `state` | string | Leaderboard state |
| `orderColumn` | number | Display ordering |
| `createdAt` | datetime | Creation timestamp |
| `updatedAt` | datetime | Last update timestamp |

**New in v2:** `format`, `rankAsc`, `state`, `developer` relationship, and proper server-side ranking with pagination.

## Relationships

| Relationship | Type | Access |
|-------------|------|--------|
| `game` | BelongsTo | via `?include=game` |
| `developer` | BelongsTo | via `?include=developer` |
| `entries` | HasMany | `GET /api/v2/leaderboards/{id}/entries` |

## Source

- [PR #4406](https://github.com/RetroAchievements/RAWeb/pull/4406) (merged Jan 18, 2026)

# Achievements

`GET /api/v2/achievements` — list achievements
`GET /api/v2/achievements/{id}` — get a single achievement

**V1 equivalents:** `API_GetAchievementCount.php` ([v1](../v1/get-achievement-count.md)), `API_GetAchievementUnlocks.php` ([v1](../v1/get-achievement-unlocks.md))

## Query Parameters

**Filters:**
- `filter[id]` — by achievement ID
- `filter[gameId]` — achievements for a specific game (queries through achievement sets)
- `filter[state]` — `promoted` or `unpromoted` (defaults to promoted only)
- `filter[type]` — comma-delimited achievement types: `progression`, `win_condition`, `missable`

**Sorting:** `sort=-pointsWeighted`, `sort=orderColumn`, etc.

**Includes:** `achievementSet`, `developer`, `games`

**Index exclusions:** Achievements from Hub/Event games are excluded. Defaults to promoted achievements only.

## Achievement Types

Individual achievements have a `type` classifying their purpose within a set:

| Type | Description |
|------|-------------|
| `progression` | Steady advancement; cannot be missed in normal play |
| `win_condition` | Marks game completion or ending |
| `missable` | Can be missed during normal play |
| `null` | Default/miscellaneous |

**Beaten status:** A game is "beaten" when ALL `progression` achievements are earned AND ANY `win_condition` achievement is earned.

**Important:** Subsets should not use progression/win_condition typing. Only core (Base) sets use the typing system.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `title` | string | Achievement title |
| `description` | string | Achievement description |
| `points` | number | Point value |
| `pointsWeighted` | number | Weighted (RetroPoints) |
| `badgeUrl` | string | Badge image URL |
| `badgeLockedUrl` | string | Locked badge image URL |
| `type` | string | `progression`, `win_condition`, `missable`, or `null` |
| `state` | string | `promoted` or `unpromoted` |
| `orderColumn` | number | Display order within the achievement set |
| `unlocksTotal` | number | Total unlock count |
| `unlocksHardcore` | number | Hardcore unlock count |
| `unlockPercentage` | number | Unlock percentage |
| `unlockHardcorePercentage` | number | Hardcore unlock percentage |
| `createdAt` | datetime | Creation timestamp |
| `modifiedAt` | datetime | Last modification timestamp |

**New in v2:** `achievementSet` relationship (key for subsets), `orderColumn` (from pivot table), `state`, `badgeLockedUrl`, per-achievement unlock statistics, and `filter[type]` for server-side type filtering.

## Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| `achievementSet` | HasOne | The set this achievement belongs to |
| `developer` | BelongsTo | The user who created the achievement |
| `games` | HasMany | Games linked through achievement sets |

The relationship chain is: achievement -> achievement_set_achievements -> achievement_sets -> game_achievement_sets -> games.

## Source

- [PR #4361](https://github.com/RetroAchievements/RAWeb/pull/4361) (merged Jan 10, 2026)

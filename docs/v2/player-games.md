# Player Games

`GET /api/v2/users/{user}/player-games` — list a user's game library

This resource is only accessible as a relationship endpoint on Users. There is no standalone index.

**V1 equivalents:** `API_GetUserRecentlyPlayedGames.php` ([v1](../v1/get-user-recently-played-games.md)), `API_GetUserCompletedGames.php` ([v1](../v1/get-user-completed-games.md))

## Query Parameters

**Filters:** `filter[id]`, `filter[gameId]`

**Includes:** `game`, `achievementSets`, `playerAchievementSets`

**Default sort:** `-lastPlayedAt` (most recently played first)

**Index exclusions:** Hub and Event games are excluded.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `lastPlayedAt` | datetime | When the game was last played |
| `firstUnlockAt` | datetime | First achievement unlock |
| `lastUnlockAt` | datetime | Most recent unlock |
| `lastUnlockHardcoreAt` | datetime | Most recent hardcore unlock |
| `beatenAt` | datetime | When the game was beaten (softcore) |
| `beatenHardcoreAt` | datetime | When the game was beaten (hardcore) |
| `playtimeTotalSeconds` | number | Total playtime in seconds |
| `timeToBeatSeconds` | number | Time to beat in seconds |
| `timeToBeatHardcoreSeconds` | number | Time to beat hardcore in seconds |

**New in v2:** Timestamps for beat milestones, playtime tracking, and the `playerAchievementSets` relationship for per-subset progress breakdowns.

## Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| `game` | BelongsTo | The game resource |
| `achievementSets` | BelongsToMany | Achievement sets for this game |
| `playerAchievementSets` | HasManyThrough | Per-set progress records |

## Usage Pattern

To get a user's games with per-subset progress:

```
GET /api/v2/users/{user}/player-games?include=playerAchievementSets
```

## Source

- [PR #4528](https://github.com/RetroAchievements/RAWeb/pull/4528) (merged Feb 13, 2026)

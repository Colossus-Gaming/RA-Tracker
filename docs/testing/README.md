# API Testing Data

Real API response samples captured from RetroAchievements endpoints using the test account `RetroS3xual`. These are used as reference fixtures for development and testing.

## Test Game

**Super Mario Bros.** — Game ID `1446` (NES/Famicom)
- 77 achievements, 775 total points
- Achievement types: 76 standard (null), 1 win_condition
- 68,465 distinct players
- No subsets (single achievement set)

**Super Mario 64** — Game ID `10003` (Nintendo 64)
- 114 achievements
- Achievement types: 107 standard, 5 progression, 1 missable, 1 win_condition
- 48,671 distinct players

## Test User

**RetroS3xual** — User ID `87358`, ULID `01D0TJC2MP13HPD4GDMYACAF2Q`
- 23,445 points (105,587 true points), Rank 4,231
- 37 mastery awards, 39 beaten awards
- Last played: Gauntlet Legends (N64)

## Files

| File | V1 Endpoint | Description |
|------|-------------|-------------|
| [v1-user-profile.json](v1-user-profile.json) | `API_GetUserProfile` | Basic user info with ULID |
| [v1-user-summary.json](v1-user-summary.json) | `API_GetUserSummary` | User info + recently played |
| [v1-user-points.json](v1-user-points.json) | `API_GetUserPoints` | Points breakdown |
| [v1-user-awards.json](v1-user-awards.json) | `API_GetUserAwards` | Mastery/beaten/site awards |
| [v1-user-completed.json](v1-user-completed.json) | `API_GetUserCompletedGames` | All completed/mastered games |
| [v1-recently-played.json](v1-recently-played.json) | `API_GetUserRecentlyPlayedGames` | Recent 5 games with progress |
| [v1-game.json](v1-game.json) | `API_GetGame` | Basic game info (SMB) |
| [v1-game-extended.json](v1-game-extended.json) | `API_GetGameExtended` | Full game with all achievements |
| [v1-game-progress.json](v1-game-progress.json) | `API_GetGameInfoAndUserProgress` | Game + per-user achievement status |
| [v1-game-hashes.json](v1-game-hashes.json) | `API_GetGameHashes` | ROM hashes for game |
| [v1-game-rank.json](v1-game-rank.json) | `API_GetGameRankAndScore` | Top 10 scorers for game |
| [v1-achievement-count.json](v1-achievement-count.json) | `API_GetAchievementCount` | Achievement ID list for game |
| [v1-achievement-unlocks.json](v1-achievement-unlocks.json) | `API_GetAchievementUnlocks` | Recent unlockers for achievement 3159 |
| [v1-achievement-distribution.json](v1-achievement-distribution.json) | `API_GetAchievementDistribution` | Unlock distribution histogram |
| [v1-achievement-of-week.json](v1-achievement-of-week.json) | `API_GetAchievementOfTheWeek` | Current AotW event |
| [v1-top-ten.json](v1-top-ten.json) | `API_GetTopTenUsers` | Global top 10 users |
| [v2-expected-responses.md](v2-expected-responses.md) | V2 endpoints | Expected V2 JSON:API response shapes |

## V2 Note

V2 endpoints (`/api/v2/*`) are protected by Cloudflare bot detection and cannot be accessed via plain `curl`. They require a browser session or a client that handles JavaScript challenges. The V2 expected response shapes are documented in [v2-expected-responses.md](v2-expected-responses.md) based on the [V2 API docs](../v2/README.md).

## Endpoint Cross-Reference: V1 → V2

| V1 Endpoint | V2 Equivalent | New in V2 |
|-------------|---------------|-----------|
| `API_GetUserProfile` | `GET /api/v2/users/{id}` | ULID-based identity |
| `API_GetGame` | `GET /api/v2/games/{id}` | JSON:API format |
| `API_GetGameExtended` | `GET /api/v2/games/{id}?include=achievements` | Sparse fieldsets |
| `API_GetGameInfoAndUserProgress` | `GET /api/v2/users/{id}/player-games?filter[gameId]={id}` + `player-achievement-sets` | Per-subset progress |
| `API_GetGameHashes` | `GET /api/v2/games/{id}/hashes` | Compatibility filter, labels as array |
| `API_GetAchievementUnlocks` | `GET /api/v2/achievements/{id}` + unlocks relationship | — |
| — | `GET /api/v2/achievement-sets` | **New**: subset discovery |
| — | `GET /api/v2/users/{id}/player-achievement-sets` | **New**: per-subset progress |
| — | `GET /api/v2/hubs` | **New**: game categorization |

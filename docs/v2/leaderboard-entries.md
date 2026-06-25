# Leaderboard Entries

`GET /v2/leaderboards/{leaderboard}/entries` — list entries for a leaderboard
`GET /v2/leaderboard-entries/{id}` — get a single entry

Entries are primarily accessed through the leaderboard relationship endpoint.

**V1 equivalent:** `API_GetLeaderboardEntries.php` ([v1](../v1/get-leaderboard-entries.md))

## Query Parameters

**Filters:** `filter[id]`, `filter[user]` (accepts ULID, username, or display_name)

**Includes:** `user`, `leaderboard`

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `score` | number | Raw score value |
| `formattedScore` | string | Score formatted per leaderboard format |
| `rank` | number | Calculated rank (uses SQL RANK() for correct tie handling) |
| `createdAt` | datetime | Creation timestamp |
| `updatedAt` | datetime | Last update timestamp |

**New in v2:** `formattedScore` (server-formatted), correct tie-aware ranking via SQL window functions.

## Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| `user` | BelongsTo | The user who set the score |
| `leaderboard` | BelongsTo | The parent leaderboard |

## Source

- [PR #4460](https://github.com/RetroAchievements/RAWeb/pull/4460) (merged Jan 27, 2026)

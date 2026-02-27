# RetroAchievements API v2 Reference

The v2 API is a JSON:API-compliant web API built on Laravel, actively deployed in production since December 2025. It runs in parallel with v1 — v1 is under an indefinite code freeze but continues to function without breaking changes. All new API capabilities are v2-only.

**Base URL:** `https://retroachievements.org/api/v2/`

**Spec:** [JSON:API 1.1](https://jsonapi.org/)

## Authentication

V2 supports three authentication methods (all equivalent):

| Method | Header |
|--------|--------|
| API Key (header) | `X-API-Key: {apiKey}` |
| Bearer (API key) | `Authorization: Bearer {apiKey}` |
| Bearer (OAuth2) | `Authorization: Bearer {accessToken}` |

All requests must include `Accept: application/vnd.api+json`.

OAuth2 uses Laravel Passport with the authorization code grant flow. Until OAuth2 is broadly available, the API key approach works identically to v1 (same key, different transport).

**Rate limit:** 60 requests per minute.

## Common Query Parameters

All collection endpoints support:

| Parameter | Description | Example |
|-----------|-------------|---------|
| `page[number]` | Page number (default: 1) | `page[number]=2` |
| `page[size]` | Items per page (default: 50, max: 100) | `page[size]=25` |
| `sort` | Sort field; prefix `-` for descending | `sort=-pointsTotal` |
| `filter[field]` | Filter by field value | `filter[systemId]=1` |
| `fields[type]` | Sparse fieldsets — limit returned attributes | `fields[games]=title,badgeUrl` |
| `include` | Side-load related resources | `include=system,achievementSets` |

## Resources

| Resource | Index | Show | Relationship endpoints | Doc |
|----------|-------|------|----------------------|-----|
| Systems | Yes | Yes | — | [systems.md](systems.md) |
| Games | Yes | Yes | — | [games.md](games.md) |
| Users | Yes | Yes | player-games, player-achievement-sets | [users.md](users.md) |
| Achievement Sets | — | Yes | — | [achievement-sets.md](achievement-sets.md) |
| Achievements | Yes | Yes | — | [achievements.md](achievements.md) |
| Leaderboards | Yes | Yes | entries | [leaderboards.md](leaderboards.md) |
| Leaderboard Entries | — | Yes | — | [leaderboard-entries.md](leaderboard-entries.md) |
| Hubs | Yes | Yes | games, links | [hubs.md](hubs.md) |
| Player Games | — | — | via users | [player-games.md](player-games.md) |
| Player Achievement Sets | — | — | via users | [player-achievement-sets.md](player-achievement-sets.md) |
| Game Hashes | — | — | via games (PR open) | [game-hashes.md](game-hashes.md) |

## Subset Model

V2 introduces a three-level model for achievement subsets that has no v1 equivalent:

```
Game
 └─ AchievementSet (type: Base | Bonus | Specialty | Exclusive)
     └─ Achievement (type: progression | win_condition | missable | null)
```

- **AchievementSet** groups achievements into Core (Base), Bonus, Specialty, or Exclusive sets per game.
- **PlayerAchievementSet** tracks a user's progress within each set separately.
- To get per-subset progress for a user on a game:

```
GET /api/v2/users/{user}/player-achievement-sets?filter[gameId]={gameId}&include=achievementSet
```

See [achievement-sets.md](achievement-sets.md) and [player-achievement-sets.md](player-achievement-sets.md) for details.

## V1 to V2 Migration Summary

| Aspect | V1 | V2 |
|--------|----|----|
| Response format | Flat JSON | JSON:API (`data`, `attributes`, `relationships`, `links`, `meta`) |
| Base URL | `/API/API_*.php` | `/api/v2/{resource}` |
| Auth transport | `?y={apiKey}` query param | `X-API-Key` header or `Bearer` token |
| Content-Type | `application/json` | `application/vnd.api+json` |
| User identity | Username (mutable) | ULID (stable); username/display_name lookup supported |
| Pagination | Inconsistent, some unpaginated | Uniform `page[number]`/`page[size]` on all collections |
| Filtering | Ad-hoc query params | Uniform `filter[field]` |
| Sorting | Generally unavailable | Uniform `sort=field` / `sort=-field` |
| Subset support | None | Full (AchievementSet + PlayerAchievementSet) |
| Relationship loading | Inline embedded | Side-loaded via `?include=` or relationship endpoints |

## Development Timeline

| Date | PR | Resource |
|------|-----|----------|
| Dec 1, 2025 | [#4163](https://github.com/RetroAchievements/RAWeb/pull/4163) | V2 infrastructure + healthcheck |
| Dec 8, 2025 | [#4171](https://github.com/RetroAchievements/RAWeb/pull/4171) | System |
| Dec 14, 2025 | [#4237](https://github.com/RetroAchievements/RAWeb/pull/4237) | Game |
| Dec 14, 2025 | [#4255](https://github.com/RetroAchievements/RAWeb/pull/4255) | OAuth2 + API key dual auth |
| Dec 23, 2025 | [#4271](https://github.com/RetroAchievements/RAWeb/pull/4271) | User |
| Jan 2, 2026 | [#4299](https://github.com/RetroAchievements/RAWeb/pull/4299) | AchievementSet |
| Jan 10, 2026 | [#4361](https://github.com/RetroAchievements/RAWeb/pull/4361) | Achievement |
| Jan 18, 2026 | [#4406](https://github.com/RetroAchievements/RAWeb/pull/4406) | Leaderboard |
| Jan 27, 2026 | [#4460](https://github.com/RetroAchievements/RAWeb/pull/4460) | LeaderboardEntry |
| Feb 2, 2026 | [#4491](https://github.com/RetroAchievements/RAWeb/pull/4491) | Hub |
| Feb 13, 2026 | [#4528](https://github.com/RetroAchievements/RAWeb/pull/4528) | PlayerGame |
| Feb 23, 2026 | [#4562](https://github.com/RetroAchievements/RAWeb/pull/4562) | PlayerAchievementSet |
| Open | [#4593](https://github.com/RetroAchievements/RAWeb/pull/4593) | GameHash |

## Canonical Sources

- Discussion: https://github.com/RetroAchievements/RAWeb/discussions/2081
- PR query: https://github.com/RetroAchievements/RAWeb/pulls?q=feat%28api%29+V2
- RAWeb source: https://github.com/RetroAchievements/RAWeb

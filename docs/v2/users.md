# Users

`GET /api/v2/users` — list users (default sort: `pointsHardcore` descending)
`GET /api/v2/users/{identifier}` — get a single user

The `{identifier}` accepts ULID, display_name, or username. ULID is the preferred stable identifier.

**V1 equivalents:** `API_GetUserProfile.php` ([v1](../v1/get-user-profile.md)), `API_GetUserPoints.php` ([v1](../v1/get-user-points.md)), `API_GetUserSummary.php` ([v1](../v1/get-user-summary.md)), `API_GetTopTenUsers.php` ([v1](../v1/get-top-ten-users.md))

## Query Parameters

**Filters:** `filter[role]`

**Sorting:** Default by `pointsHardcore` descending; `sort=-pointsWeighted`, etc.

**Index exclusions:** Banned and unverified users are excluded.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `displayName` | string | User's display name |
| `avatarUrl` | string | Avatar image URL |
| `motto` | string | User motto |
| `points` | number | Softcore points |
| `pointsHardcore` | number | Hardcore points |
| `pointsWeighted` | number | RetroPoints |
| `yieldUnlocks` | number | Total unlocks given (developers) |
| `yieldPoints` | number | Total points given (developers) |
| `joinedAt` | datetime | Account creation date |
| `lastActivityAt` | datetime | Last activity timestamp |
| `deletedAt` | datetime | Soft-delete timestamp (only for deleted users) |
| `isUnranked` | boolean | Whether the user is unranked |
| `isUserWallActive` | boolean | Whether user wall is active |
| `richPresence` | string | Current rich presence message |
| `richPresenceUpdatedAt` | datetime | When rich presence was last updated |
| `visibleRole` | string | Primary visible role name |
| `displayableRoles` | array | All displayable role names |

**New in v2:** `richPresence`, `richPresenceUpdatedAt`, `visibleRole`, `displayableRoles`, `yieldUnlocks`, `yieldPoints`, ULID-based identification.

## Relationships

| Relationship | Type | Access |
|-------------|------|--------|
| `playerGames` | HasMany | `GET /api/v2/users/{id}/player-games` |
| `playerAchievementSets` | HasMany | `GET /api/v2/users/{id}/player-achievement-sets` |

These relationships cannot be eager-loaded via `?include=` — they must be accessed through their dedicated relationship endpoints.

## Source

- [PR #4271](https://github.com/RetroAchievements/RAWeb/pull/4271) (merged Dec 23, 2025)

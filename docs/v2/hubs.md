# Hubs

`GET /api/v2/hubs` — list hubs
`GET /api/v2/hubs/{id}` — get a single hub

**No v1 equivalent.** Hubs are an entirely new resource backed by the `GameSet` model.

## Query Parameters

**Filters:** `filter[id]`, `filter[parentId]`, `filter[title]` (contains search)

**Sorting:** `sort=title`, `sort=sortTitle`, `sort=-gamesCount`, `sort=-linkedHubsCount`, `sort=createdAt`, `sort=updatedAt`

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `title` | string | Hub title |
| `sortTitle` | string | Sort title |
| `badgeUrl` | string | Badge image URL |
| `hasMatureContent` | boolean | Mature content flag |
| `gamesCount` | number | Number of games in the hub |
| `linkedHubsCount` | number | Number of linked hubs |
| `isEventHub` | boolean | Whether this is an event hub |
| `createdAt` | datetime | Creation timestamp |
| `updatedAt` | datetime | Last update timestamp |

## Relationships

| Relationship | Type | Access |
|-------------|------|--------|
| `games` | BelongsToMany | `GET /api/v2/hubs/{id}/games` (paginated) |
| `links` | BelongsToMany | `GET /api/v2/hubs/{id}/links` (self-referential, paginated) |

These relationships cannot be accessed via `?include=` (returns 400). Use the dedicated relationship endpoints.

## Source

- [PR #4491](https://github.com/RetroAchievements/RAWeb/pull/4491) (merged Feb 2, 2026)

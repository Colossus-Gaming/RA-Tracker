# Systems

`GET /v2/systems` — list all systems
`GET /v2/systems/{id}` — get a single system

**V1 equivalent:** `API_GetConsoleIDs.php` ([v1 docs](../v1/get-console-ids.md))

## Query Parameters

**Filters:** `filter[id]`, `filter[active]` (boolean)

**Sorting:** `sort=name`

**Index exclusions:** Hub (system_id=100) and Event (system_id=101) systems are excluded from the index.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `name` | string | System name (e.g. "Nintendo 64") |
| `nameFull` | string | Full display name |
| `nameShort` | string | Abbreviated name |
| `manufacturer` | string | e.g. "Nintendo" |
| `iconUrl` | string | URL to system icon |
| `active` | boolean | Whether the system accepts new content |

**New in v2:** `iconUrl`, `nameFull`, `nameShort`, `manufacturer`, `active` — none of these are available in v1.

## Relationships

None currently. A `games` HasMany relationship is planned.

## Source

- [PR #4171](https://github.com/RetroAchievements/RAWeb/pull/4171) (merged Dec 8, 2025)

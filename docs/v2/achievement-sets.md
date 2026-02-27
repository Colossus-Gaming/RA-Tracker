# Achievement Sets

`GET /api/v2/achievement-sets/{id}` — get a single achievement set (show only, no index)

**No v1 equivalent.** This is an entirely new resource that enables subset support.

## What Is an Achievement Set?

An achievement set is a group of achievements associated with a game. A single game can have multiple sets:

| Set Type | Description |
|----------|-------------|
| **Base** (Core) | The primary achievement set, loaded by default |
| **Bonus** | Additional achievements linked to a base set |
| **Specialty** | Require patched ROMs; automatically load base and bonus alongside |
| **Exclusive** | Load in isolation, incompatible with other sets |

The `type` field is **contextual** — when accessed through a game's `achievementSets` relationship (via `?include=achievementSets`), it reflects the pivot type (Base, Bonus, Specialty, Exclusive). When accessed directly via the show endpoint, `type` returns `null`.

## Query Parameters

**Filters:** `filter[id]`

**Includes:** `games`

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `title` | string | Set title (from pivot, e.g. "Bonus", "Subset - Challenge") |
| `pointsTotal` | number | Total points in this set |
| `pointsWeighted` | number | Weighted (RetroPoints) |
| `achievementsPublished` | number | Published achievement count |
| `achievementsUnpublished` | number | Unpublished count |
| `badgeUrl` | string | Badge image URL |
| `achievementsFirstPublishedAt` | datetime | When first achievement was published |
| `types` | array | Game/type context pairs |
| `createdAt` | datetime | Creation timestamp |
| `updatedAt` | datetime | Last update timestamp |

## Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| `games` | BelongsToMany | Games linked to this set |

Subset backing games are forcibly excluded from both the `gameIds` attribute and `games` relationship to prevent consumer dependency on internal implementation details.

## Usage Pattern for Subset Discovery

To discover all achievement sets for a game:

```
GET /api/v2/games/{gameId}?include=achievementSets
```

The included `achievementSets` will each have their contextual `type` populated (Base, Bonus, Specialty, or Exclusive).

## Source

- [PR #4299](https://github.com/RetroAchievements/RAWeb/pull/4299) (merged Jan 2, 2026)

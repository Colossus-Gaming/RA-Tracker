# Games

`GET /v2/games` — list games
`GET /v2/games/{id}` — get a single game

**V1 equivalents:** `API_GetGame.php` ([v1](../v1/get-game.md)), `API_GetGameExtended.php` ([v1](../v1/get-game-extended.md)), `API_GetGameList.php` ([v1](../v1/get-game-list.md))

## Query Parameters

**Filters:** `filter[id]`, `filter[systemId]`

**Sorting:** `sort=playersTotal`, `sort=-pointsTotal`, etc.

**Includes:** `system`, `achievementSets`

**Index exclusions:** Hub games (system_id=100), Event games (system_id=101), and subset games (titles containing "[Subset -") are excluded from the index.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `title` | string | Game title |
| `sortTitle` | string | Title used for sorting |
| `badgeUrl` | string | Badge image URL |
| `imageBoxArtUrl` | string | Box art image URL |
| `imageTitleUrl` | string | Title screen image URL |
| `imageIngameUrl` | string | In-game screenshot URL |
| `releasedAt` | datetime | Release date |
| `releasedAtGranularity` | string | Precision: `day`, `month`, or `year` |
| `playersTotal` | number | Total distinct players |
| `playersHardcore` | number | Hardcore players |
| `achievementsPublished` | number | Published achievement count |
| `achievementsUnpublished` | number | Unpublished achievement count |
| `pointsTotal` | number | Total points available |
| `pointsWeighted` | number | Weighted (RetroPoints) |
| `timesBeaten` | number | Times beaten (softcore) |
| `timesBeatenHardcore` | number | Times beaten (hardcore) |
| `medianTimeToBeatMinutes` | number | Median time to beat |
| `medianTimeToBeatHardcoreMinutes` | number | Median time to beat (hardcore) |

**New in v2:** `timesBeaten`, `timesBeatenHardcore`, `medianTimeToBeatMinutes`, `medianTimeToBeatHardcoreMinutes`, `sortTitle`, `achievementsUnpublished`, and the `achievementSets` relationship.

## Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| `system` | BelongsTo | The console/system |
| `achievementSets` | BelongsToMany | Achievement sets linked to this game (core, bonus, subsets) |

The `achievementSets` relationship is critical for subset support — it returns all sets associated with a game, each with a contextual `type` indicating Base, Bonus, Specialty, or Exclusive.

## Source

- [PR #4237](https://github.com/RetroAchievements/RAWeb/pull/4237) (merged Dec 14, 2025)

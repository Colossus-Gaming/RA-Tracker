# Player Achievement Sets

`GET /v2/users/{user}/player-achievement-sets` — list a user's per-set progress

This resource is only accessible as a relationship endpoint on Users. There is no standalone index.

**No v1 equivalent.** This is the cornerstone resource for subset support, providing per-achievement-set progress for a user.

## Query Parameters

**Filters:** `filter[id]`, `filter[achievementSetId]`, `filter[gameId]`

**Includes:** `achievementSet`, `game`

**Default sort:** `-lastUnlockAt` (most recent activity first)

Nearly every field is sortable.

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `achievementsUnlocked` | number | Achievements unlocked (softcore) |
| `achievementsUnlockedHardcore` | number | Achievements unlocked (hardcore) |
| `points` | number | Points earned (softcore) |
| `pointsHardcore` | number | Points earned (hardcore) |
| `pointsWeighted` | number | RetroPoints earned |
| `completionPercentage` | number | Completion percentage (softcore) |
| `completionPercentageHardcore` | number | Completion percentage (hardcore) |
| `lastUnlockAt` | datetime | Most recent unlock |
| `lastUnlockHardcoreAt` | datetime | Most recent hardcore unlock |
| `completedAt` | datetime | When 100% completed (softcore) |
| `completedHardcoreAt` | datetime | When 100% completed (hardcore) |
| `timeTakenSeconds` | number | Time taken to complete |
| `timeTakenHardcoreSeconds` | number | Time taken (hardcore) |
| `setContext` | array | Game/type context pairs for this set |

The `setContext` array tells you whether the set is Core (Base), Bonus, Specialty, or Exclusive without requiring a full game include.

## Relationships

| Relationship | Type | Description |
|-------------|------|-------------|
| `achievementSet` | BelongsTo | The achievement set |
| `game` | HasOneThrough | The game (through the achievement set) |

## Usage Patterns

### Get per-subset progress for a specific game

```
GET /v2/users/{user}/player-achievement-sets?filter[gameId]={gameId}&include=achievementSet
```

Returns individual progress records for each set (Core, Bonus, subsets) the user has interacted with for that game.

### Get all recent achievement set activity

```
GET /v2/users/{user}/player-achievement-sets?sort=-lastUnlockAt&page[size]=10
```

### Subset notification use case

For the subset notifications feature, this endpoint provides the data needed to distinguish between Core and Bonus achievement unlocks. By including `achievementSet`, the response contains the set type context, allowing the UI to route notifications to the appropriate visual treatment.

## Source

- [PR #4562](https://github.com/RetroAchievements/RAWeb/pull/4562) (merged Feb 23, 2026)

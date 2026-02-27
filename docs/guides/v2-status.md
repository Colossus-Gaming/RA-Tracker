# API v2 Status (as of 2026-02-27)

## Current State

V2 is **in production** and actively deployed since December 2025. It is a JSON:API-compliant web API running in parallel with v1.

- **V1** is under an indefinite code freeze. It continues to work without breaking changes, but no new features will be added.
- **V2** receives all new API capabilities. 10+ resource types are merged and live, with new resources landing roughly every 2-3 weeks.

Base URL: `https://retroachievements.org/api/v2/`

Full reference: [../v2/README.md](../v2/README.md)

## What V2 Adds

- **JSON:API spec** with standardized pagination, filtering, sorting, sparse fieldsets, and relationship includes.
- **Achievement subset support** via AchievementSet and PlayerAchievementSet resources — no v1 equivalent.
- **OAuth2 authentication** alongside API key auth (key now passed via header, not query param).
- **ULID-based user identity** — stable identifiers that don't change when users rename.
- **Hubs** — entirely new resource with no v1 equivalent.

## Key Resources for This Project

| Resource | Why it matters |
|----------|---------------|
| [PlayerAchievementSet](../v2/player-achievement-sets.md) | Per-subset progress tracking (Core vs Bonus vs Specialty vs Exclusive) |
| [AchievementSet](../v2/achievement-sets.md) | Subset discovery and type classification |
| [Achievement](../v2/achievements.md) | Server-side `filter[type]` for progression/win_condition/missable |
| [PlayerGame](../v2/player-games.md) | Game-level progress with subset breakdowns via relationship |

## Tracking Links

- Discussion: https://github.com/RetroAchievements/RAWeb/discussions/2081
- PR query: https://github.com/RetroAchievements/RAWeb/pulls?q=feat%28api%29+V2

## Migration Guidance

- V1 and v2 can be used simultaneously — no need for a hard cutover.
- Subset-aware features (like subset notifications) should target v2 since v1 has no subset model.
- Auth and transport isolation (already in this repo's implementation rules) makes dual-version support straightforward.

# API v2 Status (as of 2026-02-27; verification note 2026-05-29)

## Live Test Result (2026-05-29) — v2 IS live (on the api. subdomain)

An earlier note here said "v2 returns 404 / is DOA." **That was wrong** — it was caused by calling the wrong host/prefix. Verified against RAWeb source ([`app/Api/RouteServiceProvider.php`](https://github.com/RetroAchievements/RAWeb/blob/master/app/Api/RouteServiceProvider.php)) and live requests:

- **Correct base: `https://api.retroachievements.org/v2`** — NOT `https://retroachievements.org/api/v2` (that 404s).
- **Auth:** `X-API-Key: <web api key>` header + `Accept: application/vnd.api+json`. The `api.` subdomain returns clean JSON 401s (no Cloudflare JS challenge), so v2 does not require the WebView2 session — the API key is enough.
- **User addressing:** username, display name, or ULID all work (`/v2/users/RetroS3xual` → 200).
- **Routes are dasherized** and there is **no `/progress` route**:

| Need | v2 endpoint |
|------|-------------|
| User summary | `GET /v2/users/{idOrName}` |
| Recently played / current game | `GET /v2/users/{idOrName}/player-games?include=game&sort=-lastPlayedAt` |
| Recent unlocks | `GET /v2/users/{idOrName}/player-achievements?include=achievement` |
| Per-game progress | `GET /v2/users/{idOrName}/player-games?filter[gameId]={id}&include=game,achievementSets,playerAchievementSets` |

**Confirmed live, in-app:** all of the above return **200**. Key shape notes the app relies on:
- The `player-games` resource `id` is a *player-game record id*; the real game id is in `relationships.game.data.id`.
- Nested includes like `game.system` are **rejected (400)** — include only `game`.
- `player-games` does **not** carry the per-achievement list (only `achievementSets`/`playerAchievementSets`). The app sources the full per-game achievement list from **v1 `GetGameInfoAndUserProgress`** (one call, complete).
- Live-verified routes that **don't exist** (don't try them): `/v2/games/{id}/achievements` → 404; `/v2/achievement-sets/{id}/achievements` → 404; `include=achievements` on achievement-sets → 400; `filter[achievementSetId]` on `/v2/achievements` → 400. There is no "list a game's achievements scoped to one set" endpoint.
- The one route that **does** give per-achievement set membership: **`/v2/achievements?filter[gameId]={gameId}&include=achievementSet&page[size]=100`** → returns every achievement on the game (all sets), each with an `achievementSet` relationship; the included set's `types[]` array (matched by gameId) gives the set type. Single call.

### Final design (efficient hybrid for per-game progress)

Phase 1 — fired **in parallel** every game-change poll:

| Source | Endpoint | What it gives |
|---|---|---|
| v1 | `API_GetGameInfoAndUserProgress` | Full **published core** achievement list + unlock status (1 call, complete for the core set) |
| v2 | `/games/{id}?include=achievementSets` | All sets defined on the game (Core/Bonus/Specialty/Exclusive) with totals and type-per-game from `types[]` |
| v2 | `/users/{u}/player-achievement-sets?filter[gameId]&include=achievementSet` | Engaged-set earned counts + `setContext.type` discriminator |
| v2 | `/users/{u}/player-achievements?filter[gameId]&page[size]=100` | Every unlock the user has on this game, indexed by achievement id |

Phase 2 — **only for multiset games** (1 extra call total, regardless of how many non-core sets):

| Source | Endpoint | What it gives |
|---|---|---|
| v2 | `/achievements?filter[gameId]&include=achievementSet&page[size]=100` | Every achievement on the game tagged with its set; non-core ones are added to the progress, unlock dates applied from the unlocks map |

Net call count: **4** for single-set games, **5** for multiset (independent of set count). Single-set common case stays fast; multiset is one extra call. **Pagination** is implemented on both achievement and unlock fetches (page[size]=100, follows `meta.page.lastPage`).

### Verified live (automated `--probe-game <id;id;…>` harness, 2026-05-29)

| Game (system) | Sets | Achievements | Set breakdown |
|---|---|---|---|
| Super Mario Bros. (NES) | 4 | 153 | Core 77, Bonus 34, Exclusive 33, Exclusive 9 |
| Sonic the Hedgehog (Genesis) | 2 | 59 | Core 35, Exclusive 24 |
| Zelda: A Link to the Past (SNES) | 3 | 160 | Core 109, Exclusive 37, Bonus 14 |
| **Super Mario 64 (N64)** | **6** | **506** | Core 114, Bonus 48, Bonus 18, Specialty 137, Challenge 127, Challenge 62 |
| Castlevania: SOTN (PS1) | 1 | 105 | Core (single-set baseline) |
| Dragster (Atari 2600) | 1 | 15 | Core (single-set baseline) |

All five set types — Core, Bonus, Specialty, Exclusive, **Challenge** — were observed and correctly mapped. (The Challenge type was discovered when SM64's "A Button Challenge" and "Speedrun Showcase" first resolved to `Unknown`; their achievement-set `types[]` carry `"type":"challenge"`, now in the enum/parser.)

### Focus + per-set counts validated

Same `--probe-game` harness also asserts each set's first-locked achievement (the Focus) and earned/total counts. Sample real results:

| Game | Set | Earned/Total | Focus achievement |
|---|---|---|---|
| FFTA 519 (completed) | Core | 138/138 | *(all unlocked)* |
| Jurassic Park 12846 | Core | 1/48 | "Stage 1 - Triceratops" |
| SMB 1446 | Core | 0/77 | "Shroooooms..." |
| SMB 1446 | Exclusive — Sub 5-Min Speedrun | 0/9 | "WARNING - Began run too quickly!" |
| SM64 10003 | Core | 0/114 | "A New Journey" |
| SM64 10003 | Challenge — Speedrun Showcase | 0/62 | "Too Fast for Lakitu" |
| SM64 10003 | Challenge — A Button Challenge | 0/127 | "Take A Walk" |

`CreateGameInfoFromProgress` sorts each set's achievements by `DisplayOrder` (v1) / `orderColumn` (v2 — mapped to `DisplayOrder` by `V2ResourceMapper`), so the Focus pick is **deterministic** across runs/sources.

## Verification Status (2026-05-29)

Independent research (web + RAWeb sources) qualifies the claims below. Read this first:

- The v2 resource/field details in `docs/v2/*` were derived from **RAWeb source and pull requests**, which are legitimate primary sources — but v2 is **not published as a stable public contract** on `api-docs.retroachievements.org` (that site documents v1 only). Treat v2 field names as **observed, not guaranteed**.
- v2 is reachable from this app via the WebView2 **session** (it sits behind Cloudflare); plain `curl` is blocked.
- The **public v1** Web API has **no subset/multiset model**. On V1 fallback, a game collapses to a single Core set. Subset tracking depends on the v2 path.
- First-class multiset resolution (server-side Base/Bonus/Specialty/Exclusive) is a **private Connect API** feature for registered emulators, not third-party apps.
- **To confirm the real shapes:** run the app with `RA_USERNAME`/`RA_API_KEY`/`RA_PASSWORD` set and API logging enabled, capture live responses, and tighten the mapper/docs accordingly. The mapper (`Services/V2ProgressService.cs`) is intentionally defensive about field names.

## Current State

V2 is reported **in production** and actively deployed since December 2025 as a JSON:API-compliant web API running in parallel with v1 (see verification note above before relying on specifics).

- **V1** is under an indefinite code freeze. It continues to work without breaking changes, but no new features will be added.
- **V2** receives all new API capabilities. 10+ resource types are merged and live, with new resources landing roughly every 2-3 weeks.

Base URL: **`https://api.retroachievements.org/v2/`** (the `api.` subdomain, bare `/v2` prefix). The old `https://retroachievements.org/api/v2/` is wrong and 404s.

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

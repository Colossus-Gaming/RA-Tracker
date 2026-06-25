# Game Hashes

`GET /api/v2/games/{game}/hashes` — list hashes for a game

This resource is only accessible as a relationship endpoint on Games. There is no standalone index.

**Status: merged & live** — [#4593](https://github.com/RetroAchievements/RAWeb/pull/4593) (merged 2026-03-18). Confirmed in master `app/Api/RouteServiceProvider.php` (delta research 2026-06-24).

**V1 equivalent:** `API_GetGameHashes.php` ([v1](../v1/get-game-hashes.md))

## Query Parameters

**Filters:** `filter[compatibility]` (comma-separated: `compatible`, `incompatible`, `untested`, `patch-required`)

**Includes:** `game`

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `raMd5` | string | MD5 hash of the ROM (named `raMd5` in the merged v2 resource — **not** `md5`) |
| `name` | string | ROM filename / label |
| `compatibility` | string | Compatibility status |
| `patchUrl` | string | URL to required patch (if applicable) |

> **Corrected 2026-06-24:** the attribute is `raMd5`/`name`, per the merged PR. The earlier draft naming (`md5` + a `labels` array) came from an in-progress revision and was never the shipped shape. Re-confirm the full attribute set with a live `--probe-v2 GET /v2/games/{id}/hashes` before relying on `compatibility`/`patchUrl`.

**New in v2:** `compatibility` as a filterable field; soft-deleted hashes excluded.

## Source

- [PR #4593](https://github.com/RetroAchievements/RAWeb/pull/4593) (merged 2026-03-18)

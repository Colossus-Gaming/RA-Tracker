# Game Hashes

`GET /api/v2/games/{game}/hashes` — list hashes for a game

This resource is only accessible as a relationship endpoint on Games. There is no standalone index.

**Status: PR open** — [#4593](https://github.com/RetroAchievements/RAWeb/pull/4593) (submitted Feb 24, 2026, not yet merged)

**V1 equivalent:** `API_GetGameHashes.php` ([v1](../v1/get-game-hashes.md))

## Query Parameters

**Filters:** `filter[compatibility]` (comma-separated: `compatible`, `incompatible`, `untested`, `patch-required`)

**Includes:** `game`

## Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `md5` | string | MD5 hash of the ROM |
| `name` | string | ROM filename |
| `labels` | array | Labels (converted from CSV to JSON array in v2) |
| `compatibility` | string | Compatibility status |
| `patchUrl` | string | URL to required patch (if applicable) |

**New in v2:** `labels` as a proper JSON array (was comma-separated string in v1), `compatibility` as a filterable field, soft-deleted hashes excluded.

## Source

- [PR #4593](https://github.com/RetroAchievements/RAWeb/pull/4593) (open)

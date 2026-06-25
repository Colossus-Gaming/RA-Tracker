# User Awards

`GET /v2/users/{user}/awards` — a user's awards (badges)

**Status: merged & live** — [#4765](https://github.com/RetroAchievements/RAWeb/pull/4765) (merged 2026-04-25).

The `PlayerBadge` model: server-computed awards such as **beaten (hardcore)**, **mastered**, **completed**, and **event** awards. Authoritative award state — useful for detecting mastery/beaten transitions without recomputing from raw unlock data.

## Query Parameters

**Filters:** `filter[kind]`, `filter[gameId]`
**Includes:** relationship-path form — `include=game.system`, `include=event.awards` (not flat `include=game,system,event`)

## Attributes

- A `context` attribute carries `gameId` / `mode` / `eventId`. Awards are keyed at the **game/award** level, not per achievement set.

## Notes

- Field names observed from PR #4765 / RAWeb source; verify live before relying on specifics.

## Source

- [PR #4765](https://github.com/RetroAchievements/RAWeb/pull/4765) (merged 2026-04-25)

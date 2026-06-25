# Achievement Set Claims

`GET /v2/achievement-set-claims` — list claims
`GET /v2/users/{user}/achievement-set-claims` — a user's claims
`GET /v2/games/{game}/achievement-set-claims` — claims on a game

**Status: merged & live** — [#4865](https://github.com/RetroAchievements/RAWeb/pull/4865) (merged 2026-05-13).

A developer "claim" on an achievement set (the RA claim system, used to coordinate who is building/revising a set). List-only — there is **no `/{id}` show route**. Replaces the v1 `API_GetClaims` / `API_GetActiveClaims` / `API_GetUserClaims` endpoints.

## Query Parameters

**Filters:** by status, claim type, set type, special type, and expiration; `filter[user]` accepts a username **or** ULID. Banned users are excluded.
**Sort:** default `-claimedAt`

## Notes

- Useful as a secondary signal that a tracked game's set is **being revised** (its definitions/counts may shift).
- Field names observed from PR #4865 / RAWeb source — not a published contract; verify live before relying on specific attribute names.

## Source

- [PR #4865](https://github.com/RetroAchievements/RAWeb/pull/4865) (merged 2026-05-13)

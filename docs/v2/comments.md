# Comments

`GET /v2/games/{game}/comments` — comments on a game
`GET /v2/achievements/{achievement}/comments` — comments on an achievement
`GET /v2/users/{user}/wall-comments` — a user's wall

**Status: merged & live** — [#4818](https://github.com/RetroAchievements/RAWeb/pull/4818) (merged 2026-05-05).

Relationship-accessed comment threads. No standalone index.

## Query Parameters

- Paginated **50 per page**, ordered **ascending** by `submittedAt` (pass `sort=-submittedAt` for newest-first).
- User lookup for the wall is by **ULID** (not username); a disabled wall returns 404.

## Notes

- **Not used by this app** (treat as user-triggered, not hot-loop polled). Documented for contract completeness.
- Field names observed from PR #4818 / RAWeb source; verify live before relying on specifics.

## Source

- [PR #4818](https://github.com/RetroAchievements/RAWeb/pull/4818) (merged 2026-05-05)

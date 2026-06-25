# Events

`GET /v2/events` — list events
`GET /v2/events/{id}` — get a single event

**Status: merged & live** — [#4711](https://github.com/RetroAchievements/RAWeb/pull/4711) (merged 2026-04-05).

Time-bound RA events (plus `EventAward`), backed by legacy game records. Today the resource exposes event + award-tier **definitions**, not per-user player state (it is groundwork for a future player-award resource).

## Notes

- **Not used by this app.** Documented for contract completeness.
- Field names observed from PR #4711 / RAWeb source; verify live before relying on specifics.

## Source

- [PR #4711](https://github.com/RetroAchievements/RAWeb/pull/4711) (merged 2026-04-05)

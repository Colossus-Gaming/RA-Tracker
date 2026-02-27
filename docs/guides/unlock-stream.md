# Building an Achievement Unlock Stream

If your core feature is "show what users just unlocked", polling is the most reliable approach today.

## 1. Poll Per User: `get-user-recent-achievements`

- Endpoint: `API_GetUserRecentAchievements.php`
- Local docs: [`get-user-recent-achievements`](../v1/get-user-recent-achievements.md)

Suggested strategy:

- Poll every 2-5 minutes (based on user count and rate limits).
- Use a request window with a small overlap buffer.
- Deduplicate with `(userId, achievementId, dateAwarded, hardcoreMode)`.

State management:

- Persist last processed timestamp per user (with overlap), or
- Keep a rolling set of recently seen event keys.

## 2. Backfill and Repair

For first imports or missed windows:

- [`get-achievements-earned-on-day`](../v1/get-achievements-earned-on-day.md)
- [`get-achievements-earned-between`](../v1/get-achievements-earned-between.md)

Day-by-day chunks are usually easiest and rate-limit friendly.

## 3. Enrichment

Recent unlock payloads are often enough for feed UIs. For richer displays:

- [`get-game-info-and-user-progress`](../v1/get-game-info-and-user-progress.md)
- [`get-game-extended`](../v1/get-game-extended.md)

## 4. Cache Aggressively

Static or slow-changing:

- console lists
- game lists
- game metadata
- achievement metadata

Dynamic:

- user progress
- recent unlocks
- leaderboard data

## 5. Treat Usernames as Mutable

Many endpoints accept username or ULID. Prefer durable identifiers where available.

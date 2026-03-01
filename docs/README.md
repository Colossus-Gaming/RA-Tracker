# RetroAchievements API Documentation

This folder contains API endpoint references, integration guides, and project plans for the RetroAchievements Layout Manager.

## Getting Started

### Get Your Web API Key

1. Sign in to RetroAchievements.
2. Open your control panel: https://retroachievements.org/controlpanel.php
3. Copy the Web API key from the Keys section.

Treat this key like a secret.

### Quick Start: HTTP

```bash
curl "https://retroachievements.org/API/API_GetTopTenUsers.php?y=YOUR_WEB_API_KEY"
```

### Official Client Libraries

- JavaScript/TypeScript: https://github.com/RetroAchievements/api-js
- Kotlin/JVM: https://github.com/RetroAchievements/api-kotlin

## Implementation Rules

These rules apply to API usage within this repository:

- Isolate auth and transport concerns from UI/view models.
- Normalize response payloads before model mapping.
- Avoid treating usernames as permanent identifiers.
- Cache slow-changing metadata aggressively.
- Poll dynamic resources conservatively and deduplicate events.

## Guides

- [Unlock Stream Polling](guides/unlock-stream.md): architecture for building achievement unlock feeds.
- [Connect API](guides/connect-api.md): session management, heartbeat pings, and achievement awards for standalone integrations.
- [v2 Status](guides/v2-status.md): current status of v2 API and migration guidance.

## V2 API Reference

The v2 API is in production and provides subset support, JSON:API compliance, and OAuth2 auth. Full reference: [v2/README.md](v2/README.md).

Key resources for this project:

- [Achievement Sets](v2/achievement-sets.md) — subset discovery (Core/Bonus/Specialty/Exclusive)
- [Player Achievement Sets](v2/player-achievement-sets.md) — per-subset user progress
- [Achievements](v2/achievements.md) — server-side type filtering (progression/win_condition/missable)
- [Player Games](v2/player-games.md) — game-level progress with subset breakdowns

## V1 API Endpoint Reference

The `v1/` directory contains endpoint-by-endpoint Markdown exports from the public API documentation. V1 is under a code freeze but continues to work. These are grouped by category below.

### Achievements

- [get-achievement-count](v1/get-achievement-count.md) — count of achievements per game
- [get-achievement-distribution](v1/get-achievement-distribution.md) — distribution by difficulty/points
- [get-achievement-of-the-week](v1/get-achievement-of-the-week.md) — weekly highlighted achievement
- [get-achievement-unlocks](v1/get-achievement-unlocks.md) — unlock counts for a specific achievement
- [get-achievements-earned-between](v1/get-achievements-earned-between.md) — achievements earned within a date range
- [get-achievements-earned-on-day](v1/get-achievements-earned-on-day.md) — achievements earned on a specific day

### Games

- [get-console-ids](v1/get-console-ids.md) — mapping of console IDs to names
- [get-game](v1/get-game.md) — basic game metadata
- [get-game-extended](v1/get-game-extended.md) — extended game metadata
- [get-game-hashes](v1/get-game-hashes.md) — ROM hash mapping for a game
- [get-game-info-and-user-progress](v1/get-game-info-and-user-progress.md) — game details combined with user progress
- [get-game-leaderboards](v1/get-game-leaderboards.md) — leaderboard data for a game
- [get-game-list](v1/get-game-list.md) — list games by console
- [get-game-progression](v1/get-game-progression.md) — unlock progression stats for a game
- [get-game-rank-and-score](v1/get-game-rank-and-score.md) — user rank/score for a specific game

### Users

- [get-user-awards](v1/get-user-awards.md) — mastery/completion awards
- [get-user-claims](v1/get-user-claims.md) — achievement claims by user
- [get-user-completed-games](v1/get-user-completed-games.md) — games completed or mastered
- [get-user-completion-progress](v1/get-user-completion-progress.md) — overall completion stats
- [get-user-game-leaderboards](v1/get-user-game-leaderboards.md) — user positions in game leaderboards
- [get-user-game-rank-and-score](v1/get-user-game-rank-and-score.md) — rank/score for a specific game
- [get-user-points](v1/get-user-points.md) — casual and hardcore points
- [get-user-profile](v1/get-user-profile.md) — basic profile info (ID, motto, avatar)
- [get-user-progress](v1/get-user-progress.md) — progress on a specific game
- [get-user-recent-achievements](v1/get-user-recent-achievements.md) — recently unlocked achievements (polling)
- [get-user-recently-played-games](v1/get-user-recently-played-games.md) — recently played games
- [get-user-set-requests](v1/get-user-set-requests.md) — achievement set requests
- [get-user-summary](v1/get-user-summary.md) — comprehensive stats (rank, points, completion)
- [get-user-want-to-play-list](v1/get-user-want-to-play-list.md) — want-to-play list

### Social

- [get-users-following-me](v1/get-users-following-me.md) — users following the queried user
- [get-users-i-follow](v1/get-users-i-follow.md) — users the queried user follows

### Claims

- [get-active-claims](v1/get-active-claims.md) — active achievement set claims
- [get-claims](v1/get-claims.md) — all achievement set claims

### Community

- [get-comments](v1/get-comments.md) — comments on achievements/games
- [get-recent-game-awards](v1/get-recent-game-awards.md) — recently awarded achievements feed
- [get-top-ten-users](v1/get-top-ten-users.md) — top-ranked users

### Leaderboards

- [get-leaderboard-entries](v1/get-leaderboard-entries.md) — leaderboard data by type

### Tickets

- [get-ticket-by-id](v1/get-ticket-data/get-ticket-by-id.md) — individual ticket details
- [get-most-recent-tickets](v1/get-ticket-data/get-most-recent-tickets.md) — recently created tickets
- [get-most-ticketed-games](v1/get-ticket-data/get-most-ticketed-games.md) — games with the most tickets
- [get-achievement-ticket-stats](v1/get-ticket-data/get-achievement-ticket-stats.md) — ticket stats for an achievement
- [get-developer-ticket-stats](v1/get-ticket-data/get-developer-ticket-stats.md) — ticket stats for a developer
- [get-game-ticket-stats](v1/get-ticket-data/get-game-ticket-stats.md) — ticket stats for a game

## Testing

- [Testing Data](testing/README.md): real API response samples and V1/V2 endpoint cross-reference.
- **Integration Tests**: FlaUI.UIA3 tests in `Retro Achievement Tracker.Tests/IntegrationTests/` launch the WPF app, interact via UI Automation (no focus stealing), and capture diagnostic logs. Run with: `dotnet test --filter "Category=Integration"`.

## Project Plans

- [Subset Notifications](plans/subset-notifications-plan.md): implementation plan for separate Core/Bonus/Specialty achievement notifications.

## Historical Upgrade Notes

- [.NET Upgrade Plan](../.github/upgrades/dotnet-upgrade-plan.md)
- [.NET Upgrade Report](../.github/upgrades/dotnet-upgrade-report.md)

## Canonical Sources

- API docs: https://api-docs.retroachievements.org/
- RAWeb endpoints: https://github.com/RetroAchievements/RAWeb/tree/master/public/API
- API docs repository: https://github.com/RetroAchievements/api-docs

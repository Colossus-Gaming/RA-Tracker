# API Integration Guide

This guide consolidates practical RetroAchievements API notes used by this project.

## API Surface

RetroAchievements currently exposes two APIs relevant to integrations:

- Web API (`/API/API_*.php`): public, documented endpoint set.
- Connect API (`/dorequest.php`): private integration API used by emulators/standalones.

Official references:

- https://api-docs.retroachievements.org/
- https://github.com/RetroAchievements/RAWeb/tree/master/public/API
- https://github.com/RetroAchievements/api-docs

## Credentials

### Web API key

Get your web API key from your RetroAchievements control panel:

- https://retroachievements.org/controlpanel.php

Treat it as a secret.

### Connect API token (standalone/integration workflows)

Fetch token via Connect API login:

```text
https://retroachievements.org/dorequest.php?u=YourUsername&p=YourPassword&r=login2
```

Always send a User-Agent header on Connect API requests.

## Quick Web API Usage

Basic request example:

```bash
curl "https://retroachievements.org/API/API_GetTopTenUsers.php?y=YOUR_WEB_API_KEY"
```

For client libraries, see official projects:

- JS/TS: https://github.com/RetroAchievements/api-js
- Kotlin: https://github.com/RetroAchievements/api-kotlin

## Recommended Unlock Stream Strategy

If your feature is "show recent unlocks", use polling with deduplication.

1. Poll `get-user-recent-achievements` every 2-5 minutes.
2. Include a small overlap buffer in each request window.
3. Deduplicate using `(userId, achievementId, dateAwarded, hardcoreMode)`.
4. Persist a high-water mark timestamp per user.

Backfill options for missed windows:

- `get-achievements-earned-on-day`
- `get-achievements-earned-between`

Enrichment options:

- `get-game-info-and-user-progress`
- `get-game-extended`

## Standalone Integration Flow (Condensed)

1. Create a dedicated integration account.
2. Get web API key and connect token.
3. Link players to RA account (commonly via profile motto verification).
4. Start a session when gameplay begins:

```text
https://retroachievements.org/dorequest.php?u=YourUsername&t=YourToken&r=startsession&g=GameId&k=PlayerUsername
```

5. Send heartbeat pings periodically:

```text
https://retroachievements.org/dorequest.php?u=YourUsername&t=YourToken&r=ping&g=GameId&k=PlayerUsername
```

6. Award unlocks using:
- `awardachievement` (single unlock)
- `awardachievements` (batch unlock)

## "API v2" Status

As of February 25, 2026, production integrations should still target the currently documented Web API endpoints.

In community discussions, "v2" usually refers to in-progress modernization efforts (for example OAuth2 and improved filtering/sorting), not a complete production replacement today.

Useful tracking links:

- Discussion: https://github.com/RetroAchievements/RAWeb/discussions/2081
- PR query: https://github.com/RetroAchievements/RAWeb/pulls?q=feat%28api%29+V2

## Implementation Guidance for This Repo

- Keep transport/auth concerns isolated from app logic.
- Normalize API responses before mapping to view models.
- Treat usernames as mutable; prefer durable identifiers when available.
- Cache static metadata aggressively and poll dynamic resources conservatively.

# Standalone Integration Guide

This guide summarizes a practical integration path for standalone frontends/services using RetroAchievements Web API and Connect API.

## Prerequisites

- Dedicated integration account (for example, an account named after your standalone).
- One or more associated game pages (typically under the Standalones console).
- Web API key.
- Connect API token.

If you need standalone game setup help, contact RA administrators on RetroAchievements.

## APIs Involved

- Web API: endpoint-based API documented in this docs folder (`../v1/*.md`).
- Connect API: `dorequest.php` command API used for sessions/pings/unlocks.

## Security Requirements

- Treat Web API key and Connect API token as secrets.
- Always send a User-Agent header for Connect API requests.

Recommended User-Agent pattern:

```text
{FrontendName}/{version} ({platform}) {Integration}/{version}
```

## Obtain Connect API Token

```text
https://retroachievements.org/dorequest.php?u=YourUsername&p=YourPassword&r=login2
```

## Account Linking (Current Practical Pattern)

Until OAuth2 is broadly available, a common pattern is:

1. Ask for the user's RA username.
2. Ask user to place a generated verification token in their profile motto.
3. Verify via Web API profile lookup.
4. Instruct user to remove/reset the motto token.

## Start Session

```text
https://retroachievements.org/dorequest.php?u=YourUsername&t=YourConnectToken&r=startsession&g=YourCoreGameId&k=TheirUsername
```

## Heartbeat Ping

```text
https://retroachievements.org/dorequest.php?u=YourUsername&t=YourConnectToken&r=ping&g=YourCoreGameId&k=TheirUsername
```

Optional rich presence payload field:

```text
m=Current status text
```

## Award a Single Achievement

```text
https://retroachievements.org/dorequest.php?u=YourUsername&t=YourConnectToken&r=awardachievement&k=TheirUsername&a=9&v=HASH&h=1
```

`v` is an MD5 hash of:

```text
achievementId + username + hardcore + achievementId
```

Example:

```text
md5("9TheirUsername19")
```

## Award Multiple Achievements

Use `awardachievements` with a multipart payload:

- `a`: CSV of achievement IDs
- `h`: hardcore flag (`1` or `0`)
- `v`: MD5 of `a + username + h`

Endpoint:

```text
https://retroachievements.org/dorequest.php?u=YourUsername&t=YourConnectToken&r=awardachievements&k=TheirUsername
```

## Related Docs

- [Documentation Index](../README.md)
- [Unlock Stream Polling](unlock-stream.md)
- [v2 Status](v2-status.md)

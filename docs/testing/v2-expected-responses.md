# V2 Expected Response Shapes

V2 endpoints use the **JSON:API** specification. All responses follow a consistent envelope structure. These expected shapes are derived from the [V2 API docs](../v2/README.md) and the merged RAWeb PRs.

> **Cloudflare Note:** V2 endpoints are behind Cloudflare bot protection. Plain `curl` cannot reach them — the app's `V2Client.cs` (which runs in a .NET HttpClient) should work fine, but CLI testing requires a browser-based approach or a tool that handles JS challenges.

## Common Envelope

```json
{
  "data": { ... } | [ ... ],
  "links": {
    "first": "https://retroachievements.org/api/v2/...?page[number]=1",
    "last": "https://retroachievements.org/api/v2/...?page[number]=N",
    "prev": null,
    "next": "https://retroachievements.org/api/v2/...?page[number]=2"
  },
  "meta": {
    "total": 77,
    "perPage": 50,
    "currentPage": 1,
    "lastPage": 2
  },
  "included": [ ... ]
}
```

## Authentication

```
X-API-Key: {web_api_key}
```
or
```
Authorization: Bearer {oauth_token}
```

---

## GET /api/v2/games/1446

```json
{
  "data": {
    "id": "1446",
    "type": "game",
    "attributes": {
      "title": "Super Mario Bros.",
      "sortTitle": "super mario bros",
      "system": "NES/Famicom",
      "achievementsPublished": 77,
      "pointsTotal": 775,
      "pointsWeighted": 3685,
      "playersTotal": 68465,
      "badgeUrl": "https://retroachievements.org/Images/036035.png",
      "forumTopicId": 282,
      "guideUrl": null,
      "imageIconUrl": "/Images/036035.png",
      "imageTitleUrl": "/Images/079350.png",
      "imageIngameUrl": "/Images/000387.png",
      "imageBoxArtUrl": "/Images/013238.png",
      "publisher": "Nintendo",
      "developer": null,
      "genre": "2D Platforming",
      "releasedAt": "1985-09-13",
      "releasedAtGranularity": "day",
      "createdAt": "...",
      "updatedAt": "2026-02-27T18:19:38.000000Z"
    },
    "relationships": {
      "system": { "data": { "id": "7", "type": "system" } },
      "achievements": { "links": { "related": "/api/v2/games/1446/achievements" } },
      "leaderboards": { "links": { "related": "/api/v2/games/1446/leaderboards" } },
      "hashes": { "links": { "related": "/api/v2/games/1446/hashes" } }
    }
  }
}
```

---

## GET /api/v2/games/1446/achievements?page[size]=3

```json
{
  "data": [
    {
      "id": "3159",
      "type": "achievement",
      "attributes": {
        "title": "Shroooooms...",
        "description": "Find and collect a Magic Mushroom",
        "points": 1,
        "truePoints": 1,
        "type": null,
        "displayOrder": 1,
        "badgeUrl": "/Badge/321909.png",
        "badgeLockedUrl": "/Badge/321909_lock.png",
        "author": "Scott",
        "createdAt": "2013-10-06T01:38:51.000000Z",
        "updatedAt": "2023-08-12T10:43:54.000000Z"
      },
      "relationships": {
        "game": { "data": { "id": "1446", "type": "game" } }
      }
    },
    {
      "id": "3158",
      "type": "achievement",
      "attributes": {
        "title": "Now You're Playing With Fire!",
        "description": "Find and collect a Fire Flower",
        "points": 1,
        "truePoints": 1,
        "type": null,
        "displayOrder": 2
      }
    },
    {
      "id": "3223",
      "type": "achievement",
      "attributes": {
        "title": "I'm a Super Star!",
        "description": "Find and collect a Starman",
        "points": 1,
        "truePoints": 1,
        "type": null,
        "displayOrder": 3
      }
    }
  ],
  "links": { "first": "...", "last": "...", "prev": null, "next": "..." },
  "meta": { "total": 77, "perPage": 3, "currentPage": 1, "lastPage": 26 }
}
```

### Achievement `type` values

| Value | Meaning | V1 equivalent |
|-------|---------|---------------|
| `null` | Standard achievement | `"type": null` |
| `"progression"` | Marks story/gameplay progress | `"type": "progression"` |
| `"win_condition"` | Marks game completion | `"type": "win_condition"` |
| `"missable"` | Can be permanently missed | `"type": "missable"` |

V2 adds server-side filtering: `?filter[type]=progression,win_condition`

---

## GET /api/v2/games/1446/achievement-sets

This is the **key V2-only resource** for subset support. Games without subsets return a single Core set.

```json
{
  "data": [
    {
      "id": "1446-core",
      "type": "achievement-set",
      "attributes": {
        "title": "Core",
        "type": "core",
        "achievementsPublished": 77,
        "pointsTotal": 775,
        "pointsWeighted": 3685,
        "imageIconUrl": "/Images/036035.png",
        "updatedAt": "2026-02-27T18:19:38.000000Z"
      },
      "relationships": {
        "game": { "data": { "id": "1446", "type": "game" } },
        "achievements": { "links": { "related": "/api/v2/achievement-sets/1446-core/achievements" } }
      }
    }
  ]
}
```

For a game **with subsets** (e.g., a hypothetical game with Bonus set):
```json
{
  "data": [
    {
      "id": "{gameId}-core",
      "type": "achievement-set",
      "attributes": {
        "title": "Core",
        "type": "core",
        "achievementsPublished": 50,
        "pointsTotal": 500
      }
    },
    {
      "id": "{subsetGameId}-bonus",
      "type": "achievement-set",
      "attributes": {
        "title": "[Subset - Challenge Runs]",
        "type": "bonus",
        "achievementsPublished": 25,
        "pointsTotal": 400
      }
    }
  ]
}
```

Set types: `core`, `bonus`, `specialty`, `exclusive`

---

## GET /api/v2/users/{ulid}/player-achievement-sets?filter[gameId]=1446&include=achievementSet

This is the **per-subset progress** resource. Critical for STORY-001 (subset notifications).

```json
{
  "data": [
    {
      "id": "87358-1446-core",
      "type": "player-achievement-set",
      "attributes": {
        "achievementsUnlocked": 0,
        "achievementsUnlockedHardcore": 0,
        "achievementsUnlockedSoftcore": 0,
        "pointsHardcore": 0,
        "pointsSoftcore": 0,
        "pointsWeightedHardcore": 0,
        "completionPercentage": "0.00",
        "completionPercentageHardcore": "0.00",
        "beatenAt": null,
        "beatenHardcoreAt": null,
        "completedAt": null,
        "completedHardcoreAt": null,
        "lastActivityAt": null
      },
      "relationships": {
        "user": { "data": { "id": "01D0TJC2MP13HPD4GDMYACAF2Q", "type": "user" } },
        "achievementSet": { "data": { "id": "1446-core", "type": "achievement-set" } }
      }
    }
  ],
  "included": [
    {
      "id": "1446-core",
      "type": "achievement-set",
      "attributes": {
        "title": "Core",
        "type": "core",
        "achievementsPublished": 77,
        "pointsTotal": 775
      }
    }
  ]
}
```

---

## GET /api/v2/users/{ulid}/player-games?filter[gameId]=1446

```json
{
  "data": [
    {
      "id": "87358-1446",
      "type": "player-game",
      "attributes": {
        "achievementsUnlocked": 0,
        "achievementsUnlockedHardcore": 0,
        "achievementsUnlockedSoftcore": 0,
        "pointsHardcore": 0,
        "pointsSoftcore": 0,
        "completionPercentage": "0.00",
        "completionPercentageHardcore": "0.00",
        "firstUnlockAt": null,
        "firstUnlockHardcoreAt": null,
        "beatenAt": null,
        "beatenHardcoreAt": null,
        "completedAt": null,
        "completedHardcoreAt": null,
        "lastPlayedAt": null,
        "playtimeTotal": 562,
        "timeToBeat": null
      },
      "relationships": {
        "user": { "data": { "id": "01D0TJC2MP13HPD4GDMYACAF2Q", "type": "user" } },
        "game": { "data": { "id": "1446", "type": "game" } }
      }
    }
  ]
}
```

---

## GET /api/v2/users/01D0TJC2MP13HPD4GDMYACAF2Q

```json
{
  "data": {
    "id": "01D0TJC2MP13HPD4GDMYACAF2Q",
    "type": "user",
    "attributes": {
      "displayName": "RetroS3xual",
      "avatarUrl": "/UserPic/RetroS3xual.png",
      "isMuted": false,
      "joinedAt": "2019-01-10T00:25:12.000000Z",
      "points": 23445,
      "pointsSoftcore": 0,
      "pointsWeighted": 105587,
      "richPresenceMsg": "Playing Gauntlet Legends",
      "lastActivityAt": "2024-06-28T23:08:53.000000Z",
      "motto": "twitch.tv/RetroS3xual"
    }
  }
}
```

---

## GET /api/v2/games/1446/hashes

> **Status:** PR open ([#4593](https://github.com/RetroAchievements/RAWeb/pull/4593)), not yet merged

```json
{
  "data": [
    {
      "id": "hash-1",
      "type": "game-hash",
      "attributes": {
        "md5": "8e3630186e35d477231bf8fd50e54cdd",
        "name": "Super Mario Bros. (World).nes",
        "labels": ["nointro"],
        "compatibility": "compatible",
        "patchUrl": null
      }
    },
    {
      "id": "hash-2",
      "type": "game-hash",
      "attributes": {
        "md5": "293303fe565de2333bc1e4115d38fa3f",
        "name": "Super Mario Bros. (Japan).fds",
        "labels": ["nointro"],
        "compatibility": "compatible",
        "patchUrl": null
      }
    }
  ]
}
```

Note: V2 returns `labels` as a proper JSON array (V1 returns comma-separated string).

---

## GET /api/v2/hubs?filter[title]=Mario

```json
{
  "data": [
    {
      "id": "123",
      "type": "hub",
      "attributes": {
        "title": "[Series - Super Mario]",
        "sortTitle": "series super mario",
        "badgeUrl": "/Images/...",
        "hasMatureContent": false,
        "gamesCount": 42,
        "linkedHubsCount": 3,
        "isEventHub": false,
        "createdAt": "...",
        "updatedAt": "..."
      }
    }
  ]
}
```

Hubs have no V1 equivalent. Use dedicated relationship endpoints for games/links (not `?include=`).

---

## GET /api/v2/leaderboards/{id}/entries?page[size]=3

```json
{
  "data": [
    {
      "id": "entry-1",
      "type": "leaderboard-entry",
      "attributes": {
        "score": 999999,
        "formattedScore": "999,999",
        "rank": 1,
        "createdAt": "...",
        "updatedAt": "..."
      },
      "relationships": {
        "user": { "data": { "id": "...", "type": "user" } },
        "leaderboard": { "data": { "id": "...", "type": "leaderboard" } }
      }
    }
  ]
}
```

V2 adds `formattedScore` (server-formatted) and correct tie-aware ranking via SQL RANK().

# Retro Achievement Tracker — User Guide

A guide for setting the tracker up and getting it into OBS. No command line needed.

---

## 1. Install

1. Download **`RATracker-Setup-<version>.exe`** from the
   [latest release](https://github.com/Colossus-Gaming/RA-Tracker/releases/latest).
2. Double-click it and follow the wizard. It asks where to install.

Windows will probably show **"Windows protected your PC"**. This is because the app is not
code-signed yet, not because anything is wrong with it. Click **More info → Run anyway**.

Your antivirus may also object for the same reason. If the install fails, a file called
**`RATracker-Install-Report.txt`** appears on your Desktop and opens automatically — send that file
to the developer and it will say exactly what went wrong.

Nothing else needs installing. The app is self-contained.

---

## 2. First run — sign in

The tracker needs two things from your RetroAchievements account.

1. **Username** — your RA username.
2. **Web API key** — from [retroachievements.org/controlpanel.php](https://retroachievements.org/controlpanel.php),
   under **Keys**. It is a long string of letters and numbers. This is *not* your password.

Type both into the boxes at the top of the window, then press **▶ Start**.

Optionally tick **Remember** so you do not have to type them again.

---

## 3. "It doesn't show the game I'm playing"

The tracker does not watch your emulator. It asks RetroAchievements which game **your account** most
recently played. So:

- **Press ▶ Start.** Nothing is fetched until polling is running. The button turns into **■ Stop**
  when it is working.
- **Launch a game in an emulator that is logged in to RetroAchievements.** The site only registers a
  game once the emulator reports it. If you have not played anything since setting up RA, there is
  nothing for the tracker to find.
- **Give it a few seconds.** The tracker re-checks on a timer, it is not instant.

If the dashboard shows a game you played previously rather than the one open right now, RA has not
registered the new session yet. Play for a few seconds and it will catch up.

> A game whose achievement set has been demoted shows as **0 / 0 achievements**. That is correct —
> the tracker deliberately shows the game you are actually playing rather than silently skipping to a
> different one.

---

## 4. The overlay windows look "wrong"

They are supposed to. The overlay windows have **no title bar and no border**, and their background
is fully transparent. That is what makes them usable as a stream overlay — OBS captures only the
content, with nothing framing it.

Open them from the **Overlay Windows** panel on the dashboard. Each has a **⚙ button** next to it
that opens its settings page.

### Moving and sizing an overlay

Because there is no title bar, dragging works differently:

- Turn on **Position Mode** for that overlay (checkbox in its settings page). A dashed orange outline
  appears so you can see where the window actually is.
- Drag it anywhere, and resize from the bottom-right corner.
- Press **P**, or untick Position Mode, to hide the outline again.

Always turn Position Mode **off** before streaming — the outline is visible to OBS.

### Adding one to OBS

1. In OBS: **Sources → + → Window Capture**.
2. Pick the overlay window (for example *Alerts Overlay*).
3. Set **Capture Method** to *Windows 10 (1903 and up)*.
4. Tick **Client Area**.

Repeat per overlay you want on stream.

---

## 5. Alerts

### Testing them

Dashboard → the **⚙** next to **Alerts** → **Alerts Settings**. There are four test buttons:

| Button | What it previews |
|---|---|
| Test Achievement | A normal achievement unlock |
| Test Mastery | Mastering a game |
| Test Subset Achievement | An unlock from a Bonus/Specialty set |
| Test Subset Mastery | Completing a subset |

The Alerts overlay opens automatically when you press one. If it is off-screen, turn on Position Mode
to find it.

### Changing the alert's shape

Same page, **Alert Container** section: width, height, padding, corner radius, badge position
(left / right / top / bottom / hidden), badge size and spacing. **Height 0** means "size to fit the
text", which is the default banner look. Moving the badge to the top turns the banner into a card.

### Using your own alert video

Same page, **Custom Video Alerts** section.

1. Tick **Use a custom video for achievement alerts**.
2. **Browse…** to your `.webm` file. Transparent (alpha) WebM works — that is the point.
3. Set the position and timing:

| Setting | Meaning |
|---|---|
| X / Y | Where the video sits. Negative values move it up/left, off the edge |
| Scale | Multiplier on the video's own size. `2.00` is double size |
| **In (ms)** | **Point in the video** where the achievement panel appears |
| **Out (ms)** | **Point in the video** where it leaves |
| In / Out speed (ms) | How long those animations take. `0` snaps instantly |
| In / Out-Animation | Direction it moves. `Static` means no movement |

> **In and Out are positions on the video's timeline, not durations.** `In = 4000` means the panel
> appears 4 seconds into the video. This lets the text land on a specific frame of your animation.

Mastery alerts have their own identical set of settings.

### Positioning it visually

Press **Edit Layout** on that page. The alert is pinned on screen with your video looping behind it:

- **Drag the panel** to move it
- **Drag anywhere else** to move the video
- **Scroll** to scale the video
- A readout in the corner shows the exact numbers as you drag

Press **Done Editing** when finished. Everything saves as you go.

> If your video is scaled larger than the overlay window, you will only see part of it. Make the
> Alerts overlay window bigger (Position Mode → drag the corner) — the window size is remembered.

---

## 6. Updates

The app updates itself. It checks on startup, downloads in the background, and installs when you next
close it — so an update can never interrupt a stream. When one is waiting you will see a note in the
top bar.

---

## 7. Something went wrong

Crash details are written to:

```
%APPDATA%\RATracker\logs\
```

Paste that path into the Explorer address bar. Send the newest file in there.

For a failed **install**, the report is on your Desktop instead — see section 1.

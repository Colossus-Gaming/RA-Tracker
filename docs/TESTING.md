# Test checklist — v1.2.1

Thanks for testing. This should take about 15 minutes.

You do not need to type any commands. Everything here is clicking.

If a step does not work, that is useful — write down what you saw and move on to the next
step. Do not try to fix anything.

---

## ⚠️ Step 0 — Do this FIRST, before anything else

**Do not re-enter your API key yet, even if the app looks broken.**

This release changed how your saved key is stored. The most important thing to find out is
whether your *existing* saved key still works after the update. If you type it in again, that
answer is lost and cannot be recovered.

So: open the app, look, and report — but do not retype anything until Step 3.

---

## Step 1 — Update and check the version

The app updates itself. Open it, leave it running for about a minute, then close it. The
update installs as it closes. Open it again.

Now check which version you have:

1. Press the **Windows key**, type `Installed apps`, press Enter.
2. Find **RATracker** in the list.
3. The version number is shown next to it.

| | |
|---|---|
| **You should see** | `1.2.1` |
| **Write down** | The version you actually see |

If it still says `1.2.0` or `1.1.0`, wait a few minutes, close and reopen once more. If it
still does not change, tell me — that means auto-update is not working, which is important
to know.

---

## Step 2 — Is your API key still there? (most important)

Look at the top of the window, at the **Username** and **Web API key** boxes.

| Question | Answer |
|---|---|
| Is your username still filled in? | yes / no |
| Is the API key box still filled in (dots or characters)? | yes / no |

Now press **▶ Start**.

| Question | Answer |
|---|---|
| Does it start working, or show an error? | |
| What does the small text under the buttons say? | |

**This is the single most important result in this whole document.** If your saved key stopped
working, I need to know before other people update.

---

## Step 3 — Now you can re-enter the key if needed

If Step 2 failed, type your username and API key in again and press **▶ Start**.

| Question | Answer |
|---|---|
| Does it work after re-entering? | yes / no |

---

## Step 4 — Language

This is the new feature.

1. Click the **⚙ gear** icon to open **General Settings**.
2. Find the **Language** dropdown.
3. Choose **Deutsch**.

| | |
|---|---|
| **You should see** | The text changes to German immediately, with no restart |
| **Write down** | Did it change instantly? Did anything stay English that should not have? |

Then try **System default**.

| | |
|---|---|
| **You should see** | The app follows whatever language Windows is set to |
| **Write down** | What language did it switch to? Is that the language your Windows is in? |

---

## Step 5 — German translation quality (you are the only person who can do this)

The German was machine-translated. It has **not** been checked by a German speaker. You are
the first one to see it.

Set the language to **Deutsch** and look around the app — the main screen, the buttons down
the right side, and the settings pages.

Please tell me:

- Anything that is **wrong** or means something different than intended
- Anything that sounds **strange, robotic, or not how a German gamer would say it**
- Anything **cut off** or overlapping other text
- Better words if you can think of them

Be blunt. "That word is wrong, it should be X" is exactly what I need.

Two things are **expected** and not bugs:

- **Achievement names and game names stay in English.** Those come from RetroAchievements
  itself and only exist in English.
- **Some settings pages are still English.** The deep configuration pages are not translated
  yet. The main screen should be fully German — if part of the *main screen* is still English,
  that IS a bug, tell me.

---

## Step 6 — Three things that were broken and should now be fixed

These were broken in v1.2.0. Please confirm they look right now.

### 6a. The Stop button

Press **▶ Start**. The button becomes the stop button.

| | |
|---|---|
| **You should see** | A **square** ■ next to the word Stop |
| **Was broken as** | A play triangle ▶ next to the word Stop |

### 6b. Spacing around the colons

Look at the **Current Game** card on the main screen.

| | |
|---|---|
| **You should see** | `Set: ` and `Progress: ` — the colon tight against the word |
| **Was broken as** | `Set   :   ` — a gap before the colon |

### 6c. Buttons not cut off in German

With the language set to **Deutsch**, look at the three buttons under **Current Focus**
(the Prev / Set Focus / Next row).

| | |
|---|---|
| **You should see** | Full German words, nothing cut off |
| **Was broken as** | Words cut off in the middle, like `Imposta fo` |

---

## Step 7 — The things you reported before

### Does it show the game you are playing?

1. Press **▶ Start** in the tracker.
2. Open an emulator that is logged in to RetroAchievements and start a game.
3. Play for 30 seconds.
4. Look at the tracker.

| Question | Answer |
|---|---|
| Does the **Current Game** card show the right game? | yes / no |
| If not, what game does it show? | |

Note: the tracker asks RetroAchievements what your **account** last played. It does not watch
your emulator. So it can be a few seconds behind.

### Mastery alert

1. Click the **⚙** next to **Alerts** to open **Alerts Settings**.
2. Press **Test Mastery**.

| | |
|---|---|
| **You should see** | An alert appear showing `Sample Game`, even with no game loaded |
| **Was broken as** | Nothing happened at all |

Also press **Test Achievement**, **Test Subset Achievement** and **Test Subset Mastery**.

| Question | Answer |
|---|---|
| Which of the four test buttons worked? | |

---

## How to send results back

Copy the questions above with your answers. Rough notes are fine — you do not need to write
full sentences.

**Screenshots are very helpful**, especially for anything that looks wrong in German. Press
`Windows key + Shift + S` to take one.

**If the app crashes**, send the newest file from this folder:

1. Press `Windows key + R`
2. Type `%APPDATA%\RATracker\logs` and press Enter
3. Send the newest file

**If an install fails**, a file called `RATracker-Install-Report.txt` appears on your Desktop
and opens by itself. Send that.

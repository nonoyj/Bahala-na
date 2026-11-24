# Handoff — frontend → backend/content

**From:** frontend (Unity client)
**Date:** 2026-11-24
**Branch:** `frontend/unity-6.3-upgrade`
**Deadline:** 2026-12-09

This file supersedes `BACKEND_HANDOFF.md` and `ROADMAP.md` where they disagree.

---

## 0. Read these five things first

1. **Install Unity `6000.3.29f1`** (6.3 LTS). The project will not open cleanly on anything else.
2. **`unity/` is frontend-owned.** Do not reformat, re-save, move, or `git add -A` anywhere in the repo. Every `.meta` file is committed; a stray resave produces GUID churn that is painful to unwind.
3. **The game is a landscape, Pokémon-style RPG**, not a menu-based card game. That is the current architecture.
4. **The client runs fully offline.** No Firebase SDK is imported. See §4.1.
5. **Nothing is merged to `main` yet.** See §5 for the merge gate and what remains.

---

## 1. Where we are

### 1.1 Working and verified

| Area | State |
|---|---|
| Unity editor | Upgraded 6000.0.31f1 → **6000.3.29f1** |
| Compile | **0 errors, 0 warnings** |
| UI layout | Landscape, 1920×1080 reference, centred 1180-wide content column, no clipping |
| Screens | Menu, Character Spark, Barangay, Paano Maglaro, Settings, Tungkol, card encounter, outcome, climax, ending — all render and navigate |
| Content | 40 cards + echoes + endings + eras + backgrounds loaded from `StreamingAssets/Content/` via `ContentDatabase` |
| Dice | d20 + stat modifier + assists, DC 8/12/15/18, resolves and reports |
| World | Grid-step movement, 4-direction facing, 2-frame walk, camera follow clamped to map |
| Depth sorting | Characters render in front of the plaza; Y-based sorting |
| Collision | 32×18 grid over the plaza, painted in-game via `CollisionPainter` |
| Art | Plaza, 2 interiors, 4 NPC portraits, 2 character atlases — all imported and sliced |
| Repo hygiene | Only deliberate paths staged; no `Library/`, `Temp/`, or build output committed |

### 1.2 What broke and got fixed, so you don't reintroduce it

These were real defects in code that had **never been compiled by anyone** before this branch:

| Defect | Fix |
|---|---|
| `Destroy(...)` called outside a `MonoBehaviour` (2 sites) | `UnityEngine.Object.Destroy(...)` |
| `Toggle.Transition.None` bound to a local method, not the type | `UnityEngine.UI.Selectable.Transition.None` |
| `ContentDatabase.Mod()` is `static`, called on an instance (2 sites) | qualified with the type name |
| `int[].Contains(string)` — type mismatch | explicit string comparisons |
| `Choice.Bind(Card)` called but never written | added the field + method to `Models.cs` |
| **No `EventSystem` existed anywhere** — every UI click was dead, silently | created in `UIManager` with `StandaloneInputModule` |
| `AddComponent<MonoBehaviour>()` — illegal, threw in `UIManager` ctor | `UIRunner : MonoBehaviour` host class |
| `ContentLoader.Start(IEnumerator)` — Unity reserves parameterless `Start`, so `AddComponent` **refused to attach** and content never loaded | renamed to `Begin(...)` |
| Emoji in UI strings — LiberationSans has no emoji coverage; every one logged a warning and drew a `□` | stripped from all `.cs`; TMP atlas set to **Dynamic** so `₱` and Latin symbols work |
| Plaza drawn offset by half a map (Center pivot vs. bottom-left grid origin) | transform shifted by half the sprite size |
| Sprite sorting went negative → character drew behind the plaza | base offset of 2000 in `sortingOrder` |

**Do not "clean up" any of these.** Each one was load-bearing.

---

## 2. What we are doing now

The client is being converted from a full-screen card UI into a **landscape, top-down RPG** with the existing card system as the *encounter layer*:

```
World (new)          walk, collide, camera follow, interact
   ↓ walk into a trigger
Encounter (existing) fade → interior art → NPC portrait dialogue → card choices
   ↓ resolved
World (new)          return to plaza, NPC/quest state advances
```

The card flow itself was never broken — cards, dice, outcomes, endings all work. The world layer wraps it.

### Scope decision for 2026-12-09 — vertical slice

| In the demo | Cut |
|---|---|
| One walkable barangay plaza (32×18 grid over a painted backdrop) | Multiple maps |
| 4 story NPCs + ~8 ambient NPCs | 40 cards live in the world |
| Encounter screens for 2 interiors (sari-sari store, barangay hall) | Walkable interiors |
| 1 main quest + 4 one-beat errands | Branching quest chains |
| **3 of 10 endings reachable** | All 10 |
| Skippable intro + attract screen | Animation beyond the d20 |
| Pitch page + one recorded playthrough | — |

**This is a scope change and needs minuting** as **D-015**. The *systems* still support 10 endings; the demo exposes 3.

---

## 3. What we are about to do

In order, next working sessions:

| # | Task | Notes |
|---|---|---|
| 1 | Finish painting collision | `CollisionPainter` shipped; needs F2 save after painting |
| 2 | `EncounterFlow.cs` | door trigger → fade → interior → walk in 2 tiles → portrait slides in → dialogue → card over art → fade back to plaza |
| 3 | NPC map sprites | 4 characters × 4 directions × 2 frames, same 89×170 cell as the player |
| 4 | Place 4 story NPCs + 8 ambient walkers | ambient = walk cycle, no interaction |
| 5 | Wire 2 interior door transitions | |
| 6 | **Full 18-card run to an ending** | **merge gate** |
| 7 | Skippable intro + attract screen | see §3.1 |
| 8 | Home screen redesign pass | |
| 9 | 5 quests on flags | reuses `GameManager.HasFlag` |
| 10 | 3 endings reachable in demo build | |
| 11 | Fade system, d20 roll animation, result reveal | |
| 12 | UI kit — panels, buttons, icons, frames | |
| 13 | Android build: `ph.bahalana.game`, landscape-locked | first APK |
| 14 | Device test + perf pass | texture compression → ASTC |
| 15 | **Pitch page** + gameplay capture | |
| 16 | PR to `main` | |

### 3.1 Intro / attract — the plan

- **Cold open (4s, no input):** black screen, three Tagalog lines fading in and out — *"Isang barangay. Isang gabi. Isang desisyon." / "Walang tama o mali." / "May kwento."* Then the title. **Zero art required.**
- **Home screen:** existing menu kept; plaza image behind it at ~25% brightness so text stays legible and the game has a place from the first second.
- **Skippable intro (3 stills, 2.5s each):** plaza at dusk → silhouette at the flagpole → close on the d20. Caption at the bottom, **"Laktawan / Skip"** always available. This is where the backstory currently stranded in the About screen belongs.
- Total: **half a day.** Text, fades, one background image.

---

## 4. Your job (backend / content)

Answer these **in writing**. Do not edit `unity/` to "fix" a mismatch without agreement.

### 4.1 Which backend is active at boot? — blocking decision

`GameApp.CreateBackend()` picks an implementation. The in-game About screen reports **`v1 · Offline`**, so the client is currently running **fully offline** with device-local saves.

Confirm one of:
- **(a)** offline is correct for the demo, and `FirebaseBackend.cs` stays unused → say so explicitly, and I will mark it dead code
- **(b)** Firebase must be live by 2026-12-09 → this is a **frontend blocker** and needs an owner decision now, not a silent assumption

### 4.2 Firebase SDK is not imported

`unity/Packages/manifest.json` has **no** `com.google.firebase.*` package and no `ExternalDependencyManager`. `FirebaseBackend.cs` exists but cannot compile against a live SDK as things stand. Firebase Unity SDK **14.2.0** is current; Auth + Firestore only.

If Firebase is required, this is **days of work** (SDK import, `google-services.json`, project settings, Auth flow, Firestore rules) and it is **not** in the current schedule. Decide.

### 4.3 Shared Barangay read path

The client displays a live era pill (`Karaniwan · 37% nagsalita · tipping point 40%`) and a Barangay screen with Pag-asa 45/100, Tiwala 60/100, Imprastruktura 35, Walang Kibo 0. Confirm:

- exact document path and field names to read
  (working assumption: `barangay/state` with `tiwala`, `imprastruktura`, `pagAsa`, `walangKibo`, `era`, `version`)
- that it is **public-read**, since the client reads without auth
- that Cloud Function `submitRunContribution` is the **only** writer
- how the client should react when the document is missing (first run, unseeded emulator). A seed of **Pag-asa 45** was specified — confirm the seed is actually written on project init
- what `Runs: 24` in the UI means and whether it is global or per-player

### 4.4 Endings and echoes payloads

The client renders endings and echoes from **local JSON** (`StreamingAssets/Content/`). If the backend computes or overrides any ending/echo text, say which, and send the authoritative shape. Otherwise local JSON is authoritative and the backend must not duplicate it.

### 4.5 Content changes go through me

If a card's text, choices, or effects change, the JSON in `unity/Assets/StreamingAssets/Content/cards/` must be re-copied. There is an editor script `Assets/Editor/ContentExport.cs`. **Tell me when content changes** rather than editing the client copy directly — otherwise the two drift and the client ships stale text.

### 4.6 Decide the two design findings

**D-011 — Singil trigger rate.** Measured on the canonical config (`balance 5000`, `newcomer @17%`): **34.9%** of runs see a Singil card against a target of **≥50%**. Singil fires at `Utang ≥ 3`.

Measured alternative: **`Utang ≥ 2` → 52.5%** at 15% newcomer weight, **58.0%** at 17%. That is the fix, but it was **deliberately reverted earlier** because it is a design decision, not a bug. It needs the content/narrative owner's sign-off.

Per-lane: honest 0/0 · padrino 100/100 · collective 33.2/14.3 · newcomer 62.2/34.9 · passive 0/0.

Other canonical numbers: shortcuts ≥2 **62.2%** (target ~60 ✅), spoke up 68.7%, silent somewhere 29.1%, Silent Majority 47.7%, **10/10 endings reachable** ✅.

**D-015 — vertical slice scope.** Confirm 3-of-10 endings for the 2026-12-09 demo. See §2.

### 4.7 Add the owed decision entries to `DECISIONS.md`

**D-014 — Unity editor upgrade.** Project upgraded to 6000.3.29f1. `ProjectVersion.txt` had been pinned to 6000.0.31f1 while §C.1 locks 6.3 LTS. Only `ProjectVersion.txt`, `Packages/manifest.json`, generated `ProjectSettings/*.asset`, `packages-lock.json` and `.meta` files changed. **All team members must install 6000.3.29f1.**

**D-015 — Demo scope is a vertical slice.** See §2.

---

## 5. Merge gate

`main` is the shared baseline. **Nothing goes to `main` until all four pass:**

| # | Gate | Status |
|---|---|---|
| 1 | Console shows 0 errors across a full session | ✅ |
| 2 | Menus and cards respond to clicks | ✅ |
| 3 | **One complete 18-card run reaches an ending** | ❌ **not yet done** |
| 4 | Landscape renders without clipping on a 16:9 and a 20:9 device | ⚠️ desktop 16:9 ✅, device ❌ |

**Gate 3 has not been verified since the world layer landed.** The card flow worked from the menu before the landscape change; it needs re-confirming end to end. Until then, `frontend/unity-6.3-upgrade` is staging.

Merge via **pull request** into `main`. Never `git push origin main` from a local branch.

---

## 6. Rules that must not be broken

- **Utang is never rendered as a number.** Only `SaveManager.UtangHint()` prose.
- **No partial success** on rolls.
- Stats: base 3, cap 10. Modifier 0–1→+0, 2–3→+1, 4–5→+2, 6–7→+3, 8–10→+4.
- 18 cards per run. Ch1 4-of-6, Ch2 5-of-8, Ch3 6 (incl. Singil at Utang ≥3), Ch4 3-of-4 plus a forced morning card.
- Kasama +1d4, max 2 per roll. Echoes +1d6, max 2, Ch4 only.
- **Built-in Render Pipeline.** Do not add URP.
- Version rule: **`6000.3.x` is correct; `6000.0.x` and `6000.6.x` are wrong.**
- Do not `git add -A`. Stage explicit paths.
- `Assets/TextMesh Pro/` is **required project content**, not cache. Never gitignore it.
- Art under `Assets/Resources/Art/` is loaded by path. Renaming or moving it breaks `Resources.Load` at runtime.

---

## 7. Files a new frontend session should read first

| Path | Why |
|---|---|
| `docs/FRONTEND_HANDOFF.md` | architecture, pipeline, runtime flow |
| `handoff/ROADMAP.md` | the 25-item task list |
| `unity/Assets/Scripts/World/*.cs` | the world layer (6 files, ~700 lines) |
| `unity/Assets/Scripts/Models.cs` | content schema |
| `unity/Assets/Scripts/UI/UIBuilder.cs` | palette + widget factory |
| `art/pilot/sprites/README.md` | atlas geometry and import settings |

---

## 8. One-line summary

The Unity client was committed unbuilt and had never run. It now compiles, boots, plays cards end to end, and has a walkable landscape world with grid movement and painted collision. **Backend: decide Firebase yes/no (§4.1–4.2), confirm the Shared Barangay read path (§4.3), sign off D-011 and D-015 (§4.6), and minuted D-014/D-015 (§4.7). `unity/` is off-limits.**

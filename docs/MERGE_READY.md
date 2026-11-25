# Merge request — `frontend/unity-6.3-upgrade` → `main`

**From:** frontend
**Date:** 2026-11-25
**Branch head:** `61630fd` (+ any commits after)
**Deadline:** 2026-12-09

---

## 0. Why this is mergeable now

The merge gate is **cleared**. Evidence:

| # | Gate | Status |
|---|---|---|
| 1 | Console 0 errors across a full session | ✅ |
| 2 | Menus, cards, dialogues all respond to clicks | ✅ |
| 3 | Landscape renders without clipping at 16:9 | ✅ |
| 4 | **Full 18-card run completed, ending screen reached** | ✅ **`Ang Sumama`, Ending 3, 24% nagsalita, 27/100 Utang** |
| 5 | World ↔ encounter loop returns to plaza | ✅ |

What merges: a Unity client that **compiles from a clean clone, boots, plays a complete
run, and has a walkable world layer**. What it is *not*: a finished game (see §4).

---

## 1. What happened to `main` if you merge

| Area | Change |
|---|---|
| `unity/ProjectSettings/ProjectVersion.txt` | `6000.0.31f1` → **`6000.3.29f1`** |
| `unity/Packages/manifest.json` | textmeshpro removed (now inside ugui 2.0.0), newtonsoft 3.2.2, addressables 2.10.3, **`com.unity.modules.audio` added** |
| `unity/Assets/Scripts/` | 7 compile-error fixes, 3 runtime-blocker fixes, 1 logic fix, **6 new world-layer files** |
| `unity/Assets/Resources/Art/` | **new** — plaza, 2 interiors, 4 NPC portraits, 2 character atlases |
| `unity/Assets/Scenes/Main.unity` | **new** — the launch scene |
| `unity/Assets/TextMesh Pro/` | **new** — required project content, not cache |
| `unity/ProjectSettings/*.asset` | 20 auto-generated on first open |
| `.meta` files | all committed |

**Nothing under `Assets/StreamingAssets/Content/` changed.** Content JSON is untouched.

---

## 2. Action required before you pull

**Install Unity `6000.3.29f1` (6.3 LTS).** The project will not open cleanly on
`6000.0.x` or `6000.6.x`. The middle digit is the LTS stream; the trailing number is a
patch within it. `6000.3.x` is correct.

If you have a stale `Library/` folder, **delete it** and let Unity rebuild. A `Library/`
from a different editor version causes import errors that look like code errors.

---

## 3. Do not do these things

| Don't | Why |
|---|---|
| `git add -A` without a path prefix | a `Library/` or `Temp/` would be committed |
| Reformat / re-save / move anything under `unity/Assets/` | every `.meta` is committed; GUID churn is painful to unwind |
| Add URP | the client is Built-in RP deliberately — lighter on low-end Android |
| Add `com.unity.inputsystem` | `PlayerController` uses legacy `Input.GetKey`; switching breaks all movement |
| Rename or move art under `Assets/Resources/Art/` | loaded **by path** via `Resources.Load` — renaming breaks it at runtime |
| Edit `unity/Assets/StreamingAssets/Content/*.json` directly | re-export instead, and tell the frontend |
| "Clean up" the fixes in §5 | each was a real defect; all are load-bearing |

---

## 4. What is NOT done — read before you plan your work

| Not built | Consequence |
|---|---|
| **Touch controls** | keyboard only; no joystick, no interact button. Mobile is not playable yet |
| **Android build** | never attempted |
| **NPC walk sprites** | story NPCs are amber boxes, ambient NPCs bluish boxes |
| **Dialogue variants per chapter** | every NPC says the same 4 lines in every chapter |
| **Quest graph** | talking order does not matter; no errands, no cross-NPC confirmation |
| **Age gate + privacy notice** | not started; audience decision is 14+/18+ (D-016) |
| **Attract / intro** | does not exist |
| **Pitch page** | not started |
| **Firebase** | never imported; client runs fully offline |

### The honest state of the game loop

The **world is a hub**: walk to an NPC, press `F`, read 4 lines, play **one card**, return
to the plaza. A HUD shows chapter name, card progress (`2/4`), and who to talk to next.

A **complete 18-card run works from the menu** (`Maglaro`) — that is the verified path.
The world path plays one card per interaction and advances chapters when the deck
empties, but the **order of conversations does not matter** yet. That is the quest graph,
and it is Phase 6.

---

## 5. Fixes that must not be reverted

Each was a defect in code **nobody had ever compiled**. All are load-bearing.

| File | Defect | Fix |
|---|---|---|
| `UI/GameScreens.cs`, `UI/MenuScreens.cs` | `Destroy` outside a `MonoBehaviour` | `UnityEngine.Object.Destroy` |
| `UI/UIBuilder.cs` | `Toggle.Transition` bound to a local method | `UnityEngine.UI.Selectable.Transition` |
| `GameManager.cs` ×2 | `static Mod()` called on an instance | qualified with type |
| `GameManager.cs` `Redemption()` | `int[].Contains(string)` | explicit string comparisons |
| `Models.cs` | `Choice.Bind(Card)` called but never written | added field + method |
| **`UI/UIManager.cs`** | **no `EventSystem` existed anywhere** — every click was silently dead | created with `StandaloneInputModule` |
| `UI/UIManager.cs` | `AddComponent<MonoBehaviour>()` illegal | `UIRunner : MonoBehaviour` |
| `ContentDatabase.cs` | `Start(IEnumerator)` — Unity reserves parameterless `Start` | renamed `Begin(...)` |
| all `UI/*.cs` | emoji in strings; LiberationSans has no emoji | stripped; TMP atlas set to **Dynamic** |
| `World/WorldMap.cs` | plaza drawn half a map off (Center pivot vs bottom-left grid) | transform offset by half size |
| `World/PlayerController.cs` | sorting went negative → drew behind plaza | base offset 2000 |
| `World/EncounterFlow.cs` | `EventSystem.current` is **null while inactive** | store the GameObject before disabling |
| `World/EncounterFlow.cs` | portraits used as map sprites (1024px faces on the plaza) | map sprite path only, no portrait fallback |
| **`GameManager.cs` `ChoosePassive()`** | `NextCard()` called there **and** in `OutcomeScreen` → skipped a card, ran the index past the deck, bounced to the main menu | removed the advance from `ChoosePassive` |

---

## 6. Backend / content: your open items

Restated from `docs/MASTER_PLAN.md` §9. **These are the blockers on your side.**

### 6.1 Firebase — decide, this is the big one

The client runs **fully offline** (`CreateBackend()` reports `v1 · Offline` in the About
screen). `FirebaseBackend.cs` exists but **no `com.google.firebase.*` package is
imported**, and there is no `ExternalDependencyManager`.

- **(a)** Offline is correct for the demo → confirm, and I will mark `FirebaseBackend.cs` dead code
- **(b)** Firebase must be live by 2026-12-09 → **days of work**: SDK import (14.2.0, Auth + Firestore), `google-services.json`, project settings, auth flow, rules. **Not in the schedule. Decide now.**

### 6.2 Shared Barangay read path

The client shows an era pill (`Karaniwan · 37% nagsalita · tipping point 40%`) and a
Barangay screen with Pag-asa 45/100, Tiwala 60/100, Imprastruktura 35, Walang Kibo 0.
Confirm:

- exact doc path + field names (assumption: `barangay/state` → `tiwala`, `imprastruktura`, `pagAsa`, `walangKibo`, `era`, `version`)
- that it is **public-read** (the client reads without auth)
- that `submitRunContribution` is the **only** writer
- behaviour when the document is missing (first run, unseeded emulator) — a seed of **Pag-asa 45** was specified; confirm it is written on init
- what `Runs: 24` means, and whether it is global or per-player

### 6.3 Content ownership

Content lives in `unity/Assets/StreamingAssets/Content/`. There is an editor script
`Assets/Editor/ContentExport.cs` for the copy step. **Tell the frontend when content
changes** rather than editing the client copy — otherwise the two drift.

### 6.4 Dialogue variants — needed for Phase 6

For per-chapter NPC dialogue the frontend needs, per NPC:

```
{ npcId, chapter, requiresFlag, { tl: "...", en: "..." }[] }
```

Placeholder lines are hardcoded in `World/EncounterDef.cs` today. They are **not final copy**.

### 6.5 Decisions to minute

| ID | Decision |
|---|---|
| **D-014** | Unity upgraded to 6000.3.29f1; `ProjectVersion.txt` had been pinned to 6000.0.31f1 while §C.1 locks 6.3 LTS |
| **D-015** | 2026-12-09 demo scope is a vertical slice: 3 of 10 endings exposed, systems still support 10 |
| **D-016** | Target audience is **14+/18+**. Age is a local gate only, never transmitted. No behavioural profiling, no per-user choice logs. Aggregate run outcomes only. A privacy notice is still required |
| **D-011** | Singil at `Utang ≥ 3` yields 34.9% against a ≥50% target. `Utang ≥ 2` yields 52.5–58.0%. **Still needs owner sign-off** |
| **NEW** | `ChapterAges` copy says `~8–12` for Ch1 — **must change** with D-016. Narrative-owned, needs re-export |

---

## 7. How to review this in 5 minutes

```
1. Install Unity 6000.3.29f1, delete Library/, open unity/
2. Wait for import (several minutes the first time)
3. Console: expect 0 errors, 1 warning (enableWordWrapping obsolete — benign)
4. Open Assets/Scenes/Main.unity
5. Press Play
6. Menu appears → Maglaro → roll d6 → Simulan → click through 18 cards
7. Or: walk with WASD, approach an NPC, press F
```

Expected warning: `'TMP_Text.enableWordWrapping' is obsolete`. Leave it — `UIBuilder.cs`
builds every screen and is not worth touching mid-flight.

---

## 8. Frontend's remaining schedule

| Phase | Work | Days |
|---|---|---|
| Art | NPC walk sprites, facing, ambient villagers | 1–2 |
| **6** | `ChapterPlan` polish, quest graph, state-selected dialogue, 5 quests | 3–4 |
| 7 | Age gate + privacy notice, attract screen, fades, d20 animation, UI kit | 1–2 |
| **8** | **Touch controls, Android build, first APK** | 1 |
| 9 | Pitch page, capture | 1 |

**8–11 working days against 14.** First item to cut if it slips: authored quests (ship 2
instead of 5).

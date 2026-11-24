# Frontend handoff — Unity client

**Owner:** frontend
**Branch:** `frontend/unity-6.3-upgrade`
**Last updated:** 2026-11-22
**Read this before touching `unity/`.**

---

## 0. TL;DR for the other agents

1. The Unity client **now compiles and runs**. It did not before.
2. **Install Unity `6000.3.29f1`** (6.3 LTS). `6000.0.x` and `6000.6.x` are wrong.
3. **`unity/` is frontend-owned.** Do not reformat, re-save, move, or `git add -A`
   anywhere in the repo. Every `.meta` file is committed; a stray resave produces
   GUID churn that is painful to unwind.
4. **The game is a landscape, Pokémon-style RPG**, not a menu-based card game.
   That reframes the client architecture (section 4). Content and backend payloads
   are unaffected.
5. **Not merged to `main` yet, and must not be** until the merge gate in section 6
   is met. `main` is your shared baseline.

---

## 1. What was broken and is now fixed

The client was committed **unbuilt** — nobody had ever run the compiler on it.
Getting it to boot took four rounds:

### Commit `b96d636` — editor upgrade

`ProjectVersion.txt` was pinned to `6000.0.31f1` while §C.1 locks 6.3 LTS.
Upgraded to `6000.3.29f1`. Changed: `ProjectVersion.txt`,
`Packages/manifest.json`, 20 auto-generated `ProjectSettings/*.asset`,
`packages-lock.json`, and all `.meta` files. **No file under `Assets/Scripts/`
was touched by the upgrade.**

Companion decision **D-014** (owed in `DECISIONS.md`):
> Project upgraded to Unity 6000.3.29f1. `ProjectVersion.txt` was pinned to
> 6000.0.31f1 while §C.1 locks 6.3 LTS. Only `ProjectVersion.txt`,
> `Packages/manifest.json`, generated `ProjectSettings/*.asset`,
> `packages-lock.json` and `.meta` files changed. All team members must install
> 6000.3.29f1.

### Commit `f6954ff` — TextMeshPro Essentials

`Assets/TextMesh Pro/` committed. **This folder is required project content, not
cache.** Without it every label renders blank with no error. Do not gitignore it.

### Commit `e8e1f71` — 7 compile errors, 6 files

These were real code defects, not environment problems:

| File | Defect | Fix |
|---|---|---|
| `UI/GameScreens.cs`, `UI/MenuScreens.cs` | `Destroy(...)` called from a non-`MonoBehaviour` | `UnityEngine.Object.Destroy(...)` |
| `UI/UIBuilder.cs` | `Toggle.Transition.None` resolved to the local `UIBuilder.Toggle(...)` method instead of the type | `UnityEngine.UI.Selectable.Transition.None` |
| `GameManager.cs` ×2 | `ContentDatabase.Mod(...)` is `static`, called through an instance | qualified with the type name |
| `GameManager.cs` `Redemption()` | `new[]{2,4,6,8,10}.Contains(Run.endingId)` — `int[]` vs a **string** `endingId` | explicit `id == "2" \|\| ...` comparisons |
| `Models.cs` | `Choice.Bind(Card)` was **called but never written** | added `[JsonIgnore] public Card card;` + `public void Bind(Card owner)` |

Note: `Card` also declares an unused `[JsonIgnore] public Card card;`. Left in
place deliberately — it compiles, and removing it is a separate cleanup.

### Commit group `d4c0cxx` — runtime blockers (the ones that made the UI dead)

These compiled fine but made the game unusable:

1. **No `EventSystem` existed anywhere in the project.** Verified:
   `Select-String EventSystem` across `Assets/Scripts` returned zero hits.
   The Canvas rendered, but no click could ever be received. Symptom: a fully
   drawn menu that ignores every click, with a clean Console. Now created in the
   `UIManager` constructor next to the Canvas, using `StandaloneInputModule`
   (the project has no `com.unity.inputsystem`, so it is on the legacy Input
   Manager — **do not add the new Input System package without coordination**).
2. **`UIManager` called `AddComponent<MonoBehaviour>()`**, which is illegal
   (you can only add a concrete subclass). It threw in the constructor, so
   `UIManager` never existed. Fixed with a tiny `UIRunner : MonoBehaviour`
   host class.
3. **`ContentLoader.Start(IEnumerator)`** — Unity reserves `Start()` and calls
   it with zero arguments, so `AddComponent` *refused to attach the component*
   and content never loaded. Renamed to `Begin(...)` (call site updated).
4. **`com.unity.modules.audio` was missing**, so the `AudioListener` on the
   camera was stripped every frame. Enabled. `Packages/manifest.json` and
   `packages-lock.json` both changed.
5. **Emoji in UI string literals.** LiberationSans SDF has no emoji or symbol
   coverage, so `ℹ ▶ ⚙ ❓ 🏘 📣 💾 🧺 🫂` and the `U+FE0F` variation selector all
   rendered as tofu boxes (`□`) with a `LogWarning` each. Stripped from every
   `.cs` file. The font asset is set to **Dynamic** atlas population, which
   covers `₱` (U+20B1), `←`, `·`, `…`, `—`, `≥` — all of which do render.

**State at time of writing:** `0 errors`, `3 warnings` (residual `U+FE0F`). The
menu, Character Spark, Barangay, Paano Maglaro, Settings, Tungkol and the Ch1
card screen all render and navigate correctly.

---

## 2. How to open the project

```
Unity Hub → Add project from disk → C:\...\Bahala-na\unity
Editor: 6000.3.29f1
```

First open takes several minutes (imports and generates `Library/`). **Never
commit `Library/`, `Temp/`, `Logs/`, `obj/`, or `Build/`.** The repo's
`.gitignore` root `Library/` rule does **not** cover `unity/Bahala Na/`, so use
`.git/info/exclude` if a stray build folder ever appears.

There is **no scene asset committed**. The game bootstraps itself:
`GameApp.Boot()` is `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, so pressing
▶ Play in the default empty scene builds the whole UI in code. Do not create a
scene and expect it to be used — that is a frontend decision to make deliberately
later.

---

## 3. Runtime pipeline (how content reaches the screen)

```
Assets/StreamingAssets/Content/
  manifest.json          → card id list + contentVersion
  cards/<id>.json        → one card per file (40 total)
  echoes.json            → echo lines
  endings.json           → 10 endings
  backgrounds.json       → background defs
  eras.json              → era bands
  final_asks.json        → location-specific final asks
        ↓  ContentDatabase.LoadRoutine() (UnityWebRequest / file read)
ContentDatabase            → typed models from Models.cs (Newtonsoft.Json)
        ↓
GameManager               → run state, dice, chapter sequencing, endings
        ↓
UI/*Screens.cs            → uGUI built entirely in code via UIBuilder
```

**`Assets/StreamingAssets/` is the single source of truth for content in the
client.** If content changes upstream, the JSON must be re-copied here. There is
an editor script `Assets/Editor/ContentExport.cs` for this.

---

## 4. Architecture change in progress: landscape RPG

The client is currently a **portrait** UI (`UIManager.cs` `referenceResolution =
520 × 960`) that presents cards as a full-screen form. That is being replaced.

The game is a **landscape, top-down, Pokémon-style RPG** with:
- 4-direction player movement, tile-based
- NPCs you walk up to and interact with (interaction triggers the card encounter)
- objects with depth sorting (walk behind / in front depending on Y)
- small quests that gate or unlock encounters

**The existing card flow is not being thrown away** — it becomes the *encounter
layer* that the world calls into:

```
World (new)  →  walk, collide, camera follow, interact
   ↓ trigger
Encounter    →  existing GameScreens card flow (works today)
   ↓ resolved
World        →  return, NPC state advances, quest flag set
```

### Scope decision for the 2026-12-09 demo — vertical slice, not full game

| In | Out |
|---|---|
| one walkable barangay map (~40×25 tiles) | multiple maps, interiors |
| 3–4 NPCs with interaction triggers | 40 cards live in the world |
| landscape card-encounter screen (art left / dialogue right) | cutscene animation |
| 2–3 props with Y-depth sorting | every prop behaving uniquely |
| one fetch quest: talk → retrieve → return | branching quest chains |
| 3 of 10 endings reachable | all 10 endings reachable |
| pitch page + one recorded playthrough | — |

**This is a scope change, not a technical constraint.** It should be minuted —
proposed as **D-015** — because it reduces the shipped ending count from 10 to 3
for the demo build. The *systems* still support 10; the demo just doesn't expose
them all.

---

## 5. Open questions for backend / content

Answer these in writing; **do not edit `unity/` to "fix" a mismatch** without
agreement first.

### 5.1 Which backend is active at boot?
`GameApp.CreateBackend()` selects an implementation. The in-game About screen
reports **`v1 · Offline`**, so the client is currently running **fully offline**
with device-local saves. Confirm that is the intended state for the demo, or say
which endpoint should be live.

### 5.2 Firebase SDK is not imported
`Packages/manifest.json` contains **no** `com.google.firebase.*` package and no
`ExternalDependencyManager`. `FirebaseBackend.cs` exists but cannot compile
against a live SDK as things stand. If Firebase is required by 2026-12-09, this
is a **frontend blocker and an owner decision**, not a silent assumption.
Firebase Unity SDK 14.2.0 needs Auth + Firestore only.

### 5.3 Shared Barangay read path
The client displays a live era pill (`Karaniwan · 37% nagsalita · tipping point
40%`) and a Barangay screen with Pag-asa 45/100, Tiwala 60/100, Imprastruktura
35, Walang Kibo 0. Confirm:
- exact document path and field names to read
  (working assumption: `barangay/state` with `tiwala`, `imprastruktura`,
  `pagAsa`, `walangKibo`, `era`, `version`)
- that it is **public-read**, since the client reads it without auth
- that `submitRunContribution` is the **only** writer, per the security rules
- how the client should react when the document is missing (first run, unseeded
  emulator) — a seed of Pag-asa 45 was specified; confirm the seed is actually
  written on project init
- what `runs` count means (the UI shows `Runs: 24`) and whether it is global

### 5.4 Endings and echoes payloads
The client renders endings and echoes from **local** JSON. If the backend
computes or overrides any ending/echo text, say which, and send the authoritative
shape. Otherwise local JSON is authoritative and the backend must not duplicate
it.

### 5.5 About-screen copy is now wrong
It currently reads *"Isang malking dice-driven na card game tungkol sa isang
barangay"*. With the RPG reframe that copy needs rewriting by whoever owns
narrative voice. Placeholder for now.

---

## 6. Merge gate — do not merge to `main` until all three are true

1. `Console` shows **0 errors** during a full session.
2. Menus and cards respond to clicks (EventSystem verified in Play mode).
3. **One complete run reaches an ending** (18 cards: Ch1 4, Ch2 5, Ch3 6, Ch4 3).

Currently: 1 and 2 are met. **3 has not been done yet** — only Card 1/4 has been
played. Until then this branch is staging.

Merge via **pull request** into `main`. Never `git push origin main` from a local
branch.

---

## 7. Decisions and findings this branch owes

### D-014 — Unity editor upgrade (see §1)
### D-015 — 2026-12-09 scope is a vertical slice (see §4)

### D-011 — Singil trigger rate (needs owner sign-off, not a code fix)

Measured on the canonical configuration (`balance 5000`, `newcomer @17%`):
**34.9%** of runs see a Singil card, against a target of **≥50%**. Singil fires at
`Utang ≥ 3`.

Measured alternative: **`Utang ≥ 2` → 52.5%** at 15% newcomer weight, **58.0%** at
17%. That is the fix, but it was **deliberately reverted earlier** because it is a
design decision, not a bug. It needs the content/narrative owner's sign-off.

Per-lane: honest 0/0 · padrino 100/100 · collective 33.2/14.3 · newcomer
62.2/34.9 · passive 0/0.

Other canonical numbers: shortcuts ≥2 **62.2%** (target ~60 ✅), spoke up 68.7%,
silent somewhere 29.1%, Silent Majority 47.7%, **10/10 endings reachable** ✅.

---

## 8. Rules that must not be broken

- **Utang is never rendered as a number.** Only `SaveManager.UtangHint()` prose.
- **No partial success** on rolls: d20 + mod + assists vs DC 8/12/15/18.
- Stats: base 3, cap 10. Modifier 0–1→+0, 2–3→+1, 4–5→+2, 6–7→+3, 8–10→+4.
- 18 cards per run. Ch1 4-of-6, Ch2 5-of-8, Ch3 6 (incl. Singil at Utang ≥3),
  Ch4 3-of-4 plus a forced morning card.
- Kasama +1d4, max 2 per roll. Echoes +1d6, max 2, Ch4 only.
- **Built-in Render Pipeline.** Do not add URP.
- Do not `git add -A`. Stage explicit paths.

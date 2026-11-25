# Bahala Na? — Frontend master plan

**Branch:** `frontend/unity-6.3-upgrade`
**Updated:** 2026-11-25
**Deadline:** 2026-12-09
**Owner:** frontend (Unity client)
**Supersedes:** `FRONTEND_HANDOFF.md`, `BACKEND_HANDOFF.md`, `ROADMAP.md`

---

## 0. Correction to the record

The earlier claim **"10/10 endings reachable"** was about the **content JSON and the
simulation script**, not the shipped client. It is accurate for `endings.json` and the
balance test — every ending has a defined condition and the simulator can reach all of
them.

It is **not** true of the Unity client as a playable experience, because:

- The client plays 18 cards drawn from pools. Nothing in it *connects* NPCs to each
  other, so no errand, no dispute, no confirmation can occur.
- `EncounterDef` dialogue is **hardcoded placeholder text** — 4 lines per NPC, written
  to prove the plumbing works. It is context-free and repeats identically every time.
- The card shown at an encounter is **whatever is next in the deck**, unrelated to the
  NPC you spoke to.
- Chapter progression exists in `GameManager` (pools per chapter) but the **world does
  not change between chapters** — same plaza, same four NPCs, same dialogue.

**So: do not chase "reaching an ending" as a milestone yet.** It is not a bug to be
found; it is a system that has not been built. The merge gate has been revised
accordingly (§6).

---

## 1. Decision: target audience

**Changed to 14+ / 18+.** This resolves the data-protection problem raised on
2026-11-25. Consequences:

- `ChapterAges` copy currently says `~8–12` for Ch1. That is **content copy**, owned by
  the narrative owner, not the client. It must change upstream and be re-exported.
- The age prompt is now a **gate**, not a research input: confirm the player is 14+,
  store on-device, never transmit.
- No behavioural profiling, no per-user choice logs, no age transmitted to any server.
  Aggregate run outcomes via `barangay/state` remain fine and non-identifying.
- A plain-language privacy notice is still required (device storage + what is sent).
  Both languages. One screen. **Half a day of work.**

This must be minuted. Proposed **D-016**.

---

## 2. What is actually done

| Area | State |
|---|---|
| Unity editor | **6000.3.29f1** (6.3 LTS) — everyone must install this |
| Compile | 0 errors, 1 benign warning (`enableWordWrapping` obsolete) |
| Layout | Landscape 1920×1080, centred 1180 column, no clipping |
| Screens | Menu, Character Spark, Barangay, How to Play, Settings, About, card, outcome, climax, ending, chapter intro, career — all render and navigate |
| Content pipeline | 40 cards, echoes, endings, eras, backgrounds, final asks — loaded from `StreamingAssets` |
| Dice | d20 + mod + assists vs DC 8/12/15/18, resolves and reports |
| World | Grid movement (64 px cells, 32×18), camera follow, depth sorting, painted collision |
| Encounters | 4 NPCs, proximity + `F`, interior art, portrait dialogue, loop back to plaza |
| Art | Plaza, 2 interiors, 4 portraits, 2 walk atlases (sliced, bottom-centre pivot) |
| Scene | `Scenes/Main.unity` with all three components wired |
| Repo | Clean, pushed, no `Library/`/`Temp/` committed |

### Fixes that must not be undone

Each of these was a real defect in code nobody had ever compiled. Every one is
load-bearing:

`Destroy` scope (×2) · `Toggle.Transition` → `Selectable` · static `Mod` (×2) ·
`int[].Contains(string)` · `Choice.Bind` · **no `EventSystem` existed at all** ·
`AddComponent<MonoBehaviour>()` illegal · `ContentLoader.Start()` with params ·
emoji stripped from UI strings · plaza pivot offset · sprite sorting base offset ·
canvas re-enable on encounter · `EventSystem.current` is **null while inactive**.

---

## 3. What is NOT built (and was assumed to be)

| Missing | Consequence |
|---|---|
| **Chapter-to-chapter state change** | Ch2/3/4 look identical to Ch1 in the world |
| **NPC quest graph** | No errands, no disputes, no cross-NPC confirmation |
| **Contextual dialogue** | Every NPC says 4 fixed placeholder lines |
| **NPC↔card mapping** | The card is "next in deck", not the NPC's card |
| **Age gate + privacy notice** | Not started |
| **Touch controls** | No joystick, no interact button — keyboard only |
| **NPC walk sprites** | Amber placeholder boxes on the map |
| **Fade transitions** | Abrupt cuts |
| **Attract / intro** | Does not exist |
| **Android build** | Never attempted |
| **Pitch page** | Not started |

---

## 4. How chapters work — the answer to "will characters change?"

**Yes, and here is the concrete design.** Nothing in `GameManager` needs rewriting; it
already selects card pools per chapter. What is missing is the **world-side data for
each chapter**.

### 4.1 The model

A chapter is a **world state**, not a new map. Same plaza, different contents:

| | Ch1 · Kabataan | Ch2 · Pag-aaral / Trabaho | Ch3 · Tungkulin | Ch4 · Baha |
|---|---|---|---|---|
| **Player age** | 8–12 (per content spec) | 17 | 25–35 | 35+ |
| **Cast present** | Kuya Jun, Aling Nena | + Ma'am Lourdes, a classmate | + Kapitan, a colleague | Kapitan, Ma'am Lourdes, the whole barangay |
| **What's open** | the plaza, the court | + the school gate area | + the barangay hall | + the flooded street |
| **Weather / light** | warm afternoon | dusk | night, lamps | grey, raining |
| **NPCs who left** | — | Kuya Jun still there | Aling Nena's store closed | most NPCs gone |

So characters **do** change: some appear, some vanish, some move, and their dialogue
changes entirely. Reusing one background is a **budget decision**, not a design limit —
a second background (the school, or the flooded street) is the highest-value art
addition if time allows.

### 4.2 Implementation shape

```
ChapterDefinition
  int chapter
  string[] NpcIdsPresent        // which encounters spawn
  Vector2Int[] cells            // where each stands
  string backgroundResourcesPath // per-chapter backdrop (may repeat)
  string[] AmbientNpcIds        // non-interactable walkers
  Color ambientTint             // dusk / night / rain colour grade
```

`EncounterFlow.PlaceNpcs()` currently reads `EncounterDef.All()`. It becomes
`ChapterPlan.For(GameApp.Game.Run.chapter)`. `OnHide`/`OnShow` of the world re-reads it
when the chapter changes.

**This is the single most important unbuilt system.** Until it exists, the game has one
chapter's worth of world and four chapters' worth of cards.

### 4.3 Where it sits in the schedule

Phase 6 (§7). It is **not** a 2026-12-09 requirement — but without it the demo is a card
game with a plaza attached, not a story.

---

## 4A. The job system — how careers, quests and interactions connect

### 4A.1 What already exists

The **card layer already has jobs**, and it works:

| Piece | Where |
|---|---|
| `CareerOptions()` returns eligible careers | `GameManager` |
| `AssignCareer(career)` / `PickCareer(career)` | `GameManager` |
| `CareerScreen` — the picker UI | `MenuScreens.cs` |
| **Ch3 card pool filtered by job** — `Pool("ch3_role").FindAll(c => c.role == Run.career)` | `GameManager.StartChapter(3)` |

Ch3 card IDs already encode the job:

```
ch3_official_consultant_01   ch3_official_friend_02   ch3_official_whistle_03
ch3_police_checkpoint_01     ch3_police_report_02     ch3_police_harass_03
ch3_teacher_cement_01        ch3_teacher_receipts_02  ch3_teacher_evidence_03
ch3_journalist_source_01     ch3_journalist_offer_02  ch3_journalist_group_03
ch3_citizen_assembly_01      ch3_citizen_cosign_02    ch3_citizen_vote_03
ch3_shared_*  (always available regardless of job)
```

So **the cards are already job-specific.** What is missing is the **world** knowing that.

### 4A.2 Answer: yes — different quests and interactions per job

| Job | Career id | Patron NPC | Quest style | Ch3 cards it unlocks |
|---|---|---|---|---|
| Barangay **Official** | `official` | Kapitan | insider access — you can *change* things, at a cost | `ch3_official_*` |
| **Pulis** | `police` | Kuya Jun (a contact) | coercion — checkpoints, harassment, reporting up | `ch3_police_*` |
| **Teacher** | `teacher` | Ma'am Lourdes | documentation — receipts, evidence, cement | `ch3_teacher_*` |
| **Organizer / Journalist** | `organizer` | Ma'am Lourdes + a group NPC | mobilising — sources, offers, group action | `ch3_journalist_*` |
| **Ordinary citizen** | `citizen` | Aling Nena | the assembly — cosign, speak, vote | `ch3_citizen_*` |

**Design rule:** the same plaza, the same NPCs, but **which errand the NPC gives you
depends on your job.** Aling Nena asks a teacher for help with receipts; she asks an
official for a favour; she asks a pulis to look the other way. One NPC, five
conversations, chosen by `Run.career`.

### 4A.3 Data shape

```
QuestDef
  id
  careerId          // null = available to every job
  chapter           // 1..4
  steps[]           // StepDef

StepDef
  npcId
  requiresFlag      // null = always offer this step
  linesKey          // dialogue selected by (careerId, step)
  setsFlag
  rewardFlag
```

`EncounterFlow` resolves the quest for an NPC like this:

```
candidate = quests.Where(q => q.chapter == Run.chapter
                           && (q.careerId == null || q.careerId == Run.career))
                  .FirstOrDefault(q => q.NextStepFor(npcId, flags) != null)
```

No quest → the NPC gives ambient/contextual dialogue instead. That is the fallback, and
it is what makes the plaza feel alive even before quests exist.

### 4A.4 What this costs

| Work | Days |
|---|---|
| `QuestDef` / `StepDef` model + resolver | 1 |
| `ChapterPlan` per-chapter cast (already in §4.2) | 1 |
| Wire `Run.career` into NPC/quest selection | 0.5 |
| **Author 5 jobs × ~3 steps of dialogue, 2 languages** | **2–3 (narrative, not frontend)** |

**The frontend cost is ~2.5 days. The dialogue is the expensive part, and it is not mine
to write.** Without authored lines the system runs with placeholder text and looks
identical to today.

### 4A.5 Interaction variants (what changes beyond dialogue)

Cheap, high-impact, all reuse existing code:

| Variant | Implementation |
|---|---|
| NPC turns to face you | set `SpriteRenderer.flipX` + swap to the facing frame — 10 lines |
| NPC portrait expression changes | second portrait PNG per NPC — *art cost, optional* |
| NPC unavailable ("hindi ako pwede ngayon") | one `requiresFlag` that fails; dialogue-only |
| Job-specific greeting | `linesKey` includes `careerId` — already in the data shape |
| Ambient NPC reacts to you | tint or a one-line bark — optional |

**Do not build animation systems for this.** Dialogue + portrait + facing is enough.

---

## 5. Dialogue and quests — the real work

This is **content authoring plus a small data structure**, not engine work.

### 5.1 The quest graph

Five quests, one main line, four errands. Each is a list of **steps**, each step a
condition on run flags. `GameManager` already has `HasFlag`, so no new state system.

```
Quest: "Ang Resibo ni Ma'am"
  step 0  talk to maam_lourdes      → set flag "lourdes_asked"
  step 1  talk to aling_nena        (requires lourdes_asked) → set "nena_confirmed"
  step 2  return to maam_lourdes    (requires nena_confirmed) → reward, set "receipt_quest_done"
```

Data shape:

```
QuestDef { id, steps[] }
StepDef  { npcId, requiresFlag, lineKeys[], setsFlag, rewards }
```

Dialogue is **selected by state**, not fixed:

```
EncounterDef.Lines is replaced by:
  LinesFor(flags) → string[]
```

So Aling Nena says *"Aba, umuwi ka pala"* on first meeting, *"Sabi ni Ma'am Lourdes…"*
if `lourdes_asked` is set, and *"Kumusta na ang resibo?"* after. **Same NPC, three
different conversations.**

### 5.2 Ownership

Dialogue text is **narrative**, not frontend. The frontend builds the mechanism; the
content team writes the lines. Handoff needed: a JSON file per NPC with
`{ state, tagalog, english }` entries. Until that arrives, the placeholders stay.

### 5.3 Cross-NPC confirmation

The middleman mechanic you described is **step 1 above** — a flag set by NPC A, checked
by NPC B. It needs no new system, only authored data.

---

## 6. Merge gate — revised

The old gate required one run to an ending. That was wrong (§0). Revised:

| # | Gate | Status |
|---|---|---|
| 1 | Console shows 0 errors across a full session | ✅ |
| 2 | Menus, cards, and dialogues respond to clicks | ✅ |
| 3 | Landscape renders without clipping at 16:9 | ✅ |
| 4 | **All 18 cards of a run play through in sequence** (chapter transitions included), reporting an ending screen | ❌ **BLOCKED — run appears to end after a single card.** Needs diagnosis, see §6.1 |
| 5 | World ↔ encounter loop returns to the plaza correctly | ⚠️ verified once, needs re-check |

Gate 4 is about the **card chain**, not about quests or narrative. Playing Maglaro and
clicking through 18 cards without a stall is the test. It does **not** require a
satisfying story.

**Target for this week.** Nothing goes to `main` before gate 4 passes.

### 6.1 Open bug: the run ends early

Reported 2026-11-25: *"there is only one group of choices per character and it ends."*

Three candidate causes, in order of likelihood:

1. **`I-withdraw` is being read as "continue".** `GameManager.Withdraw()` deliberately
   **ends the run** with the silent ending. The card screen shows `I-save` and
   `I-withdraw` under the choices, and there is no other button, so a player looking for
   "next" will press one of them. **This is a UX defect, not a logic defect.**
2. `NextCard()` / `AdvanceChapter()` fails to advance and falls through to `FinishRun()`.
3. The chapter deck is built with only 1 card because `Sample(...)` returned fewer than
   requested.

**Diagnosis needed:** `GameManager.NextCard()`, `AdvanceChapter()`, `StartChapter(int)`,
and the `Proceed()` method in `GameScreens.cs`. Do not guess at a fix before seeing them.

**Interim UX fix regardless of cause:** the card screen must have a clearly labelled
**next/continue** affordance, and `I-withdraw` must be visually separated and
confirmation-gated, because it irreversibly ends the run.

---

## 7. Phases

### ✅ Phase 0–3 — done
Foundation, compile, card game, world layer, encounters. See §2.

### 🔧 Phase 4 — finish the loop *(now)*
1. Verify NPC → card → resolve → back to plaza
2. **18 cards play through in sequence** → gate 4
3. Commit + push

### 🎨 Phase 5 — art *(1–2 days)*
4. NPC walk sprites — 4 characters × 4 directions × 2 frames, 89×170 cells
5. NPCs turn to face the player on approach
6. Ambient NPCs — 8 walkers, no interaction

### 📜 Phase 6 — chapters + quests *(3–4 days, the big one)*
7. `ChapterPlan` — per-chapter cast, positions, tint
8. World re-reads plan on chapter change
9. `QuestDef` / `StepDef` data model
10. Flag-driven dialogue selection
11. 5 quests authored *(needs narrative input)*
12. `EncounterDef` → chapter-aware; delete hardcoded lines

### 🖥️ Phase 7 — presentation *(1–2 days)*
13. Age gate + privacy notice *(see §1)*
14. Attract screen — cold open, 3 stills, skippable
15. Home screen — plaza at 25% behind the menu
16. Fade transitions
17. d20 roll animation + result reveal
18. UI kit — panels, buttons, icons, frames

### 📱 Phase 8 — mobile *(1 day)*
19. **Touch controls** — virtual joystick + interact button
20. Android platform switch, `ph.bahalana.game`, landscape-locked
21. **First APK on a real device**
22. Texture compression → ASTC, perf pass

### 🚀 Phase 9 — ship *(1 day)*
23. **Pitch page** — one page
24. Gameplay capture
25. PR to `main`

### Reality check on 14 days

Phases 5–7 total roughly **6–9 working days**. Phases 8–9 need 2 more. That is
**8–11 days against 14**, with no buffer. **Phase 6 item 11 (authored quests) is the
item to cut** if the schedule slips — the system can ship with 2 quests instead of 5.

---

## 8. Cleanup — remove what will not be used

| Item | Action |
|---|---|
| `art/pilot/barangay_plaza_dusk.png` | **delete** — square, superseded by the 16:9 topdown |
| `art/pilot/barangay_plaza_topdown.png` | **delete** — square, superseded |
| `art/pilot/frames/` | **delete** — intermediate slice output, superseded by the atlases |
| `art/pilot/sprites/boy_atlas.png`, `girl_atlas.png` | **delete** — superseded by `*_walk_atlas.png` |
| `art/pilot/sprites/*_idle2_*.png` | **delete** — 2-frame breathing idle was a rejected idea |
| `art/pilot/sprites/*_idle_left/right.png` | **delete** — wrong dimensions, never used |
| `art/pilot/character_boy_sheet.png`, `character_girl_sheet.png` | **keep** — source reference, not imported |
| `docs/FRONTEND_HANDOFF.md` (in repo) | **replace** with this document |
| `docs/BACKEND_HANDOFF.md` (in repo) | **keep**, still current for backend |
| `handoff/ROADMAP.md` | **delete** — folded into this file |
| Guide §7 (new project) instructions | **never re-apply** — project exists |
| My earlier `core/`, `content/cards/` overlay | **already deleted**, do not restore |

**In Unity, do not delete:** `Assets/Resources/Art/` (all of it is in use),
`Assets/StreamingAssets/Content/`, `Assets/TextMesh Pro/`.

---

## 9. For the backend / content team

Unchanged from the previous handoff, restated because it still blocks:

1. **Install Unity 6000.3.29f1.** `6000.0.x` and `6000.6.x` are wrong.
2. **`unity/` is frontend-owned.** Do not reformat, re-save, or `git add -A`.
3. **Firebase: yes or no?** Client runs fully offline today. If Firebase is required by
   2026-12-09 it is **days of work** and is not in the schedule. Decide now.
4. **Shared Barangay read path** — confirm `barangay/state` field names, public-read,
   and that `submitRunContribution` is the only writer.
5. **D-011** — Singil at `Utang ≥ 3` yields 34.9% against a ≥50% target.
   `Utang ≥ 2` yields 52.5–58.0%. Needs owner sign-off.
6. **D-015** — demo scope is a vertical slice.
7. **D-016 (new)** — audience is 14+/18+; age is a local gate, never transmitted.
8. **Content changes** go through a re-export into
   `unity/Assets/StreamingAssets/Content/`. Tell the frontend when content changes.
9. **`ChapterAges` copy** currently says `~8–12`; must change with §1.

---

## 10. Rules that must not be broken

- **Utang is never rendered as a number.** Only `SaveManager.UtangHint()` prose.
- No partial success on rolls.
- Stats base 3, cap 10. Modifier 0–1→+0, 2–3→+1, 4–5→+2, 6–7→+3, 8–10→+4.
- 18 cards/run: Ch1 4-of-6, Ch2 5-of-8, Ch3 6 (Singil at Utang ≥3), Ch4 3-of-4 + forced morning.
- Kasama +1d4 max 2. Echoes +1d6 max 2, Ch4 only.
- **Built-in Render Pipeline.** Do not add URP.
- **No Firebase → still offline.** Do not half-import it.
- Every `.meta` file is committed. Never `git add -A` without a path prefix.
- Art under `Assets/Resources/Art/` is loaded **by path**. Renaming breaks `Resources.Load`.

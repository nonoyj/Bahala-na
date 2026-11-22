# BAHALA NA? — Barangay Bayanihan

**Status:** playable demo (web) · **Target deliverable:** Unity 6.3 + Firebase mobile demo + one-page pitch (2026-12-09)
**Master reference:** [`HANDOFF.md`](HANDOFF.md) (full design + build spec) · **Card content source:** [`CARDS.md`](CARDS.md)

This repo contains a **complete, playable implementation of the full game design** as a mobile-first web demo, plus the frozen backend contract for the real Unity/Firebase build. If you are a developer joining the project and have not read the design doc, this README is your onboarding.

---

## 1. What the game is

*Bahala Na?* is a short, replayable, dice-driven life story set in a fictional Philippine barangay (**Barangay Bayanihan, bayan ng San Isidro** — fictional). You grow up from childhood to adulthood by choosing between cards; each hard moment is a d20 roll.

- Easy shortcuts (a tita's "favor," a small "pang-merienda") really do help early on — but they quietly build a **hidden debt** (*utang na loob*) that comes due in adulthood.
- Alone, fighting a rigged system is a low-odds roll. With help from other people, the same roll becomes winnable.
- Every run permanently changes a **shared barangay** that later players inherit.

**Core message (never stated until the end):** *Bahala na* can mean "leave it to fate" or "take the leap." Fate looks different when people roll together.

**Design pillars:** show-don't-preach · temptation must be real · alone is hard, together is possible · doing nothing is a choice with a cost · everyone matters (the Ordinary Citizen is a first-class path) · hope with specifics (ends on a real-world action).

### One-page pitch
See [`PITCH.md`](PITCH.md).

---

## 2. Storyline

Four chapters, 18 cards per run, ~25–30 years of a life. No real years, politicians, parties, or agencies are named.

| Ch | Name | Ages | Cards | What happens |
|---|---|---|---|---|
| 1 | **Kabataan** (Childhood) | ~8–12 | 4 of 6 | Warm, small-scale, a little funny. Teaches the rules through low-stakes moments; first small temptation. |
| 2 | **Sangandaan** (Crossroads) | ~16–22 | 5 of 8 | Pressure rises: scholarships, org elections, a barkada member in trouble, a fixer, vote-buying. **Determines your Adult Role (player picks if multiple qualify).** |
| 3 | **Tungkulin** (Duty) | ~25–35 | 6 | 3 role-specific + 2 shared adult + 1 **Singil** (debt collection) if Utang ≥ 3. Moral weight. |
| 4 | **Ang Pagsubok** (The Test) | ~35+ | 3 of 4 | A flood hits; the town discovers the drainage project was a **ghost project** (funded, never built — while a welcome arch was built instead). Town assembly, cascade of voices, the decision. |

**Cast (all fictional):** Tita Baby (padrino favors), Kap Eddie (ambiguous captain), Ma'am Lourdes (teacher), Kuya Jun (fixer), Mayor Tonyo, RC Dizon Builders, Nurse Lea (template for "echo" characters), the barkada.

**Endings:** 10 (see `content/endings.json`) — complicit endings are humanizing, never mocking; "Pagbawi" (redemption) note if Komunidad ≥ 4; every ending carries the *starting-line* note ("You began with [background]. Others began elsewhere.").

---

## 3. How it plays (mechanics)

- **Stats (base 3, 0–10):** Talino · Diskarte · Pera · Koneksyon · Integridad · Komunidad, plus **Utang na Loob (hidden)**. Modifier: 0–1→+0 · 2–3→+1 · 4–5→+2 · 6–7→+3 · 8–10→+4.
- **Roll = d20 + stat modifier + assists vs DC.** DC 8 easy / 12 moderate / 15 hard / 18 systemic. Nat 20 = critical success; Nat 1 = bigger consequence (never a dead end). **No partial success.**
- **Padrino Shortcut:** one option per card, labeled with a euphemism ("Magpa-ayos kay Tita Baby," "Pang-merienda lang," "SOP"). Auto-success; Integridad −1 now, **Utang +1/+2** hidden.
- **Singil cards:** when Utang ≥ 3 in Ch3 — pay / refuse (Integridad DC = 10 + Utang) / Bayanihan / delay (returns harder).
- **Kasama (assists):** from Komunidad (≥3 → 1, ≥6 → 2); each is +1d4, max 2 per roll.
- **Echoes:** pre-written, anonymous, from "earlier players"; +1d6, max 2 per roll, used in Ch4.
- **Career (end of Ch2):** computed from stats/flags; if 2+ qualify the **player picks**; if none, Ordinary Working Citizen.
- **Climax cascade (Ch4):** NPC groups speak once enough voices exist — Brave (0) → Angry Neighbors (1) → Worried Parents (2) → Cautious Vendors (3) → Silent Majority (4). Reach the Silent Majority → Reform. Shown as a visible chain reaction.
- **Shared Barangay:** Tiwala (0–100) · Imprastruktura (0–100) · Pag-asa (0–100) · Walang Kibo counter. Eras: **Mabulok** (0–29, systemic DC +2) · **Karaniwan** (30–59) · **Gumigising** (60–84, DC −2, more echoes) · **Bagong Umaga** (85–100, special cards, Kap Eddie changes).

---

## 4. Tech stack

**Locked (per handoff §C.1):** Unity 6.3 LTS (2D URP) · C# · **Firebase** (Firestore, Auth, Cloud Functions, Remote Config, Crashlytics, Analytics) · GitHub + LFS · GitHub Actions → app distribution · Addressables for live content · Android first, iOS second · offline-first.

**Why:** best-in-class mobile offline + real-time for a small team; fastest path to a live shared state; content tunable without store review.

**This demo's stack (no build step):** static HTML/CSS/JS, content as JSON (`content/`), shared state in `localStorage`. It is a **1:1 logic port** of the design so the rules can be playtested in a browser before Unity work; the Firebase layer is specified (not implemented) in [`API_CONTRACT.md`](API_CONTRACT.md).

| Demo | Real build |
|---|---|
| `localStorage` save | Firestore offline persistence + sync queue |
| `localStorage` BarangayState | Firestore `barangay/state` doc via Cloud Function |
| `content/cards-bundle.js` | Addressables / Remote Config JSON |
| WebAudio beeps | Unity Audio + FMOD-ish mix |
| CSS d20 | Animated physical d20 (18D) |

---

## 5. Architecture & data flow

```
Unity Client (Android/iOS)                Firebase
  • Card/run UI            ◄──read──►   Firestore: BarangayState (singleton),
  • Local save (JSON)                     Echoes (collection), PlayerProfiles
  • Dice animation         ──write─►   Cloud Functions: submitRunContribution()
  • Offline queue            (validated)  Remote Config (tuning), Auth (anon)
```

- **Reads** (BarangayState, Echoes) → Firestore directly; Security Rules: public read, **no client write**.
- **Writes** to BarangayState → **only** via `submitRunContribution()` (validates, transacts, recomputes era). Clients never write it directly (anti-cheat).
- **Offline:** local JSON save is the on-device source of truth; Firestore caches reads; writes queue and sync on reconnect.
- **Content (cards):** JSON matching `content/cards/*.json` → Addressables/Remote Config → changeable without an app update.

### Data models (§C.3)

```ts
PlayerProfile { id, displayName?, presentation: "boy"|"girl"|"neutral", locale, createdAt }
RunSave { runId, chapter, cardIndex, stats{talino,diskarte,pera,koneksyon,integridad,komunidad},
          utang /*HIDDEN*/, flags[] /*ordered*/, kasamaTokens, background, career?, startedAt }
CompletedRun { runId, playerId, background, career, endingId, spokeUp, joinedCascade,
               utangFinal, echoesLeft[], contribution: ContributionSummary, finishedAt }
BarangayState { tiwala, imprastruktura, pagAsa, walangKibo, era, version, updatedAt } // singleton
Echo { id, text /*pre-written*/, sourceRunId?, active, createdAt }
Card { id, chapter, tags[], requires?, setup, choices[{label,type,roll?{stat,dc},success,fail,hidden?}], callbackHook? }
RemoteConfig { dcModifiers{era:number}, balance{...}, featureFlags{...}, contentVersion }
```

The demo's `js/engine.js` implements exactly these models in JS (see `BN.defaultBarangay()`, `BN.newRun()`, `BN.applyContribution()`).

---

## 6. Shared Barangay backend (the real build)

- **BarangayState** is one Firestore doc; only the Cloud Function writes it.
- **Contribution rules (transaction):** Speak up +4 Pag-asa · Join +2 · Silent → Walang Kibo +1 (Tiwala drifts down) · Padrino-heavy (Utang ≥ 5) → Tiwala −3 · Echo opt-in → pool.
- **Era** from Pag-asa: 0–29 Mabulok (+2 DC) · 30–59 Karaniwan · 60–84 Gumigising (−2 DC, more echoes) · 85–100 Bagong Umaga. Crossing 60 → visible town event.
- **`submitRunContribution(summary)`:** (1) require Auth UID; (2) validate shape/ranges; (3) transaction: read state, apply, recompute era, `version++`, write; (4) add echo if left; (5) return new state.
- **Security Rules:** `barangay/state` and `echoes/{id}`: read public, write **false** (Admin SDK only). `players/{uid}`: read/write only if `request.auth.uid == uid`.
- **Seeding:** ship with preset state (Pag-asa = 45, Karaniwan) so the demo has history. The web demo seeds 24 runs / 9 spoke-up (37%, tipping point 40%).

Full signatures: [`API_CONTRACT.md`](API_CONTRACT.md).

---

## 7. Run the demo

Any static file server works:

```bash
cd Bahala-na
python3 -m http.server 8000 -b 0.0.0.0
# open http://localhost:8000
```

No build step, no dependencies. Works offline (after first load), mobile-sized (max-width 520px), touch-first.

**Try:** Play → Character Spark (name, presentation, roll d6) → 4 chapters → career pick → Singil → flood climax → ending + "Ikaw at ang Bayan" (epilogue, barangay impact, **Utang ledger reveal**, location-specific real-world ask).

---

## 8. Repo structure

```
index.html               shell
css/style.css            mobile-first UI
js/engine.js             pure game logic (no DOM) — the rules
js/ui.js                 screens, dice overlay, HUD
js/main.js               bootstrap
content/cards/*.json     40 cards (authored format, §C.6)
content/endings.json     10 epilogues
content/echoes.json      12 pre-written echoes
content/final_asks.json  location-specific CTAs (13C)
content/backgrounds.json d6 family circumstances (§B.5)
content/eras.json        4 eras (10B)
content/cards-bundle.js  generated bundle (loaded by index.html)
scripts/content_a/b/c.py card content (Python source of truth)
scripts/build_content.py generates content/*.json + cards-bundle.js
scripts/simulate.js      headless balance/crash harness (node scripts/simulate.js)
assets/title.png         pixel-art title (GBA style)
assets/hub.png           pixel-art barangay map
HANDOFF.md               master design + build spec (source of truth)
CARDS.md                 card content source
API_CONTRACT.md          frozen backend contract
PITCH.md                 one-page pitch
DECISIONS.md             decision/change log
```

**Content pipeline:** edit `scripts/content_*.py` → `python3 scripts/build_content.py` → 40 `content/cards/*.json` + `content/cards-bundle.js`. (In the real build: writer → JSON → review → Addressables/Remote Config.)

---

## 9. Build phasing (§D.3)

1. **Lock design + data model + API contract** — done (`HANDOFF.md`, `API_CONTRACT.md`).
2. **Parallel:** content (cards/endings) ∥ frontend (UI, main menu, settings) ∥ backend (Firestore, Functions, rules).
3. **Integrate continuously** via GitHub.
4. **Art + animation + world hub** (one walkable barangay hub, not an open world).
5. **Playtest → fix → polish** (5–10 people; targets: run 10–12 min · ~60% take ≥2 shortcuts first run · ≥50% meet a Singil · solo climax ~25–30%, with assists ~60–70%).
6. **Live ops** (Remote Config, Addressables, changelog, staged rollouts).

---

## 10. Verification

- `node scripts/simulate.js 3000` — 3,000 simulated runs, no crashes, stats always in 0–10, all 10 endings reachable, Singil triggers at Utang ≥ 3, contribution/era math stable.
- Headless DOM playthrough (jsdom): full 18-card run, dice, career pick, ending, ledger, echo, barangay, language toggle.

---

## 11. Locked decisions (don't drift)

See `HANDOFF.md` §D.1. Highlights: mobile-first (Android then iOS) · Shared Barangay **BUILD** · 29-card minimum (40 written) · all 10 endings · no partial success · Kasama +1d4 max 2 · pre-written echoes · career tiebreak = player picks · 4 eras · ~70% Taglish · presentation changes card text · location-specific final ask · 8 cast members · pixel/GBA art · animated physical d20 · AI-assisted writing.

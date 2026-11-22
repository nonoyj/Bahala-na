# BAHALA NA? — Backend (Barangay Bayanihan)

**Status:** backend artifacts for the playable demo (target 2026-12-09) · **Base:** `main`
**Master reference:** [`HANDOFF.md`](HANDOFF.md) (full design + build spec) · **Card content source:** [`CARDS.md`](CARDS.md) · **Pitch:** [`PITCH.md`](PITCH.md)

> **Scope of this PR — backend only.** The user owns backend; frontend (Unity client, UI, world hub, animations) is owned by other contributors. This branch carries the backend artifacts: the frozen API contract, the rules/Shared Barangay reference engine, the card/content data + pipeline, and the balance harness. The web UI demo that was prototyped here is intentionally **excluded** so it does not collide with frontend work.

---

## 1. What the game is (context)

*Bahala Na?* is a short, replayable, dice-driven life story set in a fictional Philippine barangay (**Barangay Bayanihan, bayan ng San Isidro** — fictional). You grow up from childhood to adulthood by choosing between cards; each hard moment is a d20 roll.

- Easy shortcuts (a tita's "favor," a small "pang-merienda") really do help early on — but they quietly build a **hidden debt** (*utang na loob*) that comes due in adulthood.
- Alone, fighting a rigged system is a low-odds roll. With help from other people, the same roll becomes winnable.
- Every run permanently changes a **shared barangay** that later players inherit.

**Core message (never stated until the end):** *Bahala na* can mean "leave it to fate" or "take the leap." Fate looks different when people roll together.

**Design pillars:** show-don't-preach · temptation must be real · alone is hard, together is possible · doing nothing is a choice with a cost · everyone matters (the Ordinary Citizen is a first-class path) · hope with specifics (ends on a real-world action).

Full storyline, cast, chapters, and mechanics: [`HANDOFF.md`](HANDOFF.md) §B.

---

## 2. What's in this PR

| Artifact | File(s) | Owner |
|---|---|---|
| **Frozen API contract** (Firestore schema, `submitRunContribution`, Security Rules, era/DC mapping) | [`API_CONTRACT.md`](API_CONTRACT.md) | backend |
| **Rules + Shared Barangay reference engine** (pure JS, no DOM — the exact math the Unity client must match) | [`js/engine.js`](js/engine.js) | backend |
| **Card/content data** (40 cards, 10 endings, 12 echoes, final asks, backgrounds, eras) | [`content/`](content/) | backend/writer |
| **Content pipeline** (Python source of truth → JSON → bundle) | [`scripts/content_a/b/c.py`](scripts/), [`scripts/build_content.py`](scripts/build_content.py) | backend/writer |
| **Balance/crash harness** | [`scripts/simulate.js`](scripts/simulate.js) | backend |
| **Decision log** | [`DECISIONS.md`](DECISIONS.md) | shared |
| **Design masters** | [`HANDOFF.md`](HANDOFF.md), [`CARDS.md`](CARDS.md), [`PITCH.md`](PITCH.md) | shared |

**Excluded (frontend):** `index.html`, `css/`, `js/ui.js`, `js/main.js`, `assets/` — the playable web demo. Frontend contributors build the Unity client/UI against `API_CONTRACT.md`; `js/engine.js` is the reference implementation of the rules.

---

## 3. Backend spec (summary — full detail in [`API_CONTRACT.md`](API_CONTRACT.md))

**Stack (locked, §C.1):** Unity 6.3 LTS + C# client · **Firebase** (Firestore, Auth, Cloud Functions, Remote Config, Crashlytics, Analytics) · GitHub + LFS · Addressables for live content · Android first, iOS second · offline-first.

### Data flow
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
`js/engine.js` implements exactly these models (see `BN.defaultBarangay()`, `BN.newRun()`, `BN.applyContribution()`).

### Shared Barangay rules (§B.13 / §C.4)
- **Contribution (transaction):** Speak up +4 Pag-asa · Join +2 · Silent → Walang Kibo +1 (Tiwala drifts down) · Padrino-heavy (Utang ≥ 5) → Tiwala −3 · Echo opt-in → pool.
- **Era** from Pag-asa: 0–29 **Mabulok** (+2 systemic DC) · 30–59 **Karaniwan** · 60–84 **Gumigising** (−2 DC, more echoes) · 85–100 **Bagong Umaga** (special cards, Kap Eddie changes). Crossing 60 → visible town event.
- **`submitRunContribution(summary)`:** (1) require Auth UID; (2) validate shape/ranges; (3) transaction: read state, apply, recompute era, `version++`, write; (4) add echo if left; (5) return new state. Idempotent per `runId`.
- **Security Rules:** `barangay/state` and `echoes/{id}`: read public, write **false** (Admin SDK only). `players/{uid}`: read/write only if `request.auth.uid == uid`.
- **Seeding:** ship with preset state (Pag-asa = 45, Karaniwan) so the demo has history.

---

## 4. Rules engine (`js/engine.js`) — what the client must match

- **Stats** base 3, 0–10; modifier 0–1→+0 · 2–3→+1 · 4–5→+2 · 6–7→+3 · 8–10→+4.
- **Roll = d20 + modifier + assists vs DC**; nat 20 crit, nat 1 bigger cost (never a dead end); **no partial success**.
- **Padrino** auto-success, Integridad −1, **Utang +1/+2** (hidden).
- **Singil** at Utang ≥ 3 in Ch3: comply (Utang −2) / refuse (Integridad DC = 10 + Utang) / Bayanihan / delay (Utang +1).
- **Kasama:** Komunidad ≥ 3 → 1, ≥ 6 → 2; +1d4 each, max 2 per roll.
- **Echoes:** +1d6 each, max 2 per roll (Ch4).
- **Career (end of Ch2):** computed from stats/flags; player picks if 2+ qualify; fallback Ordinary Citizen.
- **Climax cascade:** Brave (0) → Angry (1) → Worried (2) → Vendors (3) → Silent Majority (4); chain reaction on player voices.
- **Endings:** 10, deterministic (`BN.determineEnding()`); redemption note if Komunidad ≥ 4 on a complicit path.

---

## 5. Content pipeline

```
scripts/content_a.py  (Ch1 + Ch2 cards)
scripts/content_b.py  (Ch3: 15 role + 4 shared + 3 Singil)
scripts/content_c.py  (Ch4 + endings + echoes + final asks + backgrounds + eras)
        │
        ▼  python3 scripts/build_content.py
content/cards/*.json (40) · content/{endings,echoes,final_asks,backgrounds,eras}.json
        │
        ▼  (real build) Addressables / Remote Config
```
Content rules (§B.15): every card has a *Huwag makialam* option · ≤1 obviously-good option · padrino uses euphemisms, never "bribe"/"corruption" · setup ≤ 80 words · no real names/parties/agencies · Taglish (Tagalog-leaning) with English variants.

---

## 6. Verification

```bash
node scripts/simulate.js 3000
```
3,000 simulated runs: no crashes, stats always 0–10, all 10 endings reachable, Singil triggers at Utang ≥ 3, contribution/era math stable. Balance levers (DCs, backgrounds, contribution) can be re-tuned in `scripts/content_*.py` and re-simulated.

---

## 7. Build phasing (§D.3)

1. **Lock design + data model + API contract** — done (`HANDOFF.md`, `API_CONTRACT.md`).
2. **Parallel:** content (cards/endings) ∥ frontend (Unity UI, main menu, settings) ∥ backend (Firestore, Functions, rules).
3. **Integrate continuously** via GitHub.
4. **Art + animation + world hub** (one walkable barangay hub, not an open world).
5. **Playtest → fix → polish** (5–10 people; targets: run 10–12 min · ~60% take ≥2 shortcuts first run · ≥50% meet a Singil · solo climax ~25–30%, with assists ~60–70%).
6. **Live ops** (Remote Config, Addressables, changelog, staged rollouts).

---

## 8. Locked decisions (don't drift)

See `HANDOFF.md` §D.1. Highlights: mobile-first (Android then iOS) · Shared Barangay **BUILD** · 29-card minimum (40 written) · all 10 endings · no partial success · Kasama +1d4 max 2 · pre-written echoes · career tiebreak = player picks · 4 eras · ~70% Taglish · presentation changes card text · location-specific final ask · 8 cast members · pixel/GBA art · animated physical d20 · AI-assisted writing.

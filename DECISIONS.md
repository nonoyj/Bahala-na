# DECISIONS.md — decision & change log

Per HANDOFF §A.4–5 and §D.2: any change not in the AI spec is assessed, validated, and explained here.
Locked decisions (HANDOFF §D.1) are **not** overridden by anything below; where this demo adapts the spec for a browser build, the adaptation is called out explicitly.

---

## D-001 — Playable demo built as a static web app (not Unity)
**Context.** The handoff targets a Unity 6.3 + Firebase mobile demo by 2026-12-09, but no Unity toolchain exists in this environment and the immediate need is a *playable, testable* build of the full design.
**Decision.** Implement the complete rules and content as a mobile-first static web app (`index.html` + `js/engine.js` + `content/*.json`), with the same data models and the same math.
**Impact.** Rules are playtestable in any browser today; the Unity client can be built against `API_CONTRACT.md` with `js/engine.js` as the reference implementation. No locked decision changes: platform, content, and contract stay as specified. Art is placeholder pixel-art (GBA style per 17D).

## D-002 — Shared Barangay persisted locally (not Firestore)
**Decision.** The demo stores `BarangayState` in `localStorage` (`bn_barangay`) and applies the exact §C.4 contribution rules client-side.
**Impact.** Single-device demo only; no anti-cheat. The real build must use `submitRunContribution()` (see `API_CONTRACT.md`) — do not port the local write path.

## D-003 — Kasama availability thresholds
**Spec.** "Komunidad grants Kasama tokens (simplified 7B: +1d4, max 2 per roll)."
**Decision.** Available Kasama dice = Komunidad ≥ 6 → 2, ≥ 3 → 1, else 0; optional spend, max 2 per roll.
**Impact.** Makes Komunidad mechanically meaningful without a separate token economy; matches the "hard to raise, easy to spend" intent.

## D-004 — Ending thresholds
**Spec.** §B.14 gives conditions in words ("Integridad high, Utang low").
**Decision.** "High Integridad" = ≥ 6; "low Utang" ≤ 2; "high Utang" ≥ 4. Teacher/Engineer ending 5 requires a firm act (honest/collective choice or `spoke_up`); ending 6 otherwise. Organizer ending 8 requires `burned_out` or no voices; 7 if reform or the player spoke.
**Impact.** Deterministic and testable; see `BN.determineEnding()`.

## D-005 — Climax cascade interpretation
**Spec.** §B.12 thresholds (Brave 0 → Silent Majority 4) with "a chain that stops because this player was the missing voice."
**Decision.** Nurse Lea (Brave) joins at Ch4 start (1 voice). Each player-added voice triggers the chain: any group whose threshold ≤ current voices joins and adds a voice. The **roll** (systemic DC 16–18, era-modified) is the gate; the cascade is the payoff visualization.
**Impact.** One successful public voice completes the cascade — faithful to the spec's thresholds; difficulty lives in the DCs, matching the §B.19 solo ~25–30% target.

## D-006 — Seed values
**Decision.** Demo ships with Tiwala 60 · Imprastruktura 35 · Pag-asa 45 (Karaniwan) · 24 runs · 9 spoke-up (37%, tipping point 40%) · Walang Kibo 0 · 12 echoes.
**Impact.** The barangay has history out of the box (per §C.4 seeding tip) and the "37% vs 40%" fact is visible immediately.

## D-007 — Utang is never numeric in the UI
**Decision.** All Utang deltas render as hints ("May bagong binabalakid…") and the Singil DC as "10 + utang"; the ledger shows favor/cost lines without a running total. Matches §C.5.
**Impact.** None on mechanics; `RunSave.utang` still stored locally.

## D-008 — English coverage
**Decision.** Full English translations for all 40 cards, 10 endings, echoes, UI chrome; Taglish remains the default (11C).
**Impact.** Doubles content authoring; toggle in Settings (language) and at the ending (location picker).

## D-009 — Presentation variants
**Decision.** `{{maam}}` token in card text resolves per presentation (Ma'am / Sir / Sir/Ma'am); name token `{{name}}`. Only cards with gendered address use it (12C).
**Impact.** Minimal; extend the token set if more variants are written.

## D-010 — Verification
**Decision.** `scripts/simulate.js` (3,000 runs: no crashes, stats 0–10, all 10 endings reachable) + a jsdom end-to-end playthrough.
**Impact.** Balance levers (DCs, backgrounds, contribution) can be re-tuned via `scripts/content_*.py` and re-simulated.

## D-011 — Branch scoped to backend (PR scope)
**Context.** The user owns **backend**; other contributors own **frontend** (Unity client, UI, world hub, animations).
**Decision.** This branch/PR carries backend artifacts only: `API_CONTRACT.md`, `js/engine.js` (rules + Shared Barangay logic), `content/` data + pipeline, `scripts/simulate.js`, and the docs. The prototype web UI demo (`index.html`, `css/`, `js/ui.js`, `js/main.js`, `assets/`) is **excluded** from the PR so it does not collide with frontend work.
**Impact.** Frontend contributors build the Unity client against the frozen `API_CONTRACT.md`; `js/engine.js` remains the reference implementation of the rules. D-001/D-002 describe the prototype demo only as a validation vehicle — do not treat its localStorage write path as the backend.

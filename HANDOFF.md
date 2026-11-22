# BAHALA NA? — Full Handoff (for all AIs and developers)

**Version:** 1.0 · **Date:** 2026-11-22 · **Status:** Master reference — read fully before acting
**Place:** Barangay Bayanihan, bayan ng San Isidro (fictional)
**Deliverable:** Playable mobile demo + one-page pitch, by **2026-12-09**

> **Who this is for.** You are an **AI working on *Bahala Na?*** — backend, frontend, or writer. This file is the **shared master for all AIs and all developers**. It is your complete context: the full storyline, every mechanic, the world, the tech stack, and every locked decision. It is self-contained — you do not need the original design document. Read it end to end.
>
> **What you must do (see §A).** After reading, (1) implement **your part** of the build per §C, and (2) contribute to / produce a **full context explanation for the developers** — what we are building, the storyline, the mechanics, the tech stack, the architecture, and the phasing — so the developers fully understand the project even if they never read the design doc.
>
> **Precedence.** Where this file and the original design doc conflict, **this file wins** for implementation. Locked decisions are in §D.

---

## §A. Instructions to the receiving AI (read first)

1. **Your role.** You are one of the AIs on *Bahala Na?* (backend, frontend, or writer). **Read the entire file** — every AI needs the full picture, not just its own slice. Then implement your part per §C: **backend** owns the Firebase/Firestore schema, Cloud Functions, security rules, Shared Barangay logic, and API contract; **frontend** owns the Unity client, UI, world hub, and animations; **writer** owns all card/ending/dialogue content (§E). Everyone codes against the same contract (§C.2–C.4).
2. **Read everything.** §B is the complete game (storyline, world, mechanics). §C is the build spec. §D is decisions + protocol + phasing. §E is the content written so far.
3. **Write back to the developers.** Produce a clear, complete context explanation for the developers covering: what the game is, the storyline and themes, how it plays (mechanics), the tech stack and why, the architecture and how frontend↔backend communicate, the data models, the Shared Barangay backend, the build phasing, and how all the pieces fit together. Assume the developers have **not** read the design doc — bring them up to speed fully.
4. **Remember the locked decisions (§D).** If a developer asks for something that conflicts with a locked decision, flag the conflict, explain the trade-off, and log it in `DECISIONS.md` before implementing.
5. **Dev-change rule.** Any change or decision a developer makes that is **not** written in the AI spec must be **assessed, validated, and explained to the other AI**. Concretely: review the diff, validate it against this file and the design, then write an explanation + impact note to `DECISIONS.md` and the PR so the other AI understands what changed and why.
6. **Communicate via GitHub** per §D. The API contract (`API_CONTRACT.md`) is frozen before parallel build; changes need a `DECISIONS.md` entry and both-side sign-off.
7. **This is the universal master** for all AIs and developers. When GitHub is connected and the chat is re-transferred, a refreshed copy of this same master (with any updates) will be produced for the GitHub-connected session. Do not wait for it — start on §C now.

---

## §B. The Game (complete bible)

### B.1 Pitch
*Bahala Na?* is a short, replayable, dice-driven life story set in a fictional Philippine barangay. You grow up from childhood to adulthood by choosing between cards; each hard moment is a d20 roll. Easy shortcuts (a tita's "favor," a small "pang-merienda") really do help early on, but they quietly build a hidden debt that comes due in adulthood. Alone, fighting a rigged system is a low-odds roll. With help from other people, the same roll becomes winnable. Every run permanently changes a **shared barangay** that later players inherit.

**Core message (never stated until the end):** *Bahala na* can mean "leave it to fate" or "take the leap." Fate looks different when people roll together.

### B.2 Design pillars
1. **Show, don't preach.** The game never says "corruption is bad." Consequences teach.
2. **Temptation must be real.** Shortcuts work, feel reasonable, have understandable reasons. Nobody is a cartoon villain.
3. **Alone is hard. Together is possible.** Solo odds are poor on systemic problems; help visibly improves them. Avoid both "you're powerless" and "one hero fixes everything."
4. **Doing nothing is a choice with a cost.** Every card offers a "Huwag makialam" option. Its cost is quiet and cumulative.
5. **Everyone matters.** The Ordinary Citizen is a first-class path, not a consolation prize.
6. **Hope with specifics.** The game ends on a real-world action, not despair.

### B.3 World
- **Barangay Bayanihan, bayan ng San Isidro.** Fictional. (Note: *San Isidro* is a common real town name in the PH; the team chose it knowingly. The *combination* "Barangay Bayanihan, bayan ng San Isidro" is fictional.)
- A typical Philippine town: plaza, covered court, market, public school, a river/canal that floods, barangay hall, and a municipal hall with a big welcome arch.
- Timeline spans ~25–30 years of the player's life. **No real years are named.**
- **Rule:** No real politicians, parties, or agencies named as villains. Use fictional parties (e.g., "Partido Pag-unlad").

### B.4 Cast (all fictional; avoid resemblance to real officials)
| Character | Role | Function |
|---|---|---|
| **Tita Baby** | Relative who works at municipal hall | Offers padrino favors; loving but compromised |
| **Kap Eddie** | Barangay captain | Ambiguous. Does some good, bends some rules. Changes with the town's state |
| **Ma'am Lourdes** | Grade-school teacher | Early moral anchor who can be disappointed or inspired |
| **Kuya Jun** | Local fixer | Friendly, helpful, always "can arrange it" |
| **Mayor Tonyo** | Town mayor | Charismatic, owes the contractor |
| **RC Dizon Builders** | Contractor | Wins most projects, funds the welcome arch |
| **Nurse Lea** | Health worker | Early example of someone who speaks up; template for "echo" characters |
| **Barkada (2–3 friends)** | Peer group | Source of Komunidad, pressure, and allies |

### B.5 Character creation — "Character Spark" (30 seconds)
1. Player picks a **name/alias** and **gender presentation** (boy, girl, or neutral). Affects pronouns and art only, never stats. (Per 12C, presentation also changes some card text — base text + variant lines.)
2. **Roll d6 for Family Circumstance.** Unequal starting lines are intentional and acknowledged at the end.

| d6 | Background | Starting bonus | Starting disadvantage |
|---|---|---|---|
| 1 | Mahirap na pamilya (low income) | Komunidad +2, Diskarte +1 | Pera −1, Talino −1 |
| 2 | Pamilya ng vendor/driver | Diskarte +2 | Koneksyon −1 |
| 3 | Pamilyang OFW | Pera +2 | Komunidad −1 (family is away) |
| 4 | Pamilya ng kawani (govt employee) | Koneksyon +2 | More favors offered early (higher Utang risk) |
| 5 | Pamilyang may negosyo | Pera +2, Koneksyon +1 | Komunidad −1 |
| 6 | Pamilyang guro/propesyonal | Talino +2, Integridad +1 | Pera −1 |

Base stats start at **3** before bonuses/penalties. Floor 0, cap 10.

### B.6 Stats
| Stat | Meaning | Used for |
|---|---|---|
| **Talino** | Knowledge, education, critical thinking | Studying, spotting scams, reading documents |
| **Diskarte** | Resourcefulness, street smarts, resilience | Improvising, surviving hardship, negotiating |
| **Pera** | Money and material security | Paying, resisting temptation when broke |
| **Koneksyon** | Clout, who you know | Padrino shortcuts, access, introductions |
| **Integridad** | Internal moral compass, reputation for honesty | Refusing pressure, being trusted with sensitive info |
| **Komunidad** | Real bonds with neighbors, kapwa, solidarity | **Bayanihan assists**, organizing, collective rolls |
| **Utang na Loob** *(hidden)* | Favors owed and accepted | Triggers Singil cards, hardens choices |

**Modifier table** (stat → bonus to d20): 0–1 → +0 · 2–3 → +1 · 4–5 → +2 · 6–7 → +3 · 8–10 → +4.
**Design note:** Integridad and Komunidad are **hard to raise** and **easy to spend**. That tension is the point.

### B.7 Core dice mechanic
**Roll = d20 + stat modifier + assist bonuses vs. Difficulty Class (DC)**

| DC | Feels like | Examples |
|---|---|---|
| 8 | Easy | Do homework, help a neighbor |
| 12 | Moderate | Win a scholarship, get a clerk to process papers |
| 15 | Hard | Resist a persuasive padrino, stand up to a boss |
| 18 | Systemic | Expose a contractor alone, change a town practice |

**Outcomes:** Natural 20 = critical success (bonus beat, "the whole room goes quiet"). Natural 1 = critical fail (bigger consequence, **always recoverable**, never a dead end). Success = stated effect. Fail = stated cost (still advances the story; never "nothing happens"). **No partial success (6B).**
**Odds sanity:** +2 vs DC 12 = 55%. +2 vs DC 18 = 25%. Systemic problems should feel mostly unwinnable alone, winnable (~60–70%) with 2–3 assists. Barangay Era modifies systemic DCs (§B.11).

### B.8 Signature mechanics
**7.1 Padrino Shortcut (temptation).** One option per card is labeled with a euphemism ("Magpa-ayos kay Tita Baby," "Pang-merienda lang," "SOP," "Pakikisama lang"). Effect: auto-success or reroll. Cost now: minimal (Integridad −1). Hidden cost: **Utang na Loob +1/+2.** The game never labels this "bribery"/"corruption" in the moment — use the words characters would really use (moral disengagement through euphemism).
**7.2 Singil Cards (debt called in).** When **Utang ≥ 3** in adulthood: *"May kailangan lang akong ipirma mo…"* Pay (Utang −2, Integridad −1, Tiwala ↓) / Refuse (Integridad vs DC = 10 + Utang; success clears debt + Komunidad +1, fail loses Pera/Koneksyon, debt stays) / Bayanihan (Komunidad ≥ 5 or an echo adds an assist) / Huwag makialam (Utang +1, returns harder). *A favor at 14 asks for a signature at 32.*
**7.3 Bayanihan Assist (collective).** In-run: Komunidad grants **Kasama** tokens (simplified per 7B: spend one for **+1d4, max 2 per roll**). Echoes (cross-player, pre-written per 8B): e.g., *"Nurse Lea leaked the supply log: +1d6 once,"* usable in Ch4. Anti-snowball: one die per echo, max 2 echoes per roll.

### B.9 Story structure — 4 chapters, 18 cards per run
**Ch1 Kabataan (Childhood), 4 of 6.** Ages ~8–12. Warm, small-scale, a little funny. Teaches rules through low-stakes moments. Goal: baseline stats + first small temptation.
**Ch2 Sangandaan (Crossroads/Youth), 5 of 8.** Ages ~16–22. Pressure rises. Scholarships, org elections, a barkada member in trouble, first contact with a fixer, vote-buying at a family gathering. **Determines your Adult Role (player picks if multiple qualify — 9B).**
**Ch3 Tungkulin (Duty/Adult Career), 6.** Ages ~25–35. Moral weight. **3 role-specific + 2 shared adult + 1 Singil** (or another shared card if Utang < 3).
**Ch4 Ang Pagsubok (The Test/Town Climax), 3 of 4.** A major flood hits; the town discovers the drainage project was a **ghost project** (funded, never built — while a welcome arch was built instead). Everyone must choose. Echoes, assists, and the tipping-point mechanic matter most.

### B.10 Career unlock (end of Ch2) — player picks (9B)
Compute qualifying careers from stats/flags. If **one** qualifies → assign. If **2+** qualify → **show the player the list and let them pick.** If **none** → Ordinary Working Citizen. (Fallback if no pick: most-recent qualifying flag wins; flags stored in order.)

| Career | Typically unlocked by |
|---|---|
| Politician / Barangay-City Official | Koneksyon ≥ 5 **and** (flag `ran_for_org` **or** Utang ≥ 2) |
| Police Officer | Flag `took_exam_or_followed_family_service` **and** (Diskarte ≥ 4 **or** Pera ≤ 3) |
| Teacher / Municipal Engineer | Talino ≥ 5 |
| Community Organizer / Local Journalist | Komunidad ≥ 5 **or** flag `spoke_up` |
| Ordinary Working Citizen | Fallback, or flag `went_to_work_early` (flavor: vendor, driver, call-center, OFW, nurse's aide) |

**Rule:** Each role must have a real power to tip the climax. The Ordinary Citizen's *number* matters more than any single role.

### B.11 Role-specific dilemmas (Ch3 outline)
| Role | Dilemma 1 | Dilemma 2 | Dilemma 3 |
|---|---|---|---|
| Official | A "consultant" budget line with no consultant | A friend asks for a contract preference | A whistleblower comes to you |
| Police | A traffic "settle na lang" at a checkpoint | A superior wants a report softened | A neighbor reports harassment by someone powerful |
| Teacher/Engineer | Sign off on substandard cement/materials | A principal asks to inflate classroom-fund receipts | A student shows evidence of an irregularity |
| Organizer/Journalist | A source wants anonymity but evidence is thin | A threat or a sweet offer to stop | Your group is tired and splitting |
| Ordinary Citizen | Barangay assembly: go or stay? | A neighbor asks you to co-sign a request | Sell your vote for ₱500 before the election? |

**Writer note:** Police and officials must also have honest, sympathetic options. The game does not claim whole professions are corrupt — it claims systems pressure individuals, and individuals together change systems.

### B.12 The Climax — "Pagtitipon"
The flood forces a **town assembly**. **5 NPC groups**, each with a hidden threshold to speak up:
| NPC group | Speaks up once this many voices have already spoken |
|---|---|
| The Brave (e.g., Nurse Lea) | 0 |
| The Angry Neighbors | 1 |
| The Worried Parents | 2 |
| The Cautious Vendors | 3 |
| The Silent Majority | 4 |

Every visible speaker adds **+1 voice**. The player chooses to **speak up first, follow, or stay silent.** A **failed** public attempt still counts as a voice (trying matters). Echoes from earlier players each count as additional voices. Reach the Silent Majority → **Reform**. Show it visually: one voice, then a chain reaction, or a chain that stops because *this player* was the missing voice.

### B.13 Shared Barangay (async multiplayer — **BUILD**, 3A)
Each completed run changes a persistent **Barangay State**.
| Index | Range | Affects |
|---|---|---|
| **Tiwala (Trust)** | 0–100 | NPC dialogue flavor, DC modifier on trust-based rolls |
| **Imprastruktura** | 0–100 | Climax flooding severity |
| **Pag-asa (Reform Progress)** | 0–100 | Barangay "era" |
| **Walang Kibo counter** | cumulative | Number of players who stayed silent at the climax |

**Per-run contribution:** Speak up +4 Pag-asa · Join once others spoke +2 · Stay silent → Walang Kibo +1 (slowly lowers Tiwala) · Padrino-heavy (Utang ≥ 5) → Tiwala −3 · Leave an echo (opt-in).
**Barangay Eras:** 0–29 **Mabulok** (Systemic DCs +2) · 30–59 **Karaniwan** (baseline) · 60–84 **Gumigising** (Systemic DCs −2, more echoes) · 85–100 **Bagong Umaga** (special cards unlock, Kap Eddie's story changes). Crossing 60 triggers a visible town event. **Seeding tip:** playtest with 25–30 people before the demo so the barangay has history.

### B.14 Endings (10 core)
| # | Title | Condition (summary) |
|---|---|---|
| 1 | **Ang Kapitang Hindi Yumuko** | Official, Integridad high, Utang low |
| 2 | **Ang Pirma** | Official, Utang high, complied with Singil |
| 3 | **Ang Badge** | Police, Integridad high, spoke up |
| 4 | **Ang Sumunod** | Police, complied under pressure |
| 5 | **Ang Tamang Semento** | Teacher/Engineer, refused or reported |
| 6 | **Ang Pumikit** | Teacher/Engineer, stayed silent |
| 7 | **Ang Boses** | Organizer/Journalist, cascade succeeded |
| 8 | **Ang Pagod na Boses** | Organizer/Journalist, acted alone and burned out (cost of isolation; notes others could have helped) |
| 9 | **Ang Sumama** | Ordinary Citizen who joined the cascade |
| 10 | **Ang Nanood** | Ordinary Citizen who stayed silent |

**Rules:** Complicit endings are **humanizing**, not punishing (show *why* — a sick parent, a loan). **"Pagbawi" (redemption):** if on a complicit path but Komunidad ≥ 4, a late card can change the epilogue tone ("it's never too late"). **"Starting line" note:** *"You began with [background]. Others began elsewhere."*
**Final screen: "Ikaw at ang Bayan"** — (1) personal epilogue card (tarot-style illustration, 3–4 lines); (2) barangay impact (your Pag-asa contribution + a threshold fact, e.g., "37% of players spoke up. The tipping point is 40%."); (3) reveal of the hidden Utang ledger (each favor and its later cost); (4) **real-world call to action** — one concrete, **location-specific** step (13C), e.g., "Attend your next barangay assembly. Ask to see the posted budget. Bring one neighbor."

### B.15 Card template (content format)
```
ID:            ch2_scholarship_01
Chapter:       2
Tags:          [education, padrino]
Requires:      (optional) stat or flag conditions
Setup text:    2-4 sentences, in-world voice, Taglish (Tagalog-leaning)
Choices:       2-4 options, each with:
   - Label (what the player reads)
   - Type: Honest / Padrino / Collective / Huwag makialam
   - Roll: stat + DC (or none)
   - Success effect
   - Fail effect
   - Hidden effects (Utang, flags)
Callback hook: (optional) a flag this card sets for later cards
```
**Writing rules:** (1) Every card has a "Huwag makialam" option (delayed/subtle cost). (2) At most one obviously "good" option. (3) Padrino options use euphemisms, never "bribe"/"corruption." (4) Failure is interesting (earns a lesson/ally/flag). (5) Setup ≤ 80 words. (6) No real names/parties/agencies. (7) Taglish by default (Tagalog-leaning); English toggle in settings.

### B.16 Sample cards (from design doc)
**Ch2 "Scholarship ni Tita Baby":** Scholarship list closes Friday. Tita Baby texts: *"Ako na bahala. Isang tawag lang sa kaibigan ko sa munisipyo."* — "Salamat po, Tita." (Padrino) auto-success, Utang +2, Integridad −1, Koneksyon +1 · "Mag-aapply ako nang patas." (Honest) Talino DC 13 · "Magtanong tayo ng kriteria, kasama ang mga kaklase." (Collective) Komunidad DC 14 + Kasama, sets `spoke_up` · "Hindi na lang." (Huwag makialam) Pera −1.
**Ch3 (Ordinary) "Singit sa Eleksyon":** Kuya Jun hands you ₱500. *"Pang-merienda lang 'to."* — Accept (Padrino) Pera +1, Integridad −1, Utang +1 · Refuse politely (Honest) Integridad DC 12 · Refuse and tell neighbors (Collective) Komunidad DC 15, town Tiwala +1 · "Hindi ko problema." (Huwag makialam) tracked by Walang Kibo.
**Ch3 (Shared) "Ang Pirma"** (Utang ≥ 3): Tita Baby calls. *"May papel lang… pirma lang."* — Sign (Utang −2, Integridad −1, Tiwala −2) · Refuse (Integridad DC 10+Utang) · Ask a friend/Nurse Lea (Collective, add assist) · "Pag-iisipan ko." (Utang +1, returns harder).
**Ch4 "Baha":** The canal overflows; the "completed" drainage project doesn't exist; the arch is lovely. — Speak first (DC 18, adds first voice) · Wait (joins if cascade reaches threshold) · Bring neighbors (Komunidad/Kasama/echoes) · Leave (Walang Kibo +1).

### B.17 Card pool plan (29 minimum → 40 full)
| Chapter | Pool | Drawn/run | Min viable |
|---|---|---|---|
| Ch1 Kabataan | 6 | 4 | 5 |
| Ch2 Sangandaan | 8 | 5 | 6 |
| Ch3 role-specific | 15 (3×5) | 3 | 10 (2×5) |
| Ch3 shared adult | 4 | 2 | 3 |
| Ch3 Singil | 3 | 1 | 2 |
| Ch4 Pagsubok | 4 | 3 | 3 |
| **Total** | **40** | **18** | **29** |

### B.18 Tone & sensitivity
Warm, wry, Filipino; humor thins as stakes rise. "Corruption" rare in-game. Humanize everyone (a clerk taking "pang-merienda" has a reason). Violence/threats implied, not depicted; pair threats with support/exit options. No real people/parties/agencies/events as villains. If a player stays silent, the ending is sober, never mocking — invite, don't shame. No hero-worship (reform comes from many). Class awareness: the background roll must never imply poorer players are less moral. Final ask is non-coercive.

### B.19 Playtest & validation
Paper prototype (index cards, physical d20, stat sheet). Test: (1) Do players feel tempted by padrino? (2) Are Singil cards surprising yet fair? (3) Does the climax produce "I should have spoken up earlier"? (4) Does anyone feel preached at? (5) Is every career fun, incl. Ordinary Citizen? Optional pre/post 1–5 survey (collective efficacy, assembly attendance, "one person's actions matter"). **Balance targets:** run 10–12 min · ~60% take ≥2 shortcuts first run · ≥50% meet a Singil · solo climax ~25–30%, with assists ~60–70%.

### B.20 Glossary
Bahala na ("come what may"; fatalism and courage) · Utang na loob (debt of gratitude) · Padrino (patron/sponsor) · Pakikisama (going along to keep harmony) · Hiya (shame/social discomfort) · Kapwa (shared identity; "fellow person") · Bayanihan (communal effort) · Singil (a debt being collected) · Pang-merienda/SOP (euphemisms for small bribes/commissions) · Huwag makialam ("mind your own business") · Kasama (ally token) · Walang Kibo ("didn't move"; silent-player count).

---

## §C. The Build (tech spec)

### C.1 Tech stack
| Layer | Choice | Why |
|---|---|---|
| Engine | **Unity 6.3 LTS** (2D, URP 2D Renderer) | Current LTS (to Dec 2027); best cross-platform 2D mobile |
| Language | **C#** | Unity standard |
| Backend | **Firebase** — Firestore, Auth, Cloud Functions, Remote Config, Crashlytics, Analytics | Best-in-class mobile offline + real-time; fastest for a small team |
| Alt backend | Supabase (Postgres) | If open-source/SQL/portability preferred |
| Version control | **GitHub** + Git LFS (art) | Required for AI-to-AI collaboration (§D) |
| CI/CD | GitHub Actions → Firebase App Distribution / Play internal testing / TestFlight | Automated builds to testers |
| Live updates | **Firebase Remote Config** + **Unity Addressables** | Tune/push content without store review (§C.9) |
| Platforms | Android (first), iOS (second) | One Unity codebase |

**Perf targets:** light 2D pixel art; 60 fps on low-end Android; sprite atlases, object pooling, Addressables; test on a low-end device early.
**Connectivity:** offline-first — full game playable offline; BarangayState syncs when online (Firestore offline persistence + sync queue).

### C.2 Architecture & data flow ("the exchange")
```
Unity Client (Android/iOS)                Firebase
  • Card/run UI            ◄──read──►   Firestore: BarangayState (singleton),
  • Local save (JSON)                     Echoes (collection), PlayerProfiles
  • Dice animation           ──write─►   Cloud Functions: submitRunContribution()
  • Offline queue            (validated)  Remote Config (tuning), Auth (anon)
```
- **Reads** (BarangayState, Echoes) → Firestore directly, Security Rules (public read, no client write).
- **Writes** to BarangayState → **only** via `submitRunContribution()` (validates, transacts, recomputes era). Clients never write it directly (anti-cheat).
- **Offline:** local JSON save is on-device source of truth; Firestore caches reads; writes queue and sync on reconnect.
- **Content (cards):** JSON via Addressables and/or Remote Config → changeable without an app update.

### C.3 Data models
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

### C.4 Backend spec — Shared Barangay (3A = BUILD)
**BarangayState** is one Firestore doc; only the Cloud Function writes it.
**Contribution rules** (applied in a transaction): Speak up +4 Pag-asa · Join +2 · Silent → Walang Kibo +1 (Tiwala drifts down) · Padrino-heavy (Utang ≥ 5) → Tiwala −3 · Echo opt-in → added to pool.
**Era** (from Pag-asa): 0–29 Mabulok (+2 DC) · 30–59 Karaniwan · 60–84 Gumigising (−2 DC, more echoes) · 85–100 Bagong Umaga (special cards, Kap Eddie changes). Crossing 60 → visible town event.
**`submitRunContribution(summary)`:** (1) require Auth UID; (2) validate shape/ranges; (3) transaction: read state, apply, recompute era, `version++`, write; (4) add echo if left; (5) return new state.
**Security Rules:** `barangay/state` and `echoes/{id}`: read public, write **false** (Admin SDK only). `players/{uid}`: read/write only if `request.auth.uid == uid`.
**Seeding:** ship with preset state (e.g., Pag-asa = 45, Karaniwan) so the demo has history.

### C.5 Game systems spec (for implementation)
Stats (base 3, 0–10); modifier table (§B.6); roll = d20 + mod + assists vs DC (§B.7); outcomes incl. crits, **no partial success (6B)**; Kasama **+1d4 max 2 (7B)**; Padrino (euphemisms, Utang +1/+2); Singil (Utang ≥ 3, DC = 10 + Utang to refuse); Echoes pre-written, +1d6, max 2/roll (8B); **career: player picks if 2+ qualify (9B)**; climax cascade (§B.12); 10 endings (§B.14); 4 eras (10B). Utang is **never shown as a number** in UI — only indirect hints.

### C.6 Content pipeline
Cards authored as **JSON** (`content/cards/*.json`) matching §C.3. Writing (AI) → JSON → reviewed → committed → built into **Addressables** and/or **Remote Config**. Every card needs a "Huwag makialam" option; ≤1 obviously-good option; euphemisms for padrino; setup ≤80 words; Taglish (Tagalog-leaning); no real names/parties/agencies. Gender variants (12C) stored in JSON (e.g., `setup_variants`).

### C.7 Frontend spec
Clean layout, clear icons, readable text, touch-first (big tap targets, no hover UI). Pixel-art world (§C.8). 10–12 min runs, autosave after every card, Continue to resume.
**Main menu:** Play Now (Character Spark) · Continue (if save) · Barangay (shared state, era, your contribution) · Echoes · How to Play · Settings · About/Credits (backstory, FAQ, Privacy, Terms).
**Settings:** Language (Taglish default / English) · Sound FX on/off+volume · Music on/off+volume · Vibration · Dice animation (Full/Fast/Off) · Text size (Standard/Large) · High-contrast/colorblind indicators · Reset progress (confirm) · Privacy · Terms · Credits · Report an issue.
**In-run HUD:** chapter + card progress, 6 visible stats, Kasama tokens, hidden-Utang hint (never a number), location-specific final ask on the final screen (13C).

### C.8 Art & animation spec
Pixel art, GBA / Pokémon Emerald style. **Sprites (PNG, Unity-importable):** world tileset (plaza, covered court, market, school, river/canal, barangay hall, municipal hall, welcome arch, houses, streets); character sprites (player ×3 presentations, Tita Baby, Kap Eddie, Ma'am Lourdes, Kuya Jun, Mayor Tonyo, Nurse Lea, barkada, townsfolk); card illustrations; tarot-style ending cards (10); UI icons.
**World (scope):** a full open sandbox is high-risk. **Recommended:** one walkable **barangay hub** map (the locations above); locations gate chapters/cards. Sandboxy feel without open-world scope. Expand later.
**Animations:** card transitions, animated physical d20 (18D), climax cascade visualization, ending animations, per-event sprite tweens/particles. Images generated as PNG, imported to Unity, polished later.

### C.9 Testing, QA & live ops
**Testing:** paper/logic test first (5–10 people) before deep Unity work — temptation, Singil fairness, climax feel, preachy lines, every career fun. Career logic: 10 paper playthroughs (incl. player-pick, 9B). Device testing (low-end Android + iOS, 60 fps). Offline testing (full run offline, sync, no loss). Backend load-test `submitRunContribution` (concurrent writes, transaction safety, Security Rules). Balance targets in §B.19.
**Live ops ("every fix is part of progress"):** Remote Config tunes DCs/balance/flags without an app update; Addressables push cards/art/text without store review; in-app changelog ("Ano ang bago"); staged rollouts (small % first); small content drops as measurable experiments.

---

## §D. Decisions, protocol, phasing

### D.1 Locked decisions (source of truth)
| # | Decision | Value |
|---|---|---|
| 1A | Deliverable | Playable mobile demo + one-page pitch |
| 2A | Platform | Mobile app (Android first, iOS second) |
| 3A | Shared Barangay | **BUILD** (real backend) |
| 4A | Card count | **29 minimum first**, add more if time |
| 5A | Endings | **All 10** (short templates) |
| 6B | Partial success | **Cut** for v1 |
| 7B | Kasama | **+1d4, max 2 per roll** |
| 8B | Echoes | **Pre-written** pool |
| 9B | Career tiebreak | **Player picks** if 2+ qualify |
| 10B | Eras | Keep **4** |
| 11C | Language | ~70% Taglish, Tagalog-leaning |
| 12C | Gender presentation | **Changes card text** (base + variants) |
| 13C | Final ask | **Location-specific** (in-app picker) |
| 14C | Place names | Barangay **Bayanihan**, bayan ng **San Isidro** |
| 15C | Cast | Keep all **8** |
| 16C | Content advisor | Skipped |
| 17D | Art | Pixel/GBA, Unity 2D |
| 18D | Dice | Animated physical d20 |
| 19D | Playtesters | 5–10 |
| 20D | Writing | AI-assisted |

### D.2 GitHub collaboration protocol (AI-to-AI)
**Repo:** single GitHub repo (e.g., `bahala-na`), **Git LFS** on. This chat/session will be **re-transferred** once GitHub is connected — this file is self-contained so the new session can pick up cold.
**Branches:** `main` (protected) · `feature/<name>` · `content/cards` · `backend/<name>` · `frontend/<name>`.
**Shared files:** `HANDOFF.md` (this) · `DECISIONS.md` · `CHANGELOG.md` · `API_CONTRACT.md` (Firestore schema + Function signatures, §C.3–C.4) · `content/cards/*.json`.
**Rules:** (1) Both AIs read/write shared files via GitHub. (2) PRs reference decision/section IDs and update `DECISIONS.md` on any deviation. (3) **Dev-change rule:** any dev change not in the AI spec is assessed, validated, and explained to the other AI (review diff → validate vs this file → write explanation + impact to `DECISIONS.md` + PR). (4) `API_CONTRACT.md` frozen before parallel build; changes need a `DECISIONS.md` entry + both-side sign-off.

### D.3 Build order (recommended phasing)
> Story-first is correct. The adjustment: **lock the contract early, then build content + frontend + backend in parallel**, integrating continuously. Sending the handoff to backend only after frontend layout creates a bottleneck and late integration risk.
1. **Lock design + data model + API contract** (this file). *(now)*
2. **Parallel:** content (cards/endings) ∥ frontend (UI, main menu, settings) ∥ backend (Firestore, Functions, rules) — against the frozen contract.
3. **Integrate continuously** via GitHub.
4. **Art + animation + world hub.**
5. **Playtest → fix → polish.**
6. **Live ops.**
Frontend order: main menu + settings → in-run card UI → world hub → intro/FAQ/privacy → ending animations → finalize.

---

## §E. Content so far

### E.1 Chapter 1: Kabataan (approved, 6-card pool)
Assumptions: no partial success (6B), Tagalog-leaning Taglish, every card has a *Huwag makialam* option, failures advance the story. Place names avoided (school/street scenes).

**Card 1 — Pondo ng Klase** · `ch1_pondo_01` · Tags: school, integridad, padrino
Setup: *"Iniwan ni Ma'am Lourdes ang kahon ng ambagan sa mesa niya at lumabas saglit. Bumulong si Pia, ang katabi mo: '₱20 lang, babayaran ko bukas. Walang makakaalam.'"*
- "Huwag. Sabihin natin kay Ma'am." (Honest) — Integridad DC 8. Success: Integridad +1; Pia upset, Ma'am notices. Fail: "sumbungera," Komunidad −1.
- "Sige, isang beses lang." (Padrino) — Pera +1, Integridad −1, **Utang +1**.
- "Pabayaan, hindi ko kasalanan." (Huwag makialam) — Flag `saw_it`.
Callback: `saw_it` → Ch2 (Pia asks for a bigger favor).

**Card 2 — Baon** · `ch1_baon_02` · Tags: school, komunidad, pera
Setup: *"Tanghalian na. Si Rico, ang kaklaseng laging maaga, ay walang baon — nakalimutan daw ng nanay niya. May sobra kang tinapay at itlog. Tumingin siya sa pagkain mo, tapos sa'yo."*
- "Hati tayo." (Collective) — Komunidad DC 8. Success: Komunidad +1, flag `shared_food`. Fail: nahiya siya; `shared_food` still set.
- "Palit tayo ng meryenda." (Diskarte) — Diskarte DC 10. Success: Pera +1, Komunidad +1. Fail: "ayaw ng palit," flag `rico_proud`.
- "Hindi ko napansin." (Huwag makialam) — Flag `looked_away`.
Callback: `shared_food` → Ch2 (Rico returns favor). `looked_away` → Ch2 (Rico's situation worsens).

**Card 3 — Kopya** · `ch1_kopya_03` · Tags: school, integridad, utang
Setup: *"May quiz bukas at hindi nakapag-review si Bea, ang kaibigan mong magaling mag-drawing. 'Pahiram ng sagot mo mamaya, isang tingin lang. May utang ako sa'yo, promise.'"*
- "Hindi pwede. Sabay tayo mag-review." (Collective) — Talino DC 10. Success: Talino +1, Komunidad +1, flag `studied_together`. Fail: naubusan ng oras; `studied_together` still set.
- "Sige, kopyahin mo." (Padrino) — Integridad −1, **Utang +1**.
- "Bahala ka." (Huwag makialam) — Kinopya ka pa rin ni Bea. Flag `silent_copy`.
Callback: Utang +1 → Singil seed. `studied_together` → Ch2 barkada.

**Card 4 — Pirma ni Nanay** · `ch1_pirma_04` · Tags: family, padrino, koneksyon
Setup: *"Field trip bukas, pero kailangan ng pirma ng magulang. Nasa abroad si Nanay at si Tatay ay nasa bukid pa. Sabi ni Ate Pia: 'Ako na ang pipirma. Para lang makasama ka.'"*
- "Salamat, Ate." (Padrino) — Auto-success. **Utang +1**, Integridad −1, Koneksyon +1.
- "Magpapaalam ako kay Ma'am nang diretso." (Honest) — Talino DC 10. Success: Talino +1. Fail: miss trip, Diskarte +1, flag `missed_trip`.
- "Hindi na lang ako sasama." (Huwag makialam) — Flag `stayed_home`.
Callback: Utang +1 → Singil. `missed_trip` → Ch2 (Ma'am remembers honesty). Presentation: Ate Pia addresses you as "bunso"/by name per presentation.

**Card 5 — Baha sa Kalye** · `ch1_baha_05` · Tags: barangay, komunidad, diskarte
Setup: *"Malakas ang ulan at binaha ang kalye. Si Aling Cora ay nagbubuhat ng mga paninda papasok sa bahay niya, pero hanggang bewang na ang tubig. 'Tulungan mo ako, anak.'"*
- "Sige po, buhatin natin." (Collective) — Komunidad DC 10. Success: Komunidad +1, flag `helped_cora`. Fail: nadulas, Diskarte +1; `helped_cora` still set.
- "May payong at bota ako, gamitin natin." (Diskarte) — Diskarte DC 12. Success: Pera +1, Komunidad +1. Fail: Pera −1 (nabasa gamit).
- "Umuwi na lang ako." (Huwag makialam) — Flag `walked_past`.
Callback: `helped_cora` → Ch2/Ch3 (vendor contact).

**Card 6 — Tinda sa Palengke** · `ch1_tinda_06` · Tags: family, padrino, pera
Setup: *"Hapon na at nagsasara na ang pamilya mo ng tindahan sa palengke. Bumulong ang tanod: 'Maliit lang 'to… pang-merienda lang. Para hindi na maabala.'"*
- "Sige po, pang-merienda." (Padrino) — Pera +1, **Utang +1**, Integridad −1.
- "Ayusin namin ang pwesto, ngayon na." (Honest) — Diskarte DC 12. Success: Diskarte +1, no fine. Fail: Pera −1, flag `fixed_stall`.
- "Si Nanay na lang ang bahala." (Huwag makialam) — Flag `let_nanay`.
Callback: Utang +1 → Singil. `let_nanay` → Ch2 (arrangement deepens).

### E.2 Chapter 2: Sangandaan (approved, 8-card pool, 5 drawn per run)
Ages ~16–22. Pressure rises. Sets career flags. Callbacks to Ch1 flags. (Full text in `BahalaNa_Cards.md`.)

**Card 1 — Scholarship ni Tita Baby** · `ch2_scholarship_01` · Tags: education, padrino
Setup: *"Bukas na ang deadline ng scholarship list. Nag-text si Tita Baby: 'Ako na bahala. Isang tawag lang sa kaibigan ko sa munisipyo.'"*
- "Salamat po, Tita." (Padrino) — Auto-success. **Utang +2**, Integridad −1, Koneksyon +1.
- "Mag-aapply ako nang patas." (Honest) — Talino DC 13. Success: Talino +1. Fail: lose out, Pera −1.
- "Magtanong tayo ng kriteria, kasama ang mga kaklase." (Collective) — Komunidad DC 14 + Kasama. Success: Komunidad +2, flag `spoke_up`. Fail: `spoke_up` still set.
- "Hindi na lang." (Huwag makialam) — Pera −1.
Callback: Utang +2 → Singil. `spoke_up` → Organizer career.

**Card 2 — Halalan sa Org** · `ch2_org_02` · Tags: school, koneksyon, politika
Setup: *"Halalan na ng student org. Sabi ni Bea: 'Tumakbo ka. Alam kong mananalo ka.' Pero may narinig kang balita — may nag-aalok ng pera para sa boto."*
- "Tatakbo ako, patas." (Honest) — Koneksyon DC 13. Success: Koneksyon +1, flag `ran_for_org`. Fail: lose, `ran_for_org` still set; Komunidad +1.
- "Sabihin kay Kuya Jun, ayusin niya." (Padrino) — Auto-win. **Utang +2**, Integridad −1, `ran_for_org`.
- "Suportahan ko na lang si Bea." (Collective) — Komunidad DC 12. Success: Komunidad +1, `supported_friend`. Fail: bond remains.
- "Hindi ako makikialam." (Huwag makialam) — Flag `sat_out`.
Callback: `ran_for_org` → Politician career. Utang +2 → Singil.

**Card 3 — Pia, Mas Malaking Pabor** · `ch2_pia_03` · Tags: school, utang, integridad
Requires: `saw_it` OR Utang ≥ 1.
Setup: *"Lumapit si Pia, kinakabahan. 'Kailangan kong sabihin kay Ma'am Lourdes na ikaw ang kumuha ng ambagan. Hindi ko kaya… pero ikaw, pwede.'"*
- "Hindi. Hindi ko 'yan gagawin." (Honest) — Integridad DC 13. Success: Integridad +1. Fail: rumor, Komunidad −1.
- "Sige, ako na ang magsabi." (Padrino) — Take the blame. **Utang +2**, Integridad −1, Komunidad −1.
- "Sabihin natin ang totoo, tayong dalawa." (Collective) — Komunidad DC 14. Success: Komunidad +1, `told_truth`. Fail: Ma'am disappointed in both.
- "Bahala ka na, Pia." (Huwag makialam) — Flag `left_pia`.
Callback: Utang +2 → Singil. `left_pia` → Ch3.

**Card 4 — Bea, Mas Malaking Pabor** · `ch2_bea_04` · Tags: school, utang, integridad
Requires: `silent_copy` OR Utang ≥ 1.
Setup: *"May malaking exam bukas at hindi pa rin nakapag-review si Bea. 'Isang beses na lang, please. Pag hindi ako pumasa, lagot ako kay Nanay.'"*
- "Hindi. Pero tutulungan kitang mag-review ngayon." (Collective) — Talino DC 12. Success: Talino +1. Fail: exhausted, but tried.
- "Sige, kopya ka na lang." (Padrino) — **Utang +2**, Integridad −1.
- "Hindi ko 'yan pipirmahan." (Honest) — Integridad DC 12. Success: Integridad +1. Fail: Komunidad −1.
- "Bahala ka." (Huwag makialam) — Flag `left_bea`.
Callback: Utang +2 → Singil. `left_bea` → Ch3.

**Card 5 — Kuya Jun, ang Fixer** · `ch2_fixer_05` · Tags: barangay, padrino, koneksyon
Setup: *"May problema ang pamilya mo — hindi maayos ang papeles sa munisipyo. Lumapit si Kuya Jun, nakangiti. 'Ako na bahala. Kilala ko ang lahat doon.'"*
- "Salamat, Kuya." (Padrino) — Auto-success. **Utang +2**, Integridad −1, Koneksyon +1.
- "Aayusin ko nang ako lang." (Honest) — Diskarte DC 13. Success: Diskarte +1. Fail: Pera −1.
- "Isama ko si Aling Cora, baka alam niya." (Collective) — Komunidad DC 12 (DC 10 if `helped_cora`). Success: Komunidad +1. Fail: delayed, but Aling Cora helps.
- "Hayaan ko na lang si Nanay." (Huwag makialam) — Flag `let_nanay2`.
Callback: Utang +2 → Singil. Kuya Jun → recurring fixer. `let_nanay2` → Ch3.

**Card 6 — Bilhan ng Boto** · `ch2_vote_06` · Tags: family, padrino, politika
Setup: *"Pista sa baryo at may nag-aalok ng ₱500 sa bawat boto. 'Pang-merienda lang 'to,' sabi ni Kuya Jun. 'Wala namang masama.'"*
- "Sige, isang boto lang." (Padrino) — Pera +1, Integridad −1, **Utang +1**.
- "Hindi po, salamat." (Honest) — Integridad DC 12. Success: Integridad +1. Fail: Koneksyon −1.
- "Sabihin ko sa mga kapitbahay." (Collective) — Komunidad DC 15. Success: Tiwala +1, `spoke_up`. Fail: Komunidad −1, `spoke_up` set.
- "Hindi ko problema." (Huwag makialam) — Walang Kibo tracked.
Callback: Utang +1 → Singil. `spoke_up` → Organizer career.

**Card 7 — Rico, sa Barkada** · `ch2_rico_07` · Tags: barkada, komunidad
Requires: `looked_away` OR `shared_food`.
Setup: *"Na-ospital ang nanay ni Rico at kailangan ng pera. Sabi ng barkada: 'Tulungan natin siya.' Pero ang iba, ayaw madamay."*
- "Tulungan natin siya, lahat tayo." (Collective) — Komunidad DC 13. Success: Komunidad +2, `helped_rico`. Fail: Komunidad +1, `helped_rico` set.
- "Magbibigay ako ng pera." (Pera) — No roll. Pera −1, Komunidad +1, `helped_rico`.
- "Tumayo ako para kay Rico." (Honest) — Integridad DC 14. Success: Integridad +1, Komunidad +1, `spoke_up`. Fail: Komunidad −1, `spoke_up` set.
- "Hindi ko kaya." (Huwag makialam) — Flag `abandoned_rico`.
Callback: `helped_rico` → Ch3/Ch4 ally. `spoke_up` → Organizer career.

**Card 8 — Pagsusulit o Trabaho** · `ch2_exam_08` · Tags: crossroads, karera
Setup: *"Dumating ang sangandaan. May pagsusulit para sa serbisyo — o pwede kang magtrabaho agad para tumulong sa pamilya. Hindi pwedeng pareho."*
- "Kukuha ako ng pagsusulit." (Honest) — Talino DC 13. Success: Talino +1, flag `took_exam_or_followed_family_service`. Fail: Diskarte +1, flag set.
- "Sabihin kay Kuya Jun, ayusin ang resulta." (Padrino) — Auto-pass. **Utang +2**, Integridad −1, flag `took_exam_or_followed_family_service`.
- "Magtrabaho na lang ako." (Ordinary) — Flag `went_to_work_early`. Pera +1, Talino −1.
- "Hindi ko pa alam." (Huwag makialam) — Flag `undecided`.
Callback: `took_exam_or_followed_family_service` → Police career. `went_to_work_early` → Ordinary Citizen. Utang +2 → Singil.

### E.3 Chapter 3: Tungkulin (approved, 22-card pool; 6 drawn per run)
Ages ~25–35. Moral weight. 3 role-specific + 2 shared adult + 1 Singil (if Utang ≥ 3). Full text in `BahalaNa_Cards.md`. Summary of all 22:

**Official:** O1 *Ang Consultant* (sign a ₱300k "consultant" budget line with no consultant — refuse / sign (Utang +2) / post the budget (Collective, `spoke_up`) / ignore). O2 *Ang Pabor sa Kaibigan* (RC Dizon wants a contract preference — fair bidding / give it (Utang +2, ghost-project seed) / open bid / ignore). O3 *Ang Whistleblower* (a clerk brings ghost-project evidence — expose it (Collective, `spoke_up`) / report privately / pass to Kuya Jun (Utang +1) / ignore).
**Police:** P1 *Ang Checkpoint* (driver offers "settle na lang," has a sick child — ticket / settle (Utang +1) / help him get a license / ignore). P2 *Ang Report* (superior wants a report softened to protect a councilor — refuse / soften (Utang +1) / truth-but-safe (Diskarte) / ignore). P3 *Ang Reklamo* (neighbor reports harassment by someone powerful — investigate / help her file safely (Collective, `spoke_up`) / pass to Kap Eddie / ignore).
**Teacher/Engineer:** T1 *Ang Semento* (sign off on substandard cement — refuse / sign (Utang +2, ghost-project seed) / test the materials (Talino) / ignore). T2 *Ang Resibo* (principal wants inflated classroom-fund receipts — refuse / inflate (Utang +1) / audit / ignore). T3 *Ang Ebidensya* (a student shows proof of a school-project irregularity — report / document it (Collective, `spoke_up`) / pass to Kuya Jun (Utang +1) / silence the student).
**Organizer/Journalist:** J1 *Ang Source* (anonymous source, thin evidence on the ghost project — publish carefully / verify more / verify with barkada / kill the story). J2 *Ang Alok* (offered money to stop investigating — refuse / take it (Utang +2) / document the offer / stop). J3 *Ang Pagod na Grupo* (group is burned out — rest and return / continue alone (risk `burned_out` → Ending 8) / find new allies / leave the group).
**Ordinary Citizen:** C1 *Ang Assembly* (barangay assembly about the flood — attend and speak (Collective, `spoke_up`) / send a letter / pass to Kuya Jun (Utang +1) / skip). C2 *Ang Co-sign* (neighbor needs a signature for a water request — sign / refuse / pass to Kuya Jun (Utang +1) / refuse). C3 *Ang ₱500* (offered ₱500 the night before the election — accept (Utang +1) / refuse / tell neighbors (Collective, `spoke_up`) / ignore).
**Shared Adult:** S1 *Ang Sakit ni Nanay* (mother hospitalized, needs money now — Tita Baby's "one signature" (Utang +2) / find funds (Diskarte) / ask the barkada (Collective) / can't pay). S2 *Ang Sirang Kalsada* (flooded street, Kap Eddie says wait for funds — organize bayanihan (Collective, `spoke_up`) / complain to Kap Eddie / pass to Kuya Jun (Utang +1) / wait). S3 *Ang Utos ng Boss* (boss wants you to change numbers or lose your job — refuse / change them (Utang +1) / document it / ignore). S4 *Ang Pagbabalik ni Pia* (Pia returns after years, needs a signature for her sibling — sign (Utang +2) / refuse / find another way (Collective) / reject her).
**Singil (Utang ≥ 3):** G1 *Ang Pirma* (Tita Baby: "may papel lang… pirma lang" — comply (Utang −2) / refuse (Integridad DC 10+Utang) / ask Nurse Lea (Collective) / delay (Utang +1)). G2 *Ang Pirma ni Kuya Jun* (vouch for a project — comply / refuse / bring the barkada / delay). G3 *Ang Utang ni Bea* (Bea calls in the favor — comply / refuse / ask for help / delay).

### E.4 Chapter 4: Ang Pagsubok (approved, 4-card pool, 3 drawn per run)
The flood, the ghost drainage project, the town assembly. Voice count (cascade) tracked across cards; echoes add voices. Full text in `BahalaNa_Cards.md`.

- **Baha** (`ch4_flood_01`): Speak first (Integridad DC 18, adds first voice) / wait / bring neighbors (Collective, +2 voices) / leave (Walang Kibo +1).
- **Ang Pagkukumpol** (`ch4_assembly_02`): Nurse Lea speaks first. Support her (Collective, Angry Neighbors join) / speak alone (Integridad DC 17) / stay silent.
- **Ang Ebidensya** (`ch4_evidence_03`): Evidence shown. Present yours (Talino DC 16, Worried Parents join) / use echoes (+2 voices) / stay silent.
- **Ang Umaga** (`ch4_morning_04`): The decision. Reform now (Komunidad DC 18 + assists) / slow reform / accept the old way. Determines ending.

### E.5 Endings (all 10, approved)
1. Ang Kapitang Hindi Yumuko (Official, high Integridad, low Utang) · 2. Ang Pirma (Official, high Utang, complied) · 3. Ang Badge (Police, high Integridad, spoke up) · 4. Ang Sumunod (Police, complied) · 5. Ang Tamang Semento (Teacher/Engineer, refused/reported) · 6. Ang Pumikit (Teacher/Engineer, silent) · 7. Ang Boses (Organizer/Journalist, cascade succeeded) · 8. Ang Pagod na Boses (Organizer/Journalist, burned out alone) · 9. Ang Sumama (Ordinary Citizen, joined cascade) · 10. Ang Nanood (Ordinary Citizen, silent). Full epilogue text in `BahalaNa_Cards.md`. Final screen "Ikaw at ang Bayan": epilogue card + barangay impact + Utang ledger reveal + location-specific CTA.

### E.6 Echo pool (12 pre-written, approved)
Anonymous Bayanihan Echoes, each +1d6 once, max 2 per roll (full list in `BahalaNa_Cards.md`).

### E.7 Location-specific final-ask list (approved)
Template + examples by location type (small town, city barangay, school, general). See `BahalaNa_Cards.md`.

### E.8 Content status: COMPLETE
All 40 cards (Ch1 6 + Ch2 8 + Ch3 22 + Ch4 4), 10 endings, Echo pool, and final-ask list are written. Next: paper/logic playtest, then frontend layout, then backend build. A refreshed copy of this master will be produced for the GitHub-connected session.

---

*End of full handoff. All AIs: read §A, implement your part of §C, and write back the full context explanation to the developers.*

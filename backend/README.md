# Bahala Na? — Firebase backend

Implements HANDOFF §C.1–C.4: **Cloud Functions + Firestore + Security Rules + Remote Config + Admin SDK seeding**. The Unity client (frontend-owned) reads `barangay/state` + `echoes` directly and writes **only** through `submitRunContribution`.

## Layout
```
backend/
  firebase.json               project config (rules, indexes, emulators, functions predeploy)
  firestore.rules             public read / no client write on shared state
  firestore.indexes.json      runs by playerId+finishedAt
  remoteconfig.template.json  tuning keys (§C.9)
  src/
    index.ts                  functions entry (initializeApp + exports)
    submitRunContribution.ts  the ONLY writer of barangay/state (transactional, idempotent)
    contribute.ts             contribution + era rules (pure)
    era.ts                    era table, clamp, seed state (pure)
    validators.ts             zod input validation → invalid-argument
    types.ts                  frozen data models (API_CONTRACT.md)
    seed.ts                   Admin SDK: seed state + 12 pre-written echoes
  test/                       node:test unit tests (era + contribution)
```

## Requirements
- Node 20+
- A Firebase project with Firestore (native mode) + Authentication (anonymous)
- `gcloud auth application-default login` for Admin SDK scripts (seed)

## Commands
```bash
npm install
npm test            # build + unit tests
npm run build       # tsc → lib/
npm run seed        # seed barangay/state + echoes (Admin SDK; needs ADC)
npm run serve       # local emulators (auth/functions/firestore) — needs firebase-tools + JDK
npm run deploy      # functions + rules + indexes
```

## `submitRunContribution(summary)` — contract
Callable (Auth required). Validates → transaction: read `barangay/state` → apply contribution → recompute era → `version++` → write; add echo if opted in; write `runs/{runId}`; return `{ state, alreadyExists, crossed60, newEra }`. **Idempotent per `runId`** — client retries are safe.

| Field | Rule |
|---|---|
| `spokeUp` | +4 Pag-asa |
| `joinedCascade` | +2 Pag-asa |
| neither (silent) | Walang Kibo +1, Tiwala −1 |
| `utangFinal ≥ 5` | Tiwala −3 |
| `pendingTiwala` | applied, clamped 0–100 |

Eras: 0–29 Mabulok (+2 systemic DC) · 30–59 Karaniwan · 60–84 Gumigising (−2) · 85–100 Bagong Umaga (−2). `crossed60: true` → the client shows the town event.

## Remote Config keys
`dcModifiers_era` (JSON) · `balance_kasamaDie/kasamaMax/echoDie/echoMax` · `featureFlags_singil` · `contentVersion` — see `remoteconfig.template.json`.

## Security rules summary
- `barangay/state`, `echoes/{id}`: **read public, write false** (Admin SDK only).
- `runs/{runId}`: read/create only by the owning `playerId`.
- `players/{uid}`: owner only.

Full contract: [`API_CONTRACT.md`](../API_CONTRACT.md).

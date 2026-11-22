# BAHALA NA? — API Contract (frozen)

**Status:** FROZEN before parallel build (per HANDOFF §D.2). Changes require a `DECISIONS.md` entry + both-side sign-off.
**Scope:** Firestore schema, Cloud Function signatures, Security Rules, contribution/era rules.
**Implementation:** `backend/` implements this contract (Cloud Functions + rules + Admin SDK seed + Remote Config template). See `backend/README.md`. Region is configurable via `BN_REGION` (default `asia-southeast1`).

---

## 1. Firestore schema

### `barangay/state` (singleton doc)
```ts
BarangayState {
  tiwala: number          // 0–100, trust
  imprastruktura: number  // 0–100, affects climax flooding severity
  pagAsa: number          // 0–100, reform progress → era
  walangKibo: number      // cumulative silent-player count
  era: "mabulok" | "karaniwan" | "gumigising" | "bagong_umaga"
  version: number         // ++ on every write
  updatedAt: timestamp
}
```
**Writes:** Admin SDK / Cloud Function only. **Reads:** public.

### `echoes/{echoId}` (collection)
```ts
Echo {
  id: string
  text: string            // pre-written (8B)
  sourceRunId: string?    // opt-in attribution
  active: boolean
  createdAt: timestamp
}
```
**Reads:** public. **Writes:** Admin SDK / Cloud Function only.

### `players/{uid}` (collection)
```ts
PlayerProfile {
  id: string              // auth uid
  displayName?: string
  presentation: "boy" | "girl" | "neutral"
  locale: "tl" | "en"
  createdAt: timestamp
}
```
**Reads/Writes:** only if `request.auth.uid == uid`.

### `runs/{runId}` (collection, per player)
```ts
CompletedRun {
  runId: string
  playerId: string
  background: string
  career: "official" | "police" | "teacher" | "organizer" | "citizen"
  endingId: 1..10
  spokeUp: boolean
  joinedCascade: boolean
  utangFinal: number
  echoesLeft: string[]
  contribution: ContributionSummary
  finishedAt: timestamp
}
ContributionSummary { pagAsa: number, tiwala: number, walangKibo: number, spoke: boolean }
```

---

## 2. Cloud Function: `submitRunContribution`

```
POST /submitRunContribution(summary)   (callable, Auth required)
```

**Input:**
```ts
{
  runId: string
  background: string
  career: string
  endingId: number          // 1..10
  spokeUp: boolean          // spoke first at the climax
  joinedCascade: boolean    // added voices after others
  silent: boolean           // walang kibo at the climax
  utangFinal: number        // >= 5 → tiwala −3
  echoId?: string           // opt-in echo left
  pendingTiwala?: number    // in-run town-trust deltas
}
```

**Behavior (transaction):**
1. require Auth UID; validate shape and ranges (numbers 0–100, enums valid).
2. read `barangay/state`; apply:
   - `spokeUp` → `pagAsa += 4`
   - `joinedCascade` → `pagAsa += 2`
   - `silent` → `walangKibo += 1`, `tiwala -= 1`
   - `utangFinal >= 5` → `tiwala -= 3`
   - `pendingTiwala` → `tiwala += pendingTiwala`
   - clamp `tiwala`, `pagAsa` to 0–100
3. recompute `era` from `pagAsa`: 0–29 mabulok · 30–59 karaniwan · 60–84 gumigising · 85–100 bagong_umaga
4. `version++`, `updatedAt = now`, write.
5. if `echoId` → add to `echoes/` (dedupe by id).
6. write `runs/{runId}`.
7. return `{ state: BarangayState, crossed60: boolean, newEra: string }`.

**Idempotency:** reject if `runs/{runId}` already exists (client retries safe).

---

## 3. Security Rules (Firestore)

```
match /barangay/state { allow read: if true; allow write: if false; }
match /echoes/{id}    { allow read: if true; allow write: if false; }
match /runs/{runId}   { allow read, write: if request.auth.uid == resource.data.playerId; }
match /players/{uid}  { allow read, write: if request.auth.uid == uid; }
```
(All writes to shared state go through the Admin SDK inside the Cloud Function.)

---

## 4. Remote Config keys

```
dcModifiers.era        // e.g. { "mabulok": 2, "karaniwan": 0, "gumigising": -2, "bagong_umaga": -2 }
balance.kasamaDie      // 4
balance.kasamaMax      // 2
balance.echoDie        // 6
balance.echoMax        // 2
featureFlags.singil    // true
contentVersion         // cards bundle version
```

---

## 5. Client ↔ backend flow

1. **Read** `barangay/state` + `echoes` on menu/Barangay screen and at Ch4 (direct Firestore read; cached offline).
2. **Play** the run fully offline against the local save.
3. **On ending:** queue `submitRunContribution(summary)`; sync when online. UI shows pending state.
4. **Conflict:** server is source of truth for shared state; local save is source of truth for the run.

## 6. Era → DC mapping (client applies)

| Era | Pag-asa | Systemic DC modifier |
|---|---|---|
| Mabulok | 0–29 | +2 |
| Karaniwan | 30–59 | 0 |
| Gumigising | 60–84 | −2 |
| Bagong Umaga | 85–100 | −2 (+ special cards, Kap Eddie changes) |

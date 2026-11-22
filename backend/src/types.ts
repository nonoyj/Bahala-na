/**
 * Bahala Na? — data models (frozen contract, API_CONTRACT.md §1 / HANDOFF §C.3).
 * The Unity client and Cloud Functions share these shapes.
 */

export type Presentation = "boy" | "girl" | "neutral";
export type Locale = "tl" | "en";
export type Career = "official" | "police" | "teacher" | "organizer" | "citizen";
export type Era = "mabulok" | "karaniwan" | "gumigising" | "bagong_umaga";

export interface PlayerProfile {
  id: string;
  displayName?: string;
  presentation: Presentation;
  locale: Locale;
  createdAt: string;
}

export interface RunStats {
  talino: number;
  diskarte: number;
  pera: number;
  koneksyon: number;
  integridad: number;
  komunidad: number;
}

/** Local save (on-device source of truth). `utang` is HIDDEN in the UI. */
export interface RunSave {
  runId: string;
  chapter: number;
  cardIndex: number;
  stats: RunStats;
  utang: number;
  flags: string[]; // ordered
  kasamaTokens: number;
  background: string;
  career?: Career;
  startedAt: string;
}

export interface ContributionSummary {
  pagAsa: number;
  tiwala: number;
  walangKibo: number;
  spoke: boolean;
}

export interface CompletedRun {
  runId: string;
  playerId: string;
  background: string;
  career: Career;
  endingId: number;
  spokeUp: boolean;
  joinedCascade: boolean;
  silent: boolean;
  utangFinal: number;
  echoesLeft: string[];
  contribution: ContributionSummary;
  finishedAt: string;
}

/** Singleton doc at `barangay/state`. Only the Cloud Function writes it. */
export interface BarangayState {
  tiwala: number; // 0–100
  imprastruktura: number; // 0–100
  pagAsa: number; // 0–100
  walangKibo: number; // cumulative
  era: Era;
  version: number;
  updatedAt?: unknown; // Firestore Timestamp (server)
}

export interface Echo {
  id: string;
  text: string; // pre-written (8B)
  sourceRunId?: string;
  active: boolean;
  createdAt?: unknown;
  updatedAt?: unknown;
}

/** Input to `submitRunContribution` (callable). */
export interface ContributionInput {
  runId: string;
  background: string;
  career: Career;
  endingId: number; // 1..10
  spokeUp: boolean;
  joinedCascade: boolean;
  silent: boolean;
  utangFinal: number;
  echoId?: string;
  pendingTiwala?: number;
}

export interface ContributionResult {
  state: BarangayState;
  alreadyExists: boolean;
  crossed60: boolean;
  newEra?: Era;
}

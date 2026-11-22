/**
 * Contribution rules + era recompute (HANDOFF §B.13 / §C.4).
 * Pure, deterministic — used by the Cloud Function and unit tests.
 */
import { clamp, eraForPagAsa } from "./era";
import { BarangayState, ContributionInput, ContributionSummary } from "./types";

export interface ApplyResult {
  next: BarangayState;
  crossed60: boolean;
  spoke: boolean;
  summary: ContributionSummary;
}

export function applyContribution(prev: BarangayState, s: ContributionInput): ApplyResult {
  const beforePagAsa = clamp(prev.pagAsa, 0, 100);
  const beforeTiwala = clamp(prev.tiwala, 0, 100);
  const beforeWK = Math.max(0, Math.floor(prev.walangKibo || 0));

  let pagAsa = beforePagAsa;
  let tiwala = beforeTiwala;
  let walangKibo = beforeWK;
  let spoke = false;

  // Speak up +4 · Join once others spoke +2 · Stay silent → Walang Kibo +1 (Tiwala drifts down).
  if (s.spokeUp) {
    pagAsa += 4;
    spoke = true;
  } else if (s.joinedCascade) {
    pagAsa += 2;
    spoke = true;
  } else {
    // Server derives silence: a run that neither spoke first nor joined is silent.
    walangKibo += 1;
    tiwala -= 1;
  }

  // Padrino-heavy (Utang ≥ 5) → Tiwala −3.
  if ((s.utangFinal ?? 0) >= 5) tiwala -= 3;

  // In-run town-trust deltas (posting the budget, refusing vote-buying, ...).
  tiwala += s.pendingTiwala ?? 0;

  pagAsa = clamp(pagAsa, 0, 100);
  tiwala = clamp(tiwala, 0, 100);

  const next: BarangayState = {
    ...prev,
    pagAsa,
    tiwala,
    walangKibo,
    era: eraForPagAsa(pagAsa),
    version: Math.max(0, Math.floor(prev.version || 0)) + 1,
  };

  return {
    next,
    crossed60: beforePagAsa < 60 && pagAsa >= 60,
    spoke,
    summary: {
      pagAsa: pagAsa - beforePagAsa,
      tiwala: tiwala - beforeTiwala,
      walangKibo: walangKibo - beforeWK,
      spoke,
    },
  };
}

/**
 * submitRunContribution — the ONLY writer of `barangay/state` (§C.4).
 *
 * 1. require Auth UID
 * 2. validate shape/ranges
 * 3. transaction: read state, apply, recompute era, version++, write
 * 4. add echo if left (opt-in)
 * 5. return the new state
 * Idempotent per runId (client retries are safe).
 */
import { HttpsError, onCall } from "firebase-functions/v2/https";
import { FieldValue, getFirestore } from "firebase-admin/firestore";
import { applyContribution } from "./contribute";
import { parseContribution } from "./validators";
import { defaultBarangayState } from "./era";
import { BarangayState, ContributionResult } from "./types";

const REGION = process.env.BN_REGION || "asia-southeast1";

export const submitRunContribution = onCall(
  {
    region: REGION,
    memory: "256MiB",
    timeoutSeconds: 30,
    maxInstances: 20,
    minInstances: 0,
  },
  async (req): Promise<ContributionResult> => {
    if (!req.auth?.uid) {
      throw new HttpsError("unauthenticated", "Kailangan ng Auth UID. / Auth UID required.");
    }
    const uid = req.auth.uid;
    const input = parseContribution(req.data);

    const db = getFirestore();
    const runRef = db.collection("runs").doc(input.runId);
    const stateRef = db.doc("barangay/state");
    const echoRef = input.echoId ? db.collection("echoes").doc(input.echoId) : null;
    const now = FieldValue.serverTimestamp();

    try {
      return await db.runTransaction(async (tx): Promise<ContributionResult> => {
        // Idempotency: a retried runId must not double-apply.
        const existing = await tx.get(runRef);
        if (existing.exists) {
          const stateSnap = await tx.get(stateRef);
          const state = (stateSnap.exists ? stateSnap.data() : defaultBarangayState()) as BarangayState;
          return { state: publicState(state), alreadyExists: true, crossed60: false };
        }

        const stateSnap = await tx.get(stateRef);
        const prev = (stateSnap.exists ? stateSnap.data() : defaultBarangayState()) as BarangayState;

        const { next, crossed60, summary } = applyContribution(prev, input);

        tx.set(stateRef, { ...next, updatedAt: now }, { merge: true });

        tx.set(runRef, {
          runId: input.runId,
          playerId: uid,
          background: input.background,
          career: input.career,
          endingId: input.endingId,
          spokeUp: input.spokeUp,
          joinedCascade: input.joinedCascade,
          silent: !input.spokeUp && !input.joinedCascade,
          utangFinal: input.utangFinal,
          echoesLeft: input.echoId ? [input.echoId] : [],
          contribution: summary,
          finishedAt: now,
        });

        // Echo opt-in: only pre-written echoes may be attributed.
        if (echoRef) {
          const echoSnap = await tx.get(echoRef);
          if (echoSnap.exists) {
            tx.set(echoRef, { sourceRunId: input.runId, active: true, updatedAt: now }, { merge: true });
          }
        }

        return {
          state: publicState(next),
          alreadyExists: false,
          crossed60,
          newEra: next.era,
        };
      });
    } catch (err) {
      if (err instanceof HttpsError) throw err;
      throw new HttpsError("internal", "Hindi matuloy ang transaksyon. / Transaction failed.");
    }
  }
);

/** Strip server-only fields so the callable response is JSON-safe. */
function publicState(state: BarangayState): BarangayState {
  return {
    tiwala: state.tiwala,
    imprastruktura: state.imprastruktura,
    pagAsa: state.pagAsa,
    walangKibo: state.walangKibo,
    era: state.era,
    version: state.version,
    updatedAt: null,
  };
}

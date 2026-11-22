/**
 * Seed `barangay/state` + the 12 pre-written echoes (§C.4 seeding).
 * Requires Application Default Credentials (Admin SDK bypasses Security Rules):
 *   gcloud auth application-default login
 *   npm run seed
 */
import { initializeApp } from "firebase-admin/app";
import { FieldValue, getFirestore } from "firebase-admin/firestore";
import { defaultBarangayState } from "./era";

// Generated from scripts/content_c.py -> content/echoes.json
// eslint-disable-next-line @typescript-eslint/no-var-requires
const ECHOES = require("../../content/echoes.json") as Array<{ id: string; text: string }>;

async function main(): Promise<void> {
  initializeApp();
  const db = getFirestore();
  const now = FieldValue.serverTimestamp();

  const stateRef = db.doc("barangay/state");
  const snap = await stateRef.get();
  if (snap.exists) {
    console.log("barangay/state already seeded:", snap.data());
  } else {
    await stateRef.set({ ...defaultBarangayState(), updatedAt: now });
    console.log("seeded barangay/state:", defaultBarangayState());
  }

  const batch = db.batch();
  let added = 0;
  for (const e of ECHOES) {
    const ref = db.collection("echoes").doc(e.id);
    batch.set(ref, { id: e.id, text: e.text, active: true, createdAt: now }, { merge: true });
    added += 1;
  }
  await batch.commit();
  console.log(`seeded ${added} echoes`);
}

main().catch((err) => {
  console.error("seed failed:", err);
  process.exit(1);
});

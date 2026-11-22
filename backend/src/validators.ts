/**
 * Input validation for `submitRunContribution` (§C.4 step 2: validate shape/ranges).
 * Uses zod; throws `invalid-argument` so the callable returns a clean client error.
 */
import { HttpsError } from "firebase-functions/v2/https";
import { z } from "zod";
import { ContributionInput } from "./types";

const RUN_ID_RE = /^[A-Za-z0-9_-]{1,64}$/;

export const contributionSchema = z.object({
  runId: z.string().regex(RUN_ID_RE, "runId must be 1–64 chars [A-Za-z0-9_-]"),
  background: z.string().min(1).max(64),
  career: z.enum(["official", "police", "teacher", "organizer", "citizen"]),
  endingId: z.number().int().min(1).max(10),
  spokeUp: z.boolean(),
  joinedCascade: z.boolean(),
  silent: z.boolean(),
  utangFinal: z.number().int().min(0).max(20),
  echoId: z.string().min(1).max(64).optional(),
  pendingTiwala: z.number().int().min(-50).max(50).optional(),
});

export function parseContribution(data: unknown): ContributionInput {
  const parsed = contributionSchema.safeParse(data);
  if (!parsed.success) {
    const first = parsed.error.issues[0];
    throw new HttpsError(
      "invalid-argument",
      `Invalid contribution: ${first ? `${first.path.join(".")}: ${first.message}` : "unknown"}`
    );
  }
  return parsed.data as ContributionInput;
}

// Bahala Na? — Shared Barangay contribution + era recompute (§B.13 / §C.4).
// Mirrors js/engine.js applyContribution and backend/src/contribute.ts.
using System;
using UnityEngine;

namespace BahalaNa
{
    [Serializable]
    public class SubmitResult
    {
        public BarangayState state;
        public ContributionSummary summary;
        public bool crossed60;
        public bool alreadyExists;
    }

    public static class Contribution
    {
        public static SubmitResult Apply(BarangayState st, ContributionPayload p, Func<int, string> eraFor)
        {
            int beforePag = Mathf.Clamp(st.pagAsa, 0, 100);
            int beforeTiw = Mathf.Clamp(st.tiwala, 0, 100);
            int beforeWk = Math.Max(0, st.walangKibo);

            int dPag = 0, dTiw = 0, dWk = 0;
            bool spoke = false;

            if (p.spokeUp) { dPag += 4; spoke = true; }
            else if (p.joinedCascade) { dPag += 2; spoke = true; }
            else { dWk += 1; dTiw -= 1; } // silent

            if (p.utangFinal >= 5) dTiw -= 3; // padrino-heavy

            dTiw += p.pendingTiwala;

            st.pagAsa = Mathf.Clamp(beforePag + dPag, 0, 100);
            st.tiwala = Mathf.Clamp(beforeTiw + dTiw, 0, 100);
            st.walangKibo = beforeWk + dWk;
            st.runs += 1;
            if (spoke) st.spokeUp += 1;
            if (eraFor != null) st.era = eraFor(st.pagAsa);
            st.version += 1;

            bool crossed60 = beforePag < 60 && st.pagAsa >= 60;
            var summary = new ContributionSummary
            {
                pagAsa = st.pagAsa - beforePag,
                tiwala = st.tiwala - beforeTiw,
                walangKibo = st.walangKibo - beforeWk,
                spoke = spoke,
            };
            st.lastContribution = summary;

            return new SubmitResult { state = st, summary = summary, crossed60 = crossed60, alreadyExists = false };
        }
    }
}

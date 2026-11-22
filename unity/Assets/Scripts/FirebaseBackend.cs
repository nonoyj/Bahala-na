// Bahala Na? — Firebase backend (real).
// Requires the Firebase Unity SDK (Auth, Firestore, Functions) + google-services.json /
// GoogleService-Info.plist, and the BAHALANA_FIREBASE scripting define symbol.
// Without those, the game runs on LocalBackend (see Backend.cs).
#if BAHALANA_FIREBASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Functions;
using Newtonsoft.Json;
using UnityEngine;

namespace BahalaNa
{
    [Serializable]
    public class FirebaseSubmitResponse
    {
        public BarangayState state;
        public bool alreadyExists;
        public bool crossed60;
        public string newEra;
    }

    public class FirebaseBackend : IBarangayBackend
    {
        private readonly SaveManager save;
        private FirebaseApp app;
        private bool initializing;

        public bool IsOnline => app != null;

        public FirebaseBackend(SaveManager save)
        {
            this.save = save;
            initializing = true;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                initializing = false;
                if (task.Result == DependencyStatus.Available)
                {
                    app = FirebaseApp.DefaultInstance;
                    Debug.Log("[BahalaNa] Firebase ready.");
                }
                else
                {
                    Debug.LogWarning("[BahalaNa] Firebase unavailable: " + task.Result);
                }
            });
        }

        public async Task<bool> EnsureAuthenticatedAsync()
        {
            while (initializing) await Task.Yield();
            if (app == null) return false;
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null && auth.CurrentUser != null) return true;
            try
            {
                var t = auth.SignInAnonymouslyAsync();
                await t;
                return t.IsCompleted && !t.IsFaulted;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BahalaNa] anon auth failed: " + e.Message);
                return false;
            }
        }

        public async Task<SubmitResult> SubmitContributionAsync(ContributionPayload payload)
        {
            if (app == null) throw new InvalidOperationException("Firebase not initialised");
            var uid = FirebaseAuth.DefaultInstance?.CurrentUser?.UserId ?? "anonymous";

            var data = new Dictionary<string, object>
            {
                { "runId", payload.runId },
                { "background", payload.background },
                { "career", payload.career },
                { "endingId", payload.endingId },
                { "spokeUp", payload.spokeUp },
                { "joinedCascade", payload.joinedCascade },
                { "silent", payload.silent },
                { "utangFinal", payload.utangFinal },
                { "pendingTiwala", payload.pendingTiwala },
            };
            if (!string.IsNullOrEmpty(payload.echoId)) data["echoId"] = payload.echoId;

            var result = await FirebaseFunctions.DefaultInstance
                .GetHttpsCallable("submitRunContribution")
                .CallAsync(data);

            var json = JsonConvert.SerializeObject(result.Data);
            var resp = JsonConvert.DeserializeObject<FirebaseSubmitResponse>(json);
            if (resp?.state != null)
            {
                MirrorState(resp.state);
            }
            save.DropQueued(payload.runId);
            save.Save();

            return new SubmitResult
            {
                state = save.Barangay,
                summary = save.Barangay.lastContribution,
                crossed60 = resp != null && resp.crossed60,
                alreadyExists = resp != null && resp.alreadyExists,
            };
        }

        public async Task<BarangaySnapshot> GetBarangayAsync()
        {
            if (app == null) return new BarangaySnapshot { state = save.Barangay, echoIds = new List<string>(save.Barangay.echoes) };
            var snap = await FirebaseFirestore.DefaultInstance.Document("barangay/state").GetSnapshotAsync();
            if (snap.Exists)
            {
                var st = new BarangayState
                {
                    tiwala = snap.GetValue<int>("tiwala"),
                    imprastruktura = snap.GetValue<int>("imprastruktura"),
                    pagAsa = snap.GetValue<int>("pagAsa"),
                    walangKibo = snap.GetValue<int>("walangKibo"),
                    era = snap.GetValue<string>("era"),
                    version = snap.GetValue<int>("version"),
                    runs = save.Barangay.runs,
                    spokeUp = save.Barangay.spokeUp,
                    echoes = save.Barangay.echoes,
                };
                return new BarangaySnapshot { state = st, echoIds = new List<string>(save.Barangay.echoes) };
            }
            return new BarangaySnapshot { state = save.Barangay, echoIds = new List<string>(save.Barangay.echoes) };
        }

        public async Task FlushQueueAsync()
        {
            var queued = new List<ContributionPayload>(save.Queue);
            foreach (var p in queued)
            {
                try { await SubmitContributionAsync(p); }
                catch (Exception e) { Debug.LogWarning("[BahalaNa] flush failed for " + p.runId + ": " + e.Message); }
            }
        }

        private void MirrorState(BarangayState s)
        {
            save.Barangay.tiwala = s.tiwala;
            save.Barangay.imprastruktura = s.imprastruktura;
            save.Barangay.pagAsa = s.pagAsa;
            save.Barangay.walangKibo = s.walangKibo;
            save.Barangay.era = string.IsNullOrEmpty(s.era) ? save.Barangay.era : s.era;
            save.Barangay.version = s.version;
        }
    }
}
#endif

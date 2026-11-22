// Bahala Na? — backend abstraction.
// Reads: barangay/state + echoes (direct Firestore reads).
// Writes: ONLY submitRunContribution (callable) — never direct writes to shared state.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BahalaNa
{
    public interface IBarangayBackend
    {
        bool IsOnline { get; }
        Task<bool> EnsureAuthenticatedAsync();
        Task<SubmitResult> SubmitContributionAsync(ContributionPayload payload);
        Task<BarangaySnapshot> GetBarangayAsync();
        Task FlushQueueAsync();
    }

    public class BarangaySnapshot
    {
        public BarangayState state;
        public List<string> echoIds;
    }

    /// <summary>
    /// Offline-first backend: applies the exact §C.4 rules locally.
    /// Used in the editor / when the Firebase SDK is not wired up.
    /// </summary>
    public class LocalBackend : IBarangayBackend
    {
        private readonly SaveManager save;
        public bool IsOnline => false;

        public LocalBackend(SaveManager save) { this.save = save; }

        public Task<bool> EnsureAuthenticatedAsync() => Task.FromResult(true);

        public Task<SubmitResult> SubmitContributionAsync(ContributionPayload payload)
        {
            var result = Contribution.Apply(save.Barangay, payload, pag =>
                save.Content != null ? save.Content.EraFor(pag).id : save.Barangay.era);
            save.DropQueued(payload.runId);
            save.Save();
            return Task.FromResult(result);
        }

        public Task<BarangaySnapshot> GetBarangayAsync()
        {
            return Task.FromResult(new BarangaySnapshot
            {
                state = save.Barangay,
                echoIds = new List<string>(save.Barangay.echoes),
            });
        }

        public Task FlushQueueAsync()
        {
            save.Save();
            return Task.CompletedTask;
        }
    }
}

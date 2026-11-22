// Bahala Na? — bootstrap + service locator.
// Boots itself after the scene loads; builds the Canvas and shows the menu.
using System;
using System.Threading.Tasks;
using UnityEngine;
using BahalaNa.UI;

namespace BahalaNa
{
    public static class GameApp
    {
        public static ContentDatabase Content;
        public static SaveManager Save;
        public static GameManager Game;
        public static IBarangayBackend Backend;
        public static UIManager UI;
        public static bool FirebaseBackendActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Boot()
        {
            UI = new UIManager();
            UI.Show(new LoadingScreen());
            Save = new SaveManager();
            ContentDatabase.LoadAsync(OnContentLoaded);
        }

        private static void OnContentLoaded(ContentDatabase db)
        {
            Content = db;
            Save.Content = db;
            Save.Load();
            Game = new GameManager(db, Save);
            Backend = CreateBackend();
            WireEvents();
            UI.Show(new MainMenuScreen());
        }

        private static IBarangayBackend CreateBackend()
        {
#if BAHALANA_FIREBASE
            FirebaseBackendActive = true;
            return new FirebaseBackend(Save);
#else
            FirebaseBackendActive = false;
            return new LocalBackend(Save);
#endif
        }

        private static void WireEvents()
        {
            Game.ChapterStarted += ch => UI.Show(new ChapterIntroScreen(ch));
            Game.CareerNeeded += opts => UI.Show(new CareerScreen(opts));
            Game.ClimaxReached += () => UI.Show(new ClimaxScreen());
            Game.RunEnded += () => SubmitAndShowEnding();
            Game.RunChanged += () => { /* screens rebuild explicitly; nothing to do here */ };
        }

        private static async void SubmitAndShowEnding()
        {
            try
            {
                var payload = Game.LastPayload;
                SubmitResult res = null;
                try
                {
                    res = await Backend.SubmitContributionAsync(payload);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[BahalaNa] submit failed, will retry from queue: " + e.Message);
                }
                if (res == null)
                {
                    // Offline: the payload stays queued; the next online session flushes it.
                    res = new SubmitResult
                    {
                        state = Save.Barangay,
                        summary = Save.Barangay.lastContribution,
                        crossed60 = false,
                    };
                }
                Game.ApplySubmitResult(res);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BahalaNa] ending submit error: " + e.Message);
            }
            UI.Show(new EndingScreen());
        }

        // ---------------------------------------------------------------- helpers for screens
        public static string T(string tl, string en)
        {
            return Save != null && Save.Settings.lang == "tl" ? tl : (string.IsNullOrEmpty(en) ? tl : en);
        }

        public static string Lang => Save != null ? Save.Settings.lang : "tl";
        public static string StatName(string key) => ContentDatabase.StatName(key);
        public static string StatIcon(string key) => ContentDatabase.StatIcon(key);

        public static string EraName(EraDef era) => era != null ? T(era.name, era.name_en) : "—";
    }
}

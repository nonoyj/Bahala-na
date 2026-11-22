// Bahala Na? — local save (on-device source of truth) + settings + offline contribution queue.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BahalaNa
{
    [Serializable]
    public class SaveFile
    {
        public Profile profile;
        public SettingsData settings;
        public BarangayState barangay;
        public RunState run;
        public List<ContributionPayload> queue;
    }

    public class SaveManager
    {
        public Profile Profile = new Profile();
        public SettingsData Settings = new SettingsData();
        public BarangayState Barangay = new BarangayState();
        public RunState Run;
        public List<ContributionPayload> Queue = new List<ContributionPayload>();

        public ContentDatabase Content; // set after content loads (era resolver)

        private string SavePath => Path.Combine(Application.persistentDataPath, "bahalana_save.json");

        public void Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var sf = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(SavePath));
                    if (sf != null)
                    {
                        Profile = sf.profile ?? new Profile();
                        Settings = sf.settings ?? new SettingsData();
                        Barangay = sf.barangay ?? new BarangayState();
                        Run = sf.run;
                        Queue = sf.queue ?? new List<ContributionPayload>();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BahalaNa] save load failed: " + e.Message);
            }

            if (string.IsNullOrEmpty(Profile.presentation)) Profile.presentation = "neutral";
            if (string.IsNullOrEmpty(Settings.lang)) Settings.lang = "tl";
            if (Barangay.echoes == null) Barangay.echoes = new List<string>();
            if (Barangay.echoes.Count == 0 && Content != null)
            {
                foreach (var e in Content.Echoes) Barangay.echoes.Add(e.id);
            }
        }

        public void Save()
        {
            try
            {
                var sf = new SaveFile
                {
                    profile = Profile,
                    settings = Settings,
                    barangay = Barangay,
                    run = Run,
                    queue = Queue,
                };
                File.WriteAllText(SavePath, JsonConvert.SerializeObject(sf, Formatting.Indented));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BahalaNa] save failed: " + e.Message);
            }
        }

        public void Reset()
        {
            Profile = new Profile();
            Settings = new SettingsData();
            Barangay = new BarangayState();
            Run = null;
            Queue = new List<ContributionPayload>();
            if (Content != null)
            {
                Barangay.echoes = new List<string>();
                foreach (var e in Content.Echoes) Barangay.echoes.Add(e.id);
            }
            Save();
        }

        // ---------------- offline queue ----------------
        public void Enqueue(ContributionPayload p)
        {
            if (Queue.Exists(q => q.runId == p.runId)) return;
            Queue.Add(p);
            Save();
        }

        public void DropQueued(string runId)
        {
            Queue.RemoveAll(q => q.runId == runId);
            Save();
        }

        // ---------------- derived ----------------
        public EraDef Era() => Content != null ? Content.EraFor(Barangay.pagAsa) : null;

        public int SpokeUpPct()
        {
            if (Barangay.runs <= 0) return 0;
            return Mathf.FloorToInt((float)Barangay.spokeUp / Barangay.runs * 100f);
        }

        public string UtangHint()
        {
            if (Run == null) return null;
            int u = Run.utang;
            if (u <= 0) return null;
            bool tl = Settings.lang == "tl";
            if (u <= 2) return tl ? "May binabalakid ka bang utang…" : "You feel a debt hanging over you…";
            if (u <= 4) return tl ? "May naghihintay sa'yo." : "Something is waiting for you.";
            return tl ? "Mabigat ang nararamdaman mo." : "It feels heavy.";
        }
    }
}

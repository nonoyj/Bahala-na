// Bahala Na? — content database.
// Loads exported JSON asynchronously (editor: StreamingAssets via File; device: UnityWebRequest),
// driven by content/manifest.json so card files can be enumerated inside an APK.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace BahalaNa
{
    public class ContentDatabase
    {
        public List<Card> Cards = new List<Card>();
        public Dictionary<string, Card> CardById = new Dictionary<string, Card>();
        public List<Ending> Endings = new List<Ending>();
        public List<Echo> Echoes = new List<Echo>();
        public List<FinalAsk> FinalAsks = new List<FinalAsk>();
        public List<Background> Backgrounds = new List<Background>();
        public List<EraDef> Eras = new List<EraDef>();
        public List<CascadeGroup> Groups = BuildGroups();
        public string ContentVersion = "1";

        // ---------------------------------------------------------------- loading
        public static void LoadAsync(Action<ContentDatabase> onDone)
        {
            var go = new GameObject("BahalaNaContentLoader");
            go.AddComponent<ContentLoader>().Start(LoadRoutine(onDone));
        }

        private static IEnumerator LoadRoutine(Action<ContentDatabase> onDone)
        {
            var db = new ContentDatabase();

            var manifest = new Manifest();
            yield return GetText("Content/manifest.json", json =>
            {
                if (!string.IsNullOrEmpty(json))
                    manifest = JsonConvert.DeserializeObject<Manifest>(json) ?? new Manifest();
            });

            if (manifest.cards == null || manifest.cards.Count == 0)
            {
                // Fallback: Resources (editor / Addressables-ready builds)
                var ta = Resources.Load<TextAsset>("Content/manifest");
                if (ta != null) manifest = JsonConvert.DeserializeObject<Manifest>(ta.text) ?? manifest;
            }

            // card files
            if (manifest.cards != null)
            {
                foreach (var id in manifest.cards)
                {
                    Card card = null;
                    yield return GetText("Content/cards/" + id + ".json", json =>
                    {
                        if (!string.IsNullOrEmpty(json))
                            card = JsonConvert.DeserializeObject<Card>(json);
                    });
                    if (card == null) card = db.ResourcesCard(id);
                    if (card != null)
                    {
                        card.Bind();
                        db.Cards.Add(card);
                        db.CardById[card.id] = card;
                    }
                }
            }
            db.Cards.Sort((a, b) => string.Compare(a.id, b.id, StringComparison.Ordinal));

            // extras
            yield return LoadList<Ending>(db, "endings", v => db.Endings = v);
            yield return LoadList<Echo>(db, "echoes", v => db.Echoes = v);
            yield return LoadList<FinalAsk>(db, "final_asks", v => db.FinalAsks = v);
            yield return LoadList<Background>(db, "backgrounds", v => db.Backgrounds = v);
            yield return LoadList<EraDef>(db, "eras", v => db.Eras = v);

            db.ContentVersion = manifest.contentVersion ?? "1";
            onDone(db);
        }

        private static IEnumerator LoadList<T>(ContentDatabase db, string name, Action<List<T>> assign) where T : class
        {
            List<T> list = null;
            yield return GetText("Content/" + name + ".json", json =>
            {
                if (!string.IsNullOrEmpty(json)) list = JsonConvert.DeserializeObject<List<T>>(json);
            });
            if (list == null)
            {
                var ta = Resources.Load<TextAsset>("Content/" + name);
                if (ta != null) list = JsonConvert.DeserializeObject<List<T>>(ta.text);
            }
            assign(list ?? new List<T>());
        }

        private Card ResourcesCard(string id)
        {
            var ta = Resources.Load<TextAsset>("Content/cards/" + id);
            if (ta == null) return null;
            return JsonConvert.DeserializeObject<Card>(ta.text);
        }

        private static IEnumerator GetText(string relative, Action<string> done)
        {
            string result = null;
#if UNITY_EDITOR
            var p = Path.Combine(Application.dataPath, "StreamingAssets", relative);
            if (File.Exists(p)) result = File.ReadAllText(p);
            done(result);
            yield break;
#else
            var p = Path.Combine(Application.streamingAssetsPath, relative);
            using (var req = UnityWebRequest.Get(p))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success) result = req.downloadHandler.text;
            }
            if (result == null)
            {
                var ta = Resources.Load<TextAsset>(relative);
                if (ta != null) result = ta.text;
            }
            done(result);
#endif
        }

        [Serializable]
        public class Manifest
        {
            public List<string> cards;
            public List<string> files;
            public string contentVersion;
        }

        public class ContentLoader : MonoBehaviour { public void Start(IEnumerator routine) { StartCoroutine(routine); } }

        // ---------------------------------------------------------------- lookups
        public Card Card(string id) => CardById.ContainsKey(id) ? CardById[id] : null;

        public List<Card> Pool(string pool) => Cards.FindAll(c => c.pool == pool);

        public EraDef EraFor(int pagAsa)
        {
            int v = Mathf.Clamp(pagAsa, 0, 100);
            foreach (var e in Eras)
                if (v >= e.min && v <= e.max) return e;
            return Eras.Count > 0 ? Eras[0] : null;
        }

        public EraDef EraById(string id)
        {
            foreach (var e in Eras) if (e.id == id) return e;
            return EraFor(45);
        }

        public Background BackgroundById(string id)
        {
            foreach (var b in Backgrounds) if (b.id == id) return b;
            return Backgrounds.Count > 0 ? Backgrounds[0] : null;
        }

        public Ending EndingById(int id)
        {
            foreach (var e in Endings) if (e.id == id) return e;
            return Endings.Count > 0 ? Endings[0] : null;
        }

        public FinalAsk AskById(string id)
        {
            foreach (var a in FinalAsks) if (a.id == id) return a;
            return FinalAsks.Count > 0 ? FinalAsks[0] : null;
        }

        public Echo EchoById(string id)
        {
            foreach (var e in Echoes) if (e.id == id) return e;
            return null;
        }

        // ---------------------------------------------------------------- helpers
        public static int Mod(int stat)
        {
            if (stat <= 1) return 0;
            if (stat <= 3) return 1;
            if (stat <= 5) return 2;
            if (stat <= 7) return 3;
            return 4;
        }

        public static string StatName(string key)
        {
            switch (key)
            {
                case "talino": return "Talino";
                case "diskarte": return "Diskarte";
                case "pera": return "Pera";
                case "koneksyon": return "Koneksyon";
                case "integridad": return "Integridad";
                case "komunidad": return "Komunidad";
                default: return key;
            }
        }

        public static string StatIcon(string key)
        {
            switch (key)
            {
                case "talino": return "🧠";
                case "diskarte": return "🛠️";
                case "pera": return "💰";
                case "koneksyon": return "🤝";
                case "integridad": return "⚖️";
                case "komunidad": return "🏘️";
                default: return "•";
            }
        }

        private static List<CascadeGroup> BuildGroups()
        {
            return new List<CascadeGroup>
            {
                new CascadeGroup { id = "brave", threshold = 0, name = "Ang Matapang (Nurse Lea)", name_en = "The Brave (Nurse Lea)", icon = "✊" },
                new CascadeGroup { id = "angry", threshold = 1, name = "Mga Galit na Kapitbahay", name_en = "The Angry Neighbors", icon = "🔥" },
                new CascadeGroup { id = "worried", threshold = 2, name = "Mga Nag-aalalang Magulang", name_en = "The Worried Parents", icon = "👪" },
                new CascadeGroup { id = "vendors", threshold = 3, name = "Mga Maingat na Vendedor", name_en = "The Cautious Vendors", icon = "🧺" },
                new CascadeGroup { id = "silent", threshold = 4, name = "Ang Silent Majority", name_en = "The Silent Majority", icon = "🤫" },
            };
        }

        // ---------------------------------------------------------------- localization
        public string T(string tl, string en, string lang)
        {
            return lang == "tl" ? tl : (string.IsNullOrEmpty(en) ? tl : en);
        }

        public string Fill(string text, Profile p)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string maam = "Sir/Ma'am";
            if (p != null) maam = p.presentation == "girl" ? "Ma'am" : p.presentation == "boy" ? "Sir" : "Sir/Ma'am";
            return text.Replace("{{maam}}", maam).Replace("{{name}}", p != null && !string.IsNullOrEmpty(p.name) ? p.name : "ikaw");
        }
    }
}

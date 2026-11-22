// Bahala Na? — main menu, Character Spark, chapter intro, career pick.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BahalaNa.UI
{
    // ---------------------------------------------------------------- main menu
    public class MainMenuScreen : Screen
    {
        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);

            Widgets.Title(c, "BAHALA NA?");
            Widgets.Sub(c, GameApp.T(
                "Isang buhay, isang d20, isang barangay. Ang Desisyon mo, Desisyon ng Bayan.",
                "One life, one d20, one barangay. Your choice, the town's choice."));

            var era = GameApp.Save.Era();
            if (era != null)
            {
                Widgets.Chip(c, "■ " + GameApp.EraName(era) + " · " + GameApp.Save.SpokeUpPct() + "% " +
                    GameApp.T("nagsalita", "spoke up") + " · tipping point 40%", UIBuilder.Teal);
            }

            Widgets.Primary(c, "▶  " + GameApp.T("Maglaro", "Play"), () => UI.Show(new SparkScreen()));

            if (GameApp.Save.Run != null && !GameApp.Save.Run.ended)
                Widgets.Normal(c, "↻  " + GameApp.T("Magpatuloy", "Continue"), () =>
                {
                    var r = GameApp.Save.Run;
                    if (r == null) return;
                    if (r.chapter <= 0) GameApp.Game.GoToChapter(1);
                    else UI.Show(new GameScreen());
                });

            Widgets.Normal(c, "🏘️  " + GameApp.T("Barangay", "Barangay"), () => UI.Show(new BarangayScreen()));
            Widgets.Normal(c, "📣  " + GameApp.T("Mga Echo", "Echoes"), () => UI.Show(new EchoesScreen()));
            Widgets.Normal(c, "❓  " + GameApp.T("Paano Maglaro", "How to Play"), () => UI.Show(new HowToScreen()));
            Widgets.Normal(c, "⚙️  " + GameApp.T("Settings", "Settings"), () => UI.Show(new SettingsScreen()));
            Widgets.Normal(c, "ℹ️  " + GameApp.T("Tungkol", "About"), () => UI.Show(new AboutScreen()));

            var backend = GameApp.FirebaseBackendActive
                ? GameApp.T("Firebase: konektado", "Firebase: connected")
                : GameApp.T("Offline mode (LocalBackend)", "Offline mode (LocalBackend)");
            Widgets.Sub(c, backend);
        }
    }

    // ---------------------------------------------------------------- character spark
    public class SparkScreen : Screen
    {
        private string name;
        private string presentation = "neutral";
        private bool rolled;
        private Background bg;
        private RectTransform resultArea;
        private TMP_InputField nameInput;

        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);
            Widgets.Title(c, GameApp.T("Character Spark", "Character Spark"));
            Widgets.Sub(c, GameApp.T("30 segundo. Pangalan, presentation, at suerte.", "30 seconds. Name, presentation, and luck."));

            Widgets.Body(c, GameApp.T("Pangalan / alias", "Name / alias"), 16f);
            nameInput = UIBuilder.Input(c, "", 20, v => name = v);

            Widgets.Body(c, GameApp.T("Presentation", "Presentation"), 16f);
            var presOpts = new List<string> {
                GameApp.T("👦 Batang lalaki", "Boy"),
                GameApp.T("👧 Batang babae", "Girl"),
                GameApp.T("🧒 Neutral", "Neutral")
            };
            string[] presIds = { "boy", "girl", "neutral" };
            Widgets.Segmented(c, presOpts.ToArray(), 2, idx => presentation = presIds[idx]);

            var resultGo = new GameObject("Result", typeof(RectTransform));
            resultGo.transform.SetParent(c, false);
            resultArea = resultGo.GetComponent<RectTransform>();
            resultArea.anchorMin = new Vector2(0f, 0.5f); resultArea.anchorMax = new Vector2(1f, 0.5f);
            resultArea.pivot = new Vector2(0.5f, 0.5f); resultArea.sizeDelta = new Vector2(0f, 10f);
            var rle = resultGo.AddComponent<LayoutElement>();
            rle.minHeight = 132f;
            var rcsf = resultGo.AddComponent<ContentSizeFitter>();
            rcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(c, false);
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0.5f); rowRt.anchorMax = new Vector2(0.5f, 0.5f);
            rowRt.pivot = new Vector2(0.5f, 0.5f); rowRt.sizeDelta = new Vector2(0f, 52f);
            var rowLe = row.AddComponent<LayoutElement>(); rowLe.minHeight = 52f;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f; hlg.childControlWidth = true; hlg.childForceExpandWidth = true;
            Widgets.Normal(rowRt, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
            Widgets.Primary(rowRt, "🎲 " + GameApp.T("I-roll ang d6", "Roll d6"), () => Roll());
            Widgets.Primary(rowRt, GameApp.T("Simulan ▶", "Start ▶"), () => Start());
        }

        private void Roll()
        {
            rolled = true;
            int roll = Dice.Roll(6);
            bg = GameApp.Content.Backgrounds.Find(b => b.roll == roll) ?? GameApp.Content.Backgrounds[0];
            foreach (Transform t in resultArea) Destroy(t.gameObject);

            var card = new GameObject("BgCard", typeof(RectTransform));
            card.transform.SetParent(resultArea, false);
            var crt = card.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
            var cle = card.AddComponent<LayoutElement>(); cle.minHeight = 120f;
            var img = card.AddComponent<Image>(); img.color = UIBuilder.Panel2;

            var inner = UIBuilder.VList(crt, 4f);
            UIBuilder.Text(inner, "d6 → " + bg.roll, 16f, UIBuilder.InkDim);
            UIBuilder.Text(inner, GameApp.Content.T(bg.name, bg.name_en, GameApp.Save.Settings.lang), 22f, UIBuilder.Gold, TextAlignmentOptions.Center, true);
            UIBuilder.Text(inner, GameApp.Content.T(bg.text, bg.text_en, GameApp.Save.Settings.lang), 15f, UIBuilder.InkDim);
        }

        private void Start()
        {
            if (!rolled || bg == null) return;
            GameApp.Game.NewRun(nameInput != null ? nameInput.text : "", presentation);
            GameApp.Game.GoToChapter(1);
        }

        private void Continue()
        {
            var r = GameApp.Save.Run;
            if (r == null) return;
            if (r.chapter <= 0) { GameApp.Game.GoToChapter(1); return; }
            GameApp.Game.OpenChapter(r.chapter);
        }
    }

    // ---------------------------------------------------------------- chapter intro
    public class ChapterIntroScreen : Screen
    {
        private readonly int chapter;
        public ChapterIntroScreen(int chapter) { this.chapter = chapter; }

        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);
            var info = ChapterInfo(chapter);
            Widgets.Sub(c, GameApp.T("Kabanata " + chapter, "Chapter " + chapter));
            Widgets.Title(c, GameApp.T(info.name, info.nameEn));
            Widgets.Sub(c, GameApp.T(info.desc, info.descEn));
            Widgets.Sub(c, GameApp.T("Edad", "Ages") + ": " + info.ages + " · " + info.cards + " " + GameApp.T("na card", "cards"));
            Widgets.Primary(c, GameApp.T("Magpatuloy ▶", "Continue ▶"), () => UI.Show(new GameScreen()));
        }

        private static (string name, string nameEn, string desc, string descEn, string ages, int cards) ChapterInfo(int ch)
        {
            switch (ch)
            {
                case 1: return ("Kabataan", "Childhood",
                    "Mga edad ~8–12. Mainit, maliit, medyo nakakatawa.", "Ages ~8–12. Warm, small-scale, a little funny.", "~8–12", 4);
                case 2: return ("Sangandaan", "Crossroads",
                    "Mga edad ~16–22. Tumatindi ang pressure.", "Ages ~16–22. The pressure rises.", "~16–22", 5);
                case 3: return ("Tungkulin", "Duty",
                    "Mga edad ~25–35. Bigat ng moralidad.", "Ages ~25–35. Moral weight.", "~25–35", 6);
                default: return ("Ang Pagsubok", "The Test",
                    "Ang baha. Ang ghost project. Ang town assembly.", "The flood. The ghost project. The town assembly.", "~35+", 3);
            }
        }
    }

    // ---------------------------------------------------------------- career pick (9B)
    public class CareerScreen : Screen
    {
        private readonly List<string> options;
        public CareerScreen(List<string> options) { this.options = options; }

        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);
            Widgets.Title(c, GameApp.T("Ang Sangandaan", "The Crossroads"));
            Widgets.Sub(c, options.Count > 1
                ? GameApp.T("Maraming tungkulin ang bumagay sa'yo. Ikaw ang pipili (9B).", "Several paths fit you. You pick (9B).")
                : GameApp.T("Isang tungkulin ang bumagay sa'yo.", "One path fits you."));

            foreach (var career in options)
            {
                Widgets.Normal(c, CareerName(career) + "  ·  " + CareerReason(career),
                    () => GameApp.Game.PickCareer(career));
            }
            Widgets.Normal(c, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
        }

        private static string CareerName(string c)
        {
            switch (c)
            {
                case "official": return "🏛️ " + GameApp.T("Opisyal / Barangay-City Official", "Barangay/City Official");
                case "police": return "🛡️ " + GameApp.T("Pulis", "Police Officer");
                case "teacher": return "📐 " + GameApp.T("Guro / Municipal Engineer", "Teacher / Municipal Engineer");
                case "organizer": return "📣 " + GameApp.T("Community Organizer / Mamamahayag", "Community Organizer / Journalist");
                default: return "🧑‍🌾 " + GameApp.T("Ordinaryong Mamamayan", "Ordinary Working Citizen");
            }
        }

        private string CareerReason(string c)
        {
            var r = GameApp.Game.Run;
            var s = r.stats;
            switch (c)
            {
                case "official": return GameApp.T("Koneksyon " + s.koneksyon, "Connections " + s.koneksyon);
                case "police": return GameApp.T("Diskarte " + s.diskarte + " · Pera " + s.pera, "Resourcefulness " + s.diskarte + " · Money " + s.pera);
                case "teacher": return GameApp.T("Talino " + s.talino, "Wits " + s.talino);
                case "organizer": return GameApp.T("Komunidad " + s.komunidad, "Community " + s.komunidad);
                default: return GameApp.T("Ordinaryo", "Ordinary");
            }
        }
    }
}

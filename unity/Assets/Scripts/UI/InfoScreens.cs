// Bahala Na? — info screens: Barangay, Echoes, How to Play, Settings, About.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BahalaNa.UI
{
    public class BarangayScreen : Screen
    {
        protected override void BuildContent()
        {
            var st = GameApp.Save.Barangay;
            var content = GameApp.Content;
            var lang = GameApp.Lang;
            var era = GameApp.Save.Era();

            Widgets.Page(Root, out var c);
            Widgets.Title(c, " " + GameApp.T("Barangay", "Barangay"));
            Widgets.Sub(c, GameApp.T("Ang town na pinagsasaluhan ng lahat ng player.", "The town shared by every player."));

            Widgets.Sub(c, era != null ? GameApp.EraName(era) : "—");
            if (era != null)
                Widgets.Body(c, GameApp.Content.T(era.desc, era.desc_en, lang), 14f);

            Widgets.Sub(c, GameApp.T("Pag-asa", "Hope") + ": " + st.pagAsa + "/100");
            Widgets.Bar(c, st.pagAsa / 100f, UIBuilder.Gold, 16f);
            Widgets.Sub(c, GameApp.T("Tiwala", "Trust") + ": " + st.tiwala + "/100");
            Widgets.Bar(c, st.tiwala / 100f, UIBuilder.Teal, 16f);
            Widgets.Sub(c, GameApp.T("Imprastruktura", "Infrastructure") + ": " + st.imprastruktura);
            Widgets.Sub(c, GameApp.T("Walang Kibo", "Walang Kibo") + ": " + st.walangKibo + "  ·  " +
                GameApp.T("Runs", "Runs") + ": " + st.runs + "  ·  " + GameApp.Save.SpokeUpPct() + "% " + GameApp.T("nagsalita", "spoke up"));

            if (st.lastContribution != null)
            {
                var s = st.lastContribution;
                Widgets.Sub(c, GameApp.T("Huling ambag", "Last contribution") + ": Pag-asa " + (s.pagAsa >= 0 ? "+" : "") + s.pagAsa +
                    " · Tiwala " + (s.tiwala >= 0 ? "+" : "") + s.tiwala + " · WK +" + s.walangKibo);
            }

            Widgets.Normal(c, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
        }
    }

    public class EchoesScreen : Screen
    {
        protected override void BuildContent()
        {
            var lang = GameApp.Lang;
            Widgets.Page(Root, out var c);
            Widgets.Title(c, " " + GameApp.T("Mga Echo", "Echoes"));
            Widgets.Sub(c, GameApp.T("Mga boses mula sa ibang buhay. Pwede silang gamitin sa Ch4 (+1d6, max 2/roll).",
                "Voices from other lives. Usable in Ch4 (+1d6, max 2/roll)."));

            var pool = GameApp.Save.Barangay.echoes;
            if (pool == null || pool.Count == 0)
            {
                Widgets.Sub(c, GameApp.T("Wala pang echo. Maglaro at mag-iwan ng isa.", "No echoes yet. Play a run and leave one."));
            }
            else
            {
                foreach (var id in pool)
                {
                    var e = GameApp.Content.EchoById(id);
                    if (e != null)
                        Widgets.Body(c, "• " + GameApp.Content.T(e.text, e.text_en, lang), 14f);
                }
            }

            Widgets.Normal(c, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
        }
    }

    public class HowToScreen : Screen
    {
        protected override void BuildContent()
        {
            var lang = GameApp.Lang;
            Widgets.Page(Root, out var c);
            Widgets.Title(c, " " + GameApp.T("Paano Maglaro", "How to Play"));
            string[] lines = {
                GameApp.T("1. Pumili ng buhay (Character Spark): pangalan + presentation + d6 background.", "1. Pick a life (Character Spark): name + presentation + d6 background."),
                GameApp.T("2. Apat na kabanata, 18 card bawat buhay. Bawat card, isang desisyon.", "2. Four chapters, 18 cards per life. Every card, one decision."),
                GameApp.T("3. Roll = d20 + stat modifier + assists vs DC. Walang partial success.", "3. Roll = d20 + stat modifier + assists vs DC. No partial success."),
                GameApp.T("4. Padrino = auto-success, pero may utang na loob (LAGING tinatago sa UI).", "4. Padrino = auto-success, but it creates a hidden debt (never shown as a number)."),
                GameApp.T("5. Utang ≥ 3: lumalabas ang mga Singil card sa Ch3.", "5. Debt ≥ 3: Singil cards appear in Ch3."),
                GameApp.T("6. Ch4: voice-count cascade. Magsalita ka nang una, sumunod, o manahimik.", "6. Ch4: voice-count cascade. Speak first, follow, or stay silent."),
                GameApp.T("7. Ang ending mo + ang desisyon mo ay ambag sa Barangay (Shared Barangay).", "7. Your ending + your choices feed the Shared Barangay."),
                GameApp.T("8. 10 endings. Walang tama o mali — may kwento.", "8. Ten endings. No right or wrong — there is a story."),
            };
            foreach (var l in lines) Widgets.Body(c, l, 15f);

            Widgets.Normal(c, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
        }
    }

    public class SettingsScreen : Screen
    {
        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);
            Widgets.Title(c, " " + GameApp.T("Settings", "Settings"));

            var s = GameApp.Save.Settings;
            Widgets.Sub(c, GameApp.T("Wika / Language", "Wika / Language"));
            Widgets.Segmented(c, new string[] { "Tagalog", "English" }, s.lang == "tl" ? 0 : 1, idx =>
            {
                s.lang = idx == 0 ? "tl" : "en";
                GameApp.Save.Save();
                GameApp.UI.Reload();
            });

            UIBuilder.Toggle(c, GameApp.T("Tunog (SFX)", "Sound (SFX)"), s.sfx, v => { s.sfx = v; GameApp.Save.Save(); });
            UIBuilder.Toggle(c, GameApp.T("Music", "Music"), s.music, v => { s.music = v; GameApp.Save.Save(); });
            UIBuilder.Toggle(c, GameApp.T("Vibration", "Vibration"), s.vibration, v => { s.vibration = v; GameApp.Save.Save(); });

            Widgets.Sub(c, GameApp.T("Laki ng teksto", "Text size"));
            Widgets.Segmented(c, new string[] { "S", "M", "L" },
                s.textSize == "small" ? 0 : s.textSize == "large" ? 2 : 1, idx =>
                {
                    s.textSize = idx == 0 ? "small" : idx == 2 ? "large" : "standard";
                    GameApp.Save.Save();
                    GameApp.UI.Reload();
                });

            UIBuilder.Toggle(c, GameApp.T("High contrast", "High contrast"), s.contrast, v => { s.contrast = v; GameApp.Save.Save(); GameApp.UI.Reload(); });

            Widgets.Sub(c, GameApp.T("Final ask location", "Final ask location"));
            Widgets.Segmented(c, new string[] { "Small town", "City", "School" },
                s.location == "small_town" ? 0 : s.location == "city" ? 1 : s.location == "school" ? 2 : 0, idx =>
                {
                    s.location = idx == 0 ? "small_town" : idx == 1 ? "city" : "school";
                    GameApp.Save.Save();
                });

            Widgets.Normal(c, GameApp.T(" I-reset ang save", " Reset save"), () =>
            {
                GameApp.Save.Reset();
                GameApp.Save.Run = null;
                UI.Show(new MainMenuScreen());
            });
            Widgets.Normal(c, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
        }
    }

    public class AboutScreen : Screen
    {
        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);
            Widgets.Title(c, " " + GameApp.T("Tungkol", "About"));
            Widgets.Sub(c, "BAHALA NA?");
            Widgets.Body(c, GameApp.T(
                "Isang maikling dice-driven na card game tungkol sa isang barangay, sa utang na loob, at sa boses na mahirap gamitin. Walang tunay na tao, partido, o ahensya.",
                "A short dice-driven card game about a barangay, debt of gratitude, and the voice that is hard to use. No real people, parties, or agencies."), 15f);
            Widgets.Sub(c, GameApp.T("Backstory", "Backstory"));
            Widgets.Body(c, GameApp.T(
                "Bumalik ka sa barangay pagkatapos ng ilang taon. May baha. May ghost project. May welcome arch na maganda. At may town assembly na kailangan ng isang boses — maaaring 'yung sa'yo.",
                "You return to your barangay after some years. There is a flood. A ghost project. A lovely welcome arch. And a town assembly that needs a voice — possibly yours."), 15f);
            Widgets.Sub(c, GameApp.T("FAQ", "FAQ"));
            Widgets.Body(c, GameApp.T(
                "• Ang Utang na Loob ay hindi kailanman ipinapakita bilang numero.\n• Offline mode: ang progress mo ay naka-save sa device; ang mga ambag ay naka-queue hanggang mag-online.\n• Walang partial success: o tumama ka, o may natutunan kang masakit.",
                "• Utang na Loob is never shown as a number.\n• Offline mode: progress is saved on device; contributions queue until you are online.\n• No partial success: either it lands, or you learn something painful."), 15f);
            Widgets.Sub(c, GameApp.T("Credits", "Credits"));
            Widgets.Body(c, GameApp.T("Design, content, engine: Bahala Na? team. Backend: Firebase. Client: Unity 6.3.",
                "Design, content, engine: Bahala Na? team. Backend: Firebase. Client: Unity 6.3."), 13f);
            Widgets.Sub(c, "v" + GameApp.Content.ContentVersion + " · " + (GameApp.FirebaseBackendActive ? "Firebase" : "Offline"));

            Widgets.Normal(c, GameApp.T("← Bumalik", "← Back"), () => UI.Show(new MainMenuScreen()));
        }
    }
}

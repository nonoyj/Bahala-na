// Bahala Na? — gameplay screens: card, roll sheet, outcome, climax, endings.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BahalaNa.UI
{
    // ---------------------------------------------------------------- card screen
    public class GameScreen : Screen
    {
        protected override void BuildContent()
        {
            var r = GameApp.Game.Run;
            if (r == null) { UI.Show(new MainMenuScreen()); return; }

            Widgets.Page(Root, out var c);
            Widgets.Sub(c, GameApp.T("Kabanata " + r.chapter, "Chapter " + r.chapter) + " · " +
                GameApp.Game.ChapterLabel + " · " + GameApp.Game.ChapterAges(r.chapter));
            Widgets.Sub(c, GameApp.T("Card ", "Card ") + (r.cardIndex + 1) + "/" + r.deck.Count +
                (r.career != null ? " · " + GameApp.Game.CareerName(r.career, GameApp.Lang) : ""));

            if (r.chapter == 4) BuildClimax(c);
            else BuildCard(c);
        }

        private void BuildClimax(Transform c)
        {
            var r = GameApp.Game.Run;
            var content = GameApp.Content;
            var lang = GameApp.Lang;

            // voice cascade
            var vgo = new GameObject("Voices", typeof(RectTransform));
            vgo.transform.SetParent(c, false);
            var vrt = vgo.GetComponent<RectTransform>();
            vrt.anchorMin = new Vector2(0.5f, 0.5f); vrt.anchorMax = new Vector2(0.5f, 0.5f);
            vrt.pivot = new Vector2(0.5f, 0.5f); vrt.sizeDelta = new Vector2(0f, 10f);
            var vle = vgo.AddComponent<LayoutElement>(); vle.minHeight = 10f; vle.flexibleWidth = 1f;
            var vcsf = vgo.AddComponent<ContentSizeFitter>(); vcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vv = UIBuilder.VList(vrt, 4f);
            Widgets.Sub(vv, GameApp.T("Voice-count", "Voice-count") + ": " + r.voices);
            foreach (var g in content.Groups)
            {
                bool joined = r.cascade.Contains(g.id);
                UIBuilder.Text(vv, (joined ? "🔊 " : "· ") + content.T(g.name, g.name_en, lang) +
                    "  <size=12>(" + GameApp.T("threshold", "threshold") + " " + g.threshold + ")</size>",
                    14f, joined ? UIBuilder.Teal : UIBuilder.InkDim);
            }
            if (r.spokeFirst)
                Widgets.Chip(vv, "⭐ " + GameApp.T("Ikaw ang nagsalita nang una.", "You spoke first."), UIBuilder.Gold);
            else if (r.joinedCascade)
                Widgets.Chip(vv, "🔊 " + GameApp.T("Sumama ka sa cascade.", "You joined the cascade."), UIBuilder.Teal);
            else if (r.silentClimax)
                Widgets.Chip(vv, "🤫 " + GameApp.T("Tahimik ka sa climax.", "You stayed silent at the climax."), UIBuilder.InkDim);

            BuildChoices(c);
        }

        private void BuildCard(Transform c)
        {
            var card = GameApp.Game.CurrentCard;
            if (card == null) { UI.Show(new MainMenuScreen()); return; }
            BuildCardBody(c, card);
            BuildChoices(c);
        }

        private void BuildCardBody(Transform c, Card card)
        {
            var content = GameApp.Content;
            var lang = GameApp.Lang;

            Widgets.Title(c, content.T(card.title, card.title_en, lang));

            var rc = card.RollChoice();
            int dc = GameApp.Game.DCFor(card);
            string dcTxt = "DC " + dc;
            if (rc != null && rc.roll != null && rc.roll.dc_plus_utang) dcTxt += " = 10 + " + GameApp.T("utang", "utang");
            if (card.systemic && GameApp.Game.EraDcMod != 0) dcTxt += " (" + GameApp.Game.EraDcMod + " " + GameApp.T("era", "era") + ")";
            Widgets.Chip(c, dcTxt, UIBuilder.Gold);

            var sceneGo = new GameObject("Scene", typeof(RectTransform));
            sceneGo.transform.SetParent(c, false);
            var srt = sceneGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f); srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f); srt.sizeDelta = new Vector2(0f, 10f);
            var sle = sceneGo.AddComponent<LayoutElement>(); sle.minHeight = 10f; sle.flexibleWidth = 1f;
            var scsf = sceneGo.AddComponent<ContentSizeFitter>(); scsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var simg = sceneGo.AddComponent<Image>(); simg.color = UIBuilder.Panel2;
            var inner = UIBuilder.VList(srt, 6f);
            Widgets.Body(inner, content.T(card.setup, card.setup_en, lang), 17f);

            // hidden-utang hint (never a number — §C.6 / §D)
            var hint = GameApp.Save.UtangHint();
            if (!string.IsNullOrEmpty(hint))
                Widgets.Chip(inner, "🌫️ " + hint, UIBuilder.InkDim);

            // HUD stats
            var statsGo = new GameObject("Stats", typeof(RectTransform));
            statsGo.transform.SetParent(c, false);
            var stRt = statsGo.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.5f, 0.5f); stRt.anchorMax = new Vector2(0.5f, 0.5f);
            stRt.pivot = new Vector2(0.5f, 0.5f); stRt.sizeDelta = new Vector2(0f, 54f);
            var stLe = statsGo.AddComponent<LayoutElement>(); stLe.minHeight = 54f; stLe.flexibleWidth = 1f;
            var hlg = statsGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f; hlg.childControlWidth = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;
            var vals = GameApp.Game.Stats;
            var mods = GameApp.Game.StatMods;
            for (int i = 0; i < GameApp.Game.StatKeys.Length && i < vals.Length; i++)
                Widgets.HudStat(stRt, GameApp.StatIcon(GameApp.Game.StatKeys[i]), GameApp.StatName(GameApp.Game.StatKeys[i]), vals[i], mods[i]);
        }

        private void BuildChoices(Transform c)
        {
            var card = GameApp.Game.CurrentCard;
            if (card == null || card.choices == null) return;
            var lang = GameApp.Lang;
            var acts = new GameObject("Actions", typeof(RectTransform));
            acts.transform.SetParent(c, false);
            var art = acts.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0.5f, 0.5f); art.anchorMax = new Vector2(0.5f, 0.5f);
            art.pivot = new Vector2(0.5f, 0.5f); art.sizeDelta = new Vector2(0f, 10f);
            var ale = acts.AddComponent<LayoutElement>(); ale.minHeight = 10f; ale.flexibleWidth = 1f;
            var acsf = acts.AddComponent<ContentSizeFitter>(); acsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var al = UIBuilder.VList(art, 8f);

            foreach (var ch in card.choices)
            {
                string icon = ChoiceIcon(ch.type);
                string label = icon + " " + GameApp.Content.T(ch.label, ch.label_en, lang);
                bool isPadrino = ch.type == "padrino" || ch.type == "comply";
                if (isPadrino && !GameApp.Game.HasIntegrityForPadrino)
                    label += "  🔒";

                if (ch.roll == null)
                {
                    // instant choice (huwag / pera / escape / comply)
                    Widgets.Normal(al, label, () => { GameApp.Game.ChoosePassive(card, ch); UI.Show(new OutcomeScreen(card, ch, null, true)); });
                }
                else if (isPadrino)
                {
                    // padrino shortcut: auto-success, no d20 (§B.7 7.1)
                    Widgets.Normal(al, label, () => {
                        if (GameApp.Game.UsePadrino())
                            UI.Show(new OutcomeScreen(card, ch, GameApp.Game.LastRoll, false));
                    });
                }
                else
                {
                    Widgets.Normal(al, label, () => UI.Show(new RollSheetScreen(card, ch)));
                }
            }

            Widgets.Normal(al, GameApp.T("💾 I-save", "💾 Save"), () => { GameApp.Save.Save(); Widgets.Body(al, GameApp.T("Naka-save.", "Saved."), 13f); });
            Widgets.Normal(al, GameApp.T("🏳️ I-withdraw", "🏳️ Withdraw"), () =>
                UI.Show(new ConfirmScreen(
                    GameApp.T("Tiyak ka?", "Are you sure?"),
                    GameApp.T("I-withdraw mo ang desisyon. Magtatapos ang buhay na ito.", "You are withdrawing. This life ends."),
                    () => { GameApp.Game.Withdraw(); })));
        }

        private static string ChoiceIcon(string type)
        {
            switch (type)
            {
                case "honest": return "⚖️";
                case "padrino": return "🤝";
                case "comply": return "📝";
                case "collective": return "🫂";
                case "huwag": return "🤫";
                case "diskarte": return "🛠️";
                case "pera": return "💰";
                case "escape": return "🚪";
                default: return "•";
            }
        }
    }

    // ---------------------------------------------------------------- roll sheet (10A)
    public class RollSheetScreen : Screen
    {
        private readonly Card card;
        private readonly Choice choice;
        private int assist = 0;
        private bool useKasama = true;
        private bool useEchoes = true;
        private TextMeshProUGUI noteText;
        private RectTransform resultArea;
        private RollResult result;
        private string statLabel;

        public RollSheetScreen(Card card, Choice choice)
        {
            this.card = card;
            this.choice = choice;
        }

        protected override void BuildContent()
        {
            var content = GameApp.Content;
            var lang = GameApp.Lang;
            var roll = choice.roll;

            Widgets.Page(Root, out var c);
            Widgets.Title(c, GameApp.T("Roll Sheet", "Roll Sheet"));
            Widgets.Sub(c, content.T(card.title, card.title_en, lang));
            statLabel = GameApp.StatName(roll.stat) + " " + GameApp.StatIcon(roll.stat);
            Widgets.Sub(c, statLabel + " · DC " + GameApp.Game.DCFor(card) +
                (roll.systemic || card.systemic ? " · " + GameApp.T("sistematiko", "systemic") : ""));

            // modifier
            var r = GameApp.Game.Run;
            int mod = ContentDatabase.Mod(r.stats.Get(roll.stat));
            Widgets.Chip(c, GameApp.T("Modifier", "Modifier") + ": +" + mod + " (" + r.stats.Get(roll.stat) + ")", UIBuilder.Gold);

            // note
            var noteGo = new GameObject("Note", typeof(RectTransform));
            noteGo.transform.SetParent(c, false);
            var nrt = noteGo.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0.5f, 0.5f); nrt.anchorMax = new Vector2(0.5f, 0.5f);
            nrt.pivot = new Vector2(0.5f, 0.5f); nrt.sizeDelta = new Vector2(0f, 10f);
            var nle = noteGo.AddComponent<LayoutElement>(); nle.minHeight = 10f; nle.flexibleWidth = 1f;
            var ncsf = noteGo.AddComponent<ContentSizeFitter>(); ncsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            noteText = UIBuilder.Text(nrt, "", 15f, UIBuilder.InkDim);

            // assists
            var kasamaN = GameApp.Game.KasamaNow;
            var echoN = GameApp.Game.EchoPoolAvailable;
            if (roll.kasama && kasamaN > 0)
            {
                UIBuilder.Toggle(c, "🫂 " + GameApp.T("Kasama", "Kasama") + " (+" + kasamaN + "d4)", useKasama, v => { useKasama = v; RefreshNote(); });
            }
            if (roll.echoes && echoN > 0)
                UIBuilder.Toggle(c, "📣 " + GameApp.T("Mga Echo", "Echoes") + " (+" + echoN + "d6)", useEchoes, v => { useEchoes = v; RefreshNote(); });

            var amt = new GameObject("Assists", typeof(RectTransform));
            amt.transform.SetParent(c, false);
            var art = amt.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0.5f, 0.5f); art.anchorMax = new Vector2(0.5f, 0.5f);
            art.pivot = new Vector2(0.5f, 0.5f); art.sizeDelta = new Vector2(0f, 44f);
            var ale = amt.AddComponent<LayoutElement>(); ale.minHeight = 44f; ale.flexibleWidth = 1f;
            var seg = new string[6];
            for (int i = 0; i <= 5; i++) seg[i] = i == 0 ? GameApp.T("Wala", "None") : "+" + i;
            Widgets.Segmented(art, seg, 0, idx => { assist = idx; RefreshNote(); });

            // result area
            var resGo = new GameObject("Result", typeof(RectTransform));
            resGo.transform.SetParent(c, false);
            var rrt = resGo.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0.5f); rrt.anchorMax = new Vector2(0.5f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f); rrt.sizeDelta = new Vector2(0f, 10f);
            var rle = resGo.AddComponent<LayoutElement>(); rle.minHeight = 10f; rle.flexibleWidth = 1f;
            var rcsf = resGo.AddComponent<ContentSizeFitter>(); rcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            resultArea = rrt;

            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(c, false);
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0.5f); rowRt.anchorMax = new Vector2(0.5f, 0.5f);
            rowRt.pivot = new Vector2(0.5f, 0.5f); rowRt.sizeDelta = new Vector2(0f, 52f);
            var rowLe = row.AddComponent<LayoutElement>(); rowLe.minHeight = 52f; rowLe.flexibleWidth = 1f;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f; hlg.childControlWidth = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;
            Widgets.Normal(rowRt, "←", () => UI.Show(new GameScreen()));
            Widgets.Primary(rowRt, "🎲 " + GameApp.T("I-roll", "Roll"), () => Roll());
            Widgets.Primary(rowRt, "▶ " + GameApp.T("Tuloy", "Proceed"), () => Proceed());

            RefreshNote();
        }

        private void RefreshNote()
        {
            if (noteText == null) return;
            var roll = choice.roll;
            int mod = ContentDatabase.Mod(GameApp.Game.Run.stats.Get(roll.stat));
            string total = "d20 +" + mod;
            if (assist > 0) total += " +" + assist;
            if (useKasama && roll.kasama && GameApp.Game.KasamaNow > 0) total += " +kasama";
            if (useEchoes && roll.echoes && GameApp.Game.EchoPoolAvailable > 0) total += " +echo";
            noteText.text = GameApp.T("Inaasahang total: ", "Expected total: ") + total +
                "  ·  " + GameApp.T("walang partial success", "no partial success");
        }

        private void Roll()
        {
            result = GameApp.Game.ComputeRoll(card, choice, useKasama, useEchoes);
            foreach (Transform t in resultArea) Destroy(t.gameObject);
            var v = UIBuilder.VList(resultArea, 4f);

            var color = result.success ? UIBuilder.Green : UIBuilder.Red;
            string headline;
            if (result.auto) headline = GameApp.T("TULOY — auto-success (may halaga)", "THROUGH — auto-success (it costs)");
            else if (result.crit) headline = result.success ? "🎯 " + GameApp.T("CRITICAL — TAGUMPAY", "CRITICAL — SUCCESS") : "💥 " + GameApp.T("CRITICAL — MALAKING SAKUNA", "CRITICAL — MAJOR CONSEQUENCE");
            else headline = result.success ? GameApp.T("TAGUMPAY", "SUCCESS") : GameApp.T("MABIGO", "FAILURE");

            if (!result.auto)
                UIBuilder.Text(v, "d20 = " + result.d20 + "  +  " + result.mod + "  +  " + result.bonus + "  =  " + result.total + "   vs  DC " + result.dc, 15f, UIBuilder.InkDim);
            UIBuilder.Text(v, headline, 22f, color, TextAlignmentOptions.Center, true);
            if (!string.IsNullOrEmpty(result.eraNote))
                UIBuilder.Text(v, result.eraNote, 13f, UIBuilder.InkDim);
            if (result.kasamaRolls.Count > 0)
                UIBuilder.Text(v, "🫂 Kasama: " + string.Join(", ", result.kasamaRolls), 13f, UIBuilder.Teal);
            if (result.echoRolls.Count > 0)
                UIBuilder.Text(v, "📣 Echo: " + string.Join(", ", result.echoRolls), 13f, UIBuilder.Teal);
        }

        private void Proceed()
        {
            if (result == null) { Roll(); return; }
            GameApp.Game.ApplyRollChoice(card, choice, result);
            UI.Show(new OutcomeScreen(card, choice, result, false));
        }
    }

    // ---------------------------------------------------------------- outcome
    public class OutcomeScreen : Screen
    {
        private readonly Card card;
        private readonly Choice choice;
        private readonly RollResult roll;
        private readonly bool passive;

        public OutcomeScreen(Card card, Choice choice, RollResult roll, bool passive)
        {
            this.card = card;
            this.choice = choice;
            this.roll = roll;
            this.passive = passive;
        }

        protected override void BuildContent()
        {
            var content = GameApp.Content;
            var lang = GameApp.Lang;
            var r = GameApp.Game.Run;

            Widgets.Page(Root, out var c);
            Widgets.Sub(c, GameApp.T("Kabanata " + r.chapter, "Chapter " + r.chapter) + " · " + content.T(card.title, card.title_en, lang));

            Outcome outcome = null;
            if (passive || choice.roll == null)
                outcome = new Outcome { text = choice.label, text_en = choice.label_en, effects = choice.effects };
            else if (roll != null && choice.roll != null) outcome = roll.success ? choice.success : choice.fail;

            bool ok = passive || (roll != null && roll.success);
            var color = passive ? UIBuilder.InkDim : (ok ? UIBuilder.Green : UIBuilder.Red);
            string headline = passive ? GameApp.T("HINDI MAKIALAM", "MINDING YOUR OWN BUSINESS")
                : roll == null ? GameApp.T("HINDI", "NO")
                : roll.auto ? GameApp.T("TULOY", "THROUGH")
                : roll.crit ? (ok ? "🎯 CRITICAL" : "💥 CRITICAL")
                : ok ? GameApp.T("TAGUMPAY", "SUCCESS") : GameApp.T("MABIGO", "FAILURE");

            UIBuilder.Text(c, headline, 24f, color, TextAlignmentOptions.Center, true);

            if (roll != null && !roll.auto)
                UIBuilder.Text(c, "d20 " + roll.d20 + " + " + roll.mod + " + " + roll.bonus + " = " + roll.total + " vs DC " + roll.dc, 14f, UIBuilder.InkDim);
            if (roll != null && roll.utangClears)
                Widgets.Chip(c, "🌫️ " + GameApp.T("Nawala ang utang na loob.", "The debt is cleared."), UIBuilder.Green);

            if (outcome != null)
            {
                Widgets.Body(c, content.T(outcome.text, outcome.text_en, lang), 17f);
                var effs = outcome.effects;
                if (effs != null)
                {
                    var lines = new List<string>();
                    if (effs.stats != null)
                        foreach (var k in GameApp.Game.StatKeys)
                        {
                            int d = effs.stats.Get(k);
                            if (d != 0) lines.Add(GameApp.StatIcon(k) + " " + GameApp.StatName(k) + " " + (d > 0 ? "+" : "") + d);
                        }
                    if (effs.utang > 0) lines.Add("🌫️ " + GameApp.T("May nagbihis sa'yo… (+utang)", "Someone covered you… (+debt)"));
                    if (effs.utangClear) lines.Add("🌫️ " + GameApp.T("Utang: nawala", "Debt: cleared"));
                    if (effs.tiwala != 0) lines.Add("🏘️ Tiwala " + (effs.tiwala > 0 ? "+" : "") + effs.tiwala);
                    if (effs.kasama != 0) lines.Add("🫂 Kasama " + (effs.kasama > 0 ? "+" : "") + effs.kasama);
                    if (effs.voices > 0) lines.Add("🔊 " + GameApp.T("Boses", "Voices") + " +" + effs.voices);
                    if (effs.spokeUp || effs.spokeFirst) lines.Add("⭐ " + GameApp.T("Nagsalita ka", "You spoke up"));
                    if (effs.reform) lines.Add("🌅 " + GameApp.T("Reporma", "Reform"));
                    if (effs.complied) lines.Add("📝 " + GameApp.T("Sumunod ka sa utos", "You complied"));
                    if (effs.flags != null && effs.flags.Count > 0) lines.Add("🏷️ " + string.Join(", ", effs.flags));
                    foreach (var l in lines)
                        UIBuilder.Text(c, "• " + l, 14f, UIBuilder.InkDim);
                }
            }

            // stats after
            var statsGo = new GameObject("Stats", typeof(RectTransform));
            statsGo.transform.SetParent(c, false);
            var stRt = statsGo.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.5f, 0.5f); stRt.anchorMax = new Vector2(0.5f, 0.5f);
            stRt.pivot = new Vector2(0.5f, 0.5f); stRt.sizeDelta = new Vector2(0f, 54f);
            var stLe = statsGo.AddComponent<LayoutElement>(); stLe.minHeight = 54f; stLe.flexibleWidth = 1f;
            var hlg = statsGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f; hlg.childControlWidth = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;
            var vals = GameApp.Game.Stats;
            var mods = GameApp.Game.StatMods;
            for (int i = 0; i < GameApp.Game.StatKeys.Length && i < vals.Length; i++)
                Widgets.HudStat(stRt, GameApp.StatIcon(GameApp.Game.StatKeys[i]), GameApp.StatName(GameApp.Game.StatKeys[i]), vals[i], mods[i]);

            Widgets.Primary(c, "▶ " + GameApp.T("Magpatuloy", "Continue"), () => {
                GameApp.Game.LastPhase = null;
                GameApp.Game.NextCard();
                var run = GameApp.Game.Run;
                if (run == null || run.ended) return;             // RunEnded already changed the screen
                // chapter transitions already swapped the screen via events
                var cur = GameApp.UI.Current;
                if (cur is ChapterIntroScreen || cur is CareerScreen || cur is ClimaxScreen || cur is EndingScreen)
                    return;
                UI.Show(new GameScreen());
            });
        }
    }

    // ---------------------------------------------------------------- climax result
    public class ClimaxScreen : Screen
    {
        private bool leaveEcho;
        private string echoChoice;

        protected override void BuildContent()
        {
            var r = GameApp.Game.Run;
            var content = GameApp.Content;
            var lang = GameApp.Lang;
            bool reform = r != null && (r.reform || r.cascade.Contains("silent"));

            Widgets.Page(Root, out var c);
            Widgets.Sub(c, GameApp.T("Ang Pagsubok — resulta", "The Test — result"));
            Widgets.Title(c, reform ? GameApp.T("REPORMA", "REFORM") : GameApp.T("HINDI PA SAPAT", "NOT YET"));
            Widgets.Sub(c, GameApp.T("Voice-count: ", "Voice-count: ") + (r != null ? r.voices : 0));

            var vv = UIBuilder.VList(c, 4f);
            foreach (var g in content.Groups)
            {
                bool joined = r != null && r.cascade.Contains(g.id);
                UIBuilder.Text(vv, (joined ? "🔊 " : "· ") + content.T(g.name, g.name_en, lang), 15f, joined ? UIBuilder.Teal : UIBuilder.InkDim);
            }

            if (reform)
                Widgets.Body(c, GameApp.T("Tumaas ang boses hanggang sa Silent Majority. Nagbago ang bayan — dahil nagsalita ka, at dahil may sumunod.",
                    "The voices rose until the Silent Majority joined. The town changed — because you spoke, and because others followed."), 16f);
            else
                Widgets.Body(c, GameApp.T("Tumigil ang cascade. Hindi pa sapat ang boses. Ang bayan ay naghihintay pa rin.",
                    "The cascade stopped. Not enough voices yet. The town is still waiting."), 16f);

            // echo opt-in (§B.13): attribute one pre-written echo to this run
            var pool = GameApp.Save.Barangay.echoes;
            if (pool != null && pool.Count > 0)
            {
                UIBuilder.Toggle(c, "📣 " + GameApp.T("Iwan ang isang echo sa Barangay", "Leave an echo in the Barangay"), leaveEcho, v =>
                {
                    leaveEcho = v;
                    if (v && string.IsNullOrEmpty(echoChoice)) echoChoice = pool[0];
                });
                if (leaveEcho)
                {
                    if (string.IsNullOrEmpty(echoChoice)) echoChoice = pool[0];
                    var labels = new List<string>();
                    foreach (var id in pool)
                    {
                        var e = content.EchoById(id);
                        string t = e != null ? content.T(e.text, e.text_en, lang) : id;
                        if (t.Length > 28) t = t.Substring(0, 28) + "…";
                        labels.Add(t);
                    }
                    Widgets.Segmented(c, labels.ToArray(), Mathf.Max(0, pool.IndexOf(echoChoice)), idx =>
                    {
                        if (idx >= 0 && idx < pool.Count) echoChoice = pool[idx];
                    });
                }
            }

            Widgets.Primary(c, "▶ " + GameApp.T("Magpatuloy", "Continue"), () =>
            {
                GameApp.Game.PendingEchoId = leaveEcho ? echoChoice : null;
                GameApp.Game.FinishRun();
            });
        }
    }

    // ---------------------------------------------------------------- ending
    public class EndingScreen : Screen
    {
        protected override void BuildContent()
        {
            var r = GameApp.Game.Run;
            var content = GameApp.Content;
            var lang = GameApp.Lang;

            Widgets.Page(Root, out var c);
            if (r == null) { UI.Show(new MainMenuScreen()); return; }

            var ending = content.EndingById(r.endingId);
            Widgets.Sub(c, GameApp.T("Ang buhay mo", "Your life"));
            Widgets.Title(c, ending != null ? content.T(ending.title, ending.title_en, lang) : "—");
            if (ending != null)
            {
                Widgets.Sub(c, GameApp.T("Ending ", "Ending ") + ending.id + " · " + GameApp.Game.CareerName(ending.career, lang));
                Widgets.Body(c, content.T(ending.text, ending.text_en, lang), 17f);
            }

            // redemption note (§B.14)
            if (GameApp.Game.Redemption())
                Widgets.Chip(c, "🌅 " + GameApp.T("Pagbawi: hindi pa huli ang lahat.", "Redemption: it's not too late."), UIBuilder.Gold);

            // starting line note
            var bg = content.BackgroundById(r.background);
            if (bg != null)
                Widgets.Sub(c, GameApp.T("Nagsimula ka bilang: ", "You began as: ") + content.T(bg.name, bg.name_en, lang) +
                    ". " + GameApp.T("Iba ang simula ng iba.", "Others began elsewhere."));

            // contribution summary
            if (GameApp.Game.LastSubmit != null)
            {
                var s = GameApp.Game.LastSubmit.summary;
                var st = GameApp.Game.LastSubmit.state;
                Widgets.Sub(c, GameApp.T("Ambag sa Barangay", "Contribution to the Barangay"));
                if (s != null)
                    UIBuilder.Text(c, "Pag-asa " + (s.pagAsa >= 0 ? "+" : "") + s.pagAsa +
                        " · Tiwala " + (s.tiwala >= 0 ? "+" : "") + s.tiwala +
                        " · Walang Kibo +" + s.walangKibo, 14f, UIBuilder.InkDim);
                if (GameApp.Game.LastSubmit.crossed60)
                    Widgets.Chip(c, "🌅 " + GameApp.T("Tumawid sa 60 — Gumigising!", "Crossed 60 — Awakening!"), UIBuilder.Gold);
                if (st != null)
                {
                    var era = content.EraById(st.era);
                    Widgets.Chip(c, "■ " + GameApp.EraName(era) + " · " + GameApp.Save.SpokeUpPct() + "% " + GameApp.T("nagsalita", "spoke up"), UIBuilder.Teal);
                }
            }

            // final ask (location-specific, 13C)
            var ask = content.AskById(GameApp.Save.Settings.location);
            if (ask != null)
            {
                Widgets.Sub(c, GameApp.T("Isang tanong para sa'yo", "One question for you"));
                Widgets.Chip(c, content.T(ask.label, ask.label_en, lang), UIBuilder.Teal);
                Widgets.Body(c, content.T(ask.text, ask.text_en, lang), 14f);
            }

            Widgets.Primary(c, "↻ " + GameApp.T("Buhay muli", "Live again"), () => { GameApp.Save.Run = null; GameApp.Save.Save(); UI.Show(new MainMenuScreen()); });
            Widgets.Normal(c, GameApp.T("🏘️ Barangay", "🏘️ Barangay"), () => UI.Show(new BarangayScreen()));
        }
    }

    // ---------------------------------------------------------------- generic confirm
    public class ConfirmScreen : Screen
    {
        private readonly string title;
        private readonly string body;
        private readonly System.Action onYes;

        public ConfirmScreen(string title, string body, System.Action onYes)
        {
            this.title = title;
            this.body = body;
            this.onYes = onYes;
        }

        protected override void BuildContent()
        {
            Widgets.Page(Root, out var c);
            Widgets.Title(c, title);
            Widgets.Body(c, body);

            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(c, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = new Vector2(0f, 52f);
            var le = row.AddComponent<LayoutElement>(); le.minHeight = 52f; le.flexibleWidth = 1f;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f; hlg.childControlWidth = true; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;
            Widgets.Normal(rt, GameApp.T("Hindi", "No"), () => UI.Show(new GameScreen()));
            Widgets.Primary(rt, GameApp.T("Oo", "Yes"), () => onYes());
        }
    }
}

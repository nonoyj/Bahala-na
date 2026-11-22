// Bahala Na? — rules engine (direct port of js/engine.js, the verified reference).
// Chapters/decks/careers/climax/endings per HANDOFF §B.5–§B.14.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BahalaNa
{
    public class GameManager
    {
        public readonly string[] StatKeys =
            { "talino", "diskarte", "pera", "koneksyon", "integridad", "komunidad" };

        public ContentDatabase Content;
        public SaveManager Save;

        public RunState Run => Save.Run;
        public Profile Profile => Save.Profile;
        public string Lang => Save.Settings.lang;

        // UI hooks
        public event Action RunChanged;                 // after a card resolves
        public event Action<int> ChapterStarted;        // UI shows the chapter intro
        public event Action<List<string>> CareerNeeded; // UI shows the career picker
        public event Action ClimaxReached;              // Ch4 cards done — UI shows the climax result
        public event Action RunEnded;                   // UI submits + shows the ending

        public ContributionPayload LastPayload;
        public SubmitResult LastSubmit;

        public GameManager(ContentDatabase content, SaveManager save)
        {
            Content = content;
            Save = save;
        }

        // ---------------------------------------------------------------- derived
        public EraDef Era => Save.Era();
        public int EraDcMod => Era != null ? Era.dc_mod : 0;
        public int KasamaNow => KasamaAvailable(Run);
        public int EchoPoolAvailable => EchoAvailable(Run, Save != null && Save.Barangay != null ? Save.Barangay.echoes.Count : 0);
        public int[] Stats => Run != null ? Run.stats.ToArray() : new int[0];
        public bool HasIntegrityForPadrino => Run != null && Run.stats.integridad > 0 && Run.padrinoCount < Run.padrinoLimit;

        public static int KasamaAvailable(RunState r)
        {
            if (r == null) return 0;
            int k = r.stats.komunidad;
            return k >= 6 ? 2 : k >= 3 ? 1 : 0;
        }

        public static int EchoAvailable(RunState r, int poolSize)
        {
            if (r == null) return 0;
            return Math.Max(0, Math.Min(2, poolSize - r.echoesUsed.Count));
        }

        // ---------------------------------------------------------------- run lifecycle
        public void NewRun(string name, string presentation)
        {
            int roll = Dice.Roll(6);
            var bg = Content.Backgrounds.Find(b => b.roll == roll) ?? Content.Backgrounds[0];
            var stats = new Stats();
            foreach (var k in StatKeys) stats.Add(k, 3);
            if (bg.bonus != null) foreach (var k in StatKeys) stats.Add(k, bg.bonus.Get(k));
            if (bg.penalty != null) foreach (var k in StatKeys) stats.Add(k, bg.penalty.Get(k));

            Save.Run = new RunState
            {
                runId = "run_" + DateTime.UtcNow.Ticks + "_" + Dice.Roll(1000000),
                chapter = 0,
                stats = stats,
                background = bg.id,
                backgroundRoll = roll,
                startedAt = DateTime.UtcNow.ToString("o"),
            };
            Save.Profile = new Profile { name = string.IsNullOrEmpty(name) ? "Bata" : name, presentation = presentation };
            RefreshMods();
            Save.Save();
        }

        public bool HasFlag(string f) => Run != null && Run.flags.Contains(f);

        /// <summary>Withdraw (3W): the run ends with the "Nanood"-style silent ending.</summary>
        public void Withdraw()
        {
            if (Run == null) return;
            Run.endingId = 10;
            Run.ended = true;
            LastPayload = BuildPayload();
            Save.Enqueue(LastPayload);
            Save.Save();
            RunEnded?.Invoke();
        }

        /// <summary>Instant "huwag makialam" choice (no roll, direct effects); advances to the next card.</summary>
        public void ChoosePassive(Card card, Choice choice)
        {
            if (Run == null) return;
            LastOutcome = Resolve(card, choice, null);
            LastRoll = null;
            LastChoiceWasPassive = true;
            NextCard();
        }

        public bool ReqOk(Card c)
        {
            var r = c.requires;
            if (r == null) return true;
            if (r.any_flags != null)
                foreach (var f in r.any_flags) if (HasFlag(f)) return true;
            if (r.utang_min > 0 && Run.utang >= r.utang_min) return true;
            return false;
        }

        public void StartChapter(int chapter)
        {
            if (Run == null) return;
            Run.chapter = chapter;
            Run.cardIndex = 0;
            var ids = new List<string>();

            if (chapter == 1)
            {
                ids.AddRange(Ids(Sample(Content.Pool("ch1"), 4)));
            }
            else if (chapter == 2)
            {
                var pool = Content.Pool("ch2");
                var eligible = pool.FindAll(ReqOk);
                var chosen = Sample(eligible, 5);
                if (chosen.Count < 5)
                {
                    var rest = pool.FindAll(c => !chosen.Contains(c));
                    chosen.AddRange(Sample(rest, 5 - chosen.Count));
                }
                ids.AddRange(Ids(chosen));
            }
            else if (chapter == 3)
            {
                var rolePool = Content.Pool("ch3_role").FindAll(c => c.role == Run.career);
                var sharedPool = Content.Pool("ch3_shared");
                var roleIds = Ids(Sample(rolePool, 3));
                var sharedIds = Ids(Sample(sharedPool, 2));
                string last;
                if (Run.utang >= 3)
                {
                    last = Sample(Content.Pool("ch3_singil"), 1)[0].id;
                }
                else
                {
                    var remaining = sharedPool.FindAll(c => !sharedIds.Contains(c.id));
                    if (remaining.Count == 0) remaining = sharedPool;
                    last = Sample(remaining, 1)[0].id;
                }
                ids.Add(roleIds[0]); ids.Add(sharedIds[0]); ids.Add(roleIds[1]);
                ids.Add(last); ids.Add(sharedIds[1]); ids.Add(roleIds[2]);
            }
            else if (chapter == 4)
            {
                ids.AddRange(Ids(Sample(Content.Pool("ch4"), 3)));
                Run.voices = 0;
                Run.voicesAdded = 0;
                Run.cascade = new List<string>();
                Run.cascade.Add("brave"); // The Brave (Nurse Lea) speaks at threshold 0
                Run.voices = 1;
            }

            Run.deck = ids;
            RefreshMods();
            Save.Save();
        }

        public void GoToChapter(int chapter)
        {
            StartChapter(chapter);
            ChapterStarted?.Invoke(chapter);
        }

        public void AdvanceChapter()
        {
            if (Run.chapter == 1) GoToChapter(2);
            else if (Run.chapter == 2)
            {
                var opts = CareerOptions();
                if (opts.Count > 1) { PendingCareerOptions = opts; CareerNeeded?.Invoke(opts); }
                else AssignCareer(opts[0]);
            }
            else if (Run.chapter == 3) GoToChapter(4);
            else if (Run.chapter == 4) ClimaxReached?.Invoke();
        }

        /// <summary>Set by AdvanceChapter when the player must pick a career (9B).</summary>
        public List<string> PendingCareerOptions;

        public void AssignCareer(string career)
        {
            Run.career = career;
            Save.Save();
            GoToChapter(3);
        }

        /// <summary>Called by the career screen when the player picks (9B).</summary>
        public void PickCareer(string career) => AssignCareer(career);

        /// <summary>Resume a saved run at its current chapter (re-deals the chapter deck, keeps the card index).</summary>
        public void OpenChapter(int chapter)
        {
            if (Run == null) return;
            if (Run.deck == null || Run.deck.Count == 0 || Run.cardIndex >= Run.deck.Count)
            {
                int idx = Run.cardIndex;
                StartChapter(chapter);
                if (idx >= 0 && idx < Run.deck.Count) Run.cardIndex = idx;
                ChapterStarted?.Invoke(chapter);
            }
            else
            {
                Run.chapter = chapter;
                ChapterStarted?.Invoke(chapter);
            }
        }

        public Card CurrentCard
        {
            get
            {
                if (Run == null || Run.cardIndex >= Run.deck.Count) return null;
                return Content.Card(Run.deck[Run.cardIndex]);
            }
        }

        // ---------------------------------------------------------------- careers (§B.10 / 9B)
        public List<string> CareerOptions()
        {
            var opts = new List<string>();
            if (Run.stats.koneksyon >= 5 && (HasFlag("ran_for_org") || Run.utang >= 2)) opts.Add("official");
            if (HasFlag("took_exam_or_followed_family_service") && (Run.stats.diskarte >= 4 || Run.stats.pera <= 3)) opts.Add("police");
            if (Run.stats.talino >= 5) opts.Add("teacher");
            if (Run.stats.komunidad >= 5 || HasFlag("spoke_up")) opts.Add("organizer");
            if (HasFlag("went_to_work_early")) opts.Add("citizen");
            if (opts.Count == 0) opts.Add("citizen");
            return opts;
        }

        // ---------------------------------------------------------------- dice (§B.7 / 7B / 8B)
        public RollResult ComputeRoll(Card card, Choice choice, bool useKasama, bool useEchoes)
        {
            var r = choice.roll;
            int dc = r.dc;
            if (r.dc_if_flag != null && HasFlag(r.dc_if_flag.flag)) dc = r.dc_if_flag.dc;
            if (r.dc_plus_utang) dc = 10 + Run.utang;

            bool systemic = r.systemic || card.systemic;
            string eraNote = null;
            if (systemic && EraDcMod != 0)
            {
                dc += EraDcMod;
                eraNote = (EraDcMod > 0 ? "+" : "") + EraDcMod + " (" + (Era != null ? Era.name : "") + ")";
            }

            int mod = Content.Mod(Run.stats.Get(r.stat));
            int kasamaDice = useKasama ? Math.Min(2, KasamaAvailable(Run)) : 0;
            int echoDice = useEchoes ? Math.Min(2, EchoAvailable(Run, Save != null ? Save.Barangay.echoes.Count : 0)) : 0;

            // Padrino / Singil "pay" choices: auto-success, no d20 (§B.7 7.1, §B.7 7.2).
            if (choice.type == "padrino" || choice.type == "comply")
            {
                return new RollResult { d20 = 0, mod = mod, dc = dc, systemic = systemic, eraNote = eraNote, stat = r.stat, auto = true, success = true };
            }

            int d20 = Dice.Roll20();

            var res = new RollResult { d20 = d20, mod = mod, dc = dc, systemic = systemic, eraNote = eraNote, stat = r.stat };
            for (int i = 0; i < kasamaDice; i++) { int v = Dice.Roll4(); res.kasamaRolls.Add(v); res.bonus += v; }
            for (int i = 0; i < echoDice; i++) { int v = Dice.Roll6(); res.echoRolls.Add(v); res.bonus += v; }
            res.total = d20 + mod + res.bonus;
            res.crit = d20 == 20;
            res.fumble = d20 == 1;
            res.success = res.crit || (!res.fumble && res.total >= dc);
            return res;
        }

        /// <summary>Padrino shortcut: mark the card as resolved with its padrino outcome (§B.7 7.1).</summary>
        public bool UsePadrino()
        {
            var card = CurrentCard;
            if (card == null || Run == null || Run.padrinoCount >= Run.padrinoLimit || Run.stats.integridad <= 0) return false;
            Choice pad = null;
            if (card.choices != null)
                foreach (var ch in card.choices) if (ch.type == "padrino") { pad = ch; break; }
            if (pad == null) return false;
            LastRoll = new RollResult { auto = true, success = true, dc = DCFor(card) };
            LastOutcome = Resolve(card, pad, LastRoll);
            return true;
        }

        /// <summary>Applies a choice (roll result or no-roll effects) and returns the outcome text.</summary>
        public Outcome Resolve(Card card, Choice choice, RollResult roll)
        {
            Effects effs;
            Outcome text = null;
            if (choice.roll != null && roll != null)
            {
                bool ok = roll.success;
                text = ok ? choice.success : choice.fail;
                var src = ok ? choice.success : choice.fail;
                effs = src != null ? src.effects : null;
            }
            else
            {
                effs = choice.effects;
            }

            if (choice.type == "padrino" || choice.type == "comply") Run.padrinoCount++;
            if (choice.type == "honest" || choice.type == "collective") Run.stoodFirm = true;

            ApplyEffects(effs);
            if (roll != null && roll.utangClears && Run.utang > 0)
            {
                Run.utang = 0;
                Run.ledger.Add(new LedgerEntry { kind = "cleared", chapter = Run.chapter, card = card != null ? card.title : "" });
            }
            Save.Save();
            RunChanged?.Invoke();
            return text;
        }

        public void NextCard()
        {
            if (Run == null) return;
            Run.cardIndex++;
            Save.Save();
            if (Run.cardIndex >= Run.deck.Count) { LastPhase = "chapter-end"; RunChanged?.Invoke(); AdvanceChapter(); }
            else RunChanged?.Invoke();
        }

        /// <summary>Set to "chapter-end" when the last card advanced into a chapter transition (UI reads it).</summary>
        public string LastPhase;

        /// <summary>Applies a roll choice and advances to the next card (roll-sheet flow).</summary>
        public Outcome ApplyRollChoice(Card card, Choice choice, RollResult roll)
        {
            if (roll != null && roll.success && choice.type == "honest" && choice.roll != null && choice.roll.dc_plus_utang)
                roll.utangClears = true;
            LastRoll = roll;
            var outcome = Resolve(card, choice, roll);
            LastOutcome = outcome;
            return outcome;
        }

        // ---------------------------------------------------------------- effects
        public void ApplyEffects(Effects e)
        {
            if (e == null || Run == null) return;

            if (e.stats != null)
                foreach (var k in StatKeys) Run.stats.Add(k, e.stats.Get(k));
            RefreshMods();

            if (e.utang != 0)
            {
                Run.utang = Math.Max(0, Run.utang + e.utang);
                if (e.utang > 0)
                    Run.ledger.Add(new LedgerEntry { kind = "utang", amount = e.utang, chapter = Run.chapter, card = CurrentCard != null ? CurrentCard.title : "" });
            }
            if (e.utangClear)
            {
                Run.utang = 0;
                Run.ledger.Add(new LedgerEntry { kind = "cleared", chapter = Run.chapter, card = CurrentCard != null ? CurrentCard.title : "" });
            }
            if (e.flags != null)
                foreach (var f in e.flags) if (!Run.flags.Contains(f)) Run.flags.Add(f);

            if (e.tiwala != 0) Run.pendingTiwala += e.tiwala;
            if (e.kasama != 0) Run.kasamaTokens = Math.Max(0, Run.kasamaTokens + e.kasama);
            if (e.voices > 0) AddVoices(e.voices);
            if (e.walangKibo > 0 && Run.chapter == 4) Run.silentClimax = true;
            if (e.spokeUp) Run.spokeUp = true;
            if (e.spokeFirst) { Run.spokeFirst = true; Run.spokeUp = true; }
            if (e.complied)
            {
                Run.complied = true;
                Run.ledger.Add(new LedgerEntry { kind = "complied", chapter = Run.chapter, card = CurrentCard != null ? CurrentCard.title : "" });
            }
            if (e.reform) Run.reform = true;

            if (e.usesEchoes > 0)
            {
                var avail = new List<string>(Save.Barangay.echoes);
                avail.RemoveAll(id => Run.echoesUsed.Contains(id));
                for (int i = 0; i < e.usesEchoes && avail.Count > 0; i++)
                    Run.echoesUsed.Add(avail[0]);
            }
        }

        // ---------------------------------------------------------------- climax cascade (§B.12)
        public void AddVoices(int n)
        {
            if (n <= 0 || Run == null) return;
            Run.voices += n;
            Run.voicesAdded += n;
            if (Run.chapter == 4 && !Run.spokeFirst) Run.joinedCascade = true;

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var g in Content.Groups)
                {
                    if (!Run.cascade.Contains(g.id) && Run.voices >= g.threshold)
                    {
                        Run.cascade.Add(g.id);
                        Run.voices += 1;
                        changed = true;
                    }
                }
            }
        }

        // ---------------------------------------------------------------- endings (§B.14)
        public int DetermineEnding()
        {
            int integ = Run.stats.integridad;
            bool highInteg = integ >= 6;
            bool spoke = Run.spokeUp || HasFlag("spoke_up");
            switch (Run.career)
            {
                case "official": return highInteg && Run.utang <= 2 ? 1 : 2;
                case "police": return highInteg && (spoke || Run.stoodFirm) ? 3 : 4;
                case "teacher": return (Run.stoodFirm || spoke) ? 5 : 6;
                case "organizer":
                    if (HasFlag("burned_out")) return 8;
                    if (Run.reform) return 7;
                    return spoke ? 7 : 8;
                default: return (Run.joinedCascade || Run.reform) ? 9 : 10;
            }
        }

        public bool Redemption()
        {
            return Run.stats.komunidad >= 4 && new[] { 2, 4, 6, 8, 10 }.Contains(Run.endingId);
        }

        // ---------------------------------------------------------------- submit
        public void FinishRun()
        {
            Run.endingId = DetermineEnding();
            Run.ended = true;
            LastPayload = BuildPayload();
            LastPayload.echoId = PendingEchoId;
            Save.Enqueue(LastPayload);
            Save.Save();
            RunEnded?.Invoke();
        }

        /// <summary>Echo id the player opted to leave (UI sets this before the run ends).</summary>
        public string PendingEchoId;

        public ContributionPayload BuildPayload()
        {
            return new ContributionPayload
            {
                runId = Run.runId,
                background = Run.background,
                career = string.IsNullOrEmpty(Run.career) ? "citizen" : Run.career,
                endingId = Run.endingId,
                spokeUp = Run.spokeUp,
                joinedCascade = Run.joinedCascade,
                silent = !Run.spokeUp && !Run.joinedCascade,
                utangFinal = Run.utang,
                pendingTiwala = Run.pendingTiwala,
            };
        }

        public void ApplySubmitResult(SubmitResult r)
        {
            LastSubmit = r;
            if (r != null && r.state != null)
            {
                Save.Barangay.tiwala = r.state.tiwala;
                Save.Barangay.imprastruktura = r.state.imprastruktura;
                Save.Barangay.pagAsa = r.state.pagAsa;
                Save.Barangay.walangKibo = r.state.walangKibo;
                Save.Barangay.era = string.IsNullOrEmpty(r.state.era) ? Save.Barangay.era : r.state.era;
                Save.Barangay.version = r.state.version;
                Save.Save();
            }
        }

        // ---------------------------------------------------------------- chapter metadata
        public static string ChapterName(int chapter, string lang)
        {
            bool tl = lang == "tl";
            switch (chapter)
            {
                case 1: return tl ? "Kabataan" : "Childhood";
                case 2: return tl ? "Sangandaan" : "Crossroads";
                case 3: return tl ? "Tungkulin" : "Duty";
                case 4: return tl ? "Ang Pagsubok" : "The Test";
                default: return "";
            }
        }

        public string ChapterAges(int chapter)
        {
            switch (chapter)
            {
                case 1: return "~8–12";
                case 2: return "~16–22";
                case 3: return "~25–35";
                default: return "~35+";
            }
        }

        public static string CardCountFor(int chapter)
        {
            switch (chapter)
            {
                case 1: return "4";
                case 2: return "5";
                case 3: return "6";
                default: return "3";
            }
        }

        public string CareerName(string career, string lang)
        {
            bool tl = lang == "tl";
            switch (career)
            {
                case "official": return tl ? "Opisyal / Barangay-City Official" : "Barangay/City Official";
                case "police": return tl ? "Pulis" : "Police Officer";
                case "teacher": return tl ? "Guro / Municipal Engineer" : "Teacher / Municipal Engineer";
                case "organizer": return tl ? "Community Organizer / Mamamahayag" : "Community Organizer / Journalist";
                default: return tl ? "Ordinaryong Mamamayan" : "Ordinary Working Citizen";
            }
        }

        // ---------------------------------------------------------------- ui helpers
        public int CardsPlayedThisChapter => Run == null ? 0 : Run.cardIndex;
        public int ChapterCardTotal => Run == null ? 0 : Run.deck.Count;
        public string ChapterLabel => Run == null ? "" : ChapterName(Run.chapter, Lang);
        public int[] StatValues => Run == null ? new int[0] : Run.stats.ToArray();
        public int[] StatMods => Run == null ? new int[0] : RefreshMods();

        public int[] RefreshMods()
        {
            if (Run == null) return new int[0];
            for (int i = 0; i < StatKeys.Length; i++)
                Run.mods[i] = Content.Mod(Run.stats.Get(StatKeys[i]));
            return Run.mods;
        }

        /// <summary>DC preview shown on the card screen (era already folded in).</summary>
        public int DCFor(Card card)
        {
            var c = CardById(card.id);
            if (c == null) return 0;
            int dc = c.DC;
            if (EraDcMod != 0 && (c.systemic || (c.roll_choice != null && c.roll_choice.roll != null && c.roll_choice.roll.systemic))) dc += EraDcMod;
            if (c.roll_choice != null && c.roll_choice.roll != null && c.roll_choice.roll.dc_plus_utang) dc = 10 + Run.utang;
            return dc;
        }

        public Outcome LastOutcome;
        public RollResult LastRoll;
        public bool LastChoiceWasPassive;

        private Card CardById(string id) => Content != null ? Content.Card(id) : null;

        // ---------------------------------------------------------------- utils
        public static List<T> Sample<T>(List<T> src, int n)
        {
            var a = new List<T>(src);
            for (int i = a.Count - 1; i > 0; i--)
            {
                int j = Dice.Roll(i + 1) - 1;
                if (j < 0) j = 0;
                if (j > i) j = i;
                var tmp = a[i]; a[i] = a[j]; a[j] = tmp;
            }
            var outList = new List<T>();
            for (int i = 0; i < n && i < a.Count; i++) outList.Add(a[i]);
            return outList;
        }

        private static List<string> Ids(List<Card> cards)
        {
            var ids = new List<string>();
            foreach (var c in cards) ids.Add(c.id);
            return ids;
        }
    }
}

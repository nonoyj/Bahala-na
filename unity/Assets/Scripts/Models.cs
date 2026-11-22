// Bahala Na? — data models.
// Mirrors the frozen contract (API_CONTRACT.md §1) and the content JSON (content/cards/*.json).
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BahalaNa
{
    // ---------------------------------------------------------------- stats
    [Serializable]
    public class Stats
    {
        public int talino, diskarte, pera, koneksyon, integridad, komunidad;

        public Stats() { }

        public Stats(Stats o)
        {
            if (o == null) return;
            talino = o.talino; diskarte = o.diskarte; pera = o.pera;
            koneksyon = o.koneksyon; integridad = o.integridad; komunidad = o.komunidad;
        }

        public int Get(string key)
        {
            switch (key)
            {
                case "talino": return talino;
                case "diskarte": return diskarte;
                case "pera": return pera;
                case "koneksyon": return koneksyon;
                case "integridad": return integridad;
                case "komunidad": return komunidad;
                default: return 0;
            }
        }

        public void Add(string key, int delta)
        {
            switch (key)
            {
                case "talino": talino = Clamp(talino + delta); break;
                case "diskarte": diskarte = Clamp(diskarte + delta); break;
                case "pera": pera = Clamp(pera + delta); break;
                case "koneksyon": koneksyon = Clamp(koneksyon + delta); break;
                case "integridad": integridad = Clamp(integridad + delta); break;
                case "komunidad": komunidad = Clamp(komunidad + delta); break;
            }
        }

        public static int Clamp(int v) => Math.Max(0, Math.Min(10, v));

        public int[] ToArray()
        {
            return new int[] { talino, diskarte, pera, koneksyon, integridad, komunidad };
        }
    }

    // ---------------------------------------------------------------- content DTOs
    [Serializable]
    public class Requires
    {
        public List<string> any_flags;
        public int utang_min;
    }

    [Serializable]
    public class DcIfFlag
    {
        public string flag;
        public int dc;
    }

    [Serializable]
    public class RollSpec
    {
        public string stat;
        public int dc;
        public bool kasama;
        public bool echoes;
        public bool systemic;
        public DcIfFlag dc_if_flag;
        public bool dc_plus_utang;
    }

    [Serializable]
    public class Effects
    {
        public Stats stats;
        public int utang;
        public List<string> flags;
        public int tiwala;
        public int kasama;
        public int voices;
        public int walangKibo;
        public bool spokeUp;
        public bool spokeFirst;
        public bool complied;
        public bool utangClear;
        public bool reform;
        public int usesEchoes;
    }

    [Serializable]
    public class Outcome
    {
        public string text;
        public string text_en;
        public Effects effects;
    }

    [Serializable]
    public class Choice
    {
        public string label;
        public string label_en;
        public string type; // honest | padrino | collective | huwag | ordinary | comply | pera | escape | diskarte
        public RollSpec roll;
        public Effects effects; // used when roll == null
        public Outcome success;
        public Outcome fail;

        [JsonIgnore] public Card card; // owner, set by ContentDatabase after load

        public void Bind(Card owner) { card = owner; }
    }
    
    [Serializable]
    public class Card
    {
        public string id;
        public int chapter;
        public string pool;   // ch1 | ch2 | ch3_role | ch3_shared | ch3_singil | ch4
        public string role;   // official | police | teacher | organizer | citizen (ch3_role)
        public string title;
        public string title_en;
        public List<string> tags;
        public Requires requires;
        public string setup;
        public string setup_en;
        public bool systemic;
        public List<Choice> choices;
        public string callback;

        [JsonIgnore] public Card card; // set by ContentDatabase after load
        [JsonIgnore] public Choice roll_choice; // cached first choice with a roll
        [JsonIgnore] public bool roll_clears_utang; // cached from the honest "Hindi." outcome

        public void Bind(Card owner)
        {
            card = owner;
            roll_choice = null;
            roll_clears_utang = false;
            if (choices == null) return;
            foreach (var ch in choices)
            {
                if (roll_choice == null && ch.roll != null) roll_choice = ch;
                if (ch.type == "honest" && ch.roll != null && ch.success != null &&
                    ch.success.effects != null && ch.success.effects.utangClear)
                    roll_clears_utang = true;
            }
        }

        /// <summary>Primary roll (auto-success for padrino/comply per §B.7 7.1).</summary>
        public Choice RollChoice()
        {
            if (roll_choice == null)
                foreach (var ch in choices) if (ch.roll != null) { roll_choice = ch; break; }
            return roll_choice;
        }

        /// <summary>DC preview for the card header (base DC of the primary roll; era/dc_plus_utang resolved at roll time).</summary>
        public int DC
        {
            get
            {
                var ch = RollChoice();
                if (ch == null || ch.roll == null) return 0;
                return ch.roll.dc;
            }
        }
    }

    [Serializable]
    public class Ending
    {
        public int id;
        public string title;
        public string title_en;
        public string career;
        public string condition;
        public string text;
        public string text_en;
    }

    [Serializable]
    public class Echo
    {
        public string id;
        public string text;
        public string text_en;
    }

    [Serializable]
    public class FinalAsk
    {
        public string id;
        public string label;
        public string label_en;
        public string text;
        public string text_en;
    }

    [Serializable]
    public class Background
    {
        public int roll;
        public string id;
        public string name;
        public string name_en;
        public Stats bonus;
        public Stats penalty;
        public string text;
        public string text_en;
    }

    [Serializable]
    public class EraDef
    {
        public int min;
        public int max;
        public string id;
        public string name;
        public string name_en;
        public int dc_mod;
        public string desc;
        public string desc_en;
    }

    // ---------------------------------------------------------------- runtime state
    [Serializable]
    public class LedgerEntry
    {
        public string kind;   // utang | cleared | complied
        public int amount;
        public int chapter;
        public string card;
    }

    [Serializable]
    public class RunState
    {
        public string runId;
        public int chapter;
        public List<string> deck = new List<string>();
        public int cardIndex;
        public Stats stats = new Stats();
        public int utang;                 // HIDDEN in UI
        public int[] mods = new int[6];   // cached stat modifiers (§B.6)
        public List<string> flags = new List<string>();
        public int kasamaTokens;
        public string background;
        public int backgroundRoll;
        public string career;
        public List<LedgerEntry> ledger = new List<LedgerEntry>();
        public int voices;
        public int voicesAdded;
        public List<string> cascade = new List<string>(); // joined group ids, ordered
        public bool spokeUp, spokeFirst, joinedCascade, silentClimax, reform, complied, stoodFirm;
        public int padrinoCount;
        public int padrinoLimit = 3;      // max padrino shortcuts per run (soft cap, §B.7 7.1)
        public int pendingTiwala;
        public List<string> echoesUsed = new List<string>();
        public string startedAt;
        public bool ended;
        public int endingId;
    }

    [Serializable]
    public class ContributionSummary
    {
        public int pagAsa, tiwala, walangKibo;
        public bool spoke;
    }

    [Serializable]
    public class BarangayState
    {
        public int tiwala = 60;
        public int imprastruktura = 35;
        public int pagAsa = 45;           // Karaniwan seed
        public int walangKibo;
        public string era = "karaniwan";
        public int version;
        public int runs = 24;             // seed: barangay has history
        public int spokeUp = 9;           // 37% — tipping point is 40%
        public List<string> echoes = new List<string>();
        public ContributionSummary lastContribution;
    }

    [Serializable]
    public class Profile
    {
        public string name = "Bata";
        public string presentation = "neutral"; // boy | girl | neutral
    }

    [Serializable]
    public class SettingsData
    {
        public string lang = "tl";
        public bool sfx = true;
        public bool music = false;
        public bool vibration = true;
        public string dice = "full";      // full | fast | off
        public string textSize = "standard";
        public bool contrast;
        public string location = "general";
    }

    // ---------------------------------------------------------------- backend payloads
    [Serializable]
    public class ContributionPayload
    {
        public string runId;
        public string background;
        public string career;
        public int endingId;
        public bool spokeUp;
        public bool joinedCascade;
        public bool silent;
        public int utangFinal;
        public string echoId;
        public int pendingTiwala;
    }

    [Serializable]
    public class RollResult
    {
        public int d20;
        public int mod;
        public int bonus;
        public List<int> kasamaRolls = new List<int>();
        public List<int> echoRolls = new List<int>();
        public int total;
        public int dc;
        public bool systemic;
        public string eraNote;
        public bool crit;
        public bool fumble;
        public bool success;
        public string stat;
        public bool auto;          // padrino/comply shortcut — success without a d20 (§B.7 7.1)
        public bool utangClears;   // Singil refusal path cleared the hidden debt
    }

    // ---------------------------------------------------------------- cascade groups (§B.12)
    [Serializable]
    public class CascadeGroup
    {
        public string id;
        public int threshold;
        public string name;
        public string name_en;
        public string icon;
    }
}

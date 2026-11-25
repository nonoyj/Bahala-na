// Bahala Na? — world layer: encounter definitions.
//
// One entry per story NPC. Cells are grid coordinates in the 32x18 plaza grid
// (col 0 = left, row 0 = bottom of the image). Adjust after playtesting.
//
// Portrait path convention:  Resources/Art/NPCs/<Id>
// Map sprite path (optional): Resources/Art/NPCs/<Id>_walk

using System.Collections.Generic;
using UnityEngine;

namespace BahalaNa.World
{
    public class EncounterDef
    {
        public string Id;             // also the Resources filename root
        public string DisplayName;    // shown in the dialogue header
        public string DisplayNameEn;
        public int CellX;
        public int CellY;
        public string InteriorArt;    // Resources path, or null for "talk outside"
        public string[] Lines;        // Tagalog dialogue, in order
        public string[] LinesEn;

        public static List<EncounterDef> All()
        {
            return new List<EncounterDef>
            {
                new EncounterDef
                {
                    Id = "aling_nena",
                    DisplayName = "Aling Nena",
                    DisplayNameEn = "Aling Nena",
                    CellX = 11, CellY = 10,
                    InteriorArt = "Art/Interiors/sari_sari_interior",
                    Lines = new[]
                    {
                        "Aba, umuwi ka pala.",
                        "Tingnan mo ang tindahan ko. Puno ng paninda, walang bumibili.",
                        "May listahan ako dito ng mga utang. Hindi ko naman pinapakita sa iba.",
                        "Basta. Alam mo na ang gusto kong sabihin.",
                    },
                    LinesEn = new[]
                    {
                        "Well now, you're back.",
                        "Look at my store. Full of stock, no one buying.",
                        "I keep a list of debts here. I don't show it to anyone.",
                        "Anyway. You know what I'm saying.",
                    },
                },
                new EncounterDef
                {
                    Id = "kapitan",
                    DisplayName = "Kapitan",
                    DisplayNameEn = "The Captain",
                    CellX = 21, CellY = 8,
                    InteriorArt = "Art/Interiors/barangay_hall_interior",
                    Lines = new[]
                    {
                        "Bumalik ka. Mabuti.",
                        "Maraming kailangan ayusin sa barangay na ito.",
                        "Pero hindi lahat ng problema ay nasa papel.",
                        "Kaya kong ipasok ang proyekto mo… kung magtutulungan tayo.",
                    },
                    LinesEn = new[]
                    {
                        "You're back. Good.",
                        "There's a lot that needs fixing in this barangay.",
                        "But not every problem is on paper.",
                        "I can push your project through… if we help each other.",
                    },
                },
                new EncounterDef
                {
                    Id = "maam_lourdes",
                    DisplayName = "Ma'am Lourdes",
                    DisplayNameEn = "Ma'am Lourdes",
                    CellX = 27, CellY = 7,
                    InteriorArt = null,      // chapel steps — talk outdoors
                    Lines = new[]
                    {
                        "Alam mo ba kung ilang resibo ang naitago ko?",
                        "Tatlong taon. Bawat isa may pirma.",
                        "Hindi ko pa ito nailalabas. Takot ako.",
                        "Kung may tutulong sa akin… baka kaya.",
                    },
                    LinesEn = new[]
                    {
                        "Do you know how many receipts I've kept?",
                        "Three years. Every one signed.",
                        "I haven't brought them out. I'm afraid.",
                        "If someone would help me… maybe it's possible.",
                    },
                },
                new EncounterDef
                {
                    Id = "kuya_jun",
                    DisplayName = "Kuya Jun",
                    DisplayNameEn = "Kuya Jun",
                    CellX = 5, CellY = 7,
                    InteriorArt = null,      // by the covered court
                    Lines = new[]
                    {
                        "Uy! Umuwi ka!",
                        "Alam mo, may kilala ako. Pulis, taga-labas.",
                        "Isang tawag lang, tapos na ang problema mo.",
                        "Basta ako ang kasama mo, ha?",
                    },
                    LinesEn = new[]
                    {
                        "Hey! You're back!",
                        "You know, I know someone. A cop, from outside.",
                        "One call and your problem's finished.",
                        "Just make sure I'm with you, yeah?",
                    },
                },
            };
        }
    }
}

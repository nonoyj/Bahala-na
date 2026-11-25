// Bahala Na? — world layer: per-chapter world state.
//
// OPTION A: the world IS the game. Each chapter defines which NPCs exist, where
// they stand, and how the plaza is lit. Ch1 has 4 cards and 4 NPCs, so one card
// per NPC interaction. When the deck is exhausted the chapter advances and the
// cast changes.
//
// This file is pure data + lookup. No Unity calls, no side effects.

using System.Collections.Generic;

namespace BahalaNa.World
{
    public class ChapterPlan
    {
        public int Chapter;
        public string BackgroundPath;      // Resources path
        public float AmbientDim;           // 0 = noon, 0.6 = night
        public UnityEngine.Color Tint;     // colour grade over the plaza
        public List<PlacedNpc> Cast;
        public List<AmbientNpc> Ambient;

        public class PlacedNpc
        {
            public string Id;              // matches EncounterDef.Id
            public int CellX;
            public int CellY;
        }

        public class AmbientNpc
        {
            public string Id;              // sprite in Resources/Art/NPCs/<id>_walk
            public int CellX;
            public int CellY;
            public int WalkRange;          // tiles it wanders, 0 = stands still
        }

        // ------------------------------------------------------------------ data
        public static ChapterPlan For(int chapter)
        {
            switch (chapter)
            {
                case 2: return Ch2();
                case 3: return Ch3();
                case 4: return Ch4();
                default: return Ch1();
            }
        }

        // Ch1 — Kabataan. Afternoon. Kuya Jun and Aling Nena only.
        private static ChapterPlan Ch1()
        {
            return new ChapterPlan
            {
                Chapter = 1,
                BackgroundPath = "Art/Backgrounds/barangay_plaza_16x9",
                AmbientDim = 0.0f,
                Tint = UnityEngine.Color.white,
                Cast = new List<PlacedNpc>
                {
                    new PlacedNpc { Id = "kuya_jun",  CellX = 7,  CellY = 9 },
                    new PlacedNpc { Id = "aling_nena", CellX = 11, CellY = 8 },
                },
                Ambient = new List<AmbientNpc>
                {
                    new AmbientNpc { Id = "amb_kid_a", CellX = 6,  CellY = 6, WalkRange = 2 },
                    new AmbientNpc { Id = "amb_kid_b", CellX = 14, CellY = 6, WalkRange = 2 },
                },
            };
        }

        // Ch2 — Trabaho. Dusk. Ma'am Lourdes arrives, a classmate appears.
        private static ChapterPlan Ch2()
        {
            return new ChapterPlan
            {
                Chapter = 2,
                BackgroundPath = "Art/Backgrounds/barangay_plaza_16x9",
                AmbientDim = 0.22f,
                Tint = new UnityEngine.Color(1f, 0.92f, 0.82f, 1f),   // warm dusk
                Cast = new List<PlacedNpc>
                {
                    new PlacedNpc { Id = "kuya_jun",     CellX = 7,  CellY = 9 },
                    new PlacedNpc { Id = "aling_nena",   CellX = 11, CellY = 8 },
                    new PlacedNpc { Id = "maam_lourdes", CellX = 26, CellY = 7 },
                },
                Ambient = new List<AmbientNpc>
                {
                    new AmbientNpc { Id = "amb_student", CellX = 16, CellY = 5, WalkRange = 3 },
                    new AmbientNpc { Id = "amb_kid_a",   CellX = 6,  CellY = 6, WalkRange = 2 },
                },
            };
        }

        // Ch3 — Tungkulin. Night. The Kapitan is present. Nena's store is shut.
        private static ChapterPlan Ch3()
        {
            return new ChapterPlan
            {
                Chapter = 3,
                BackgroundPath = "Art/Backgrounds/barangay_plaza_16x9",
                AmbientDim = 0.45f,
                Tint = new UnityEngine.Color(0.78f, 0.82f, 1f, 1f),   // cool night
                Cast = new List<PlacedNpc>
                {
                    new PlacedNpc { Id = "kapitan",      CellX = 19, CellY = 8 },
                    new PlacedNpc { Id = "maam_lourdes", CellX = 26, CellY = 7 },
                    new PlacedNpc { Id = "kuya_jun",     CellX = 7,  CellY = 9 },
                },
                Ambient = new List<AmbientNpc>
                {
                    new AmbientNpc { Id = "amb_guard", CellX = 21, CellY = 5, WalkRange = 2 },
                },
            };
        }

        // Ch4 — Baha. Everything happens at once. Everybody is present.
        private static ChapterPlan Ch4()
        {
            return new ChapterPlan
            {
                Chapter = 4,
                BackgroundPath = "Art/Backgrounds/barangay_plaza_16x9",
                AmbientDim = 0.58f,
                Tint = new UnityEngine.Color(0.72f, 0.78f, 0.86f, 1f),  // grey rain
                Cast = new List<PlacedNpc>
                {
                    new PlacedNpc { Id = "kapitan",      CellX = 19, CellY = 8 },
                    new PlacedNpc { Id = "maam_lourdes", CellX = 16, CellY = 7 },
                    new PlacedNpc { Id = "kuya_jun",     CellX = 22, CellY = 7 },
                },
                Ambient = new List<AmbientNpc>(),
            };
        }

        public bool HasNpc(string id)
        {
            if (Cast == null) return false;
            foreach (var n in Cast) if (n.Id == id) return true;
            return false;
        }

        public PlacedNpc Find(string id)
        {
            if (Cast == null) return null;
            foreach (var n in Cast) if (n.Id == id) return n;
            return null;
        }
    }
}

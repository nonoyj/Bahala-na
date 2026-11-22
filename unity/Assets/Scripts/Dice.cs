// Bahala Na? — dice (System.Random; no Unity dependency so rules stay testable).
using System;

namespace BahalaNa
{
    public static class Dice
    {
        private static readonly Random Rng = new Random();

        public static int Roll(int sides)
        {
            if (sides < 1) sides = 1;
            return Rng.Next(1, sides + 1);
        }

        public static int Roll20() => Roll(20);
        public static int Roll4() => Roll(4);
        public static int Roll6() => Roll(6);

        public static int Roll20Seeded(int seed) => new Random(seed).Next(1, 21);
    }
}

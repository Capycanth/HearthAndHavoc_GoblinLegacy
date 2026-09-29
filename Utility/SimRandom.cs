using System;

namespace HearthAndHavoc_GoblinLegacy.Utility
{
    public static class SimRandom
    {
        public static Random Instance { get; private set; }

        public static void Initialize(int seed)
        {
            Instance = new Random(seed);
        }
    }
}

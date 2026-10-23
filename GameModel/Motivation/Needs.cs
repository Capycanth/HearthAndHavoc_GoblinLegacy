using HearthAndHavoc_GoblinLegacy.Enumeration;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Motivation
{
    // The needs one creature has (Milestone 7, decision 8). In Phase 1 every creature has the three body needs.
    public class Needs
    {
        private readonly List<Need> needs;

        public Needs(params Need[] needs)
        {
            this.needs = new List<Need>(needs);
        }

        // Returns the creature's need of this type. Every creature is built with the needs its chains score, so a
        // missing one is a bug and throws rather than returning null.
        public Need Get(NeedType type)
        {
            foreach (Need need in needs)
            {
                if (need.Type == type) return need;
            }

            throw new KeyNotFoundException($"This creature has no {type} need.");
        }
    }
}

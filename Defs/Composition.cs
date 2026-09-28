using HearthAndHavoc_GoblinLegacy.Enumeration;
using System.Collections.Generic;
using System.IO;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class Composition
    {
        public float Water { get; init; }
        public Dictionary<Substance, float> Substances { get; init; } = new();

        public void Validate(string ownerKey)
        {
            if (Water < 0)
            {
                throw new InvalidDataException($"Def '{ownerKey}' has a composition with Water {Water}; it must be 0 or more.");
            }

            float total = Water;
            if (Substances != null)
            {
                foreach (KeyValuePair<Substance, float> entry in Substances)
                {
                    if (entry.Value < 0)
                    {
                        throw new InvalidDataException($"Def '{ownerKey}' has a composition with {entry.Key} {entry.Value}; it must be 0 or more.");
                    }

                    total += entry.Value;
                }
            }

            if (total > 100)
            {
                throw new InvalidDataException($"Def '{ownerKey}' has a composition totalling {total} g per 100 g; it must be at most 100.");
            }
        }
    }
}

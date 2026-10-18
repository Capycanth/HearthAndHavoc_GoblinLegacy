using HearthAndHavoc_GoblinLegacy.Enumeration;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    // An animal species: its body (BodyDef) plus what only animals have, such as diet, perception and breeding.
    public class FaunaDef : BodyDef
    {
        private static readonly string[] DietTags = ["herbivore", "omnivore", "carnivore"];

        private readonly List<FaunaDef> predators = new();

        public int PerceptionRange { get; init; }
        public ActivityCycle ActivityCycle { get; init; }
        public int LifespanDays { get; init; }
        public int GestationDays { get; init; }
        public int LitterSize { get; init; }
        public List<string> DietKeys { get; init; }

        [JsonIgnore]
        public List<Def> Diet { get; private set; }

        [JsonIgnore]
        public IReadOnlyList<FaunaDef> Predators => predators;

        public override void Resolve()
        {
            base.Resolve();

            Diet = new List<Def>();
            if (DietKeys != null)
            {
                foreach (string key in DietKeys)
                {
                    Def food = DefRegistry.Get<Def>(key);
                    if (food is not (FloraDef or FaunaDef or CoverDef))
                    {
                        throw new InvalidDataException($"Def '{Key}' has '{key}' in its diet; diet entries must be flora, fauna or cover.");
                    }

                    Diet.Add(food);

                    if (food is FaunaDef prey)
                    {
                        prey.predators.Add(this);
                    }
                }
            }
        }

        public override void Validate()
        {
            base.Validate();

            if (PerceptionRange <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has PerceptionRange {PerceptionRange}; it must be above 0.");
            }

            if (LifespanDays <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has LifespanDays {LifespanDays}; it must be above 0.");
            }

            float fullRateMaturityDays = GrowthStage.GetFullRateMaturityDays(GrowthStages);
            if (fullRateMaturityDays >= LifespanDays)
            {
                throw new InvalidDataException($"Def '{Key}' reaches its final growth stage after {fullRateMaturityDays} days at full rate; LifespanDays {LifespanDays} must be later than that.");
            }

            if (GestationDays <= 0 || LitterSize < 1)
            {
                throw new InvalidDataException($"Def '{Key}' needs GestationDays above 0 and LitterSize of at least 1.");
            }

            if (Diet.Count == 0)
            {
                throw new InvalidDataException($"Def '{Key}' has an empty diet.");
            }

            int dietTagCount = 0;
            foreach (string tag in DietTags)
            {
                if (Tags.Contains(tag))
                {
                    dietTagCount++;
                }
            }

            if (dietTagCount != 1)
            {
                throw new InvalidDataException($"Def '{Key}' must have exactly one of the tags herbivore, omnivore or carnivore.");
            }
        }
    }
}

using HearthAndHavoc_GoblinLegacy.Enumeration;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class FaunaDef : Def
    {
        private static readonly string[] DietTags = ["herbivore", "omnivore", "carnivore"];

        private readonly List<FaunaDef> predators = new();

        public byte Size { get; init; }
        public float BodyMassKg { get; init; }
        public float BasalKcalPerHour { get; init; }
        public float SleepMultiplier { get; init; }
        public float WalkMultiplier { get; init; }
        public float RunMultiplier { get; init; }
        public float StomachCapacityKg { get; init; }
        public float MaxFatKg { get; init; }
        public float WaterLitersPerDay { get; init; }
        public float WalkSpeed { get; init; }
        public float RunSpeed { get; init; }
        public int PerceptionRange { get; init; }
        public ActivityCycle ActivityCycle { get; init; }
        public int SleepHoursPerDay { get; init; }
        public int LifespanDays { get; init; }
        public int MaturityDays { get; init; }
        public int GestationDays { get; init; }
        public int LitterSize { get; init; }
        public List<string> DietKeys { get; init; }
        public List<CarcassYield> CarcassYield { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public List<Def> Diet { get; private set; }

        [JsonIgnore]
        public IReadOnlyList<FaunaDef> Predators => predators;

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        public override void Resolve()
        {
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

            if (CarcassYield != null)
            {
                foreach (CarcassYield yield in CarcassYield)
                {
                    yield.Item = DefRegistry.Get<ItemDef>(yield.ItemKey);
                }
            }
        }

        public override void Validate()
        {
            if (Size < 1 || Size > 100)
            {
                throw new InvalidDataException($"Def '{Key}' has Size {Size}; it must be from 1 to 100.");
            }

            if (BodyMassKg <= 0 || BasalKcalPerHour <= 0 || StomachCapacityKg <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs BodyMassKg, BasalKcalPerHour and StomachCapacityKg above 0.");
            }

            if (SleepMultiplier <= 0 || WalkMultiplier <= 0 || RunMultiplier <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs sleep, walk and run multipliers above 0.");
            }

            if (MaxFatKg < 0 || WaterLitersPerDay < 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs MaxFatKg and WaterLitersPerDay of 0 or more.");
            }

            if (WalkSpeed <= 0 || RunSpeed < WalkSpeed)
            {
                throw new InvalidDataException($"Def '{Key}' has WalkSpeed {WalkSpeed} and RunSpeed {RunSpeed}; walk must be above 0 and run at least walk.");
            }

            if (PerceptionRange <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has PerceptionRange {PerceptionRange}; it must be above 0.");
            }

            if (SleepHoursPerDay < 0 || SleepHoursPerDay > 24)
            {
                throw new InvalidDataException($"Def '{Key}' has SleepHoursPerDay {SleepHoursPerDay}; it must be from 0 to 24.");
            }

            if (LifespanDays <= 0 || MaturityDays < 0 || MaturityDays >= LifespanDays)
            {
                throw new InvalidDataException($"Def '{Key}' has LifespanDays {LifespanDays} and MaturityDays {MaturityDays}; lifespan must be above 0 and maturity below it.");
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

            if (CarcassYield != null)
            {
                foreach (CarcassYield yield in CarcassYield)
                {
                    if (yield.Amount <= 0)
                    {
                        throw new InvalidDataException($"Def '{Key}' yields {yield.Amount} of '{yield.ItemKey}'; carcass amounts must be above 0.");
                    }
                }
            }

            if (Tint == null || Tint.Length != 3)
            {
                throw new InvalidDataException($"Def '{Key}' needs a tint of exactly 3 values.");
            }

            foreach (int channel in Tint)
            {
                if (channel < 0 || channel > 255)
                {
                    throw new InvalidDataException($"Def '{Key}' has tint value {channel}; each must be from 0 to 255.");
                }
            }
        }
    }
}

using HearthAndHavoc_GoblinLegacy.Enumeration;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    // The data every living creature's body needs: growth, metabolism, movement, sleep, digestion and what its
    // carcass yields. FaunaDef and KremlitDef build on it, so animals and Kremlits share one Metabolism.
    public abstract class BodyDef : Def
    {
        public List<FaunaGrowthStage> GrowthStages { get; init; }
        public float BasalKcalPerHour { get; init; }
        public float SleepMultiplier { get; init; }
        public float WalkMultiplier { get; init; }
        public float RunMultiplier { get; init; }
        public float StomachCapacityKg { get; init; }
        public float EatGramsPerMinute { get; init; }
        public float MaxFatKg { get; init; }
        public float WaterLitersPerDay { get; init; }
        public float WalkSpeed { get; init; }
        public float RunSpeed { get; init; }
        public int MaxRunningMinutes { get; init; }
        public int SleepHoursPerDay { get; init; }
        public List<Enzyme> Enzymes { get; init; }
        public List<CarcassYield> CarcassYield { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        [JsonIgnore]
        public float FinalWeightKg => GrowthStages[^1].StageMaturityWeightKg;

        public override void Resolve()
        {
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
            GrowthStage.Validate(GrowthStages, Key);

            foreach (FaunaGrowthStage stage in GrowthStages)
            {
                if (stage.Size < 1 || stage.Size > 100)
                {
                    throw new InvalidDataException($"Def '{Key}' growth stage '{stage.Name}' has Size {stage.Size}; it must be from 1 to 100.");
                }
            }

            if (BasalKcalPerHour <= 0 || StomachCapacityKg <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs BasalKcalPerHour and StomachCapacityKg above 0.");
            }

            if (EatGramsPerMinute <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has EatGramsPerMinute {EatGramsPerMinute}; it must be above 0.");
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

            if (MaxRunningMinutes <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has MaxRunningMinutes {MaxRunningMinutes}; it must be above 0.");
            }

            if (SleepHoursPerDay < 1 || SleepHoursPerDay > 24)
            {
                throw new InvalidDataException($"Def '{Key}' has SleepHoursPerDay {SleepHoursPerDay}; it must be from 1 to 24.");
            }

            if (Enzymes == null || Enzymes.Count == 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs at least one enzyme.");
            }

            if (new HashSet<Enzyme>(Enzymes).Count != Enzymes.Count)
            {
                throw new InvalidDataException($"Def '{Key}' lists the same enzyme more than once.");
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

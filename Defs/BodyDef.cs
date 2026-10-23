using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel;
using Microsoft.Xna.Framework;
using System;
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
        public float DrinkLitersPerMinute { get; init; }
        public float WalkSpeed { get; init; }
        public float RunSpeed { get; init; }
        public int MaxRunningMinutes { get; init; }
        public int SleepHoursPerDay { get; init; }
        public ActivityCycle ActivityCycle { get; init; }
        public List<Enzyme> Enzymes { get; init; }
        public List<CarcassYield> CarcassYield { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        [JsonIgnore]
        public float FinalWeightKg => GrowthStages[^1].StageMaturityWeightKg;

        // How many separate sleeps the day's rest is split into: two halves for crepuscular species, one for the rest.
        [JsonIgnore]
        public int RestPeriodsPerDay => ActivityCycle == ActivityCycle.CREPUSCULAR ? 2 : 1;

        // Whether a minute of the day falls in the species' rest window (Milestone 7, decision 10). The window is
        // SleepHoursPerDay long, centred on midnight for diurnal species and on noon for nocturnal ones; crepuscular
        // species get two halves, centred on noon and midnight. A minute is rest time when it is strictly closer
        // than half a rest period to the nearest centre.
        public bool IsRestTime(int minuteOfDay)
        {
            // Crepuscular centres repeat every 12 hours, the others every 24.
            int cycleMinutes = SimClock.MinutesPerDay / RestPeriodsPerDay;
            // Shifting nocturnal time by 12 hours puts their noon centre at 0, like everyone else's midnight.
            int shiftMinutes = ActivityCycle == ActivityCycle.NOCTURNAL ? SimClock.MinutesPerDay / 2 : 0;

            // Minutes since the last centre, then the distance to whichever centre is nearer, before or after.
            int minutesPastCentre = (minuteOfDay + shiftMinutes) % cycleMinutes;
            int minutesFromCentre = Math.Min(minutesPastCentre, cycleMinutes - minutesPastCentre);

            float halfPeriodMinutes = SleepHoursPerDay * SimClock.MinutesPerHour / RestPeriodsPerDay / 2f;
            return minutesFromCentre < halfPeriodMinutes;
        }

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

            if (MaxFatKg <= 0 || WaterLitersPerDay <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs MaxFatKg and WaterLitersPerDay above 0.");
            }

            if (WalkSpeed <= 0 || RunSpeed < WalkSpeed)
            {
                throw new InvalidDataException($"Def '{Key}' has WalkSpeed {WalkSpeed} and RunSpeed {RunSpeed}; walk must be above 0 and run at least walk.");
            }

            if (DrinkLitersPerMinute <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has DrinkLitersPerMinute {DrinkLitersPerMinute}; it must be above 0.");
            }

            if (MaxRunningMinutes <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has MaxRunningMinutes {MaxRunningMinutes}; it must be above 0.");
            }

            if (SleepHoursPerDay < 1 || SleepHoursPerDay > 23)
            {
                throw new InvalidDataException($"Def '{Key}' has SleepHoursPerDay {SleepHoursPerDay}; it must be from 1 to 23.");
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

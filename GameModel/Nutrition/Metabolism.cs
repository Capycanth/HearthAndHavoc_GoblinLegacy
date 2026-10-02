using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Nutrition
{
    public class Metabolism
    {
        private const float GramsPerKg = 1000f;
        private const float MaxHealth = 100f;
        private const float HungryStomachFraction = 0.25f;
        private const float ThirstyReserveFraction = 0.5f;
        private const float WaterReserveDays = 3f;
        private const float DigestHours = 6f;
        private const float GrowthKcalPerKg = 3000f;
        private const float FatKcalPerKg = 7700f;
        private const float KleiberExponent = 0.75f;
        private const float StarvationDays = 3f;
        private const float DehydrationDays = 1f;
        private const float RecoveryPerDay = 10f;

        private readonly Animal owner;

        public float StomachKg { get; private set; }
        public float StomachKcal { get; private set; }
        public float StomachWaterLiters { get; private set; }
        public float FatKg { get; private set; }
        public float WaterLiters { get; private set; }
        public float SleepDebtHours { get; private set; }
        public float Health { get; private set; }

        public float StomachCapacityKg => owner.Def.StomachCapacityKg * WeightRatio;
        public float StomachRoomKg => StomachCapacityKg - StomachKg;
        public float MaxFatKg => owner.Def.MaxFatKg * WeightRatio;
        public float WaterCapacityLiters => owner.Def.WaterLitersPerDay * WaterReserveDays * WeightRatio;
        public float BiteGrams => owner.Def.EatGramsPerMinute * WeightRatio;

        public HungerState HungerState
        {
            get
            {
                if (StomachKg >= StomachCapacityKg * HungryStomachFraction) return HungerState.SATISFIED;
                return FatKg <= 0f ? HungerState.STARVING : HungerState.HUNGRY;
            }
        }

        public ThirstState ThirstState
        {
            get
            {
                if (WaterLiters <= 0f) return ThirstState.DEHYDRATED;
                if (WaterLiters < WaterCapacityLiters * ThirstyReserveFraction) return ThirstState.THIRSTY;
                return ThirstState.SATISFIED;
            }
        }

        public TirednessState TirednessState
        {
            get
            {
                int sleepHours = owner.Def.SleepHoursPerDay;
                if (SleepDebtHours >= sleepHours) return TirednessState.EXHAUSTED;
                if (SleepDebtHours >= sleepHours / 2f) return TirednessState.TIRED;
                return TirednessState.RESTED;
            }
        }

        private float WeightRatio => owner.WeightKg / owner.Def.FinalWeightKg;

        public Metabolism(Animal owner)
        {
            this.owner = owner;
            FatKg = MaxFatKg;
            WaterLiters = WaterCapacityLiters;
            SleepDebtHours = 0f;
            Health = MaxHealth;
        }

        // Takes one tick's bite of food. The bite is limited by the species' bite rate and the room left
        // in the stomach. Every gram accepted fills the stomach, including water, inert matter and substances
        // this animal has no enzyme for; only the kcal depend on enzymes. Returns the grams accepted.
        public float Ingest(Composition food, float grams)
        {
            float accepted = MathF.Min(grams, MathF.Min(BiteGrams, StomachRoomKg * GramsPerKg));
            if (accepted <= 0f) return 0f;

            AddToStomach(food, accepted);
            return accepted;
        }

        // Fills the stomach to capacity with one food, with no bite limit. Used for test spawns.
        public void FillStomach(Composition food)
        {
            float grams = StomachRoomKg * GramsPerKg;
            if (grams <= 0f) return;

            AddToStomach(food, grams);
        }

        // Adds kcal only for substances this animal has an enzyme for, but the full mass fills the stomach.
        private void AddToStomach(Composition food, float grams)
        {
            if (food.Substances != null)
            {
                foreach (KeyValuePair<Substance, float> entry in food.Substances)
                {
                    if (!owner.Def.Enzymes.Contains(SubstanceTable.GetEnzyme(entry.Key))) continue;

                    float substanceGrams = grams * entry.Value / 100f;
                    StomachKcal += substanceGrams * SubstanceTable.GetKcalPerGram(entry.Key);
                }
            }

            // 1 g of water is 1 mL, so grams of water / 1000 gives liters.
            StomachWaterLiters += grams * food.Water / 100f / GramsPerKg;
            StomachKg += grams / GramsPerKg;
        }

        // Adds drunk water straight to the water reserve, up to its capacity. Returns the liters accepted.
        public float Drink(float liters)
        {
            float accepted = MathF.Min(liters, WaterCapacityLiters - WaterLiters);
            if (accepted <= 0f) return 0f;

            WaterLiters += accepted;
            return accepted;
        }

        // Runs once per game hour, in the order of Milestone 6, decision 3. The death check is done by Animal.
        public void UpdateHour()
        {
            float releasedKcal = Digest();
            float netKcal = releasedKcal - BurnKcal();
            bool starving = BalanceEnergy(netKcal);
            bool dehydrated = DrainWater();
            UpdateSleep();
            UpdateHealth(starving, dehydrated);
        }

        // Releases up to 1/DigestHours of the stomach's capacity. Kcal and water leave in the same proportion
        // as the kg, so a mixed meal stays consistent. Released water refills the reserve. Returns the kcal.
        private float Digest()
        {
            if (StomachKg <= 0f) return 0f;

            float releasedKg = MathF.Min(StomachKg, StomachCapacityKg / DigestHours);
            float fraction = releasedKg / StomachKg;
            float releasedKcal = StomachKcal * fraction;
            float releasedWaterLiters = StomachWaterLiters * fraction;

            StomachKg -= releasedKg;
            StomachKcal -= releasedKcal;
            StomachWaterLiters -= releasedWaterLiters;
            WaterLiters = MathF.Min(WaterLiters + releasedWaterLiters, WaterCapacityLiters);
            return releasedKcal;
        }

        private float BurnKcal()
        {
            return owner.Def.BasalKcalPerHour * MathF.Pow(WeightRatio, KleiberExponent) * GetActivityMultiplier(owner.Activity);
        }

        // A surplus goes to growth first, then fat. A deficit is paid from fat. Returns true when fat ran out.
        private bool BalanceEnergy(float netKcal)
        {
            if (netKcal >= 0f)
            {
                float maxGrowthKg = owner.Def.GrowthStages[owner.StageIndex].GrowthKgPerDay / SimClock.HoursPerDay;
                float grownKg = owner.Grow(MathF.Min(netKcal / GrowthKcalPerKg, maxGrowthKg));
                float leftoverKcal = netKcal - grownKg * GrowthKcalPerKg;
                FatKg = MathF.Min(FatKg + leftoverKcal / FatKcalPerKg, MaxFatKg);
                return false;
            }

            float fatNeededKg = -netKcal / FatKcalPerKg;
            if (FatKg >= fatNeededKg)
            {
                FatKg -= fatNeededKg;
                return false;
            }

            FatKg = 0f;
            return true;
        }

        // Drains one hour of the daily water need. Returns true when the reserve ran out.
        private bool DrainWater()
        {
            float drainLiters = owner.Def.WaterLitersPerDay * WeightRatio / SimClock.HoursPerDay;
            if (WaterLiters > drainLiters)
            {
                WaterLiters -= drainLiters;
                return false;
            }

            WaterLiters = 0f;
            return true;
        }

        private void UpdateSleep()
        {
            if (owner.Activity != CreatureActivity.SLEEPING)
            {
                SleepDebtHours += 1f;
                return;
            }

            float sleepHours = owner.Def.SleepHoursPerDay;
            float repaidHours = (SimClock.HoursPerDay - sleepHours) / sleepHours;
            SleepDebtHours = MathF.Max(0f, SleepDebtHours - repaidHours);
        }

        private void UpdateHealth(bool starving, bool dehydrated)
        {
            if (starving) Health -= MaxHealth / (StarvationDays * SimClock.HoursPerDay);
            if (dehydrated) Health -= MaxHealth / (DehydrationDays * SimClock.HoursPerDay);
            if (!starving && !dehydrated) Health = MathF.Min(Health + RecoveryPerDay / SimClock.HoursPerDay, MaxHealth);

            Health = MathF.Max(Health, 0f);
        }

        private float GetActivityMultiplier(CreatureActivity activity) => activity switch
        {
            CreatureActivity.SLEEPING => owner.Def.SleepMultiplier,
            CreatureActivity.RESTING => 1f,
            CreatureActivity.WALKING => owner.Def.WalkMultiplier,
            CreatureActivity.RUNNING => owner.Def.RunMultiplier,
            CreatureActivity.CROUCHING => owner.Def.WalkMultiplier,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "Activity has no burn multiplier.")
        };
    }
}

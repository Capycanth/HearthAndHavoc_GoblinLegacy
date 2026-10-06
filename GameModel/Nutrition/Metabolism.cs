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
        private const float WalkStaminaPerMinute = 0.1f;
        private const float RestStaminaPerMinute = 0.25f;
        private const float SleepStaminaPerMinute = 0.25f;
        private const float ExhaustionRestMinutes = 60f;

        private readonly Creature owner;

        public float StomachKg { get; private set; }
        public float StomachKcal { get; private set; }
        public float StomachWaterLiters { get; private set; }
        public float FatKg { get; private set; }
        public float WaterLiters { get; private set; }
        public float SleepDebtHours { get; private set; }
        public float Health { get; private set; }
        public float StaminaMinutes { get; private set; }

        // Exhaustion left, in minutes of rest. Resting or sleeping clears one per minute; walking or crouching clears
        // less, at the same ratio as their stamina recovery.
        private float exhaustionLeft = 0f;

        // While exhausted the animal can't run; stamina refills only when the exhaustion has passed
        // (Milestone 7, decision 18).
        public bool IsExhausted => exhaustionLeft > 0f;

        public float StomachCapacityKg => owner.Body.StomachCapacityKg * WeightRatio;
        public float StomachRoomKg => StomachCapacityKg - StomachKg;
        public float MaxFatKg => owner.Body.MaxFatKg * WeightRatio;
        public float WaterCapacityLiters => owner.Body.WaterLitersPerDay * WaterReserveDays * WeightRatio;
        // Rounded up to whole grams so a bite can always be taken from whole-gram food such as cover.
        public float BiteGrams => MathF.Max(1f, MathF.Ceiling(owner.Body.EatGramsPerMinute * WeightRatio));

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
                int sleepHours = owner.Body.SleepHoursPerDay;
                if (SleepDebtHours >= sleepHours) return TirednessState.EXHAUSTED;
                if (SleepDebtHours >= sleepHours / 2f) return TirednessState.TIRED;
                return TirednessState.RESTED;
            }
        }

        private float WeightRatio => owner.WeightKg / owner.Body.FinalWeightKg;

        public Metabolism(Creature owner)
        {
            this.owner = owner;
            FatKg = MaxFatKg;
            WaterLiters = WaterCapacityLiters;
            SleepDebtHours = 0f;
            Health = MaxHealth;
            StaminaMinutes = owner.Body.MaxRunningMinutes;
        }

        // Runs once per tick (one game minute), after the animal has acted, so it reads the activity of this tick.
        // Running spends one minute of stamina; reaching 0 starts the exhaustion, after which stamina refills in
        // one go. Any other activity recovers stamina at its own rate, and wears exhaustion off at that rate
        // relative to resting. An exhausted animal can't run, so running never reaches the exhaustion countdown.
        public void UpdateStamina()
        {
            if (IsExhausted)
            {
                exhaustionLeft -= GetStaminaRecovery(owner.Activity) / RestStaminaPerMinute;
                if (!IsExhausted) StaminaMinutes = owner.Body.MaxRunningMinutes;
                return;
            }

            if (owner.Activity == CreatureActivity.RUNNING)
            {
                StaminaMinutes = MathF.Max(0f, StaminaMinutes - 1f);
                if (StaminaMinutes <= 0f) exhaustionLeft = ExhaustionRestMinutes;
                return;
            }

            StaminaMinutes = MathF.Min(StaminaMinutes + GetStaminaRecovery(owner.Activity), owner.Body.MaxRunningMinutes);
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
                    if (!owner.Body.Enzymes.Contains(SubstanceTable.GetEnzyme(entry.Key))) continue;

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

        // Runs once per game hour, in the order of Milestone 6, decision 3. The death check is done by Creature.
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
            return owner.Body.BasalKcalPerHour * MathF.Pow(WeightRatio, KleiberExponent) * GetActivityMultiplier(owner.Activity);
        }

        // A surplus goes to growth first, then fat. A deficit is paid from fat. Returns true when fat ran out.
        private bool BalanceEnergy(float netKcal)
        {
            if (netKcal >= 0f)
            {
                float maxGrowthKg = owner.Body.GrowthStages[owner.StageIndex].GrowthKgPerDay / SimClock.HoursPerDay;
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
            float drainLiters = owner.Body.WaterLitersPerDay * WeightRatio / SimClock.HoursPerDay;
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

            float sleepHours = owner.Body.SleepHoursPerDay;
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
            CreatureActivity.SLEEPING => owner.Body.SleepMultiplier,
            CreatureActivity.RESTING => 1f,
            CreatureActivity.WALKING => owner.Body.WalkMultiplier,
            CreatureActivity.RUNNING => owner.Body.RunMultiplier,
            CreatureActivity.CROUCHING => owner.Body.WalkMultiplier,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "Activity has no burn multiplier.")
        };

        private static float GetStaminaRecovery(CreatureActivity activity) => activity switch
        {
            CreatureActivity.SLEEPING => SleepStaminaPerMinute,
            CreatureActivity.RESTING => RestStaminaPerMinute,
            CreatureActivity.WALKING => WalkStaminaPerMinute,
            CreatureActivity.CROUCHING => WalkStaminaPerMinute,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "Activity has no stamina recovery.")
        };
    }
}

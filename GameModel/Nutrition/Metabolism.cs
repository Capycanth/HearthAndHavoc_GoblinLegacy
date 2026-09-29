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
        public float WaterCapacityLiters => owner.Def.WaterLitersPerDay * WeightRatio;
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

            if (food.Substances != null)
            {
                foreach (KeyValuePair<Substance, float> entry in food.Substances)
                {
                    if (!owner.Def.Enzymes.Contains(SubstanceTable.GetEnzyme(entry.Key))) continue;

                    float substanceGrams = accepted * entry.Value / 100f;
                    StomachKcal += substanceGrams * SubstanceTable.GetKcalPerGram(entry.Key);
                }
            }

            // 1 g of water is 1 mL, so grams of water / 1000 gives liters.
            StomachWaterLiters += accepted * food.Water / 100f / GramsPerKg;
            StomachKg += accepted / GramsPerKg;
            return accepted;
        }

        // Adds drunk water straight to the water reserve, up to its capacity. Returns the liters accepted.
        public float Drink(float liters)
        {
            float accepted = MathF.Min(liters, WaterCapacityLiters - WaterLiters);
            if (accepted <= 0f) return 0f;

            WaterLiters += accepted;
            return accepted;
        }
    }
}

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class FloraDef : Def
    {
        public bool BlocksMovement { get; init; }
        public int[] Tint { get; init; }
        public List<GrowthStage> GrowthStages { get; init; }
        public float MaxFoliageGrams { get; init; }
        public float FoliageRegrowGramsPerDay { get; init; }
        public Composition FoliageComposition { get; init; }
        public string FruitItemKey { get; init; }
        public int FruitMaxCount { get; init; }
        public int FruitRegrowDays { get; init; }
        public float SpreadChance { get; init; }
        public int SpreadRadius { get; init; }
        public int LifespanDays { get; init; }

        [JsonIgnore]
        public ItemDef FruitItem { get; private set; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        [JsonIgnore]
        public float FullRateMaturityDays => GrowthStage.GetFullRateMaturityDays(GrowthStages);

        public override void Resolve()
        {
            if (FruitItemKey != null)
            {
                FruitItem = DefRegistry.Get<ItemDef>(FruitItemKey);
            }
        }

        public override void Validate()
        {
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

            GrowthStage.Validate(GrowthStages, Key);

            if (MaxFoliageGrams < 0 || FoliageRegrowGramsPerDay < 0)
            {
                throw new InvalidDataException($"Def '{Key}' has negative foliage values; they must be 0 or more.");
            }

            if (MaxFoliageGrams > 0 && FoliageComposition == null)
            {
                throw new InvalidDataException($"Def '{Key}' has edible foliage, so it needs a foliage composition.");
            }

            FoliageComposition?.Validate(Key);

            if (FruitItem != null && (FruitMaxCount <= 0 || FruitRegrowDays <= 0))
            {
                throw new InvalidDataException($"Def '{Key}' has a fruit item, so FruitMaxCount and FruitRegrowDays must be above 0.");
            }

            if (SpreadChance < 0 || SpreadChance > 1)
            {
                throw new InvalidDataException($"Def '{Key}' has SpreadChance {SpreadChance}; it must be from 0 to 1.");
            }

            if (SpreadRadius < 0)
            {
                throw new InvalidDataException($"Def '{Key}' has SpreadRadius {SpreadRadius}; it must be 0 or more.");
            }

            if (LifespanDays <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has LifespanDays {LifespanDays}; it must be above 0.");
            }

            if (FullRateMaturityDays >= LifespanDays)
            {
                throw new InvalidDataException($"Def '{Key}' reaches its final growth stage after {FullRateMaturityDays} days at full rate; LifespanDays {LifespanDays} must be later than that.");
            }
        }
    }
}

using HearthAndHavoc_GoblinLegacy.Enumeration;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class FloraDef : Def
    {
        public int[] Tint { get; init; }
        public List<FloraGrowthStage> GrowthStages { get; init; }
        public float MaxFoliageGrams { get; init; }
        public float FoliageRegrowGramsPerDay { get; init; }
        public Composition FoliageComposition { get; init; }
        public string FruitItemKey { get; init; }
        public int FruitMaxCount { get; init; }
        public int FruitRegrowDays { get; init; }
        public float SpreadChance { get; init; }
        public int SpreadRadius { get; init; }
        public int LifespanDays { get; init; }
        public Dictionary<Season, float> SeasonGrowth { get; init; }
        public Dictionary<Season, float> SeasonFruiting { get; init; }
        public List<List<string>> HostFloraTags { get; init; }
        public int HostRadius { get; init; }

        [JsonIgnore]
        public ItemDef FruitItem { get; private set; }

        [JsonIgnore]
        public HashSet<FloraDef> Hosts { get; private set; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        [JsonIgnore]
        public float FullRateMaturityDays => GrowthStage.GetFullRateMaturityDays(GrowthStages);

        [JsonIgnore]
        public float FinalWeightKg => GrowthStages[^1].StageMaturityWeightKg;

        public override void Resolve()
        {
            if (FruitItemKey != null)
            {
                FruitItem = DefRegistry.Get<ItemDef>(FruitItemKey);
            }

            if (HostFloraTags != null)
            {
                Hosts = DefHelper.MatchTags<FloraDef>(HostFloraTags, Key);
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

            DefHelper.ValidateSeasonTable(SeasonGrowth, nameof(SeasonGrowth), Key);

            if (FruitItem != null)
            {
                DefHelper.ValidateSeasonTable(SeasonFruiting, nameof(SeasonFruiting), Key);
            }
            else if (SeasonFruiting != null)
            {
                throw new InvalidDataException($"Def '{Key}' has SeasonFruiting but no fruit item.");
            }

            if (Hosts != null && HostRadius < 1)
            {
                throw new InvalidDataException($"Def '{Key}' has host tags, so HostRadius must be at least 1.");
            }

            if (Hosts == null && HostRadius != 0)
            {
                throw new InvalidDataException($"Def '{Key}' has HostRadius {HostRadius} but no host tags.");
            }
        }
    }
}

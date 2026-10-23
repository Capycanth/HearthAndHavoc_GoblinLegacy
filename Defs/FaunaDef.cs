using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    // An animal species: its body (BodyDef) plus what only animals have, such as diet, perception and breeding.
    public class FaunaDef : BodyDef
    {
        private static readonly string[] DietTags = ["herbivore", "omnivore", "carnivore"];

        public int PerceptionRange { get; init; }
        public int LifespanDays { get; init; }
        public int GestationDays { get; init; }
        public int LitterSize { get; init; }
        public List<string> DietKeys { get; init; }
        // Optional: species that never attack (deer, rabbits) leave it out.
        public string NaturalWeaponKey { get; init; }

        [JsonIgnore]
        public WeaponDef NaturalWeapon { get; private set; }

        [JsonIgnore]
        public List<Def> Diet { get; private set; }

        // The diet sorted by what each entry means (Milestone 7, decision 26), ready to hand to the scanners:
        // covers, plants whose foliage is eaten, fruit items, and animals whose meat is eaten.
        [JsonIgnore]
        public HashSet<CoverDef> DietCovers { get; } = new();
        [JsonIgnore]
        public HashSet<FloraDef> DietFoliage { get; } = new();
        [JsonIgnore]
        public HashSet<ItemDef> DietFruits { get; } = new();
        [JsonIgnore]
        public HashSet<FaunaDef> DietFauna { get; } = new();

        // The fauna whose diet includes this species, filled in by their Resolve (Milestone 4, decision 10). A set,
        // so it can be handed straight to a CreatureScanner.
        [JsonIgnore]
        public HashSet<FaunaDef> Predators { get; } = new();

        public override void Resolve()
        {
            base.Resolve();

            if (NaturalWeaponKey != null)
            {
                NaturalWeapon = DefRegistry.Get<WeaponDef>(NaturalWeaponKey);
            }

            Diet = new List<Def>();
            if (DietKeys != null)
            {
                foreach (string key in DietKeys)
                {
                    Def food = DefRegistry.Get<Def>(key);
                    switch (food)
                    {
                        case CoverDef cover:
                            DietCovers.Add(cover);
                            break;
                        case FloraDef flora:
                            DietFoliage.Add(flora);
                            break;
                        case ItemDef fruit:
                            DietFruits.Add(fruit);
                            break;
                        case FaunaDef prey:
                            DietFauna.Add(prey);
                            prey.Predators.Add(this);
                            break;
                        default:
                            throw new InvalidDataException($"Def '{Key}' has '{key}' in its diet; diet entries must be cover, flora (its foliage), a fruit item or fauna.");
                    }

                    Diet.Add(food);
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

            // A flora entry means its foliage, so the plant must have some.
            foreach (FloraDef flora in DietFoliage)
            {
                if (flora.MaxFoliageGrams <= 0)
                {
                    throw new InvalidDataException($"Def '{Key}' eats the foliage of '{flora.Key}', which has none; list its fruit item instead.");
                }
            }

            // An item entry means a fruit, so some plant must grow it, or it could never be found.
            IReadOnlyList<FloraDef> floras = DefRegistry.GetAll<FloraDef>();
            foreach (ItemDef fruit in DietFruits)
            {
                if (!floras.Any(flora => flora.FruitItem == fruit))
                {
                    throw new InvalidDataException($"Def '{Key}' has '{fruit.Key}' in its diet, but no flora grows it as fruit.");
                }
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

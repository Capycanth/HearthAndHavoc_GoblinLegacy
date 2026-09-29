using HearthAndHavoc_GoblinLegacy.Enumeration;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class CoverDef : Def
    {
        public ushort MaxBiomass { get; init; }
        public string TextureKey { get; init; }
        public int[] Tint { get; init; }
        public Composition Composition { get; init; }
        public float RegrowGramsPerDay { get; init; }
        public float SpreadChance { get; init; }
        public Dictionary<Season, float> SeasonGrowth { get; init; }
        public List<string> TerrainKeys { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        [JsonIgnore]
        public HashSet<TerrainDef> Terrains { get; private set; }

        public override void Resolve()
        {
            Terrains = new HashSet<TerrainDef>();
            if (TerrainKeys != null)
            {
                foreach (string key in TerrainKeys)
                {
                    Terrains.Add(DefRegistry.Get<TerrainDef>(key));
                }
            }
        }

        public override void Validate()
        {
            if (Composition == null)
            {
                throw new InvalidDataException($"Def '{Key}' needs a composition, since cover is grazed.");
            }

            Composition.Validate(Key);

            if (MaxBiomass == 0)
            {
                throw new InvalidDataException($"Def '{Key}' has MaxBiomass 0; it must be above 0.");
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

            if (RegrowGramsPerDay <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has RegrowGramsPerDay {RegrowGramsPerDay}; it must be above 0.");
            }

            if (SpreadChance < 0 || SpreadChance > 1)
            {
                throw new InvalidDataException($"Def '{Key}' has SpreadChance {SpreadChance}; it must be from 0 to 1.");
            }

            DefHelper.ValidateSeasonTable(SeasonGrowth, nameof(SeasonGrowth), Key);

            if (Terrains.Count == 0)
            {
                throw new InvalidDataException($"Def '{Key}' needs at least one terrain key it can grow on.");
            }
        }
    }
}

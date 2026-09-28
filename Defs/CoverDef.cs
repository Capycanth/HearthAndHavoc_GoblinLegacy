using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class CoverDef : Def
    {
        public ushort MaxBiomass { get; init; }
        public string TextureKey { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        public override void Validate()
        {
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
        }
    }
}

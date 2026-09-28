using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class TerrainDef : Def
    {
        public bool Passable { get; init; }
        public int MoveCost { get; init; }
        public string TextureKey { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);

        public override void Validate()
        {
            if (MoveCost < 1)
            {
                throw new InvalidDataException($"Def '{Key}' has MoveCost {MoveCost}; it must be at least 1.");
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

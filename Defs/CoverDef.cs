using Microsoft.Xna.Framework;
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
    }
}

using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class BiotaDef : Def
    {
        public bool BlocksMovement { get; init; }
        public byte Size { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);
    }
}

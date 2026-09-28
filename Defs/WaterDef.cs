using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class WaterDef : Def
    {
        public bool Drinkable { get; init; }
        public short Quality { get; init; }
        public int MoveCost { get; init; }
        public int[] Tint { get; init; }

        [JsonIgnore]
        public Color TintColor => new Color(Tint[0], Tint[1], Tint[2]);
    }
}

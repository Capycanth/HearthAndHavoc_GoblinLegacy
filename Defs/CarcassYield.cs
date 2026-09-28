using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class CarcassYield
    {
        public string ItemKey { get; init; }
        public int Amount { get; init; }

        [JsonIgnore]
        public ItemDef Item { get; internal set; }
    }
}

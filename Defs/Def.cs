using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public abstract class Def
    {
        public string Key { get; init; }
        public string Label { get; init; }
        public HashSet<string> Tags { get; init; } = new();

        [JsonIgnore]
        public int Index { get; internal set; } = -1;
    }
}

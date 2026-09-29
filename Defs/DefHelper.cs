using HearthAndHavoc_GoblinLegacy.Enumeration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public static class DefHelper
    {
        private const char NotPrefix = '!';
        private const float MaxSeasonMultiplier = 3f;

        // Each inner list is an AND group; the outer list ORs the groups together.
        // A tag starting with '!' excludes defs with that tag from its group.
        public static HashSet<T> MatchTags<T>(List<List<string>> query, string ownerKey) where T : Def
        {
            if (query == null || query.Count == 0)
            {
                throw new InvalidDataException($"Def '{ownerKey}' has an empty tag query.");
            }

            HashSet<T> matches = new();
            foreach (List<string> group in query)
            {
                if (group == null || group.Count == 0)
                {
                    throw new InvalidDataException($"Def '{ownerKey}' has an empty tag group.");
                }

                HashSet<T> groupMatches = null;
                List<string> excludedTags = new();

                foreach (string entry in group)
                {
                    bool isNot = entry.StartsWith(NotPrefix);
                    string tag = isNot ? entry.Substring(1) : entry;

                    IReadOnlyList<Def> tagged = DefRegistry.GetAllWithTag(tag);
                    if (tagged.Count == 0)
                    {
                        throw new InvalidDataException($"Def '{ownerKey}' uses tag '{tag}', which no def has.");
                    }

                    if (isNot)
                    {
                        excludedTags.Add(tag);
                        continue;
                    }

                    HashSet<T> taggedOfType = tagged.OfType<T>().ToHashSet();
                    if (groupMatches == null) groupMatches = taggedOfType;
                    else groupMatches.IntersectWith(taggedOfType);
                }

                if (groupMatches == null)
                {
                    throw new InvalidDataException($"Def '{ownerKey}' has a tag group with only '{NotPrefix}' tags; each group needs at least one tag to match.");
                }

                foreach (string tag in excludedTags)
                {
                    groupMatches.RemoveWhere(def => def.Tags.Contains(tag));
                }

                if (groupMatches.Count == 0)
                {
                    throw new InvalidDataException($"Def '{ownerKey}' has the tag group [{string.Join(", ", group)}], which matches no {typeof(T).Name}.");
                }

                matches.UnionWith(groupMatches);
            }

            return matches;
        }

        public static void ValidateSeasonTable(Dictionary<Season, float> table, string tableName, string defKey)
        {
            if (table == null)
            {
                throw new InvalidDataException($"Def '{defKey}' needs a {tableName} table.");
            }

            foreach (Season season in Enum.GetValues<Season>())
            {
                if (!table.TryGetValue(season, out float multiplier))
                {
                    throw new InvalidDataException($"Def '{defKey}' {tableName} has no value for {season}.");
                }

                if (multiplier < 0 || multiplier > MaxSeasonMultiplier)
                {
                    throw new InvalidDataException($"Def '{defKey}' {tableName} has {multiplier} for {season}; it must be from 0 to {MaxSeasonMultiplier}.");
                }
            }
        }
    }
}

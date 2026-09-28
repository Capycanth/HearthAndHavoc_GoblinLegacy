using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public static class DefRegistry
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        private static readonly List<Def> DefsByIndex = new();
        private static readonly Dictionary<string, Def> DefsByKey = new();
        private static readonly Dictionary<string, List<Def>> DefsByTag = new();

        public static void Load()
        {
            string defsRoot = Path.Combine(AppContext.BaseDirectory, "Content", "Defs");
            LoadFolder<ItemDef>(Path.Combine(defsRoot, "Items"));
            LoadFolder<BiotaDef>(Path.Combine(defsRoot, "Biota"));
            LoadFolder<TerrainDef>(Path.Combine(defsRoot, "Terrain"));
            LoadFolder<WaterDef>(Path.Combine(defsRoot, "Water"));
            LoadFolder<CoverDef>(Path.Combine(defsRoot, "Cover"));
        }

        public static T Get<T>(string key) where T : Def
        {
            if (!DefsByKey.TryGetValue(key, out Def def))
            {
                throw new KeyNotFoundException($"Def '{key}' not found in registry.");
            }

            if (def is not T typedDef)
            {
                throw new InvalidCastException($"Def '{key}' is a {def.GetType().Name}, not a {typeof(T).Name}.");
            }

            return typedDef;
        }

        public static Def GetByIndex(int index)
        {
            return DefsByIndex[index];
        }

        public static IReadOnlyList<Def> GetAllWithTag(string tag)
        {
            if (DefsByTag.TryGetValue(tag, out List<Def> defs))
            {
                return defs;
            }

            return Array.Empty<Def>();
        }

        private static void LoadFolder<T>(string folder) where T : Def
        {
            foreach (string filePath in Directory.GetFiles(folder, "*.json").OrderBy(path => path))
            {
                List<T> defs;
                try
                {
                    defs = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(filePath), JsonOptions);
                }
                catch (JsonException e)
                {
                    throw new InvalidDataException($"Def file '{filePath}' could not be read: {e.Message}", e);
                }

                if (defs == null)
                {
                    throw new InvalidDataException($"Def file '{filePath}' is empty.");
                }

                foreach (T def in defs)
                {
                    Register(def, filePath);
                }
            }
        }

        private static void Register(Def def, string filePath)
        {
            if (string.IsNullOrWhiteSpace(def.Key))
            {
                throw new InvalidDataException($"A def in '{filePath}' is missing its key.");
            }

            if (string.IsNullOrWhiteSpace(def.Label))
            {
                throw new InvalidDataException($"Def '{def.Key}' in '{filePath}' is missing its label.");
            }

            if (!DefsByKey.TryAdd(def.Key, def))
            {
                throw new InvalidDataException($"Def '{def.Key}' in '{filePath}' uses a key that is already taken.");
            }

            def.Index = DefsByIndex.Count;
            DefsByIndex.Add(def);

            foreach (string tag in def.Tags)
            {
                if (!DefsByTag.TryGetValue(tag, out List<Def> taggedDefs))
                {
                    taggedDefs = new List<Def>();
                    DefsByTag.Add(tag, taggedDefs);
                }

                taggedDefs.Add(def);
            }
        }
    }
}

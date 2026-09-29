using HearthAndHavoc_GoblinLegacy.Utility;
using System.Collections.Generic;
using System.IO;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class GrowthStage
    {
        public string Name { get; init; }
        public float StageMaturityWeightKg { get; init; }
        public float GrowthKgPerDay { get; init; }
        public string TextureKey { get; init; }

        public static int GetStageIndex(IReadOnlyList<GrowthStage> stages, float weightKg)
        {
            int lastStage = stages.Count - 1;
            for (int i = 0; i < lastStage; i++)
            {
                if (weightKg < stages[i].StageMaturityWeightKg) return i;
            }

            return lastStage;
        }

        public static float GetFullRateMaturityDays(IReadOnlyList<GrowthStage> stages)
        {
            float days = 0;
            float startWeightKg = 0;
            for (int i = 0; i < stages.Count - 1; i++)
            {
                days += (stages[i].StageMaturityWeightKg - startWeightKg) / stages[i].GrowthKgPerDay;
                startWeightKg = stages[i].StageMaturityWeightKg;
            }

            return days;
        }

        public static void Validate(IReadOnlyList<GrowthStage> stages, string defKey)
        {
            if (stages == null || stages.Count == 0)
            {
                throw new InvalidDataException($"Def '{defKey}' needs at least one growth stage.");
            }

            float previousWeightKg = 0;
            foreach (GrowthStage stage in stages)
            {
                if (string.IsNullOrWhiteSpace(stage.Name))
                {
                    throw new InvalidDataException($"Def '{defKey}' has a growth stage with no name.");
                }

                if (stage.StageMaturityWeightKg <= previousWeightKg)
                {
                    throw new InvalidDataException($"Def '{defKey}' growth stage '{stage.Name}' has StageMaturityWeightKg {stage.StageMaturityWeightKg}; it must be above 0 and above the previous stage's.");
                }

                if (stage.GrowthKgPerDay <= 0)
                {
                    throw new InvalidDataException($"Def '{defKey}' growth stage '{stage.Name}' has GrowthKgPerDay {stage.GrowthKgPerDay}; it must be above 0.");
                }

                if (string.IsNullOrWhiteSpace(stage.TextureKey) || !ContentLoader.HasTexture(stage.TextureKey))
                {
                    throw new InvalidDataException($"Def '{defKey}' growth stage '{stage.Name}' has TextureKey '{stage.TextureKey}', which isn't a loaded texture.");
                }

                previousWeightKg = stage.StageMaturityWeightKg;
            }
        }
    }
}

using System.IO;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    public class ItemDef : Def
    {
        public float WeightKg { get; init; }
        public int SpoilDays { get; init; }
        public Composition Composition { get; init; }

        public override void Validate()
        {
            Composition?.Validate(Key);

            if (WeightKg <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has WeightKg {WeightKg}; it must be above 0.");
            }

            if (SpoilDays < 0)
            {
                throw new InvalidDataException($"Def '{Key}' has SpoilDays {SpoilDays}; it must be 0 or more.");
            }
        }
    }
}

using System.IO;

namespace HearthAndHavoc_GoblinLegacy.Defs
{
    // How hard and how often something attacks (Milestone 7, decision 23). Natural weapons such as teeth are
    // WeaponDefs that a fauna def points at; crafted weapon items will point at one later.
    public class WeaponDef : Def
    {
        public float Damage { get; init; }
        public float AttacksPerMinute { get; init; }

        public override void Validate()
        {
            if (Damage <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has Damage {Damage}; it must be above 0.");
            }

            if (AttacksPerMinute <= 0)
            {
                throw new InvalidDataException($"Def '{Key}' has AttacksPerMinute {AttacksPerMinute}; it must be above 0.");
            }
        }
    }
}

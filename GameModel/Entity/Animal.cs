using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    // A creature of one fauna species. The body (weight, growth, metabolism, death) lives on Creature; Def is the
    // same object as Body, typed as FaunaDef for the animal-only data such as diet and perception.
    public class Animal : Creature
    {
        public FaunaDef Def { get; }

        public Animal(int id, FaunaDef def, Locale locale, float weightKg)
            : base(id, def, locale, weightKg)
        {
            Def = def;
        }

        protected override ActionChain ChooseChain()
        {
            return new IdleChain();
        }
    }
}

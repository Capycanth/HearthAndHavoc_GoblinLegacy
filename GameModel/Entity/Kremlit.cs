using HearthAndHavoc_GoblinLegacy.GameModel.Items;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Kremlit : Creature
    {
        public KremlitDef Def { get; }
        public List<BaseItem> Inventory { get; set; }
        public Dictionary<NeedType, float> Needs { get; set; }

        public Kremlit(int id, KremlitDef def, Locale locale, float weightKg)
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

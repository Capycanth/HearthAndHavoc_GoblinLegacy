using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Drinks one sip per tick from a water tile the chain has walked next to (Milestone 7, decision 20). Ends with
    // COMPLETE when the water reserve is full, TARGET_LOST when the tile is out of reach, or TARGET_DEPLETED if the
    // tile no longer holds water. Water quality is ignored in Phase 1.
    public class Drink : BaseAction
    {
        private const int Reach = 1;

        private readonly Target _water;

        public Drink(Target water)
        {
            _water = water;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            if (MapUtil.GetRingDistance(creature.Position, _water.Tile) > Reach) return ActionOutcome.TARGET_LOST;
            if (creature.Locale.LocaleMap.GetWater(_water.Tile) == null) return ActionOutcome.TARGET_DEPLETED;

            // The reserve takes only what fits, so less than a full sip accepted means it is now full.
            float sip = creature.Metabolism.SipLiters;
            float accepted = creature.Metabolism.Drink(sip);
            return accepted < sip ? ActionOutcome.COMPLETE : ActionOutcome.RUNNING;
        }
    }
}

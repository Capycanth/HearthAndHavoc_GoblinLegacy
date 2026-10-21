using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Creeps toward a target, crouching to stay hard to notice, until within striking distance.
    public class Stalk : Follow
    {
        private const int StrikeDistance = 5;

        private readonly Creature _target;

        public Stalk(Creature target, int range) : base(target, CreatureActivity.CROUCHING, StrikeDistance, range)
        {
            _target = target;
        }

        // A target that is fleeing, from this stalker or anything else, is running and can't be crept up on, so the
        // stalk ends with TARGET_FLED (Milestone 7, decision 21).
        public override ActionOutcome Perform(Creature creature)
        {
            if (_target.CurrentChain is FleeChain) return Finish(creature, ActionOutcome.TARGET_FLED);
            return base.Perform(creature);
        }
    }
}

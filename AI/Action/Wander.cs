using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Walks to a random nearby point, rests for a random time, and repeats. It never finishes on its own
    // (Milestone 7, decision 11); the chain running it is replaced when something else scores higher.
    public class Wander : BaseAction
    {
        private const int Radius = 16;
        private const int MinRestTicks = 15;
        private const int MaxRestTicks = 60;

        [AllowNull]
        private WalkTo _walk = null;
        private int _restTicksLeft = 0;

        public override ActionOutcome Perform(Creature creature)
        {
            if (_walk == null)
            {
                if (_restTicksLeft > 0)
                {
                    _restTicksLeft--;
                    return ActionOutcome.RUNNING;
                }

                _walk = new WalkTo(MapUtil.GetRandomPointNear(creature.Position, Radius));
            }

            if (_walk.Perform(creature) != ActionOutcome.RUNNING)
            {
                _walk = null;
                _restTicksLeft = SimRandom.Instance.Next(MinRestTicks, MaxRestTicks + 1);
                creature.Activity = Activity.RESTING;
            }

            return ActionOutcome.RUNNING;
        }
    }
}

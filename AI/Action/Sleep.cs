using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Sleeps on the spot until the sleep debt is paid off (Milestone 7, decision 20). Metabolism repays debt at
    // its hourly update for a creature that is asleep at that moment, so sleep lasts whole hours.
    public class Sleep : BaseAction
    {
        public override ActionOutcome Perform(Creature creature)
        {
            if (creature.Metabolism.SleepDebtHours <= 0f)
            {
                creature.Activity = CreatureActivity.RESTING;
                return ActionOutcome.COMPLETE;
            }

            creature.Activity = CreatureActivity.SLEEPING;
            return ActionOutcome.RUNNING;
        }
    }
}

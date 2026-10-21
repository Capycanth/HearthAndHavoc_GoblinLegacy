using HearthAndHavoc_GoblinLegacy.AI.Action;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Chain
{
    // A one-step chain that flees until safe (Milestone 7, decision 27). It isn't scored: noticing a predator forces
    // it (decision 21). It can't be interrupted, but predators noticed while it runs are passed on to its Flee.
    public class FleeChain : ActionChain
    {
        private readonly Flee _flee;

        public override bool IsInterruptible => false;

        public FleeChain(Creature firstThreat, int range)
        {
            _flee = new Flee(firstThreat, range);
        }

        public bool IsKnownThreat(Creature predator) => _flee.IsKnownThreat(predator);

        public void AddThreat(Creature predator) => _flee.AddThreat(predator);

        public override ChainStatus Perform(Creature creature)
        {
            return _flee.Perform(creature) == ActionOutcome.COMPLETE ? ChainStatus.SUCCEEDED : ChainStatus.RUNNING;
        }
    }
}

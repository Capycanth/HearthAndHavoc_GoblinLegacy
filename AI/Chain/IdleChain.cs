using HearthAndHavoc_GoblinLegacy.AI.Action;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Chain
{
    // A one-step chain that wanders. Wander never finishes, so neither does this chain.
    public class IdleChain : ActionChain
    {
        private readonly Wander _wander = new();

        public override ChainStatus Perform(Creature creature)
        {
            _wander.Perform(creature);
            return ChainStatus.RUNNING;
        }
    }
}

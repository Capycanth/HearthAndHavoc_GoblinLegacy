using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Chain
{
    // An ordered set of actions plus the rules for moving between them (Milestone 7, decisions 1 and 4). Each
    // subclass keeps its context (target, attempts and so on) in its own fields, since a chain is created fresh
    // every time it is chosen, and writes its transitions as a switch over its own step enum.
    public abstract class ActionChain
    {
        // Whether the step running now may be cut off by another chain (Milestone 7, decision 5). Most steps can;
        // Flee and Attack can't.
        public virtual bool IsInterruptible => true;

        public abstract ChainStatus Perform(Creature creature);
    }
}

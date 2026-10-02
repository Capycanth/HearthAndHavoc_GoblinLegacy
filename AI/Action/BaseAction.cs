using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    public abstract class BaseAction
    {
        public abstract ActionOutcome Perform(Creature creature);
    }
}

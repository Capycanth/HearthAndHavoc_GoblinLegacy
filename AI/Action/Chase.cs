using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Runs after a target until next to it.
    public class Chase : Follow
    {
        private const int Reach = 1;

        public Chase(Creature target, int range) : base(target, CreatureActivity.RUNNING, Reach, range)
        {
        }
    }
}

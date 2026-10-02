using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Creeps toward a target, crouching to stay hard to notice, until within striking distance.
    public class Stalk : Follow
    {
        private const int StrikeDistance = 5;

        public Stalk(Creature target, int range) : base(target, CreatureActivity.CROUCHING, StrikeDistance, range)
        {
        }
    }
}

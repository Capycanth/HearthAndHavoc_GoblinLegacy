using HearthAndHavoc_GoblinLegacy.Enumeration;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    public class WalkTo : GoTo
    {
        public WalkTo(Point destination) : base(destination, CreatureActivity.WALKING)
        {
        }
    }
}

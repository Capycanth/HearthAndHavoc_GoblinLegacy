using HearthAndHavoc_GoblinLegacy.Enumeration;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    public class RunTo : GoTo
    {
        public RunTo(Point destination) : base(destination, CreatureActivity.RUNNING)
        {
        }
    }
}

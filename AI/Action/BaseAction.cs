using HearthAndHavocGoblinLegacy.GameModel.Entity;
using HearthAndHavocGoblinLegacy.GameModel.Map;

namespace HearthAndHavocGoblinLegacy.AI.Action
{
    public abstract class BaseAction
    {
        public abstract bool Perform(World world, Kremlit kremlit);
    }
}

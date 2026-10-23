using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Nutrition;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Motivation
{
    // How thirsty a creature is: how empty its water reserve is.
    public class ThirstNeed : Need
    {
        private readonly Creature owner;

        public ThirstNeed(Creature owner)
            : base(NeedType.THIRST, BodyUrgencyExponent)
        {
            this.owner = owner;
        }

        protected override float CalculateLevel()
        {
            Metabolism metabolism = owner.Metabolism;
            return 1f - metabolism.WaterLiters / metabolism.WaterCapacityLiters;
        }
    }
}

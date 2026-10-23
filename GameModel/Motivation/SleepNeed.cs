using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Motivation
{
    // How tired a creature is: its sleep debt against a full rest period's sleep. A normal waking stretch takes it
    // from 0 to 1 by the start of the rest window (Milestone 6, decision 11). Debt beyond that stays at 1, through
    // the clamp on Level.
    public class SleepNeed : Need
    {
        private readonly Creature owner;

        public SleepNeed(Creature owner)
            : base(NeedType.SLEEP, BodyUrgencyExponent)
        {
            this.owner = owner;
        }

        protected override float CalculateLevel()
        {
            return owner.Metabolism.SleepDebtHours / owner.Body.SleepHoursPerDay;
        }
    }
}

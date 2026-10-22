using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using System;

namespace HearthAndHavoc_GoblinLegacy.AI
{
    // How likely a prey animal is to notice a predator on one detection check (Milestone 7, decision 21). The chance
    // is what the predator is doing times how alert the prey is. All values are tuned in Milestone 9.
    public static class Detection
    {
        // How often, in ticks, a prey animal checks for predators. Each animal is offset by its Id so the checks
        // are spread across ticks. Replaced by the re-score timer in PR 12.
        public const int CheckIntervalTicks = 2;

        public static float GetNoticeChance(Creature predator, Creature prey)
        {
            return GetPredatorFactor(predator.Activity) * GetPreyFactor(prey.Activity);
        }

        // How easy the predator is to notice: a crouching stalker hardly at all, a running one almost surely.
        private static float GetPredatorFactor(CreatureActivity activity) => activity switch
        {
            CreatureActivity.SLEEPING => 0.05f,
            CreatureActivity.CROUCHING => 0.1f,
            CreatureActivity.RESTING => 0.25f,
            CreatureActivity.WALKING => 0.5f,
            CreatureActivity.RUNNING => 0.95f,
            CreatureActivity.ATTACKING => 1f,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "No predator detection factor for this activity.")
        };

        // How alert the prey is: a sleeping animal notices far less.
        private static float GetPreyFactor(CreatureActivity activity) => activity switch
        {
            CreatureActivity.SLEEPING => 0.25f,
            _ => 1f
        };
    }
}

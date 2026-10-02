namespace HearthAndHavoc_GoblinLegacy.Enumeration
{
    // What an action reports from Perform each tick. RUNNING means "not done yet"; every other value means the
    // action has finished and says how, so the chain running it can pick its next step (Milestone 7, decision 2).
    public enum ActionOutcome
    {
        RUNNING,
        ARRIVED,
        NO_PATH,
        TARGET_FOUND,
        TARGET_NOT_FOUND,
        TARGET_REACHED,
        TARGET_LOST,
        TARGET_FLED,
        TARGET_DEAD,
        TARGET_ESCAPED,
        TARGET_DEPLETED,
        COMPLETE,
    }
}

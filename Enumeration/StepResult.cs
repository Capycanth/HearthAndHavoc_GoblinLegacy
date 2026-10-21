namespace HearthAndHavoc_GoblinLegacy.Enumeration
{
    // Why MovingAction.StepAlong stopped stepping this tick.
    public enum StepResult
    {
        // The movement budget doesn't cover the next step yet; it carries over to the next tick.
        OUT_OF_BUDGET,
        // The next tile refused the creature (full or impassable). The budget has been reset to 0.
        BLOCKED,
        // Every step in the path has been taken.
        PATH_DONE,
        // The caller's stop check returned true after a step.
        STOPPED,
    }
}

using HearthAndHavoc_GoblinLegacy.GameModel.Entity;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Looks for one type of target around a seeking creature. A Find action runs one or more scanners and keeps
    // the nearest match (Milestone 7, decision 12).
    public abstract class Scanner
    {
        // Returns the nearest reachable match within range of the seeker, or null when there is none. The seeker
        // gives the position, the Locale to search, and the creature to leave out of its own results.
        public abstract Target FindNearest(Creature seeker, int range);
    }
}

using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Looks for one type of target around a point. A Find action runs one or more scanners and keeps the nearest
    // match (Milestone 7, decision 12).
    public abstract class Scanner
    {
        // Returns the nearest reachable match within range of center, or null when there is none.
        public abstract Target FindNearest(TileMap map, Point center, int range);
    }
}

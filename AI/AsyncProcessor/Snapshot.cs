using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor
{
    public sealed class WorldSnapshot(TileMap localeMap)
    {
        public TileMap LocaleMap { get; private set; } = localeMap;
    }

    public sealed class CreatureSnapshot(Point position)
    {
        public Point Position { get; private set; } = position;
    }
}

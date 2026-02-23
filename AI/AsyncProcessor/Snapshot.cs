using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor
{
    public sealed class WorldSnapshot(MeterTile[,] localeMap)
    {
        public MeterTile[,] LocaleMap { get; private set; } = localeMap;
    }

    public sealed class KremlitSnapshot(Point position)
    {
        public Point Position { get; private set; } = position;
    }
}

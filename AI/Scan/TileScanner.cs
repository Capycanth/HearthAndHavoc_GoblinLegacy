using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using System;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Finds a width x height area in which every tile passes the condition, for example room for a growing animal
    // (1x1) or, later, a spot for a Kremlit building. The target is the area's top-left tile (Milestone 7,
    // decision 14).
    public class TileScanner : TileGridScanner
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Func<Point, bool> _condition;

        public TileScanner(int width, int height, Func<Point, bool> condition)
        {
            _width = width;
            _height = height;
            _condition = condition;
        }

        protected override Target TryMatch(TileMap map, MapChunk chunk, int localX, int localY, Point tile)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (!_condition(new Point(tile.X + x, tile.Y + y))) return null;
                }
            }

            return new Target(TargetKind.TILE, tile);
        }
    }
}

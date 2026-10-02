using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Finds tiles holding one of the given waters.
    public class WaterScanner : TileGridScanner
    {
        private readonly HashSet<WaterDef> _waters;
        [AllowNull]
        private readonly Func<WaterDef, bool> _condition;

        public WaterScanner(HashSet<WaterDef> waters, Func<WaterDef, bool> condition = null)
        {
            _waters = waters;
            _condition = condition;
        }

        protected override Target TryMatch(TileMap map, MapChunk chunk, int localX, int localY, Point tile)
        {
            WaterDef water = chunk.Water[localY, localX];
            if (water == null || !_waters.Contains(water)) return null;
            if (_condition != null && !_condition(water)) return null;

            return new Target(TargetKind.WATER, tile);
        }
    }
}

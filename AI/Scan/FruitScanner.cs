using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Finds plants growing one of the given fruit items that have fruit left, whatever the plant is.
    public class FruitScanner : TileGridScanner
    {
        private readonly HashSet<ItemDef> _fruits;
        [AllowNull]
        private readonly Func<Plant, bool> _condition;

        public FruitScanner(HashSet<ItemDef> fruits, Func<Plant, bool> condition = null)
        {
            _fruits = fruits;
            _condition = condition;
        }

        protected override Target TryMatch(TileMap map, MapChunk chunk, int localX, int localY, Point tile)
        {
            Plant plant = chunk.Plants[localY, localX];
            if (plant == null || plant.Def.FruitItem == null || !_fruits.Contains(plant.Def.FruitItem)) return null;
            if (plant.FruitCount <= 0f) return null;
            if (_condition != null && !_condition(plant)) return null;

            return new Target(TargetKind.FRUIT, tile, plant);
        }
    }
}

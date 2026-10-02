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
    // Finds plants of one of the given flora defs that have foliage left.
    public class FoliageScanner : TileGridScanner
    {
        private readonly HashSet<FloraDef> _floras;
        [AllowNull]
        private readonly Func<Plant, bool> _condition;

        public FoliageScanner(HashSet<FloraDef> floras, Func<Plant, bool> condition = null)
        {
            _floras = floras;
            _condition = condition;
        }

        protected override Target TryMatch(TileMap map, MapChunk chunk, int localX, int localY, Point tile)
        {
            Plant plant = chunk.Plants[localY, localX];
            if (plant == null || !_floras.Contains(plant.Def) || plant.FoliageGrams <= 0f) return null;
            if (_condition != null && !_condition(plant)) return null;

            return new Target(TargetKind.FOLIAGE, tile, plant);
        }
    }
}

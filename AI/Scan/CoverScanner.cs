using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Finds tiles covered by one of the given covers. A tile with no cover left has a null cover, so any
    // cover found still has biomass to eat.
    public class CoverScanner : TileGridScanner
    {
        private readonly HashSet<CoverDef> _covers;
        [AllowNull]
        private readonly Func<CoverDef, bool> _condition;

        public CoverScanner(HashSet<CoverDef> covers, Func<CoverDef, bool> condition = null)
        {
            _covers = covers;
            _condition = condition;
        }

        protected override Target TryMatch(TileMap map, MapChunk chunk, int localX, int localY, Point tile)
        {
            CoverDef cover = chunk.Cover[localY, localX];
            if (cover == null || !_covers.Contains(cover)) return null;
            if (_condition != null && !_condition(cover)) return null;

            return new Target(TargetKind.COVER, tile);
        }
    }
}

using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Finds fresh carcasses of the given fauna defs that still have meat, using the Locale's carcass grid. A plant
    // may have spread onto a carcass's tile since the death, so the stand-tile check still applies.
    public class CarcassScanner : Scanner
    {
        private readonly HashSet<FaunaDef> _faunas;
        [AllowNull]
        private readonly Func<Carcass, bool> _condition;
        private readonly List<Carcass> _candidates = new();

        public CarcassScanner(HashSet<FaunaDef> faunas, Func<Carcass, bool> condition = null)
        {
            _faunas = faunas;
            _condition = condition;
        }

        public override Target FindNearest(Creature seeker, int range)
        {
            Point center = seeker.Position;
            Rectangle area = new Rectangle(center.X - range, center.Y - range, 2 * range + 1, 2 * range + 1);

            _candidates.Clear();
            seeker.Locale.GetCarcassesInArea(area, _candidates);

            Carcass nearest = null;
            int nearestDistance = int.MaxValue;
            foreach (Carcass candidate in _candidates)
            {
                if (!_faunas.Contains(candidate.SourceDef) || candidate.MeatKg <= 0f || candidate.IsRotten) continue;
                if (_condition != null && !_condition(candidate)) continue;
                if (!MapUtil.TryGetStandTile(seeker.Locale.LocaleMap, candidate.Position, center, out _)) continue;

                int distance = MapUtil.GetRingDistance(candidate.Position, center);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest == null ? null : new Target(TargetKind.CARCASS, nearest.Position, nearest);
        }
    }
}

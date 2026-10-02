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
    // Finds live animals of the given fauna defs, using the Locale's creature grid rather than tile data. Removed
    // creatures are no longer in the grid, so they are never found. A creature always stands on a passable tile,
    // so no stand-tile check is needed.
    public class CreatureScanner : Scanner
    {
        private readonly HashSet<FaunaDef> _faunas;
        [AllowNull]
        private readonly Func<Creature, bool> _condition;
        private readonly List<Creature> _candidates = new();

        public CreatureScanner(HashSet<FaunaDef> faunas, Func<Creature, bool> condition = null)
        {
            _faunas = faunas;
            _condition = condition;
        }

        public override Target FindNearest(Creature seeker, int range)
        {
            Point center = seeker.Position;
            Rectangle area = new Rectangle(center.X - range, center.Y - range, 2 * range + 1, 2 * range + 1);

            _candidates.Clear();
            seeker.Locale.GetCreaturesInArea(area, _candidates);

            Creature nearest = null;
            int nearestDistance = int.MaxValue;
            foreach (Creature candidate in _candidates)
            {
                if (candidate == seeker) continue;
                if (candidate is not Animal animal || !_faunas.Contains(animal.Def)) continue;
                if (_condition != null && !_condition(candidate)) continue;

                int distance = MapUtil.GetRingDistance(candidate.Position, center);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest == null ? null : new Target(TargetKind.CREATURE, nearest.Position, nearest);
        }
    }
}

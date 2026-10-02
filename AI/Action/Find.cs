using HearthAndHavoc_GoblinLegacy.AI.Scan;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Looks for a target with the given scanners while walking to a random nearby point (Milestone 7, decision 12).
    // It scans before moving, so a target already in view is found at once, then every few ticks on the way.
    // The nearest match across all scanners is kept in Found.
    public class Find : BaseAction
    {
        private const int Radius = 24;
        private const int ScanEveryTicks = 5;

        private readonly IReadOnlyList<Scanner> _scanners;
        private readonly int _range;
        [AllowNull]
        private WalkTo _walk = null;
        private int _ticksUntilScan = 0;

        [AllowNull]
        public Target Found { get; private set; }

        public Find(IReadOnlyList<Scanner> scanners, int range)
        {
            _scanners = scanners;
            _range = range;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            if (_ticksUntilScan == 0)
            {
                if (Scan(creature)) return Finish(creature, ActionOutcome.TARGET_FOUND);
                _ticksUntilScan = ScanEveryTicks;
            }
            _ticksUntilScan--;

            _walk ??= new WalkTo(MapUtil.GetRandomPointNear(creature.Position, Radius));
            if (_walk.Perform(creature) == ActionOutcome.RUNNING) return ActionOutcome.RUNNING;

            return Finish(creature, Scan(creature) ? ActionOutcome.TARGET_FOUND : ActionOutcome.TARGET_NOT_FOUND);
        }

        private bool Scan(Creature creature)
        {
            Target nearest = null;
            int nearestDistance = int.MaxValue;

            foreach (Scanner scanner in _scanners)
            {
                Target match = scanner.FindNearest(creature, _range);
                if (match == null) continue;

                int distance = MapUtil.GetRingDistance(match.Tile, creature.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = match;
                }
            }

            Found = nearest;
            return nearest != null;
        }

        private static ActionOutcome Finish(Creature creature, ActionOutcome outcome)
        {
            creature.Activity = CreatureActivity.RESTING;
            return outcome;
        }
    }
}

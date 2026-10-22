using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Runs away from known predators in short legs (Milestone 7, decision 22). Each leg is planned with a small A*
    // on the main thread, so a prey animal starts running the tick it notices a threat. It ends with COMPLETE once
    // no known predator has been within the prey's perception range for SafeTicks in a row.
    public class Flee : MovingAction
    {
        private const int LegTiles = 8;
        private const int MaxSearchNodes = 300;
        private const int SafeTicks = 20;

        // Directions to try for a leg, as turns away from straight-away: straight first, then wider and wider.
        private static readonly float[] LegTurns =
        [
            0f,
            MathF.PI / 4f, -MathF.PI / 4f,
            MathF.PI / 2f, -MathF.PI / 2f,
        ];

        // A predator this prey has noticed, and the tile where it was last seen.
        private class Threat
        {
            public Creature Predator { get; }
            public Point LastSeen { get; set; }

            public Threat(Creature predator)
            {
                Predator = predator;
                LastSeen = predator.Position;
            }
        }

        private readonly List<Threat> _threats = new();
        private readonly int _range;
        [AllowNull]
        private Stack<Point> _path = null;
        private int _safeTicksLeft = SafeTicks;

        // range is the prey's perception range: how far away a known predator can still be seen.
        public Flee(Creature firstThreat, int range)
        {
            _range = range;
            AddThreat(firstThreat);
        }

        public bool IsKnownThreat(Creature predator)
        {
            foreach (Threat threat in _threats)
            {
                if (threat.Predator == predator) return true;
            }
            return false;
        }

        // Adds a newly noticed predator. It's in range by definition, so the safe period starts over.
        public void AddThreat(Creature predator)
        {
            if (IsKnownThreat(predator)) return;
            _threats.Add(new Threat(predator));
            _safeTicksLeft = SafeTicks;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            if (!UpdateThreats(creature))
            {
                _safeTicksLeft--;
                if (_safeTicksLeft <= 0)
                {
                    creature.Activity = CreatureActivity.RESTING;
                    return ActionOutcome.COMPLETE;
                }
            }

            if (_path == null || _path.Count == 0) _path = PlanLeg(creature);

            // Cornered: no direction away has a path. Stand still this tick and try again on the next.
            if (_path == null)
            {
                creature.Activity = CreatureActivity.RESTING;
                return ActionOutcome.RUNNING;
            }

            // Run while there's stamina left, then keep going at a walk (Milestone 7, decision 18).
            CreatureActivity mode = creature.CanRun ? CreatureActivity.RUNNING : CreatureActivity.WALKING;
            if (StepAlong(creature, _path, mode) == StepResult.BLOCKED) _path = null;

            return ActionOutcome.RUNNING;
        }

        // Refreshes the last-seen tile of every known predator still within range, and returns whether any was.
        // A removed predator sits at GameObject.RemovedPosition, so it falls out of range on its own.
        private bool UpdateThreats(Creature creature)
        {
            bool anyInRange = false;
            foreach (Threat threat in _threats)
            {
                if (MapUtil.GetRingDistance(creature.Position, threat.Predator.Position) <= _range)
                {
                    threat.LastSeen = threat.Predator.Position;
                    anyInRange = true;
                }
            }

            if (anyInRange) _safeTicksLeft = SafeTicks;
            return anyInRange;
        }

        // Plans the next leg: LegTiles out along the direction away from every last-seen tile, or turned up to 90
        // degrees either side when that way is blocked. Returns null when no direction has a path.
        private Stack<Point> PlanLeg(Creature creature)
        {
            Point start = creature.Position;
            float awayAngle = GetAwayAngle(start);

            foreach (float turn in LegTurns)
            {
                float angle = awayAngle + turn;
                Point destination = new Point(
                    start.X + (int)MathF.Round(MathF.Cos(angle) * LegTiles),
                    start.Y + (int)MathF.Round(MathF.Sin(angle) * LegTiles));

                Stack<Point> path = MapUtil.GetAStarPathQueue(creature.Locale.LocaleMap, start, destination, MaxSearchNodes);
                if (path.Count > 0) return path;
            }

            return null;
        }

        // The angle pointing away from all known predators: the sum of a unit vector away from each last-seen
        // tile, so every predator counts the same however far away it is. If they cancel out (a predator on the
        // prey's own tile, or two on opposite sides), any direction is as good as another, so pick one at random.
        private float GetAwayAngle(Point from)
        {
            Vector2 away = Vector2.Zero;
            foreach (Threat threat in _threats)
            {
                Vector2 offset = (from - threat.LastSeen).ToVector2();
                if (offset != Vector2.Zero) away += Vector2.Normalize(offset);
            }

            if (away.LengthSquared() < 0.0001f) return (float)(SimRandom.Instance.NextDouble() * 2.0 * Math.PI);
            return MathF.Atan2(away.Y, away.X);
        }
    }
}

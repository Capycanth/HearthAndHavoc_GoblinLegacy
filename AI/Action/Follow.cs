using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Moves toward a creature that can move (Milestone 7, decision 15). Every few ticks it checks whether the
    // target is still within perception range and asks the AI thread for a fresh path to where the target is now,
    // walking its old path in the meantime. Ends with TARGET_REACHED within reach of the target, or TARGET_LOST
    // when the target stays out of range for several checks or no path to it exists.
    public abstract class Follow : PathingAction
    {
        private const int RecheckTicks = 10;
        private const int MaxMissedChecks = 3;

        private readonly Creature _target;
        private readonly CreatureActivity _mode;
        private readonly int _reach;
        private readonly int _range;

        [AllowNull]
        private Stack<Point> _path = null;
        [AllowNull]
        private Stack<Point> _newPath = null;
        private Point _pathDestination;
        private float _budget = 0f;
        private int _ticksUntilRecheck = 0;
        private int _missedChecks = 0;

        protected Follow(Creature target, CreatureActivity mode, int reach, int range)
        {
            _target = target;
            _mode = mode;
            _reach = reach;
            _range = range;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            if (IsReached(creature)) return Finish(creature, ActionOutcome.TARGET_REACHED);

            // A chase can't continue once the chaser is too exhausted to run (Milestone 7, decision 18).
            if (_mode == CreatureActivity.RUNNING && !creature.CanRun) return Finish(creature, ActionOutcome.TARGET_LOST);

            // A path requested earlier has come back from the AI thread.
            if (JobHandle.HasValue && !IsActionAwaitingJobHandle())
            {
                Stack<Point> newPath = _newPath;
                _newPath = null;
                if (newPath.Count == 0) return Finish(creature, ActionOutcome.TARGET_LOST);
                if (TrimToCurrentTile(newPath, creature.Position)) _path = newPath;
            }

            if (_ticksUntilRecheck == 0)
            {
                _ticksUntilRecheck = RecheckTicks;
                if (MapUtil.GetRingDistance(creature.Position, _target.Position) > _range)
                {
                    _missedChecks++;
                    if (_missedChecks >= MaxMissedChecks) return Finish(creature, ActionOutcome.TARGET_LOST);
                }
                else
                {
                    _missedChecks = 0;
                    if (!JobHandle.HasValue) RequestPath(creature);
                }
            }
            _ticksUntilRecheck--;

            if (_path == null || _path.Count == 0) return ActionOutcome.RUNNING;

            creature.Activity = _mode;
            _budget += creature.GetMoveSpeed(_mode);

            while (_path.Count > 0)
            {
                Point next = _path.Peek();
                float cost = GetStepCost(creature, next);
                if (_budget < cost) return ActionOutcome.RUNNING;

                // A blocked step waits; the next recheck brings a fresh path anyway.
                if (!creature.Locale.MoveCreature(creature, next))
                {
                    _budget = 0f;
                    return ActionOutcome.RUNNING;
                }

                _budget -= cost;
                _path.Pop();
                if (IsReached(creature)) return Finish(creature, ActionOutcome.TARGET_REACHED);
            }

            return ActionOutcome.RUNNING;
        }

        private bool IsReached(Creature creature)
        {
            return MapUtil.GetRingDistance(creature.Position, _target.Position) <= _reach;
        }

        private void RequestPath(Creature creature)
        {
            // Read the target's position here, on the main thread; the AI thread only reads this copy.
            _pathDestination = _target.Position;
            (WorldSnapshot ws, CreatureSnapshot cs) snapshots = GenerateSnapshots(creature);
            JobHandle = GoblinGame.Processor.Enqueue(snapshots.ws, snapshots.cs, CalculatePath);
        }

        // A* ran from where the creature stood when the path was requested, and the creature may have walked on
        // along its old path since. Drop the steps it has already passed. If its tile is not on the new path at
        // all, the new path is stale and is ignored; the next recheck asks for another.
        private static bool TrimToCurrentTile(Stack<Point> path, Point current)
        {
            if (!path.Contains(current)) return false;
            while (path.Peek() != current) path.Pop();
            return true;
        }

        private static ActionOutcome Finish(Creature creature, ActionOutcome outcome)
        {
            creature.Activity = CreatureActivity.RESTING;
            return outcome;
        }

        protected override void CalculatePath(WorldSnapshot ws, CreatureSnapshot cs)
        {
            _newPath = MapUtil.GetAStarPathQueue(ws.LocaleMap, cs.Position, _pathDestination);
        }
    }
}

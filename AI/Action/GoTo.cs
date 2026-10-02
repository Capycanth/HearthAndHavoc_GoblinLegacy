using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    public abstract class GoTo : PathingAction
    {
        private const int MaxBlockedSteps = 3;
        private const int MaxRepaths = 3;

        [AllowNull]
        private Stack<Point> PathTraversal { get; set; } = null;
        private Point _destination;
        private readonly Activity _mode;
        private float _budget = 0f;
        private int _blockedSteps = 0;
        private int _repaths = 0;

        protected GoTo(Point destination, Activity mode)
        {
            _destination = destination;
            _mode = mode;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            if (IsActionAwaitingJobHandle()) return ActionOutcome.RUNNING;

            if (null == PathTraversal)
            {
                (WorldSnapshot ws, CreatureSnapshot cs) snapshots = GenerateSnapshots(creature);
                JobHandle = GoblinGame.Processor.Enqueue(snapshots.ws, snapshots.cs, CalculatePath);
                return ActionOutcome.RUNNING;
            }

            if (PathTraversal.Count == 0)
            {
                Debug.WriteLine($"GoTo found no path to {_destination}");
                return ActionOutcome.NO_PATH;
            }

            creature.Activity = _mode;
            _budget += creature.GetMoveSpeed(_mode);

            while (PathTraversal.Count > 0)
            {
                Point next = PathTraversal.Peek();
                float cost = GetStepCost(creature, next);
                if (_budget < cost) return ActionOutcome.RUNNING;

                if (!creature.Locale.MoveCreature(creature, next))
                {
                    _budget = 0f;
                    return OnBlocked(creature);
                }

                _budget -= cost;
                PathTraversal.Pop();
                _blockedSteps = 0;
            }

            creature.Activity = Activity.RESTING;
            return ActionOutcome.ARRIVED;
        }

        private ActionOutcome OnBlocked(Creature creature)
        {
            _blockedSteps++;
            if (_blockedSteps < MaxBlockedSteps) return ActionOutcome.RUNNING;

            _blockedSteps = 0;
            if (_repaths == MaxRepaths)
            {
                Debug.WriteLine($"GoTo gave up on {_destination} after {MaxRepaths} repaths");
                creature.Activity = Activity.RESTING;
                return ActionOutcome.NO_PATH;
            }

            _repaths++;
            PathTraversal = null;
            return ActionOutcome.RUNNING;
        }

        protected override (WorldSnapshot ws, CreatureSnapshot cs) GenerateSnapshots(Creature creature)
        {
            return (new WorldSnapshot(creature.Locale.LocaleMap), new CreatureSnapshot(creature.Position));
        }

        protected override void CalculatePath(WorldSnapshot ws, CreatureSnapshot cs)
        {
            PathTraversal = MapUtil.GetAStarPathQueue(ws.LocaleMap, cs.Position, this._destination);
        }
    }
}

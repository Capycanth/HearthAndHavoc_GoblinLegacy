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
        private readonly CreatureActivity _mode;
        private int _blockedSteps = 0;
        private int _repaths = 0;

        protected GoTo(Point destination, CreatureActivity mode)
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

            // A creature that can't run (exhausted) walks the rest of the way instead.
            CreatureActivity mode = _mode == CreatureActivity.RUNNING && !creature.CanRun ? CreatureActivity.WALKING : _mode;
            int stepsLeft = PathTraversal.Count;
            StepResult result = StepAlong(creature, PathTraversal, mode);

            // Only a step actually taken clears the blocked count. A tick spent saving up budget for the next step
            // doesn't, or a slow creature facing a full tile would never reach MaxBlockedSteps and repath.
            if (PathTraversal.Count < stepsLeft) _blockedSteps = 0;

            switch (result)
            {
                case StepResult.BLOCKED:
                    return OnBlocked(creature);
                case StepResult.PATH_DONE:
                    creature.Activity = CreatureActivity.RESTING;
                    return ActionOutcome.ARRIVED;
                default:
                    return ActionOutcome.RUNNING;
            }
        }

        private ActionOutcome OnBlocked(Creature creature)
        {
            _blockedSteps++;
            if (_blockedSteps < MaxBlockedSteps) return ActionOutcome.RUNNING;

            _blockedSteps = 0;
            if (_repaths == MaxRepaths)
            {
                Debug.WriteLine($"GoTo gave up on {_destination} after {MaxRepaths} repaths");
                creature.Activity = CreatureActivity.RESTING;
                return ActionOutcome.NO_PATH;
            }

            _repaths++;
            PathTraversal = null;
            return ActionOutcome.RUNNING;
        }

        protected override void CalculatePath(WorldSnapshot ws, CreatureSnapshot cs)
        {
            PathTraversal = MapUtil.GetAStarPathQueue(ws.LocaleMap, cs.Position, this._destination);
        }
    }
}

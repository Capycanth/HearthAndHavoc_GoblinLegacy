using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    public class GoTo : BaseAction
    {
        private const int MaxBlockedSteps = 3;
        private const int MaxRepaths = 3;

        [AllowNull]
        private Stack<Point> PathTraversal { get; set; } = null;
        private Point _destination;
        private int _blockedSteps = 0;
        private int _repaths = 0;

        public GoTo(Point destination)
        {
            _destination = destination;
        }

        public override bool Perform(Creature creature)
        {
            if (IsActionAwaitingJobHandle()) return false;

            if (null == PathTraversal)
            {
                (WorldSnapshot ws, CreatureSnapshot cs) snapshots = GenerateSnapshots(creature);
                JobHandle = GoblinGame.Processor.Enqueue(snapshots.ws, snapshots.cs, CalculateActionChain);
                return false;
            }

            if (PathTraversal.Count == 0)
            {
                Debug.WriteLine($"GoTo found no path to {_destination}");
                return true;
            }

            if (creature.Locale.MoveCreature(creature, PathTraversal.Peek()))
            {
                PathTraversal.Pop();
                _blockedSteps = 0;
                return PathTraversal.Count == 0;
            }

            _blockedSteps++;
            if (_blockedSteps < MaxBlockedSteps) return false;

            _blockedSteps = 0;
            if (_repaths == MaxRepaths)
            {
                Debug.WriteLine($"GoTo gave up on {_destination} after {MaxRepaths} repaths");
                return true;
            }

            _repaths++;
            PathTraversal = null;
            return false;
        }

        protected override (WorldSnapshot ws, CreatureSnapshot cs) GenerateSnapshots(Creature creature)
        {
            return (new WorldSnapshot(creature.Locale.LocaleMap), new CreatureSnapshot(creature.Position));
        }

        protected override void CalculateActionChain(WorldSnapshot ws, CreatureSnapshot cs)
        {
            PathTraversal = MapUtil.GetAStarPathQueue(ws.LocaleMap, cs.Position, this._destination);
        }
    }
}

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
        [AllowNull]
        private Stack<Point> PathTraversal { get; set; } = null;
        private Point _destination;

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

            creature.Position = PathTraversal.Pop();
            return PathTraversal.Count == 0;
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

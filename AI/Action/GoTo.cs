using HeartAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy;
using HearthAndHavocGoblinLegacy.GameModel.Entity;
using HearthAndHavocGoblinLegacy.GameModel.Map;
using HearthAndHavocGoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavocGoblinLegacy.AI.Action
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

        public override bool Perform(World world, Kremlit kremlit)
        {
            if (IsActionAwaitingJobHandle()) return false;

            if (null == PathTraversal)
            {
                (WorldSnapshot ws, KremlitSnapshot ks) snapshots = GenerateSnapshots(world, kremlit);
                JobHandle = GoblinGame.Processor.Enqueue(snapshots.ws, snapshots.ks, CalculateActionChain);
                return false;
            }

            kremlit.Position = PathTraversal.Pop();
            return PathTraversal.Count == 0;
        }

        protected override (WorldSnapshot ws, KremlitSnapshot ks) GenerateSnapshots(World world, Kremlit kremlit)
        {
            return (new WorldSnapshot(world.GetCurrentLocale().LocaleMap), new KremlitSnapshot(kremlit.Position));
        }

        protected override void CalculateActionChain(WorldSnapshot ws, KremlitSnapshot ks)
        {
            PathTraversal = MapUtil.GetAStarPathQueue(ws.LocaleMap, ks.Position, this._destination);
        }
    }
}

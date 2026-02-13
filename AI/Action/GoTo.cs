using HearthAndHavocGoblinLegacy.GameModel.Entity;
using HearthAndHavocGoblinLegacy.GameModel.Map;
using HearthAndHavocGoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavocGoblinLegacy.AI.Action
{
    public class GoTo : BaseAction
    {
        [AllowNull]
        private Stack<Point> PathTraversal { get; set; } = null;
        private Point _destination;

        private static readonly Point point = new(16, 16);

        public GoTo(Point destination)
        {
            _destination = destination;
        }

        public override bool Perform(World world, Kremlit kremlit)
        {
            if (null == PathTraversal)
            {
                PathTraversal = MapUtil.GetAStarPathQueue(world.GetCurrentLocale().LocaleMap, kremlit.Position, _destination);
            }
            kremlit.Position = PathTraversal.Pop() * point;
            if (PathTraversal.Count == 0)
            {
                Console.WriteLine($"Reached destination {_destination}");
            }
            return PathTraversal.Count == 0;
        }
    }
}

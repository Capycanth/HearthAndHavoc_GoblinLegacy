using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.Utility.AStar
{
    public class Path : IComparable<Path>
    {
        public readonly Point? parent;
        public readonly int distanceTravelled; /*g(x)*/
        public readonly int totalCost; /*f(x)*/
        public Path(Point? parent, int distanceTravelled, int totalCost)
        {
            this.parent = parent;
            this.distanceTravelled = distanceTravelled;
            this.totalCost = totalCost;
        }
        public int CompareTo(Path other) { return this.totalCost.CompareTo(other.totalCost); }
    }
}

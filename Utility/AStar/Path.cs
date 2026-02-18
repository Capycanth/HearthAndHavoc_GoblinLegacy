using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.Utility.AStar
{
    public class Path : IComparable<Path>
    {
        public readonly int parentIndex;
        public readonly int distanceTravelled; /*g(x)*/
        public readonly int totalCost; /*f(x)*/
        public Path(int parentIndex, int distanceTravelled, int totalCost)
        {
            this.parentIndex = parentIndex;
            this.distanceTravelled = distanceTravelled;
            this.totalCost = totalCost;
        }
        public int CompareTo(Path other) { return this.totalCost.CompareTo(other.totalCost); }
    }
}

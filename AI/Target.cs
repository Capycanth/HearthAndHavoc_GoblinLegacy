using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel;
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI
{
    // Something an action can go to and act on: a tile, plus the object on it when there is one. Cover, water and
    // tile targets are tile data, so their Object is null (Milestone 7, decision 13).
    public class Target
    {
        public TargetKind Kind { get; }
        public Point Tile { get; }
        [AllowNull]
        public GameObject Object { get; }

        public Target(TargetKind kind, Point tile, GameObject obj = null)
        {
            Kind = kind;
            Tile = tile;
            Object = obj;
        }
    }
}

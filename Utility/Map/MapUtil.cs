using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility.AStar;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace HearthAndHavoc_GoblinLegacy.Utility.Map
{
    public static class MapUtil
    {
        private static readonly Point point = new Point(16, 16);
        public static List<Point> GetTraversablePoints(MeterTile[,] map, Point currentPoint)
        {
            List<Point> traversablePoints = new(8);
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if (x == 0 && y == 0) continue;

                    if (!map[y,x].Impassible) traversablePoints.Add(new Point(x, y));
                }
            }
            return traversablePoints;
        }

        public static Stack<Point> GetAStarPathQueue(MeterTile[,] map, Point start, Point destination)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Debug.WriteLine($"MapUtil.GetAStarPathQueue called for distance of {GetDistance(new Point(start.X >> 4, start.Y >> 4), destination)}");
            Stack<Point> result = new MapPathSolver().Graph(map, new Point(start.X >> 4, start.Y >> 4), destination);
            sw.Stop();
            Debug.WriteLine($"MapUtil.GetAStarPathQueue completed in {sw.ElapsedMilliseconds} ms");
            return result;
        }

        private static int GetDistance(Point source, Point destination)
        {
            int dx = Math.Abs(destination.X - source.X);
            int dy = Math.Abs(destination.Y - source.Y);
            int diagonal = Math.Min(dx, dy);
            int orthogonal = dx + dy - 2 * diagonal;
            return diagonal * 7 + orthogonal * 5;
        }
    }
}

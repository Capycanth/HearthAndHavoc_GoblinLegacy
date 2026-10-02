using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility.AStar;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HearthAndHavoc_GoblinLegacy.Utility.Map
{
    public static class MapUtil
    {
        private static readonly Point point = new Point(16, 16);
        public static List<Point> GetTraversablePoints(TileMap map, Point currentPoint)
        {
            List<Point> traversablePoints = new(8);
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if (x == 0 && y == 0) continue;

                    if (map.IsPassable(new Point(currentPoint.X + x, currentPoint.Y + y))) traversablePoints.Add(new Point(x, y));
                }
            }
            return traversablePoints;
        }

        // How many square rings apart two tiles are (Chebyshev distance): diagonal steps count as one.
        public static int GetRingDistance(Point a, Point b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        // A random tile within radius of center on each axis (a square around it), using the shared SimRandom so
        // runs stay reproducible from their seed.
        public static Point GetRandomPointNear(Point center, int radius)
        {
            int x = center.X + SimRandom.Instance.Next(-radius, radius + 1);
            int y = center.Y + SimRandom.Instance.Next(-radius, radius + 1);
            return new Point(x, y);
        }

        // The tile to stand on to reach target: the target itself if it is passable, otherwise its passable
        // neighbour closest to from. Returns false when neither the target nor any neighbour can be stood on.
        public static bool TryGetStandTile(TileMap map, Point target, Point from, out Point standTile)
        {
            standTile = target;
            if (map.IsPassable(target)) return true;

            bool found = false;
            int bestDistance = int.MaxValue;
            foreach (Point offset in GetTraversablePoints(map, target))
            {
                Point neighbour = new Point(target.X + offset.X, target.Y + offset.Y);
                int distance = GetDistance(from, neighbour);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    standTile = neighbour;
                    found = true;
                }
            }

            return found;
        }

        public static Stack<Point> GetAStarPathQueue(TileMap map, Point start, Point destination)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Debug.WriteLine($"MapUtil.GetAStarPathQueue called for distance of {GetDistance(start, destination)}");
            Stack<Point> result = new MapPathSolver().Graph(map, start, destination);
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

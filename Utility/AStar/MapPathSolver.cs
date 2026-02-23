using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.Utility.AStar
{
    public class MapPathSolver : AbstractAStar<Point, Path>
    {
        private const int MAP_WIDTH = 1000;
        private const int MAP_HEIGHT = 1000;
        private const int baseOrthogonalCost = 5;
        private const int baseDiagonalCost = 7;
        public Node? solution;
        private MeterTile[,] meterMap;
        private Point destination;
        private Dictionary<Point, Path> closedList;

        public Stack<Point> Graph(MeterTile[,] meterMap, Point start, Point destination)
        {
            this.meterMap = meterMap;
            this.closedList = [];
            this.destination = destination;
            Graph(new Node(start, new Path(-1, 0, GetDistance(start, this.destination))), new PriorityQueue<Node>(), this.closedList);
            return GetCalculatedPath();
        }

        public int ToIndex(Point position) { return position.Y * MAP_WIDTH + position.X; }
        public Point ToPosition(int index) { return new Point(index % MAP_WIDTH, index / MAP_WIDTH); }

        protected override void AddNeighbours(Node node, PriorityQueue<Node> openList)
        {
            int parentIndex = ToIndex(node.position);
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    if (!(x == 0 && y == 0))
                    {
                        Point newPos = new Point(node.position.X + x, node.position.Y + y);
                        if (newPos.X >= 0 && newPos.X < MAP_WIDTH && newPos.Y >= 0 && newPos.Y < MAP_HEIGHT)
                        {
                            if (!meterMap[newPos.Y, newPos.X].Impassible)
                            {
                                int distanceCost = node.cost.distanceTravelled +
                                    ((x == 0 || y == 0) ? baseOrthogonalCost : baseDiagonalCost);
                                openList.Insert(new Node(newPos, new Path(parentIndex, distanceCost,
                                    distanceCost + GetDistance(newPos, destination))));
                            }
                        }
                    }
        }

        private static int GetDistance(Point source, Point destination)
        {
            int dx = Math.Abs(destination.X - source.X);
            int dy = Math.Abs(destination.Y - source.Y);
            int diagonal = Math.Min(dx, dy);
            int orthogonal = dx + dy - 2 * diagonal;
            return diagonal * baseDiagonalCost + orthogonal * baseOrthogonalCost;
        }
        protected override bool IsDestination(Point position)
        {
            int dx = position.X - destination.X;
            int dy = position.Y - destination.Y;
            bool isSolved = dx <= 1 && dx >= -1 && dy <= 1 && dy >= -1;

            if (isSolved) solution = new Node(position, closedList[position]);
            return isSolved;
        }

        private Stack<Point> GetCalculatedPath()
        {
            if (!solution.HasValue) return [];

            Stack<Point> fastestPath = [];

            Point pos = solution.Value.position;
            Path path = solution.Value.cost;
            fastestPath.Push(pos);
            do
            {
                pos = ToPosition(path.parentIndex);
                path = closedList[pos];
                fastestPath.Push(pos);
            }
            while (path.parentIndex >= 0);

            return fastestPath;
        }
    }
}

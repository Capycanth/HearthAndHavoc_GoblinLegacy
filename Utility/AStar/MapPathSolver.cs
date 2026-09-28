using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.Utility.AStar
{
    public class MapPathSolver : AbstractAStar<Point, Path>
    {
        private const int baseOrthogonalCost = 5;
        private const int baseDiagonalCost = 7;
        private const int MaxSearchNodes = 20000;
        public Node? solution;
        private TileMap tileMap;
        private Point destination;
        private Dictionary<Point, Path> closedList;

        public Stack<Point> Graph(TileMap tileMap, Point start, Point destination)
        {
            this.tileMap = tileMap;
            this.closedList = [];
            this.destination = destination;
            Graph(new Node(start, new Path(null, 0, GetDistance(start, this.destination))), new PriorityQueue<Node>(), this.closedList, MaxSearchNodes);
            return GetCalculatedPath();
        }

        protected override void AddNeighbours(Node node, PriorityQueue<Node> openList)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    if (!(x == 0 && y == 0))
                    {
                        Point newPos = new Point(node.position.X + x, node.position.Y + y);
                        if (tileMap.IsPassable(newPos))
                        {
                            int stepCost = ((x == 0 || y == 0) ? baseOrthogonalCost : baseDiagonalCost) * tileMap.GetMoveCost(newPos);
                            int distanceCost = node.cost.distanceTravelled + stepCost;
                            openList.Insert(new Node(newPos, new Path(node.position, distanceCost,
                                distanceCost + GetDistance(newPos, destination))));
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
            while (path.parent.HasValue)
            {
                pos = path.parent.Value;
                path = closedList[pos];
                fastestPath.Push(pos);
            }

            return fastestPath;
        }
    }
}

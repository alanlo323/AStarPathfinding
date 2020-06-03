using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static AStarPathfinding.Location;

namespace AStarPathfinding
{
    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int F { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public Location Parent { get; set; }
        public LocationType Type { get; set; }
        public LocationStatus Status { get; set; }

        public enum LocationStatus
        {
            NULL,
            SEARCHED,
            PATH,
        }

        public enum LocationType
        {
            SPACE,
            WALL,
            START_POINT,
            END_POINT,
        }
    }

    public class Engine
    {
        public Location[,] Map { get; set; }
        public Size Size { get; set; }

        private IStatesChangeRecall statesChangeRecall;

        public Engine(Size Size, IStatesChangeRecall statesChangeRecall)
        {
            this.Size = Size;
            this.statesChangeRecall = statesChangeRecall;
            Init();
        }

        public interface IStatesChangeRecall
        {
            bool IsEngineRunning { get; set; }
            void OnStatusUpdated();
        }

        public void Init()
        {
            Map = new Location[Size.Width, Size.Height];
            for (int x = 0; x < Map.GetLength(0); x++)
            {
                for (int y = 0; y < Map.GetLength(1); y++)
                {
                    Map[x, y] = new Location()
                    {
                        X = x,
                        Y = y,
                    };
                }
            }
        }

        public void Main()
        {
            statesChangeRecall.IsEngineRunning = true;

            // algorithm

            Location current = null;
            var start = new Location { };
            var target = new Location { };
            var openList = new List<Location>();
            var closedList = new List<Location>();
            int g = 0;

            for (int x = 0; x < Map.GetLength(0); x++)
            {
                for (int y = 0; y < Map.GetLength(1); y++)
                {
                    switch (Map[x, y].Type)
                    {
                        case LocationType.START_POINT:
                            start.X = x;
                            start.Y = y;
                            break;
                        case LocationType.END_POINT:
                            target.X = x;
                            target.Y = y;
                            break;
                        case LocationType.WALL:
                            break;
                        default:
                            break;
                    }
                    Map[x, y].Status = LocationStatus.NULL;
                }
            }

            // start by adding the original position to the open list
            openList.Add(start);

            while (openList.Count > 0)
            {
                // get the square with the lowest F score
                var lowest = openList.Min(l => l.F);
                current = openList.First(l => l.F == lowest);

                // add the current square to the closed list
                closedList.Add(current);

                // show current square on the map
                current.Status = LocationStatus.SEARCHED;
                statesChangeRecall.OnStatusUpdated();
                System.Threading.Thread.Sleep(5);

                // remove it from the open list
                openList.Remove(current);

                // if we added the destination to the closed list, we've found a path
                if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                    break;

                var adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, Map);
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X
                            && l.Y == adjacentSquare.Y) != null)
                        continue;

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                            && l.Y == adjacentSquare.Y) == null)
                    {
                        // compute its score, set the parent
                        adjacentSquare.G = g;
                        adjacentSquare.H = ComputeHScore(adjacentSquare.X, adjacentSquare.Y, target.X, target.Y);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // test if using the current G score makes the adjacent square's F score
                        // lower, if yes update the parent because it means it's a better path
                        if (g + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g;
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = current;
                        }
                    }
                }
            }

            // assume path was found; let's show it
            while (current != null)
            {
                if (Map[current.X, current.Y].Type == LocationType.SPACE)
                {
                    Map[current.X, current.Y].Status = LocationStatus.PATH;
                    statesChangeRecall.OnStatusUpdated();
                }
                current = current.Parent;
                System.Threading.Thread.Sleep(25);
            }

            // end

            statesChangeRecall.IsEngineRunning = false;
        }

        static List<Location> GetWalkableAdjacentSquares(int x, int y, Location[,] map)
        {
            Location Top = (y <= 0) ? null : map[x, y - 1];  // Top
            Location Bottom = (y >= map.GetLength(1) - 1) ? null : map[x, y + 1];  // Bottom
            Location Left = (x <= 0) ? null : map[x - 1, y];  // Left
            Location Right = (x >= map.GetLength(0) - 1) ? null : map[x + 1, y];  // Right

            var proposedLocations = new List<Location>()
            {
                Top,
                Bottom,
                Left,
                Right,
            };

            return proposedLocations.Where(l => IsLocationWalkable(l)).ToList();
        }

        static bool IsLocationWalkable(Location target)
        {
            return target != null && (target.Type == LocationType.SPACE || target.Type == LocationType.END_POINT);
        }

        static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y) + Math.Abs(Math.Abs(targetX) - Math.Abs(targetY));
        }
    }
}

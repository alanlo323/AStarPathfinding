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
        public double F { get; set; }
        public double G { get; set; }
        public double H { get; set; }
        public Location Parent { get; set; }
        public LocationType Type { get; set; }
        public LocationStatus Status { get; set; }

        public bool IsSlopeMove = false;
        public List<Location> SlopePath { get; set; } = new List<Location>();

        public enum LocationStatus
        {
            NULL,
            SEARCHED,
            PATH,
            ERROR,
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
        public bool AllowDiagonalMovement { get; set; } = true;

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

        public void Evolve()
        {
            statesChangeRecall.IsEngineRunning = true;

            // algorithm

            Location current = null;
            Location start = null;
            Location target = null;
            Location error = new Location();
            var openList = new List<Location>();
            var closedList = new List<Location>();

            for (int x = 0; x < Map.GetLength(0); x++)
            {
                for (int y = 0; y < Map.GetLength(1); y++)
                {
                    switch (Map[x, y].Type)
                    {
                        case LocationType.START_POINT:
                            if (start == null)
                            {
                                start = new Location()
                                {
                                    X = x,
                                    Y = y,
                                };
                            }
                            else
                            {
                                start = error;
                            }
                            break;
                        case LocationType.END_POINT:
                            if (target == null)
                            {
                                target = new Location()
                                {
                                    X = x,
                                    Y = y,
                                };
                            }
                            else
                            {
                                target = error;
                            }
                            break;
                        case LocationType.WALL:
                            break;
                        default:
                            break;
                    }
                    Map[x, y].Status = LocationStatus.NULL;
                    Map[x, y].IsSlopeMove = false;
                    Map[x, y].SlopePath = new List<Location>();
                }
            }

            //  if multiple start/targer found, return
            if (start == error || target == error || start == null || target == null)
            {
                statesChangeRecall.IsEngineRunning = false;
                statesChangeRecall.OnStatusUpdated();
                return;
            }

            // start by adding the original position to the open list
            openList.Add(start);
            int g = 0;
            while (openList.Count > 0)
            {
                // get the square with the lowest F score
                var lowest = openList.Min(l => l.F);
                current = openList.First(l => l.F == lowest);

                // add the current square to the closed list
                closedList.Add(current);

                // show current square on the map
                current.Status = LocationStatus.SEARCHED;
                if (current.IsSlopeMove)
                {
                    var lowestSlopeSquare = current.SlopePath.Min(l => l.F);
                    var slopeSquare = current.SlopePath.First(l => l.F == lowestSlopeSquare);
                    slopeSquare.Status = LocationStatus.SEARCHED;
                }
                statesChangeRecall.OnStatusUpdated();
                System.Threading.Thread.Sleep(5);

                // remove it from the open list
                openList.Remove(current);

                // if we added the destination to the closed list, we've found a path
                if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                    break;

                var adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, Map, AllowDiagonalMovement);

                //g++;
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
                        adjacentSquare.G = g + current.G + ComputeHScore(adjacentSquare.X, adjacentSquare.Y, current.X, current.Y);
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
                        if (g + current.G + ComputeHScore(adjacentSquare.X, adjacentSquare.Y, current.X, current.Y) + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g + current.G + ComputeHScore(adjacentSquare.X, adjacentSquare.Y, current.X, current.Y);
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = current;
                        }
                    }
                }
            }

            // Show result
            if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
            {
                //  Path found
                while (current != null)
                {
                    if (Map[current.X, current.Y].Type == LocationType.SPACE)
                    {
                        Map[current.X, current.Y].Status = LocationStatus.PATH;
                        statesChangeRecall.OnStatusUpdated();
                    }
                    current = current.Parent;
                    System.Threading.Thread.Sleep(5);
                }
            }
            else
            {
                //  No path found
                while (closedList.Count != 0)
                {
                    Location l = closedList.First();
                    l.Status = LocationStatus.ERROR;
                    closedList.Remove(l);

                    statesChangeRecall.OnStatusUpdated();
                    System.Threading.Thread.Sleep(1);
                }
            }

            // end

            statesChangeRecall.IsEngineRunning = false;
            statesChangeRecall.OnStatusUpdated();
        }

        static List<Location> GetWalkableAdjacentSquares(int x, int y, Location[,] map, bool allowDiagonal)
        {
            Location Top = (y <= 0) ? null : map[x, y - 1];
            Location Bottom = (y >= map.GetLength(1) - 1) ? null : map[x, y + 1];
            Location Left = (x <= 0) ? null : map[x - 1, y];
            Location Right = (x >= map.GetLength(0) - 1) ? null : map[x + 1, y];

            var proposedLocations = new List<Location>()
            {
                Top,
                Bottom,
                Left,
                Right,
            };

            if (allowDiagonal)
            {
                Location TopLeft = ((y <= 0) || (x <= 0)) ? null : map[x - 1, y - 1];
                Location TopRight = ((y <= 0) || (x >= map.GetLength(0) - 1)) ? null : map[x + 1, y - 1];
                Location BottomLeft = ((y >= map.GetLength(1) - 1) || (x <= 0)) ? null : map[x - 1, y + 1];
                Location BottomRight = ((y >= map.GetLength(1) - 1) || (x >= map.GetLength(0) - 1)) ? null : map[x + 1, y + 1];

                if (TopLeft != null && (IsLocationWalkable(Top) || IsLocationWalkable(Left)))
                {
                    TopLeft.IsSlopeMove = true;

                    if (IsLocationWalkable(Top))
                        TopLeft.SlopePath.Add(Top);
                    if (IsLocationWalkable(Left))
                        TopLeft.SlopePath.Add(Left);

                    proposedLocations.Add(TopLeft);
                }
                if (TopRight != null && (IsLocationWalkable(Top) || IsLocationWalkable(Right)))
                {
                    TopRight.IsSlopeMove = true;

                    if (IsLocationWalkable(Top))
                        TopRight.SlopePath.Add(Top);
                    if (IsLocationWalkable(Right))
                        TopRight.SlopePath.Add(Right);

                    proposedLocations.Add(TopRight);
                }
                if (BottomLeft != null && (IsLocationWalkable(Bottom) || IsLocationWalkable(Left)))
                {
                    BottomLeft.IsSlopeMove = true;

                    if (IsLocationWalkable(Bottom))
                        BottomLeft.SlopePath.Add(Bottom);
                    if (IsLocationWalkable(Left))
                        BottomLeft.SlopePath.Add(Left);

                    proposedLocations.Add(BottomLeft);
                }
                if (BottomRight != null && (IsLocationWalkable(Bottom) || IsLocationWalkable(Right)))
                {
                    BottomRight.IsSlopeMove = true;

                    if (IsLocationWalkable(Bottom))
                        BottomRight.SlopePath.Add(Bottom);
                    if (IsLocationWalkable(Right))
                        BottomRight.SlopePath.Add(Right);

                    proposedLocations.Add(BottomRight);
                }
            }

            return proposedLocations.Where(l => IsLocationWalkable(l)).ToList();
        }

        static bool IsLocationWalkable(Location target)
        {
            return target != null && (target.Type == LocationType.SPACE || target.Type == LocationType.END_POINT);
        }

        static double ComputeHScore(int x, int y, int targetX, int targetY)
        {
            double a = x > targetX ? x - targetX : targetX - x;
            double b = y > targetY ? y - targetY : targetY - y;

            return Math.Sqrt(a * a + b * b);
        }
    }
}

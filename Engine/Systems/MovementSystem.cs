// Engine/Systems/MovementSystem.cs
using OgreBattle.Engine.Entities;
using OgreBattle.Engine.World;

namespace OgreBattle.Engine.Systems
{
    public class MovementSystem
    {
        private static readonly (int X, int Y)[] Directions =
        {
            (1, 0),
            (0, 1),
            (-1, 0),
            (0, -1)
        };

        public void UpdateArmyPosition(Army army, Map map, float deltaTime)
        {
            if (army.IsAtDestination)
            {
                army.MovementProgress = 0;
                return;
            }

            army.MovementProgress += deltaTime * army.MoveSpeed;
            while (army.MovementProgress >= 1f && !army.IsAtDestination)
            {
                if (!TryGetNextStep(army, map, army.DestinationX, army.DestinationY, out var nextX, out var nextY))
                {
                    army.DestinationX = army.X;
                    army.DestinationY = army.Y;
                    army.MovementProgress = 0;
                    return;
                }

                army.X = nextX;
                army.Y = nextY;
                army.MovementProgress -= 1f;
            }
        }

        public bool CanReach(Map map, int startX, int startY, int destinationX, int destinationY)
        {
            if (startX == destinationX && startY == destinationY)
                return IsPassable(map, startX, startY);

            return TryGetNextStep(startX, startY, map, destinationX, destinationY, out _, out _);
        }

        private static bool TryGetNextStep(
            Army army,
            Map map,
            int destinationX,
            int destinationY,
            out int nextX,
            out int nextY)
        {
            return TryGetNextStep(army.X, army.Y, map, destinationX, destinationY, out nextX, out nextY);
        }

        private static bool TryGetNextStep(
            int startX,
            int startY,
            Map map,
            int destinationX,
            int destinationY,
            out int nextX,
            out int nextY)
        {
            nextX = startX;
            nextY = startY;

            if (!IsPassable(map, startX, startY) || !IsPassable(map, destinationX, destinationY))
                return false;

            var previousX = new int[map.Width, map.Height];
            var previousY = new int[map.Width, map.Height];
            var visited = new bool[map.Width, map.Height];
            for (var x = 0; x < map.Width; x++)
                for (var y = 0; y < map.Height; y++)
                    previousX[x, y] = -1;

            var queue = new Queue<(int X, int Y)>();
            queue.Enqueue((startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.X == destinationX && current.Y == destinationY)
                    break;

                foreach (var direction in Directions)
                {
                    var x = current.X + direction.X;
                    var y = current.Y + direction.Y;
                    if (x < 0 || y < 0 || x >= map.Width || y >= map.Height || visited[x, y] || !IsPassable(map, x, y))
                        continue;

                    visited[x, y] = true;
                    previousX[x, y] = current.X;
                    previousY[x, y] = current.Y;
                    queue.Enqueue((x, y));
                }
            }

            if (!visited[destinationX, destinationY] || (startX == destinationX && startY == destinationY))
                return false;

            var pathX = destinationX;
            var pathY = destinationY;
            while (previousX[pathX, pathY] != startX || previousY[pathX, pathY] != startY)
            {
                var parentX = previousX[pathX, pathY];
                var parentY = previousY[pathX, pathY];
                if (parentX < 0)
                    return false;

                pathX = parentX;
                pathY = parentY;
            }

            nextX = pathX;
            nextY = pathY;
            return true;
        }

        private static bool IsPassable(Map map, int x, int y)
        {
            var tile = map.GetTile(x, y);
            return tile != null && tile.Terrain != TerrainType.Mountain && tile.Terrain != TerrainType.Water;
        }
    }
}

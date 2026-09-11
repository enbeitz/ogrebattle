// Engine/World/Map.cs
using System.Collections.Generic;

namespace OgreBattle.Engine.World
{
    public class Map
    {
        public int Width { get; }
        public int Height { get; }

        private readonly Tile[,] _tiles;
        private readonly List<City> _cities = new List<City>();
        public IReadOnlyList<City> Cities => _cities;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new Tile[width, height];
        }

        public void SetTile(int x, int y, Tile tile)
        {
            _tiles[x, y] = tile;
            if (tile.City != null && !_cities.Contains(tile.City))
                _cities.Add(tile.City);
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;

            return _tiles[x, y];
        }
    }
}

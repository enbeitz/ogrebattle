// Engine/World/Tile.cs
namespace OgreBattle.Engine.World
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public TerrainType Terrain { get; set; }
        public City City { get; set; }

        public Tile(int x, int y, TerrainType terrain)
        {
            X = x;
            Y = y;
            Terrain = terrain;
        }

        public bool HasCity => City != null;
    }
}

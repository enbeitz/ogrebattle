// Engine/Systems/MovementSystem.cs
using OgreBattle.Engine.Entities;
using OgreBattle.Engine.World;

namespace OgreBattle.Engine.Systems
{
    public class MovementSystem
    {
        public void UpdateArmyPosition(Army army, Map map, float deltaTime)
        {
            if (army.IsAtDestination)
                return;

            var dx = army.DestinationX - army.X;
            var dy = army.DestinationY - army.Y;

            if (dx != 0)
                army.X += dx > 0 ? 1 : -1;
            else if (dy != 0)
                army.Y += dy > 0 ? 1 : -1;

            var tile = map.GetTile(army.X, army.Y);
            // Terrain modifiers could be applied here later.
        }
    }
}

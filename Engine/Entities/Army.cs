// Engine/Entities/Army.cs
using System.Collections.Generic;

namespace OgreBattle.Engine.Entities
{
    public class Army
    {
        public int OwnerId { get; set; }
        public List<Unit> Units { get; } = new List<Unit>();

        public int X { get; set; }
        public int Y { get; set; }

        public int DestinationX { get; set; }
        public int DestinationY { get; set; }

        public float MoveSpeed { get; set; } = 1f;

        public bool IsAtDestination => X == DestinationX && Y == DestinationY;

        public void AddUnit(Unit unit)
        {
            Units.Add(unit);
        }
    }
}

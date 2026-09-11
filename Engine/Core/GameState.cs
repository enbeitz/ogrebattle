// Engine/Core/GameState.cs
using OgreBattle.Engine.World;
using OgreBattle.Engine.Entities;
using System.Collections.Generic;

namespace OgreBattle.Engine.Core
{
    public class GameState
    {
        public Map WorldMap { get; set; }
        public List<Army> Armies { get; } = new List<Army>();
        public Time Time { get; } = new Time();

        public void AddArmy(Army army)
        {
            Armies.Add(army);
        }
    }
}

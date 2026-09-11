// Engine/Systems/ProgressionSystem.cs
using OgreBattle.Engine.Entities;

namespace OgreBattle.Engine.Systems
{
    public class ProgressionSystem
    {
        public void AwardExperience(Unit unit, int amount)
        {
            // Stub: XP, levels, promotions can be added later.
        }

        public void HandlePostBattle(Army winner)
        {
            foreach (var unit in winner.Units)
                if (unit.IsAlive)
                    AwardExperience(unit, 10);
        }
    }
}

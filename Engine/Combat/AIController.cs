// Engine/Combat/AIController.cs
using OgreBattle.Engine.Entities;
using System.Collections.Generic;

namespace OgreBattle.Engine.Combat
{
    public class AIController
    {
        public Unit ChooseAttacker(List<Unit> units)
        {
            foreach (var unit in units)
                if (unit.IsAlive)
                    return unit;

            return null;
        }

        public Unit ChooseTarget(List<Unit> enemies)
        {
            foreach (var unit in enemies)
                if (unit.IsAlive)
                    return unit;

            return null;
        }
    }
}

// Engine/Systems/EncounterSystem.cs
using System.Collections.Generic;
using OgreBattle.Engine.Entities;
using OgreBattle.Engine.Combat;

namespace OgreBattle.Engine.Systems
{
    public class EncounterSystem
    {
        public BattleManager CheckForEncounter(List<Army> armies)
        {
            for (int i = 0; i < armies.Count; i++)
            {
                for (int j = i + 1; j < armies.Count; j++)
                {
                    var a = armies[i];
                    var b = armies[j];

                    if (a.X == b.X && a.Y == b.Y)
                        return new BattleManager(a.Units, b.Units);
                }
            }

            return null;
        }
    }
}

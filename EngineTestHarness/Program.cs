using OgreBattle.Engine.Core;
using OgreBattle.Engine.World;
using OgreBattle.Engine.Entities;
using OgreBattle.Engine.Systems;
using OgreBattle.Engine.Combat;
using OgreBattle.Engine.Data;
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Ogre Battle Engine Test Harness ===");

        // Load data
        var loader = new DataLoader();
        var units = loader.LoadUnitDefinitions("Engine/Data/UnitDefinitions.json");

        // Build map
        var map = new Map(10, 10);
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                map.SetTile(x, y, new Tile(x, y, TerrainType.Plain));

        // Create armies
        var armyA = new Army { OwnerId = 1, X = 0, Y = 0, DestinationX = 5, DestinationY = 5 };
        armyA.AddUnit(new Unit("A1", "Knight") { HP = 120, MaxHP = 120, STR = 18, AGI = 10, INT = 6 });

        var armyB = new Army { OwnerId = 2, X = 5, Y = 5, DestinationX = 0, DestinationY = 0 };
        armyB.AddUnit(new Unit("B1", "Wizard") { HP = 80, MaxHP = 80, STR = 6, AGI = 8, INT = 20 });

        var game = new GameState { WorldMap = map };
        game.AddArmy(armyA);
        game.AddArmy(armyB);

        var movement = new MovementSystem();
        var encounters = new EncounterSystem();

        // Simulation loop
        for (int tick = 0; tick < 20; tick++)
        {
            Console.WriteLine($"Tick {tick}");

            movement.UpdateArmyPosition(armyA, map, 1f);
            movement.UpdateArmyPosition(armyB, map, 1f);

            Console.WriteLine($"Army A: ({armyA.X}, {armyA.Y})");
            Console.WriteLine($"Army B: ({armyB.X}, {armyB.Y})");

            var battle = encounters.CheckForEncounter(game.Armies);
            if (battle != null)
            {
                Console.WriteLine("Encounter detected! Starting battle...");
                RunBattle(battle);
                break;
            }
        }

        Console.WriteLine("Simulation complete.");
    }

    static void RunBattle(BattleManager battle)
    {
        while (!battle.IsFinished)
        {
            battle.Step();
        }

        Console.WriteLine("Battle finished.");
    }
}

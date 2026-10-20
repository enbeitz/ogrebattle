using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OgreBattle.Engine.Combat;
using OgreBattle.Engine.Core;
using OgreBattle.Engine.Data;
using OgreBattle.Engine.Entities;
using OgreBattle.Engine.Systems;
using OgreBattle.Engine.World;

internal static class Program
{
    private const int MapWidth = 40;
    private const int MapHeight = 28;
    private const int ViewportWidth = 20;
    private const int ViewportHeight = 12;
    private static readonly TimeSpan BattleActionInterval = TimeSpan.FromSeconds(0.55);
    private static readonly TimeSpan EnemyThinkInterval = TimeSpan.FromSeconds(2.5);

    private static int Main(string[] args)
    {
        if (args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            PrintControls();
            return 0;
        }

        RunGame();
        return 0;
    }

    private static void RunGame()
    {
        var definitions = new DataLoader().LoadUnitDefinitions(ResolveDataFile("UnitDefinitions.json"));
        var map = CreateMap();
        var game = new GameState { WorldMap = map };

        var vanguard = CreateArmy("Vanguard", 1, 4, 10, definitions, "Knight", "Wizard");
        var scouts = CreateArmy("Scouts", 1, 4, 12, definitions, "Knight");
        var imperialGuard = CreateArmy("Imperial Guard", 2, 34, 15, definitions, "Knight", "Wizard");
        var raiders = CreateArmy("Raiders", 2, 30, 11, definitions, "Knight");
        game.AddArmy(vanguard);
        game.AddArmy(scouts);
        game.AddArmy(imperialGuard);
        game.AddArmy(raiders);

        var markers = new Dictionary<Army, string>
        {
            [vanguard] = "P1",
            [scouts] = "P2",
            [imperialGuard] = "E1",
            [raiders] = "E2"
        };
        var playerArmies = new List<Army> { vanguard, scouts };
        var movement = new MovementSystem();
        var encounters = new EncounterSystem();
        var selectedArmyIndex = 0;
        var selectedArmy = playerArmies[selectedArmyIndex];
        var cameraX = 0;
        var cameraY = 5;
        var cursorX = selectedArmy.X;
        var cursorY = selectedArmy.Y;
        var paused = false;
        var gameOver = false;
        var status = "Capture the enemy capital at CC.";
        var gameOverMessage = string.Empty;
        var enemyThinkTimer = TimeSpan.Zero;
        var battleTimer = TimeSpan.Zero;
        BattleManager? activeBattle = null;
        Army? battleArmyA = null;
        Army? battleArmyB = null;

        Console.CursorVisible = false;
        try
        {
            var clock = Stopwatch.StartNew();
            while (!gameOver)
            {
                var elapsed = clock.Elapsed;
                clock.Restart();
                var deltaTime = Math.Min(elapsed.TotalSeconds, 0.25);

                while (!gameOver && Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    HandleInput(
                        key,
                        game,
                        playerArmies,
                        movement,
                        ref selectedArmyIndex,
                        ref selectedArmy,
                        ref cameraX,
                        ref cameraY,
                        ref cursorX,
                        ref cursorY,
                        ref paused,
                        ref status,
                        ref gameOver);
                }

                if (!paused && !gameOver)
                {
                    if (activeBattle == null)
                    {
                        foreach (var army in game.Armies)
                            movement.UpdateArmyPosition(army, map, (float)deltaTime);

                        CaptureCities(game);
                        if (map.Cities.First(city => city.Name == "Enemy Capital").OwnerId == 1)
                        {
                            gameOver = true;
                            gameOverMessage = "Victory! The enemy capital has fallen.";
                        }
                        else if (map.Cities.First(city => city.Name == "Home").OwnerId == 2)
                        {
                            gameOver = true;
                            gameOverMessage = "Defeat! Your home has been captured.";
                        }

                        enemyThinkTimer += TimeSpan.FromSeconds(deltaTime);
                        if (!gameOver && enemyThinkTimer >= EnemyThinkInterval)
                        {
                            enemyThinkTimer = TimeSpan.Zero;
                            IssueEnemyOrders(game, playerArmies, movement);
                        }

                        if (!gameOver)
                            TryStartEncounter(game, encounters, ref activeBattle, ref battleArmyA, ref battleArmyB, ref status);
                    }
                    else
                    {
                        battleTimer += TimeSpan.FromSeconds(deltaTime);
                        if (battleTimer >= BattleActionInterval)
                        {
                            battleTimer -= BattleActionInterval;
                            activeBattle.Step();
                            if (activeBattle.IsFinished)
                            {
                                var losingArmy = battleArmyA!.Units.Any(unit => unit.IsAlive) ? battleArmyB! : battleArmyA;
                                game.Armies.Remove(losingArmy);
                                markers.Remove(losingArmy);
                                playerArmies.Remove(losingArmy);
                                status = $"{losingArmy.Name} has been defeated.";
                                if (ReferenceEquals(selectedArmy, losingArmy) && playerArmies.Count > 0)
                                {
                                    selectedArmyIndex = Math.Min(selectedArmyIndex, playerArmies.Count - 1);
                                    selectedArmy = playerArmies[selectedArmyIndex];
                                    CenterCamera(selectedArmy, ref cameraX, ref cameraY);
                                    cursorX = selectedArmy.X;
                                    cursorY = selectedArmy.Y;
                                }
                                activeBattle = null;
                                battleArmyA = null;
                                battleArmyB = null;
                                battleTimer = TimeSpan.Zero;

                                if (playerArmies.Count == 0)
                                {
                                    gameOver = true;
                                    gameOverMessage = "Defeat! Your army has been destroyed.";
                                }
                                else if (!game.Armies.Any(army => army.OwnerId == 2))
                                {
                                    gameOver = true;
                                    gameOverMessage = "Victory! The enemy army has been destroyed.";
                                }
                            }
                        }
                    }
                }

                Render(game, markers, selectedArmy, cameraX, cameraY, cursorX, cursorY, paused, activeBattle, status);
                if (!gameOver)
                    Thread.Sleep(33);
            }

            if (gameOverMessage.Length > 0)
            {
                Render(game, markers, selectedArmy, cameraX, cameraY, cursorX, cursorY, paused, activeBattle, gameOverMessage);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(intercept: true);
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private static void HandleInput(
        ConsoleKey key,
        GameState game,
        List<Army> playerArmies,
        MovementSystem movement,
        ref int selectedArmyIndex,
        ref Army selectedArmy,
        ref int cameraX,
        ref int cameraY,
        ref int cursorX,
        ref int cursorY,
        ref bool paused,
        ref string status,
        ref bool gameOver)
    {
        switch (key)
        {
            case ConsoleKey.Q:
            case ConsoleKey.Escape:
                gameOver = true;
                status = "Game ended.";
                return;
            case ConsoleKey.Spacebar:
                paused = !paused;
                status = paused ? "Paused." : "Game resumed.";
                return;
            case ConsoleKey.Tab:
                if (playerArmies.Count == 0)
                    return;
                selectedArmyIndex = (selectedArmyIndex + 1) % playerArmies.Count;
                selectedArmy = playerArmies[selectedArmyIndex];
                CenterCamera(selectedArmy, ref cameraX, ref cameraY);
                cursorX = selectedArmy.X;
                cursorY = selectedArmy.Y;
                return;
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                SelectArmy(0, playerArmies, ref selectedArmyIndex, ref selectedArmy, ref cameraX, ref cameraY, ref cursorX, ref cursorY);
                return;
            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                SelectArmy(1, playerArmies, ref selectedArmyIndex, ref selectedArmy, ref cameraX, ref cameraY, ref cursorX, ref cursorY);
                return;
            case ConsoleKey.W:
                PanCamera(0, -1, ref cameraX, ref cameraY, ref cursorX, ref cursorY);
                return;
            case ConsoleKey.A:
                PanCamera(-1, 0, ref cameraX, ref cameraY, ref cursorX, ref cursorY);
                return;
            case ConsoleKey.S:
                PanCamera(0, 1, ref cameraX, ref cameraY, ref cursorX, ref cursorY);
                return;
            case ConsoleKey.D:
                PanCamera(1, 0, ref cameraX, ref cameraY, ref cursorX, ref cursorY);
                return;
            case ConsoleKey.UpArrow:
                cursorY = Math.Max(0, cursorY - 1);
                return;
            case ConsoleKey.DownArrow:
                cursorY = Math.Min(game.WorldMap.Height - 1, cursorY + 1);
                return;
            case ConsoleKey.LeftArrow:
                cursorX = Math.Max(0, cursorX - 1);
                return;
            case ConsoleKey.RightArrow:
                cursorX = Math.Min(game.WorldMap.Width - 1, cursorX + 1);
                return;
            case ConsoleKey.Enter:
                IssuePlayerOrder(selectedArmy, game.WorldMap, movement, cursorX, cursorY, ref status);
                return;
        }
    }

    private static void SelectArmy(
        int index,
        List<Army> playerArmies,
        ref int selectedArmyIndex,
        ref Army selectedArmy,
        ref int cameraX,
        ref int cameraY,
        ref int cursorX,
        ref int cursorY)
    {
        if (index >= playerArmies.Count)
            return;

        selectedArmyIndex = index;
        selectedArmy = playerArmies[index];
        CenterCamera(selectedArmy, ref cameraX, ref cameraY);
        cursorX = selectedArmy.X;
        cursorY = selectedArmy.Y;
    }

    private static void IssuePlayerOrder(
        Army army,
        Map map,
        MovementSystem movement,
        int destinationX,
        int destinationY,
        ref string status)
    {
        if (!movement.CanReach(map, army.X, army.Y, destinationX, destinationY))
        {
            status = $"No land route to ({destinationX},{destinationY}).";
            return;
        }

        army.DestinationX = destinationX;
        army.DestinationY = destinationY;
        army.MovementProgress = 0;
        status = $"{army.Name} ordered to ({destinationX},{destinationY}).";
    }

    private static void PanCamera(
        int dx,
        int dy,
        ref int cameraX,
        ref int cameraY,
        ref int cursorX,
        ref int cursorY)
    {
        var oldX = cameraX;
        var oldY = cameraY;
        cameraX = Math.Clamp(cameraX + dx, 0, MapWidth - ViewportWidth);
        cameraY = Math.Clamp(cameraY + dy, 0, MapHeight - ViewportHeight);
        cursorX = Math.Clamp(cursorX + cameraX - oldX, 0, MapWidth - 1);
        cursorY = Math.Clamp(cursorY + cameraY - oldY, 0, MapHeight - 1);
    }

    private static void CenterCamera(Army army, ref int cameraX, ref int cameraY)
    {
        cameraX = Math.Clamp(army.X - ViewportWidth / 2, 0, MapWidth - ViewportWidth);
        cameraY = Math.Clamp(army.Y - ViewportHeight / 2, 0, MapHeight - ViewportHeight);
    }

    private static void IssueEnemyOrders(GameState game, List<Army> playerArmies, MovementSystem movement)
    {
        var livingPlayerArmies = playerArmies.Where(army => game.Armies.Contains(army)).ToList();
        if (livingPlayerArmies.Count == 0)
            return;

        foreach (var enemy in game.Armies.Where(army => army.OwnerId == 2 && army.IsAtDestination))
        {
            var target = livingPlayerArmies
                .OrderBy(army => Math.Abs(army.X - enemy.X) + Math.Abs(army.Y - enemy.Y))
                .First();
            if (movement.CanReach(game.WorldMap, enemy.X, enemy.Y, target.X, target.Y))
            {
                enemy.DestinationX = target.X;
                enemy.DestinationY = target.Y;
            }
        }
    }

    private static void TryStartEncounter(
        GameState game,
        EncounterSystem encounters,
        ref BattleManager? battle,
        ref Army? battleArmyA,
        ref Army? battleArmyB,
        ref string status)
    {
        for (var i = 0; i < game.Armies.Count; i++)
        {
            for (var j = i + 1; j < game.Armies.Count; j++)
            {
                var first = game.Armies[i];
                var second = game.Armies[j];
                if (first.OwnerId == second.OwnerId || first.X != second.X || first.Y != second.Y)
                    continue;

                battle = encounters.CheckForEncounter(new List<Army> { first, second });
                battleArmyA = first;
                battleArmyB = second;
                status = $"Battle! {first.Name} vs {second.Name}.";
                return;
            }
        }
    }

    private static void CaptureCities(GameState game)
    {
        foreach (var army in game.Armies)
        {
            var tile = game.WorldMap.GetTile(army.X, army.Y);
            if (tile?.City != null && tile.City.OwnerId != army.OwnerId)
                tile.City.OwnerId = army.OwnerId;
        }
    }

    private static Army CreateArmy(
        string name,
        int ownerId,
        int x,
        int y,
        Dictionary<string, UnitDefinition> definitions,
        params string[] unitClasses)
    {
        var army = new Army
        {
            Name = name,
            OwnerId = ownerId,
            X = x,
            Y = y,
            DestinationX = x,
            DestinationY = y,
            MoveSpeed = 1.8f
        };

        for (var i = 0; i < unitClasses.Length; i++)
        {
            var classId = unitClasses[i];
            if (!definitions.TryGetValue(classId, out var definition))
                throw new InvalidDataException($"Unit definition '{classId}' is missing.");

            army.AddUnit(new Unit($"{name}-{i + 1}", classId)
            {
                HP = definition.HP,
                MaxHP = definition.HP,
                STR = definition.STR,
                AGI = definition.AGI,
                INT = definition.INT
            });
        }

        return army;
    }

    private static Map CreateMap()
    {
        var map = new Map(MapWidth, MapHeight);
        for (var x = 0; x < MapWidth; x++)
        {
            for (var y = 0; y < MapHeight; y++)
            {
                var terrain = TerrainType.Plain;
                if (x == 20 && y != 14)
                    terrain = TerrainType.Water;
                else if (y == 14)
                    terrain = TerrainType.Road;
                else if ((x is >= 11 and <= 15 && y is >= 5 and <= 9)
                    || (x is >= 25 and <= 29 && y is >= 18 and <= 22))
                    terrain = TerrainType.Forest;
                else if ((x is >= 16 and <= 18 && y is >= 5 and <= 9)
                    || (x is >= 21 and <= 23 && y is >= 18 and <= 22))
                    terrain = TerrainType.Mountain;

                map.SetTile(x, y, new Tile(x, y, terrain));
            }
        }

        var homeTile = map.GetTile(2, 10)!;
        homeTile.Terrain = TerrainType.Town;
        homeTile.City = new City("Home", 1);
        map.SetTile(2, 10, homeTile);
        var capitalTile = map.GetTile(37, 16)!;
        capitalTile.Terrain = TerrainType.Town;
        capitalTile.City = new City("Enemy Capital", 2);
        map.SetTile(37, 16, capitalTile);
        return map;
    }

    private static string ResolveDataFile(string fileName)
    {
        var outputPath = Path.Combine(AppContext.BaseDirectory, "Data", fileName);
        if (File.Exists(outputPath))
            return outputPath;

        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Engine", "Data", fileName));
        if (File.Exists(projectPath))
            return projectPath;

        var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "Engine", "Data", fileName);
        if (File.Exists(sourcePath))
            return sourcePath;

        throw new FileNotFoundException($"Could not find the game data file '{fileName}'.", sourcePath);
    }

    private static void Render(
        GameState game,
        Dictionary<Army, string> markers,
        Army selectedArmy,
        int cameraX,
        int cameraY,
        int cursorX,
        int cursorY,
        bool paused,
        BattleManager? activeBattle,
        string status)
    {
        Console.Clear();
        Console.WriteLine("OGRE BATTLE | " + (paused ? "PAUSED" : "REAL TIME"));
        Console.WriteLine("     " + string.Concat(Enumerable.Range(cameraX, ViewportWidth).Select(x => (x / 10).ToString())));
        for (var y = cameraY; y < cameraY + ViewportHeight; y++)
        {
            Console.Write($"{y,2} | ");
            for (var x = cameraX; x < cameraX + ViewportWidth; x++)
            {
                var army = game.Armies.FirstOrDefault(candidate => candidate.X == x && candidate.Y == y);
                var tile = game.WorldMap.GetTile(x, y)!;
                var symbol = army != null ? markers[army] : GetTileSymbol(tile);
                if (x == cursorX && y == cursorY)
                    symbol = "[]";
                Console.Write(symbol);
            }
            Console.WriteLine();
        }

        var selectedUnits = string.Join(", ", selectedArmy.Units.Select(unit => $"{unit.ClassId} {unit.HP}/{unit.MaxHP}"));
        Console.WriteLine($"Selected: {selectedArmy.Name} ({selectedArmy.X},{selectedArmy.Y}) -> ({selectedArmy.DestinationX},{selectedArmy.DestinationY})");
        Console.WriteLine($"Units: {selectedUnits}");
        Console.WriteLine($"Home: {GetCityOwner(game, "Home")} | Enemy Capital: {GetCityOwner(game, "Enemy Capital")}");
        Console.WriteLine("Camera: WASD | Target: arrows | Select: Tab/1/2 | Move: Enter | Pause: Space | Quit: Q/Esc");
        Console.WriteLine($"Status: {status}");
        if (activeBattle != null)
            Console.WriteLine($"In battle: {BattleArmySummary(activeBattle)}");
        else
            Console.WriteLine("Legend: P=player, E=enemy, CC=city, ^^=forest, MM=mountain, ~~=water, == road");
    }

    private static string BattleArmySummary(BattleManager battle)
    {
        var armyA = string.Join(", ", battle.ArmyA.Where(unit => unit.IsAlive).Select(unit => $"{unit.ClassId} {unit.HP}"));
        var armyB = string.Join(", ", battle.ArmyB.Where(unit => unit.IsAlive).Select(unit => $"{unit.ClassId} {unit.HP}"));
        return $"{armyA} vs {armyB}";
    }

    private static string GetCityOwner(GameState game, string cityName)
    {
        var city = game.WorldMap.Cities.First(candidate => candidate.Name == cityName);
        return city.OwnerId == 1 ? "Player" : "Enemy";
    }

    private static string GetTileSymbol(Tile tile)
    {
        if (tile.City != null)
            return "CC";

        return tile.Terrain switch
        {
            TerrainType.Forest => "^^",
            TerrainType.Mountain => "MM",
            TerrainType.Water => "~~",
            TerrainType.Road => "==",
            _ => ".."
        };
    }

    private static void PrintControls()
    {
        Console.WriteLine("Ogre Battle RTS prototype");
        Console.WriteLine("WASD: move the camera");
        Console.WriteLine("Arrow keys: move the map cursor");
        Console.WriteLine("Tab or 1/2: select a player army");
        Console.WriteLine("Enter: order the selected army to the cursor");
        Console.WriteLine("Space: pause or resume");
        Console.WriteLine("Q or Esc: quit");
        Console.WriteLine("Capture the enemy capital to win. Water and mountains block movement.");
    }
}

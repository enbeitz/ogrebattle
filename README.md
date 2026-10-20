# Ogre Battle

A small, playable Ogre Battle-inspired RTS prototype in C#. Run it from the repository root:

```sh
dotnet run --project EngineTestHarness
```

## Controls

- **WASD**: pan the camera
- **Arrow keys**: move the map cursor
- **Tab** or **1/2**: select a player army
- **Enter**: order the selected army to the cursor
- **Space**: pause or resume
- **Q** or **Esc**: quit

Armies march in real time, follow land routes around impassable water and mountains, and fight automatically when they meet an enemy. Capture the enemy capital to win. Run with `--help` to print the controls without starting a game.

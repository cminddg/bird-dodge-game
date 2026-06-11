# Bird Dodge MVP (Unity 2D)

This repository now contains a Unity-ready MVP implementation for a 2D bird dodge game:

- `Ready -> Running -> GameOver` state flow
- 3-life bird swap using three PNG sprites
- Infinite obstacle spawning with 15-second difficulty scaling
- Survival score + pass-through obstacle score
- Local best score with `PlayerPrefs`
- `R` quick restart

## Quick Start

1. Open this folder as a Unity project (Unity 2022 LTS or newer recommended).
2. In Unity menu, run `Tools -> Bird Game -> Generate MVP Scenes`.
3. Open `File -> Build Settings` and confirm:
   - `Assets/Scenes/Bootstrap.unity`
   - `Assets/Scenes/Game.unity`
4. Enter Play Mode.

## Controls

- `Space` or left mouse click: flap
- `R`: quick restart

## Key Scripts

- `Assets/Scripts/Core/GameManager.cs`
- `Assets/Scripts/Player/PlayerController.cs`
- `Assets/Scripts/Player/CollisionLifeSystem.cs`
- `Assets/Scripts/Obstacles/ObstacleSpawner.cs`
- `Assets/Scripts/Scoring/ScoreSystem.cs`
- `Assets/Scripts/UI/HudController.cs`

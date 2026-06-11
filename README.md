# Bird Dodge Game

This repository contains a 2D bird dodge game with both Unity and HTML5 web versions.

## Web Version

The browser-playable version lives in `docs/` and is ready for GitHub Pages.

- `Start`: begin the run
- `Space` or click: flap and cycle the current bird's pose
- `Restart`: restart after Game Over
- `R`: quick restart
- `Esc`: pause in the browser version
- Each pipe pair passed gives 10 points
- Three birds represent three lives; the bird only changes after a collision

## Unity Version

The Unity project still contains the original MVP implementation:

- `Ready -> Running -> GameOver` state flow
- 3-life bird swap using three PNG sprites
- Infinite obstacle spawning with 15-second difficulty scaling
- Survival score + pass-through obstacle score
- Local best score with `PlayerPrefs`
- `R` quick restart

## Quick Start: Web

```powershell
cd docs
python -m http.server 8080
```

Open `http://localhost:8080`.

## Quick Start: Unity

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

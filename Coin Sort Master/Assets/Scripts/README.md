# Coin Color Sort — Unity Game

## How to Run
1. Open project in Unity 2022.3 LTS
2. Open scene: Assets/Scenes/GameScene.unity
3. Press Play in Editor

---

## Approach

### Architecture

| Script | Type | Responsibility |
|---|---|---|
| `GameConfig.cs` | ScriptableObject | All tunable values (colors, capacities, thresholds) |
| `CoinStack.cs` | Data Class | Pure data: color + count, no MonoBehaviour |
| `BeltSlot.cs` | MonoBehaviour | One belt slot — coin storage + colored visual |
| `ConveyorBelt.cs` | MonoBehaviour | Drop stacks, auto-merge same colors, re-space visuals |
| `TraySlot.cs` | MonoBehaviour | One tray (active or waiting) — coin buffer + visual |
| `TrayManager.cs` | MonoBehaviour | Tray queue, coin routing, win/lose event dispatch |
| `IncomingStackDisplay.cs` | MonoBehaviour | Shows next stack; Drop button wired to GameManager |
| `LevelData.cs` | ScriptableObject | Stack sequence + tray order; can generate procedurally |
| `GameManager.cs` | MonoBehaviour (Singleton) | Central coordinator; wires all systems together |
| `UIManager.cs` | MonoBehaviour | Four screen panels + live labels + button wiring |
| `ScreenController.cs` | MonoBehaviour | Screen transition helpers (delegates to UIManager) |

### Key Design Decisions

- **GameConfig ScriptableObject** centralizes all tunable values — zero magic numbers in game logic scripts.
- **ConveyorBelt.AutoMerge()** consolidates same-color coins into the leftmost matching slot after each drop.
- **TrayManager.ReceiveCoins()** routes incoming coins: matching color → active tray, others → waiting buffer.
- **TrayManager.TryPullbackFromWaiting()** is called whenever a new active tray starts, pulling buffered coins in.
- **GameManager.CheckBeltForPromotion()** runs after every drop: if a slot hits `coinsToPromote` for the active color, those coins are automatically sent to TrayManager.
- All screens are single-scene Canvas panels toggled with `SetActive` — no scene loading, no Animator.
- All component references are Inspector-assigned — zero `Find()` or `FindObjectOfType()` at runtime.

---

## Scene Setup Checklist

1. **GameConfig asset**: Assets > Create > CoinSort > GameConfig → assign 5 colors in Inspector
2. **LevelData asset**: Assets > Create > CoinSort > LevelData (or let GameManager generate at runtime)
3. **Hierarchy**:
   - `GameManager` (empty GO) → GameManager.cs
   - `Belt` → ConveyorBelt.cs → 5 child GOs each with BeltSlot.cs
   - `TrayArea` → TrayManager.cs → `ActiveTray` (TraySlot, cap=10), `WaitingTray` (TraySlot, cap=30)
   - `IncomingArea` → IncomingStackDisplay.cs → child Cube for visual
   - `Canvas` → UIManager.cs → 4 Panel children: HomeScreen, GameScreen, WinScreen, LoseScreen
4. Drag all Inspector references in GameManager, UIManager, TrayManager
5. Wire Drop button `OnClick` → `IncomingStackDisplay.OnDropButtonPressed()`

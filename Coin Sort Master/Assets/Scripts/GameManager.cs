/* SCENE SETUP:
 * 1. Create GameObjects: Belt, TrayArea, IncomingArea, Canvas
 * 2. Belt: add ConveyorBelt.cs. Create 5 child GameObjects, each with BeltSlot.cs
 * 3. TrayArea: add TrayManager.cs. Create ActiveTray child (TraySlot.cs capacity=10)
 *    Create WaitingTray child (TraySlot.cs capacity=30)
 * 4. IncomingArea: add IncomingStackDisplay.cs. One child Cube for visual.
 * 5. Canvas: 4 panel children — HomeScreen, GameScreen, WinScreen, LoseScreen
 *    Add UIManager.cs to Canvas
 * 6. Empty GameObject "GameManager": add GameManager.cs
 *    Drag all references in Inspector
 * 7. Create GameConfig asset: Assets > Create > CoinSort > GameConfig
 *    Assign 5 colors in Inspector
 * 8. Create LevelData asset: Assets > Create > CoinSort > LevelData
 *    Or use LevelData.GenerateLevel(config) at runtime
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton coordinator — wires together belt, trays, stack display, and UI for each level.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Config & Level")]
    [SerializeField] private GameConfig _config;
    [SerializeField] private LevelData _currentLevel;
    [SerializeField] private float _spawnRadius = 2.6f;

    [Header("Scene References")]
    [SerializeField] private ConveyorBelt _belt;
    [SerializeField] private TrayManager _trayManager;
    [SerializeField] private IncomingStackDisplay _stackDisplay;
    [SerializeField] private UIManager _uiManager;

    private int _stackIndex = 0;
    private int _currentLevelNumber = 1;
    private bool _isCenterStackSelected = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Wire tray events
        if (_trayManager != null)
        {
            _trayManager.OnWin.AddListener(OnWin);
            _trayManager.OnLose.AddListener(OnLose);
        }

        if (_uiManager != null)
        {
            _uiManager.ShowHome();
        }
    }

    private void Update()
    {
        bool clicked = false;
        Vector3 clickPos = Vector3.zero;

#if ENABLE_INPUT_SYSTEM
        var mouse = UnityEngine.InputSystem.Mouse.current;
        var touchscreen = UnityEngine.InputSystem.Touchscreen.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            clicked = true;
            clickPos = mouse.position.ReadValue();
        }
        else if (touchscreen != null && touchscreen.touches.Count > 0 && touchscreen.touches[0].press.wasPressedThisFrame)
        {
            clicked = true;
            clickPos = touchscreen.touches[0].position.ReadValue();
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            clicked = true;
            clickPos = Input.mousePosition;
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            clicked = true;
            clickPos = Input.GetTouch(0).position;
        }
#endif

        if (clicked)
        {
            HandleClick(clickPos);
        }
    }

    private void HandleClick(Vector3 pos)
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 1. Try to hit CoinStackObject
            CoinStackObject stackObj = hit.collider.GetComponent<CoinStackObject>();
            if (stackObj != null)
            {
                if (stackObj.parentSlot != null)
                {
                    OnSlotClicked(stackObj.parentSlot);
                }
                else
                {
                    OnCoinStackTapped(stackObj);
                }
                return;
            }

            // 2. Try to hit TraySlot
            TraySlot slot = hit.collider.GetComponent<TraySlot>();
            if (slot != null)
            {
                OnSlotClicked(slot);
                return;
            }
        }
    }

    public float SpawnRadius => _spawnRadius;

    /// <summary>Starts a level using provided LevelData (or generates one if null).</summary>
    public void StartLevel(LevelData level)
    {
        if (level == null)
        {
            if (_currentLevel != null)
                level = _currentLevel;
            else
                level = LevelData.GenerateLevel(_config);
        }

        _currentLevel = level;
        _stackIndex = 0;
        _isCenterStackSelected = false;

        // Initialize belt targets
        if (_belt != null)
        {
            _belt.InitializeTargets(_currentLevel.beltTargets);
        }

        // Clear tray and spawn initial stacks (spawns on outer slots automatically)
        if (_trayManager != null)
        {
            _trayManager.ClearTray();
            foreach (var stack in _currentLevel.initialStacks)
            {
                _trayManager.SpawnStack(stack, _spawnRadius);
            }
        }

        // Check if any initial stacks of the same color are adjacent and should merge
        CheckAdjacentMerges();

        // Spawn first stack in the center slot (index 0)
        SpawnNextCenterStack();

        // Update UI
        if (_uiManager != null)
        {
            _uiManager.UpdateLevelNumber(_currentLevelNumber);
            _uiManager.UpdateWaitingTrayCount(_currentLevel.stackSequence.Count - _stackIndex, 0);
            _uiManager.ShowGame();
        }

        // Show first incoming stack preview
        ShowCurrentStack();
    }

    /// <summary>Callback when a tray slot (either empty floor or coin stack on top) is clicked.</summary>
    public void OnSlotClicked(TraySlot slot)
    {
        if (_trayManager == null || _belt == null) return;

        TraySlot centerSlot = _trayManager.GetSlot(0);
        if (centerSlot == null || centerSlot.currentStack == null) return;

        CoinStackObject centerStack = centerSlot.currentStack;

        // 1. Center slot clicked: toggle selection (lift up/down)
        if (slot.slotIndex == 0)
        {
            if (_isCenterStackSelected)
            {
                // Deselect: move back to the slot position
                centerStack.transform.position = centerSlot.transform.position;
                _isCenterStackSelected = false;
                Debug.Log("[GameManager] Center stack deselected.");
            }
            else
            {
                // Select: lift the stack up slightly
                centerStack.transform.position = centerSlot.transform.position + new Vector3(0f, 0.8f, 0f);
                _isCenterStackSelected = true;
                Debug.Log("[GameManager] Center stack selected and lifted.");
            }
            return;
        }

        // 2. Outer slot clicked: drop center stack there if selected
        if (_isCenterStackSelected)
        {
            // Empty outer slot clicked: move center stack there
            if (slot.currentStack == null)
            {
                centerStack.transform.position = slot.transform.position;
                slot.currentStack = centerStack;
                centerStack.parentSlot = slot;
                centerSlot.currentStack = null;
                _isCenterStackSelected = false;

                // Spawn next stack in center
                SpawnNextCenterStack();

                // Check for adjacent merges first!
                CheckAdjacentMerges();

                // Check if stack has reached 10 coins
                CheckPromotionForSlot(slot);
            }
            // Occupied outer slot clicked: merge if colors match
            else if (slot.currentStack != null && slot.currentStack.color == centerStack.color)
            {
                int newCount = slot.currentStack.count + centerStack.count;
                slot.currentStack.SetCount(newCount);

                Destroy(centerStack.gameObject);
                centerSlot.currentStack = null;
                _isCenterStackSelected = false;

                // Spawn next stack in center
                SpawnNextCenterStack();

                // Check for adjacent merges first!
                CheckAdjacentMerges();

                // Check if stack has reached 10 coins
                CheckPromotionForSlot(slot);
            }
        }
    }

    /// <summary>Checks if any adjacent active outer slots (ignoring empty spaces in between) have the same color, merging them automatically.</summary>
    public void CheckAdjacentMerges()
    {
        if (_trayManager == null) return;

        // Collect all non-empty outer slots in circular order
        List<TraySlot> activeSlots = new List<TraySlot>();
        for (int i = 1; i <= 6; i++)
        {
            TraySlot slot = _trayManager.GetSlot(i);
            if (slot != null && slot.currentStack != null)
            {
                activeSlots.Add(slot);
            }
        }

        if (activeSlots.Count <= 1) return;

        bool mergedAny = false;

        // Check each active slot against its neighbor in the active list
        for (int k = 0; k < activeSlots.Count; k++)
        {
            TraySlot slotA = activeSlots[k];
            TraySlot slotB = activeSlots[(k + 1) % activeSlots.Count];

            // If they are of the same color, merge them
            if (slotA.currentStack.color == slotB.currentStack.color)
            {
                Debug.Log($"[GameManager] Circular merge triggered: Slot {slotA.slotIndex} and Slot {slotB.slotIndex} ({slotA.currentStack.color})");

                int newCount = slotA.currentStack.count + slotB.currentStack.count;
                slotA.currentStack.SetCount(newCount);

                // Destroy slotB's stack and clear reference
                _trayManager.RemoveStack(slotB.currentStack);

                mergedAny = true;

                // Check promotion for slotA
                CheckPromotionForSlot(slotA);
                break; // Exit loop to restart checks after structure modified
            }
        }

        // Recursively check again if a merge was made to support cascading merges
        if (mergedAny)
        {
            CheckAdjacentMerges();
        }
    }

    /// <summary>Checks if a tray slot reaches 10 coins and promotes it to the matching active belt target.</summary>
    public void CheckPromotionForSlot(TraySlot slot)
    {
        if (slot == null || slot.currentStack == null || _belt == null) return;

        if (slot.currentStack.count >= 10)
        {
            BeltSlot targetSlot = _belt.FindTargetForColor(slot.currentStack.color);
            if (targetSlot != null)
            {
                // Add 10 coins of this color to the active belt target
                targetSlot.AddCoins(slot.currentStack.color, 10);

                // Clear the stack from tray slot
                _trayManager.RemoveStack(slot.currentStack);

                // Process completed belt targets and check win
                _belt.ProcessCompletedTargets();
                
                // When new targets slide in, check all slots again in case they match
                CheckAllSlotsForPromotion();

                CheckWinCondition();
            }
        }
    }

    /// <summary>Iterates through all outer slots promoting completed stacks of active colors.</summary>
    public void CheckAllSlotsForPromotion()
    {
        if (_trayManager == null || _belt == null) return;

        for (int i = 1; i <= 6; i++)
        {
            TraySlot slot = _trayManager.GetSlot(i);
            if (slot != null && slot.currentStack != null && slot.currentStack.count >= 10)
            {
                BeltSlot targetSlot = _belt.FindTargetForColor(slot.currentStack.color);
                if (targetSlot != null)
                {
                    targetSlot.AddCoins(slot.currentStack.color, 10);
                    _trayManager.RemoveStack(slot.currentStack);
                    _belt.ProcessCompletedTargets();
                    CheckAllSlotsForPromotion(); // Recurse in case targets changed
                    break;
                }
            }
        }
    }

    /// <summary>Spawns the next stack in the center slot.</summary>
    public void SpawnNextCenterStack()
    {
        if (_currentLevel == null || _trayManager == null) return;

        _isCenterStackSelected = false;

        if (_stackIndex < _currentLevel.stackSequence.Count)
        {
            CoinStack nextStack = _currentLevel.stackSequence[_stackIndex];
            _trayManager.SpawnStackOnSlot(nextStack, 0); // Spawns in center slot (index 0)
            _stackIndex++;
        }
        else
        {
            // Center slot left empty
            TraySlot centerSlot = _trayManager.GetSlot(0);
            if (centerSlot != null) centerSlot.currentStack = null;
        }

        if (_uiManager != null && _currentLevel != null)
        {
            _uiManager.UpdateWaitingTrayCount(_currentLevel.stackSequence.Count - _stackIndex, 0);
        }
    }

    /// <summary>Legacy callback when a stack on the tray is clicked/tapped.</summary>
    public void OnCoinStackTapped(CoinStackObject stackObj)
    {
        if (stackObj.parentSlot != null)
        {
            OnSlotClicked(stackObj.parentSlot);
        }
    }

    /// <summary>Drops/spawns the next stack from the incoming queue onto the tray center.</summary>
    public void DropCurrentStack()
    {
        SpawnNextCenterStack();
    }

    /// <summary>Checks if all active targets on the conveyor belt are complete.</summary>
    public void CheckWinCondition()
    {
        if (_belt != null && _belt.AreAllComplete())
        {
            OnWin();
        }
    }

    /// <summary>Advances to the next LevelData (generates procedurally if none assigned).</summary>
    public void StartNextLevel()
    {
        _currentLevelNumber++;
        StartLevel(LevelData.GenerateLevel(_config));
    }

    /// <summary>Retries the same level.</summary>
    public void RetryLevel()
    {
        StartLevel(_currentLevel);
    }

    /// <summary>Called when all belt targets are complete.</summary>
    public void OnWin()
    {
        Debug.Log("[GameManager] LEVEL WIN!");
        if (_uiManager != null)
        {
            _uiManager.ShowWin();
        }
    }

    /// <summary>Fallback lose event handler.</summary>
    public void OnLose()
    {
        Debug.Log("[GameManager] LEVEL LOSE!");
        if (_uiManager != null)
        {
            _uiManager.ShowLose();
        }
    }

    private void ShowCurrentStack()
    {
        if (_stackDisplay != null)
        {
            if (_currentLevel != null && _stackIndex < _currentLevel.stackSequence.Count)
            {
                _stackDisplay.SetStack(_currentLevel.stackSequence[_stackIndex]);
            }
            else
            {
                _stackDisplay.SetStack(null);
            }
        }
    }

    /// <summary>Returns the GameConfig (for other scripts that need it).</summary>
    public GameConfig Config => _config;

    /// <summary>Returns current level number.</summary>
    public int CurrentLevelNumber => _currentLevelNumber;
}

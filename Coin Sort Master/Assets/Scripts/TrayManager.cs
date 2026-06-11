using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the circular tray platform, creating slots dynamically and routing spawns and clicks.
/// </summary>
public class TrayManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameConfig _config;

    [Header("Tray References")]
    [SerializeField] private TraySlot _activeTray;
    [SerializeField] private TraySlot _waitingTray;

    [Header("Events")]
    public UnityEvent OnWin = new UnityEvent();
    public UnityEvent OnLose = new UnityEvent();

    private TraySlot[] _slots = new TraySlot[7]; // 0 = Center, 1-6 = Outer slots

    public TraySlot GetActiveTray() => _activeTray;
    public TraySlot GetWaitingTray() => _waitingTray;

    private void Awake()
    {
        InitializeSlots(2.6f);
    }

    /// <summary>Ensures the 7 slots are created dynamically as children of ActiveTray, or updates their positions if they already exist.</summary>
    public void InitializeSlots(float radius)
    {
        if (_slots[0] != null)
        {
            // Re-position the outer slots dynamically if radius changed
            for (int i = 0; i < 6; i++)
            {
                if (_slots[i + 1] != null)
                {
                    float angle = i * (Mathf.PI * 2f / 6f);
                    float localY = _slots[i + 1].transform.localPosition.y;
                    _slots[i + 1].transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, localY, Mathf.Sin(angle) * radius);
                }
            }
            return;
        }

        // 1. Center Slot (index 0)
        _slots[0] = CreateSlotGameObject("CenterSlot", Vector3.zero, 0, radius);

        // 2. 6 Outer Slots (indices 1-6)
        for (int i = 0; i < 6; i++)
        {
            float angle = i * (Mathf.PI * 2f / 6f);
            Vector3 relativePos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            _slots[i + 1] = CreateSlotGameObject("OuterSlot_" + (i + 1), relativePos, i + 1, radius);
        }
    }

    private TraySlot CreateSlotGameObject(string name, Vector3 relativePos, int index, float radius)
    {
        Transform parent = _activeTray != null ? _activeTray.transform : transform;
        GameObject slotGo = new GameObject(name);
        slotGo.transform.SetParent(parent, false);

        // Calculate local Y of the tray renderer's top surface
        float localY = 0f;
        if (_activeTray != null && _activeTray.trayRenderer != null)
        {
            float worldTopY = _activeTray.trayRenderer.bounds.max.y;
            Vector3 worldTopPos = new Vector3(_activeTray.transform.position.x, worldTopY, _activeTray.transform.position.z);
            localY = _activeTray.transform.InverseTransformPoint(worldTopPos).y;
        }

        slotGo.transform.localPosition = new Vector3(relativePos.x, localY, relativePos.z);

        // Add BoxCollider so it can be clicked
        BoxCollider col = slotGo.AddComponent<BoxCollider>();
        col.size = new Vector3(1.5f, 0.2f, 1.5f);
        col.center = new Vector3(0f, 0.1f, 0f);

        // Add TraySlot component
        TraySlot slot = slotGo.AddComponent<TraySlot>();
        slot.InitializeSlot(index, this);
        return slot;
    }

    /// <summary>Clears all spawned stacks from the slots.</summary>
    public void ClearTray()
    {
        float radius = GameManager.Instance != null ? GameManager.Instance.SpawnRadius : 2.6f;
        ClearTray(radius);
    }

    /// <summary>Clears all spawned stacks from the slots with a specific radius.</summary>
    public void ClearTray(float radius)
    {
        InitializeSlots(radius);
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null && _slots[i].currentStack != null)
            {
                Destroy(_slots[i].currentStack.gameObject);
                _slots[i].currentStack = null;
            }
        }
    }

    /// <summary>Spawns a stack on the first empty outer slot (for level initial stacks).</summary>
    public CoinStackObject SpawnStack(CoinStack stack, float spawnRadius)
    {
        InitializeSlots(spawnRadius);

        // Place initial stacks on outer slots 1-6
        for (int i = 1; i <= 6; i++)
        {
            if (_slots[i].currentStack == null)
            {
                return SpawnStackOnSlot(stack, i);
            }
        }

        return SpawnStackOnSlot(stack, 1);
    }

    /// <summary>Spawns a stack on a specific slot index.</summary>
    public CoinStackObject SpawnStackOnSlot(CoinStack stack, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length) return null;
        TraySlot slot = _slots[slotIndex];
        if (slot == null) return null;

        if (slot.currentStack != null)
        {
            Destroy(slot.currentStack.gameObject);
            slot.currentStack = null;
        }

        GameObject stackGo = new GameObject("CoinStack_" + stack.color.ToString());
        stackGo.transform.position = slot.transform.position;
        stackGo.transform.rotation = Quaternion.identity;

        CoinStackObject stackObj = stackGo.AddComponent<CoinStackObject>();
        GameObject coinPrefab = _activeTray != null ? _activeTray.coinPrefab : null;
        float spacingY = _activeTray != null ? _activeTray.coinSpacingY : 0.15f;

        stackObj.Initialize(stack.color, stack.count, coinPrefab, spacingY);
        stackObj.parentSlot = slot;
        slot.currentStack = stackObj;

        return stackObj;
    }

    /// <summary>Removes a stack object from its parent slot and destroys it.</summary>
    public void RemoveStack(CoinStackObject stack)
    {
        if (stack == null) return;
        if (stack.parentSlot != null)
        {
            stack.parentSlot.currentStack = null;
        }
        Destroy(stack.gameObject);
    }

    /// <summary>Called by child TraySlot components when clicked/tapped.</summary>
    public void OnSlotClicked(TraySlot slot)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSlotClicked(slot);
        }
    }

    /// <summary>Returns slot by index.</summary>
    public TraySlot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length) return null;
        return _slots[index];
    }
}

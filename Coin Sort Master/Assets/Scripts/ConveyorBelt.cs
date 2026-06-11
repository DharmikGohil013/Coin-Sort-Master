using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages target slot configurations on the conveyor belt and checks completion status.
/// </summary>
public class ConveyorBelt : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameConfig _config;

    [Header("Slots")]
    public List<BeltSlot> slots = new List<BeltSlot>();

    private void Awake()
    {
        // Auto-populate slots from children if not assigned in Inspector
        if (slots.Count == 0)
        {
            foreach (Transform child in transform)
            {
                BeltSlot slot = child.GetComponent<BeltSlot>();
                if (slot != null)
                    slots.Add(slot);
            }
        }
    }

    private Queue<BeltTarget> _pendingTargets = new Queue<BeltTarget>();

    /// <summary>Initializes belt slots with target colors and counts, queueing extra targets.</summary>
    public void InitializeTargets(List<BeltTarget> targets)
    {
        _pendingTargets.Clear();
        foreach (var target in targets)
        {
            _pendingTargets.Enqueue(target);
        }

        int simultaneousBelts = _config != null ? _config.beltSlotCount : 3;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < simultaneousBelts && _pendingTargets.Count > 0)
            {
                BeltTarget t = _pendingTargets.Dequeue();
                slots[i].SetTarget(t.color, t.targetCount);
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    /// <summary>Finds the active slot matching the specified color.</summary>
    public BeltSlot FindTargetForColor(Color color)
    {
        foreach (BeltSlot slot in slots)
        {
            if (slot.MatchesColor(color))
                return slot;
        }
        return null;
    }

    /// <summary>Checks if any active target slot is completed, clears it, and spawns the next target color from the queue.</summary>
    public void ProcessCompletedTargets()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            BeltSlot slot = slots[i];
            if (slot.isTargetActive && slot.IsComplete)
            {
                Debug.Log($"[ConveyorBelt] Slot {i} ({slot.targetColor}) completed! Removing target.");
                
                // Clear the completed slot
                slot.Clear();

                // Load next pending target on this slot if available
                if (_pendingTargets.Count > 0)
                {
                    BeltTarget nextTarget = _pendingTargets.Dequeue();
                    slot.SetTarget(nextTarget.color, nextTarget.targetCount);
                    Debug.Log($"[ConveyorBelt] Spawned new target {nextTarget.color} on slot {i}.");
                }
            }
        }
    }

    /// <summary>Checks if all targets (both active slots and pending targets queue) are complete.</summary>
    public bool AreAllComplete()
    {
        if (_pendingTargets.Count > 0) return false;

        foreach (BeltSlot slot in slots)
        {
            if (slot.isTargetActive && !slot.IsComplete)
                return false;
        }
        return true;
    }

    /// <summary>Returns a slot by index.</summary>
    public BeltSlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }

    /// <summary>Returns total number of slots.</summary>
    public int SlotCount => slots.Count;
}

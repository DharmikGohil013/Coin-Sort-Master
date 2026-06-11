using UnityEngine;

/// <summary>
/// Represents a single slot on the circular tray platform (0 = center, 1-6 = outer slots).
/// </summary>
public class TraySlot : MonoBehaviour
{
    [Header("Visual Configuration (Reference Provider Only)")]
    [SerializeField] private Renderer _trayRenderer;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private float _coinSpacingY = 0.15f;

    public GameObject coinPrefab => _coinPrefab;
    public float coinSpacingY => _coinSpacingY;
    public Renderer trayRenderer => _trayRenderer;

    public int slotIndex { get; private set; }
    public TrayManager manager { get; private set; }
    public CoinStackObject currentStack;

    /// <summary>Initializes the slot index and its parent manager.</summary>
    public void InitializeSlot(int index, TrayManager trayManager)
    {
        slotIndex = index;
        manager = trayManager;
        currentStack = null;
    }

    /// <summary>Triggered when the player clicks/taps an empty slot collider.</summary>
    private void OnMouseDown()
    {
        OnSlotClicked();
    }

    /// <summary>Forwards clicks from either the slot floor or the coin stack on top.</summary>
    public void OnSlotClicked()
    {
        if (manager != null)
        {
            manager.OnSlotClicked(this);
        }
    }
}

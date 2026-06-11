using UnityEngine;
using TMPro;

/// <summary>
/// Represents a single slot on the conveyor belt, tracking target color, coin count, and target capacity.
/// </summary>
public class BeltSlot : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Renderer _slotRenderer;
    [SerializeField] private TextMeshPro _countLabel;
    [SerializeField] private GameObject _visualRoot; // The colored cube/quad

    public Color slotColor { get; private set; }
    public int coinCount { get; private set; }
    public Color targetColor { get; private set; }
    public int targetCount { get; private set; }
    public bool isTargetActive { get; private set; }

    /// <summary>Returns true if this slot holds no coins.</summary>
    public bool IsEmpty => coinCount == 0;

    /// <summary>Returns true when target is active and coin capacity has been reached.</summary>
    public bool IsComplete => isTargetActive && coinCount >= targetCount;

    private void Awake()
    {
        // Auto-resolve references if unassigned in Inspector
        if (_slotRenderer == null)
        {
            _slotRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (_visualRoot == null)
        {
            // Search for typical child names or default to first child
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Visual") || child.name.Contains("Cube") || 
                    child.name.Contains("Holder") || child.name.Contains("Renderer") || 
                    child.name.Contains("CoinHolder"))
                {
                    _visualRoot = child.gameObject;
                    break;
                }
            }
            if (_visualRoot == null && transform.childCount > 0)
            {
                _visualRoot = transform.GetChild(0).gameObject;
            }
        }

        if (_countLabel == null)
        {
            _countLabel = GetComponentInChildren<TextMeshPro>();
        }

        Clear();
    }

    /// <summary>Initializes this slot with a target color and count.</summary>
    public void SetTarget(Color color, int count)
    {
        targetColor = color;
        targetCount = count;
        slotColor = color;
        isTargetActive = true;
        coinCount = 0;
        UpdateVisuals();
    }

    /// <summary>Adds coins if they match the target color.</summary>
    public void AddCoins(Color color, int amount)
    {
        if (!isTargetActive)
        {
            SetTarget(color, 10);
        }

        if (ColorApproxEqual(color, targetColor))
        {
            coinCount += amount;
            UpdateVisuals();
        }
        else
        {
            Debug.LogWarning($"[BeltSlot] Tried to add mismatching color {color} to target color {targetColor}!");
        }
    }

    /// <summary>Removes up to amount coins from this slot. Returns how many were actually removed.</summary>
    public int RemoveCoins(int amount)
    {
        int removed = Mathf.Min(amount, coinCount);
        coinCount -= removed;
        UpdateVisuals();
        return removed;
    }

    /// <summary>Resets the slot to empty state.</summary>
    public void Clear()
    {
        slotColor = Color.clear;
        targetColor = Color.clear;
        targetCount = 0;
        coinCount = 0;
        isTargetActive = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (!isTargetActive)
        {
            if (_visualRoot != null)
                _visualRoot.SetActive(false);

            if (_slotRenderer != null)
                _slotRenderer.material.color = Color.gray;

            if (_countLabel != null)
            {
                _countLabel.text = "";
                _countLabel.gameObject.SetActive(false);
            }
            return;
        }

        if (_visualRoot != null)
            _visualRoot.SetActive(true);

        if (_slotRenderer != null)
        {
            _slotRenderer.material.color = targetColor;
        }

        if (_countLabel != null)
        {
            if (IsComplete)
            {
                _countLabel.text = "✓";
            }
            else
            {
                _countLabel.text = $"{coinCount}/{targetCount}";
            }
            _countLabel.gameObject.SetActive(true);
        }
    }

    /// <summary>Called by ConveyorBelt to check color equality with tolerance.</summary>
    public bool MatchesColor(Color other)
    {
        return isTargetActive && ColorApproxEqual(targetColor, other);
    }

    private static bool ColorApproxEqual(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }
}

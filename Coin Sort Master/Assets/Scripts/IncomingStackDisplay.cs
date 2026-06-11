using UnityEngine;
using TMPro;

/// <summary>
/// Displays the next CoinStack the player is about to drop onto the belt.
/// </summary>
public class IncomingStackDisplay : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Renderer _stackRenderer;
    [SerializeField] private TextMeshPro _countLabel;
    [SerializeField] private GameObject _stackVisual;

    private CoinStack _currentStack;

    /// <summary>Sets the displayed stack and updates visuals.</summary>
    public void SetStack(CoinStack stack)
    {
        _currentStack = stack;
        UpdateVisuals();
    }

    /// <summary>Called by the Drop button — tells GameManager to drop the current stack.</summary>
    public void OnDropButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DropCurrentStack();
        }
        else
        {
            Debug.LogError("[IncomingStackDisplay] GameManager.Instance is null!");
        }
    }

    private void UpdateVisuals()
    {
        bool hasStack = _currentStack != null && _currentStack.count > 0;

        if (_stackVisual != null)
            _stackVisual.SetActive(hasStack);

        if (_stackRenderer != null && hasStack)
            _stackRenderer.material.color = _currentStack.color;

        if (_countLabel != null)
            _countLabel.text = hasStack ? _currentStack.count.ToString() : "";
    }
}

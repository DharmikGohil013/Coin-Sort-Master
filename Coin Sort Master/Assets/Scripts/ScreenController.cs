using UnityEngine;

/// <summary>
/// Handles explicit screen transitions via SetActive calls — simple, no animation.
/// Delegates to UIManager for actual panel control.
/// </summary>
public class ScreenController : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;

    /// <summary>Transitions to the Home screen.</summary>
    public void GoToHome()
    {
        if (_uiManager != null)
            _uiManager.ShowHome();
    }

    /// <summary>Transitions to the Game screen.</summary>
    public void GoToGame()
    {
        if (_uiManager != null)
            _uiManager.ShowGame();
    }

    /// <summary>Transitions to the Win screen.</summary>
    public void GoToWin()
    {
        if (_uiManager != null)
            _uiManager.ShowWin();
    }

    /// <summary>Transitions to the Lose screen.</summary>
    public void GoToLose()
    {
        if (_uiManager != null)
            _uiManager.ShowLose();
    }
}

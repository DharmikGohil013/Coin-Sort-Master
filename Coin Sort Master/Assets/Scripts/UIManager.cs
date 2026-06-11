using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the four UI screens (Home, Game, Win, Lose) and live in-game UI labels.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject _homeScreen;
    [SerializeField] private GameObject _gameScreen;
    [SerializeField] private GameObject _winScreen;
    [SerializeField] private GameObject _loseScreen;

    [Header("In-Game Labels")]
    [SerializeField] private TextMeshProUGUI _waitingTrayLabel;
    [SerializeField] private TextMeshProUGUI _levelLabel;

    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _homeButtonFromWin;

    private void Start()
    {
        // Wire up buttons
        if (_playButton != null)
            _playButton.onClick.AddListener(OnPlayClicked);

        if (_nextLevelButton != null)
            _nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (_retryButton != null)
            _retryButton.onClick.AddListener(OnRetryClicked);

        if (_homeButton != null)
            _homeButton.onClick.AddListener(OnHomeClicked);

        if (_homeButtonFromWin != null)
            _homeButtonFromWin.onClick.AddListener(OnHomeClicked);
    }

    /// <summary>Shows only the Home screen.</summary>
    public void ShowHome()
    {
        SetAllScreensInactive();
        if (_homeScreen != null) _homeScreen.SetActive(true);
    }

    /// <summary>Shows only the Game screen.</summary>
    public void ShowGame()
    {
        SetAllScreensInactive();
        if (_gameScreen != null) _gameScreen.SetActive(true);
    }

    /// <summary>Shows only the Win screen.</summary>
    public void ShowWin()
    {
        SetAllScreensInactive();
        if (_winScreen != null) _winScreen.SetActive(true);
    }

    /// <summary>Shows only the Lose screen.</summary>
    public void ShowLose()
    {
        SetAllScreensInactive();
        if (_loseScreen != null) _loseScreen.SetActive(true);
    }

    /// <summary>Updates the remaining stacks live label.</summary>
    public void UpdateWaitingTrayCount(int count, int capacity)
    {
        if (_waitingTrayLabel != null)
            _waitingTrayLabel.text = $"Remaining Piles: {count}";
    }

    /// <summary>Updates the level number label.</summary>
    public void UpdateLevelNumber(int level)
    {
        if (_levelLabel != null)
            _levelLabel.text = $"Level {level}";
    }

    private void SetAllScreensInactive()
    {
        if (_homeScreen != null) _homeScreen.SetActive(false);
        if (_gameScreen != null) _gameScreen.SetActive(false);
        if (_winScreen != null) _winScreen.SetActive(false);
        if (_loseScreen != null) _loseScreen.SetActive(false);
    }

    // ---- Button Handlers ----

    private void OnPlayClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartLevel(null); // null = generate a random level
    }

    private void OnNextLevelClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartNextLevel();
    }

    private void OnRetryClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RetryLevel();
    }

    private void OnHomeClicked()
    {
        ShowHome();
    }
}

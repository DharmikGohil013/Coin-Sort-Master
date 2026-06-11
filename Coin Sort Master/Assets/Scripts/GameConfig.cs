using UnityEngine;

/// <summary>
/// ScriptableObject centralizing all tunable gameplay values for Coin Color Sort.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "CoinSort/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Level Settings")]
    public int coinColorsPerLevel = 3;
    public int beltSlotCount = 5;
    public int coinsToPromote = 10;

    [Header("Tray Settings")]
    public int activeTrayCapacity = 10;
    public int waitingTrayCapacity = 30;
    public int traysPerLevel = 2;

    [Header("Colors")]
    public Color[] availableColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
        new Color(0.5f, 0f, 0.5f) // Purple
    };

    private void OnValidate()
    {
        coinColorsPerLevel = Mathf.Clamp(coinColorsPerLevel, 2, availableColors != null ? availableColors.Length : 5);
        beltSlotCount = Mathf.Clamp(beltSlotCount, 1, 10);
        coinsToPromote = Mathf.Clamp(coinsToPromote, 1, 100);
        activeTrayCapacity = Mathf.Clamp(activeTrayCapacity, 1, 100);
        waitingTrayCapacity = Mathf.Clamp(waitingTrayCapacity, activeTrayCapacity, 300);
        traysPerLevel = Mathf.Clamp(traysPerLevel, 1, 20);
    }
}

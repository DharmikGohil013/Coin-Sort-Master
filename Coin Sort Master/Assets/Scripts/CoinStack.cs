using UnityEngine;

/// <summary>
/// Pure data class representing a stack of same-colored coins (no MonoBehaviour).
/// </summary>
[System.Serializable]
public class CoinStack
{
    public Color color;
    public int count;

    /// <summary>Creates a new CoinStack with the given color and coin count.</summary>
    public CoinStack(Color color, int count)
    {
        this.color = color;
        this.count = count;
    }

    /// <summary>Returns a copy of this CoinStack.</summary>
    public CoinStack Clone()
    {
        return new CoinStack(color, count);
    }
}

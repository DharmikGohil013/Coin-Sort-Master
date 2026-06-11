using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeltTarget
{
    public Color color;
    public int targetCount;

    public BeltTarget(Color color, int targetCount)
    {
        this.color = color;
        this.targetCount = targetCount;
    }
}

/// <summary>
/// ScriptableObject defining one level: the initial stacks, the belt targets, and the sequence of incoming stacks.
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "CoinSort/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Initial Stacks on Tray")]
    public List<CoinStack> initialStacks = new List<CoinStack>();

    [Header("Belt Targets")]
    public List<BeltTarget> beltTargets = new List<BeltTarget>();

    [Header("Incoming Stack Sequence (Next Pile)")]
    public List<CoinStack> stackSequence = new List<CoinStack>();

    /// <summary>
    /// Randomly generates a balanced LevelData at runtime based on the provided GameConfig.
    /// </summary>
    public static LevelData GenerateLevel(GameConfig config)
    {
        LevelData data = CreateInstance<LevelData>();

        // Pick colors for this level
        List<Color> levelColors = new List<Color>();
        List<int> colorIndices = new List<int>();
        for (int i = 0; i < config.availableColors.Length; i++)
            colorIndices.Add(i);

        // Shuffle
        for (int i = colorIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = colorIndices[i];
            colorIndices[i] = colorIndices[j];
            colorIndices[j] = tmp;
        }

        int colorCount = Mathf.Min(config.coinColorsPerLevel, config.availableColors.Length);
        for (int i = 0; i < colorCount; i++)
            levelColors.Add(config.availableColors[colorIndices[i]]);

        // Build belt targets
        data.beltTargets = new List<BeltTarget>();
        foreach (Color c in levelColors)
        {
            data.beltTargets.Add(new BeltTarget(c, config.coinsToPromote));
        }

        // Generate coins needed to satisfy targets
        Dictionary<Color, int> coinsRemaining = new Dictionary<Color, int>();
        foreach (var target in data.beltTargets)
        {
            coinsRemaining[target.color] = target.targetCount;
        }

        // Convert coins into stacks
        List<CoinStack> initialList = new List<CoinStack>();
        foreach (var pair in coinsRemaining)
        {
            int remaining = pair.Value;
            while (remaining > 0)
            {
                int stackSize = Mathf.Min(Random.Range(2, 6), remaining);
                initialList.Add(new CoinStack(pair.Key, stackSize));
                remaining -= stackSize;
            }
        }

        // Shuffle list
        for (int i = initialList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CoinStack tmp = initialList[i];
            initialList[i] = initialList[j];
            initialList[j] = tmp;
        }

        // Rearrange to minimize adjacent same-color stacks
        List<CoinStack> rearrangedList = new List<CoinStack>();
        List<CoinStack> remainingStacks = new List<CoinStack>(initialList);
        while (remainingStacks.Count > 0)
        {
            int foundIdx = -1;
            for (int k = 0; k < remainingStacks.Count; k++)
            {
                if (rearrangedList.Count == 0 || !ColorApproxEqual(remainingStacks[k].color, rearrangedList[rearrangedList.Count - 1].color))
                {
                    foundIdx = k;
                    break;
                }
            }

            if (foundIdx != -1)
            {
                rearrangedList.Add(remainingStacks[foundIdx]);
                remainingStacks.RemoveAt(foundIdx);
            }
            else
            {
                rearrangedList.Add(remainingStacks[0]);
                remainingStacks.RemoveAt(0);
            }
        }
        initialList = rearrangedList;

        // Put the first 5 in initialStacks, the rest in stackSequence
        data.initialStacks = new List<CoinStack>();
        data.stackSequence = new List<CoinStack>();
        for (int i = 0; i < initialList.Count; i++)
        {
            if (i < 5)
                data.initialStacks.Add(initialList[i]);
            else
                data.stackSequence.Add(initialList[i]);
        }

        return data;
    }

    private static bool ColorApproxEqual(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }
}

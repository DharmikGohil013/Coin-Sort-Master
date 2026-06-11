using UnityEngine;
using TMPro;

/// <summary>
/// Represents a tappable stack of coins on the circular tray.
/// </summary>
public class CoinStackObject : MonoBehaviour
{
    public Color color { get; private set; }
    public int count { get; private set; }

    private GameObject _textGo;

    public TraySlot parentSlot;
    private GameObject _coinPrefabRef;
    private float _spacingYRef;

    /// <summary>
    /// Initializes the coin stack object, spawning visuals, collider, and count label.
    /// </summary>
    public void Initialize(Color stackColor, int coinCount, GameObject coinPrefab, float spacingY)
    {
        this.color = stackColor;
        this.count = coinCount;
        this._coinPrefabRef = coinPrefab;
        this._spacingYRef = spacingY;

        InitializeVisuals();
    }

    private void InitializeVisuals()
    {
        // 1. Spawn coin visuals stacked vertically
        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(_coinPrefabRef, transform);
            coin.transform.localPosition = new Vector3(0, i * _spacingYRef, 0);
            coin.transform.localRotation = _coinPrefabRef.transform.localRotation;
            coin.transform.localScale = _coinPrefabRef.transform.localScale;

            // Set coin color
            Renderer[] renderers = coin.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                Material[] mats = r.materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j].color = color;
                }
                r.materials = mats;
            }
        }

        // 2. Add/configure BoxCollider dynamically for click/tap detection
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        float stackHeight = Mathf.Max(0.5f, count * _spacingYRef);
        col.center = new Vector3(0, (count - 1) * _spacingYRef * 0.5f, 0);
        col.size = new Vector3(1.5f, stackHeight + 0.2f, 1.5f);

        // 3. Create the 3D TextMeshPro label above the stack
        if (_textGo != null)
        {
            Destroy(_textGo);
        }
        _textGo = new GameObject("CountLabel");
        _textGo.transform.SetParent(transform, false);
        _textGo.transform.localPosition = new Vector3(0, count * _spacingYRef + 0.3f, 0);

        TextMeshPro tmp = _textGo.AddComponent<TextMeshPro>();
        tmp.text = count.ToString();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 6f;
        tmp.color = Color.white;
        
        // Add a subtle outline to make text readable on any background
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;
    }

    /// <summary>Updates stack size count and re-renders coin visuals.</summary>
    public void SetCount(int newCount)
    {
        this.count = newCount;

        // Hide and destroy old children
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        InitializeVisuals();
    }

    private void Update()
    {
        // Billboarding: make the text label always face the camera
        if (_textGo != null && Camera.main != null)
        {
            _textGo.transform.rotation = Quaternion.LookRotation(_textGo.transform.position - Camera.main.transform.position);
        }
    }

    private void OnMouseDown()
    {
        if (parentSlot != null)
        {
            parentSlot.OnSlotClicked();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinStackTapped(this);
        }
    }
}

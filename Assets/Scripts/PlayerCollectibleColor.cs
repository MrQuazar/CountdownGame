using UnityEngine;

/// <summary>
/// Tints the player's SpriteRenderer based on collectible progress:
///   ratio &lt; 1/3           -> earlyColor  (default red, 255,0,0)
///   1/3 &lt;= ratio &lt; 2/3    -> midColor    (default white, 255,255,255)
///   ratio &gt;= 2/3           -> lateColor   (default golden yellow, 255,255,0)
/// Updates instantly whenever GameManager reports a collectible was picked up.
/// </summary>
public class PlayerCollectibleColor : MonoBehaviour
{
    [Header("Colors")]
    public Color earlyColor = new Color(1f, 0f, 0f);   // 255, 0, 0
    public Color midColor = new Color(1f, 1f, 1f);     // 255, 255, 255
    public Color lateColor = new Color(1f, 1f, 0f);    // 255, 255, 0

    [Header("References")]
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void Start()
    {
        TrySubscribe(); // in case GameManager.Instance wasn't set yet during OnEnable
        UpdateColor();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnCollectionChanged -= UpdateColor;
    }

    void TrySubscribe()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCollectionChanged -= UpdateColor; // avoid double-subscribing
        GameManager.Instance.OnCollectionChanged += UpdateColor;
    }

    void UpdateColor()
    {
        if (spriteRenderer == null || GameManager.Instance == null) return;

        int total = GameManager.Instance.TotalCollectibles;
        int collected = GameManager.Instance.CollectedCount;
        float ratio = total > 0 ? (float)collected / total : 0f;

        if (ratio < 1f / 3f) spriteRenderer.color = earlyColor;
        else if (ratio < 2f / 3f) spriteRenderer.color = midColor;
        else spriteRenderer.color = lateColor;
    }
}

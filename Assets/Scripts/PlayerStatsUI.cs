using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows Speed / Attack / Toughness as 3 bars below the health bar, reflecting
/// the player's current scale stage: Small = lowest, Normal = middle, Large = max.
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    public PlayerScale playerScale;

    [Header("Bars")]
    public Slider speedBar;
    public Slider attackBar;
    public Slider toughnessBar;

    [Header("Fill level per stage (0-1)")]
    [Tooltip("Index 0 = Small, 1 = Normal, 2 = Large. Same progression applied to all 3 stats.")]
    public float[] statLevelsByStage = { 0.33f, 0.66f, 1f };

    void Start()
    {
        if (speedBar != null) speedBar.maxValue = 1f;
        if (attackBar != null) attackBar.maxValue = 1f;
        if (toughnessBar != null) toughnessBar.maxValue = 1f;
        UpdateBars();
    }

    void Update()
    {
        UpdateBars();
    }

    void UpdateBars()
    {
        if (playerScale == null || statLevelsByStage == null || statLevelsByStage.Length == 0) return;

        int stage = Mathf.Clamp((int)playerScale.CurrentScaleStage, 0, statLevelsByStage.Length - 1);
        float level = statLevelsByStage[stage];

        if (speedBar != null) speedBar.value = level;
        if (attackBar != null) attackBar.value = level;
        if (toughnessBar != null) toughnessBar.value = level;
    }
}
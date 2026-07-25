using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI collectibleText;

    [Header("Win")]
    public GameObject winScreen;

    private int totalCollectibles;
    private int collectedCount;

    [Header("Global Mobile Speed")]
    [Tooltip("Reference to the player's PlayerScale, used to look up the multiplier below.")]
    public PlayerScale playerScale;
    [Tooltip("Speed multiplier applied to enemies, the chasing enemy, and moving platforms based on the player's current scale stage. Index 0 = Small, 1 = Normal, 2 = Large — matches PlayerScale.ScaleStage. Never affects the player itself.")]
    public float[] mobileSpeedMultipliers = { 0.5f, 1f, 2f };

    public float CurrentMobileSpeedMultiplier
    {
        get
        {
            if (playerScale == null || mobileSpeedMultipliers == null || mobileSpeedMultipliers.Length == 0) return 1f;
            int stage = Mathf.Clamp(playerScale.CurrentStage, 0, mobileSpeedMultipliers.Length - 1);
            return mobileSpeedMultipliers[stage];
        }
    }

    /// <summary>Null-safe static accessor: 1f if no GameManager/PlayerScale is set up yet.</summary>
    public static float MobileSpeedMultiplier => Instance != null ? Instance.CurrentMobileSpeedMultiplier : 1f;

    /// <summary>Fires whenever CollectedCount changes (i.e. after CollectItem()).</summary>
    public event System.Action OnCollectionChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Collectible[] found = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        totalCollectibles = found.Length;
        collectedCount = 0;

        if (winScreen != null) winScreen.SetActive(false);

        UpdateUI();
    }

    public void CollectItem()
    {
        collectedCount++;
        UpdateUI();
        OnCollectionChanged?.Invoke();

        if (collectedCount >= totalCollectibles)
            OnAllCollected();
    }

    void UpdateUI()
    {
        if (collectibleText != null)
            collectibleText.text = $"{collectedCount} / {totalCollectibles}";
    }

    void OnAllCollected()
    {
        Debug.Log("All collectibles found — goal complete!");
        if (winScreen != null) winScreen.SetActive(true);
    }

    public int CollectedCount => collectedCount;
    public int TotalCollectibles => totalCollectibles;
    public void RetryLevel()
    {
        Time.timeScale = 1f; // in case you paused via timeScale on timeout
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
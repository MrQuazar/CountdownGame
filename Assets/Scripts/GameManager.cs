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
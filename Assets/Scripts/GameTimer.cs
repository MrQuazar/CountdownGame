using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("UI")]
    public GameObject loseScreen;

    [Header("Timer Settings")]
    public float startMinutes = 2f;

    [Header("References")]
    public PlayerScale playerScale;
    public TextMeshProUGUI timerText;

    [Header("Multiplier Per Stage")]
    [Tooltip("Index 0 = Small, 1 = Normal, 2 = Large. Must match PlayerScale's scaleStages length.")]
    public float[] speedMultipliers = { 0.5f, 1f, 2f };

    private float timeRemaining;
    private bool timerRunning = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timeRemaining = startMinutes * 60f;
        UpdateDisplay();
    }

    /// <summary>Adds bonus seconds to the clock (e.g. from a collectible). No-op once time has run out.</summary>
    public void AddTime(float seconds)
    {
        if (!timerRunning) return;
        timeRemaining += seconds;
        UpdateDisplay();
    }

    void Update()
    {
        if (!timerRunning) return;

        float multiplier = 1f;
        if (playerScale != null && speedMultipliers.Length > 0)
        {
            int stage = Mathf.Clamp(playerScale.CurrentStage, 0, speedMultipliers.Length - 1);
            multiplier = speedMultipliers[stage];
        }

        timeRemaining -= Time.deltaTime * multiplier;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            OnTimerEnd();
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        int seconds = Mathf.FloorToInt(timeRemaining);
        int milliseconds = Mathf.FloorToInt((timeRemaining - seconds) * 1000f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:000}", seconds, milliseconds);
    }

    void OnTimerEnd()
    {
        Debug.Log("Time's up!");
        if (loseScreen != null) loseScreen.SetActive(true);
    }

    public void StopTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
}
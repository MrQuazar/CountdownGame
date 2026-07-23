using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startMinutes = 2f;

    [Header("References")]
    public PlayerScale playerScale;
    public TextMeshProUGUI timerText;

    [Header("Multiplier Per Stage")]
    [Tooltip("Must match the length of scaleStages on PlayerScale. Index 0 = slowest (smallest), last index = fastest (biggest).")]
    public float[] speedMultipliers = { 0.5f, 0.75f, 1f, 1.5f, 2f };

    private float timeRemaining;
    private bool timerRunning = true;

    void Start()
    {
        timeRemaining = startMinutes * 60f;
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
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void OnTimerEnd()
    {
        Debug.Log("Time's up!");
    }

    public void StopTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
}
using System.Collections;
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

    [Header("Pulse (speed follows the current multiplier above)")]
    [Tooltip("Pulses per second when multiplier = 1 (Normal). Scales up/down with the multiplier.")]
    public float basePulseFrequency = 1.5f;
    [Range(0f, 0.5f)] public float pulseAmount = 0.15f;

    [Header("Low Time Warning")]
    public float lowTimeThreshold = 10f;
    public Color normalColor = Color.white;
    public Color lowTimeColor = Color.red;

    [Header("Time-Added Popup")]
    public TextMeshProUGUI bonusText;
    public float bonusDisplayDuration = 3f;
    public float shakeMagnitude = 8f;

    private float timeRemaining;
    private bool timerRunning = true;
    private float pulsePhase = 0f;
    private Vector2 bonusRestPos;
    private Coroutine bonusRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timeRemaining = startMinutes * 60f;

        if (bonusText != null)
        {
            bonusRestPos = bonusText.rectTransform.anchoredPosition;
            bonusText.enabled = false;
        }

        UpdateDisplay();
    }

    public void AddTime(float seconds)
    {
        if (!timerRunning) return;
        timeRemaining += seconds;
        UpdateDisplay();
        ShowBonusPopup(seconds);
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

        pulsePhase += Time.deltaTime * basePulseFrequency * multiplier * Mathf.PI * 2f;

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

        if (timerText == null) return;

        timerText.text = string.Format("{0:00}:{1:000}", seconds, milliseconds);
        timerText.color = timeRemaining <= lowTimeThreshold ? lowTimeColor : normalColor;

        float scale = 1f + Mathf.Sin(pulsePhase) * pulseAmount;
        timerText.transform.localScale = Vector3.one * scale;
    }

    void ShowBonusPopup(float seconds)
    {
        if (bonusText == null) return;
        if (bonusRoutine != null) StopCoroutine(bonusRoutine);
        bonusRoutine = StartCoroutine(BonusPopupRoutine(seconds));
    }

    IEnumerator BonusPopupRoutine(float seconds)
    {
        bonusText.text = $"+{seconds:0}";
        bonusText.enabled = true;

        float t = 0f;
        while (t < bonusDisplayDuration)
        {
            t += Time.deltaTime;
            Vector2 shakeOffset = new Vector2(
                Random.Range(-shakeMagnitude, shakeMagnitude),
                Random.Range(-shakeMagnitude, shakeMagnitude));
            bonusText.rectTransform.anchoredPosition = bonusRestPos + shakeOffset;
            yield return null;
        }

        bonusText.rectTransform.anchoredPosition = bonusRestPos;
        bonusText.enabled = false;
    }

    void OnTimerEnd()
    {
        Debug.Log("Time's up!");
        if (loseScreen != null) loseScreen.SetActive(true);
    }

    public void StopTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
}
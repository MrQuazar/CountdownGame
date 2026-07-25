using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// In-game pause menu: Resume, Retry, Music slider, SFX slider, Quit to Main Menu.
/// Uncheck "Menu Enabled" in the Inspector to disable pausing entirely for this scene
/// (the panel is hidden and the pause key does nothing).
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Enable/Disable")]
    [Tooltip("Turn off to disable the pause menu completely (panel is hidden, pause key does nothing).")]
    public bool menuEnabled = true;

    [Header("References")]
    public GameObject rootPanel;
    public KeyCode pauseKey = KeyCode.Escape;
    public Button resumeButton;
    public Button retryButton;
    public Button quitToMainMenuButton;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Guards (optional)")]
    [Tooltip("If assigned, pausing is blocked once the player is dead.")]
    public PlayerHealth playerHealth;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (!menuEnabled)
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            enabled = false;
            return;
        }

        if (rootPanel != null) rootPanel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (quitToMainMenuButton != null) quitToMainMenuButton.onClick.AddListener(OnQuitToMainMenu);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else if (!IsBlockedFromPausing()) Pause();
    }

    bool IsBlockedFromPausing()
    {
        if (playerHealth != null && playerHealth.IsDead) return true;
        if (GameManager.Instance != null && GameManager.Instance.TotalCollectibles > 0 &&
            GameManager.Instance.CollectedCount >= GameManager.Instance.TotalCollectibles) return true;
        return false;
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (rootPanel != null) rootPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
        }
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (rootPanel != null) rootPanel.SetActive(false);
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        if (GameManager.Instance != null) GameManager.Instance.RetryLevel();
    }

    public void OnQuitToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = value;
    }

    public void OnSFXSliderChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SFXVolume = value;
    }
}

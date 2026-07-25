using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu screen: Play button, Music slider, SFX slider.
/// Uncheck "Menu Enabled" in the Inspector to disable the main menu entirely
/// (e.g. when testing a level directly) — the panel is hidden and this script does nothing.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Enable/Disable")]
    [Tooltip("Turn off to disable the main menu completely (panel is hidden, script does nothing).")]
    public bool menuEnabled = true;

    [Header("References")]
    public GameObject rootPanel;
    public Button playButton;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Scene")]
    public string gameplaySceneName = "Game";

    void Awake()
    {
        if (!menuEnabled)
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            enabled = false;
            return;
        }

        if (rootPanel != null) rootPanel.SetActive(true);
        Time.timeScale = 1f;

        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    void Start()
    {
        if (!menuEnabled) return;

        if (AudioManager.Instance != null)
        {
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            AudioManager.Instance.PlayMainMenuMusic();
        }
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(gameplaySceneName);
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

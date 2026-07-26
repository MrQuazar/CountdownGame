using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


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
    [Header("Level Select")]
    public GameObject levelPanel;
    public GameObject optionsPanel;
    public Transform levelListContent;
    public Button levelButtonPrefab;
    public Button openLevelPanelButton;
    public Button closeLevelPanelButton;
    [Tooltip("Scene names to exclude from the auto-generated list (e.g. this menu itself).")]
    public List<string> excludedSceneNames = new List<string> { "MainMenu" };

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
        if (openLevelPanelButton != null) openLevelPanelButton.onClick.AddListener(OpenLevelPanel);
        if (closeLevelPanelButton != null) closeLevelPanelButton.onClick.AddListener(CloseLevelPanel);
    }

    void Start()
    {
        if (!menuEnabled) return;
        if (levelPanel != null) levelPanel.SetActive(false);
        BuildLevelList();

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
    void BuildLevelList()
    {
        if (levelListContent == null || levelButtonPrefab == null) return;

        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (excludedSceneNames.Contains(sceneName)) continue;

            int buildIndex = i;
            Button btn = Instantiate(levelButtonPrefab, levelListContent);
            btn.gameObject.SetActive(true);

            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = sceneName;

            btn.onClick.AddListener(() => LoadLevel(buildIndex));
        }
    }

    void LoadLevel(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    public void OpenLevelPanel()
    {
        if (levelPanel != null) levelPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void CloseLevelPanel()
    {
        if (levelPanel != null) levelPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }
}

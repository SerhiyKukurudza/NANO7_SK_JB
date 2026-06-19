using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("UI")]
    public Slider brightnessSlider;
    public Slider masterVolumeSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Brightness")]
    private Image brightnessOverlay;

    private Resolution[] resolutions;
    private UIAudioManager cachedAudioManager;

    void Start()
    {
        if (UiManager.Instance != null)
        {
            brightnessOverlay = UiManager.Instance.brightnessOverlay;
        }

        cachedAudioManager = UIAudioManager.Instance;
        if (cachedAudioManager == null)
        {
            cachedAudioManager = FindFirstObjectByType<UIAudioManager>();
        }

        SetupBrightness();
        SetupVolume();
        SetupResolution();
        SetupFullscreen();
    }

    #region BRIGHTNESS

    void SetupBrightness()
    {
        float value = PlayerPrefs.GetFloat("Brightness", 1f);

        brightnessSlider.value = value;
        ApplyBrightness(value);

        brightnessSlider.onValueChanged.AddListener(ApplyBrightness);
    }

    public void ApplyBrightness(float value)
    {
        if (brightnessOverlay == null) return;

        Color c = brightnessOverlay.color;
        c.a = 1f - value;
        brightnessOverlay.color = c;

        PlayerPrefs.SetFloat("Brightness", value);
    }

    #endregion

    #region VOLUME

    void SetupVolume()
    {
        float value = PlayerPrefs.GetFloat("MasterVolume", 1f);

        masterVolumeSlider.value = value;

        if (cachedAudioManager != null)
        {
            cachedAudioManager.SetMasterVolume(value);
        }

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (cachedAudioManager == null)
        {
            cachedAudioManager = UIAudioManager.Instance;
            if (cachedAudioManager == null)
            {
                cachedAudioManager = FindFirstObjectByType<UIAudioManager>();
            }
        }

        if (cachedAudioManager != null)
        {
            cachedAudioManager.SetMasterVolume(value);
        }

        WeaponSoundManager weaponSoundManager = WeaponSoundManager.GetInstance();
        if (weaponSoundManager != null)
        {
            weaponSoundManager.SetMasterVolume(value);
        }
    }

    #endregion

    #region RESOLUTION

    void SetupResolution()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("Resolution", currentIndex);

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        ApplyResolution(savedIndex);

        resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
    }

    public void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;

        Resolution r = resolutions[index];

        Screen.SetResolution(
            r.width,
            r.height,
            Screen.fullScreen
        );

        PlayerPrefs.SetInt("Resolution", index);
    }

    #endregion

    #region FULLSCREEN

    void SetupFullscreen()
    {
        bool value = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        fullscreenToggle.isOn = value;
        Screen.fullScreen = value;

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;

        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
    }

    #endregion

    #region NAVIGATION

    public void OnBackClicked()
    {
        UIAudioManager.Instance?.PlayClick();
        
        string previousScene = PlayerPrefs.GetString("PreviousScene", "MainMenu");
        
        if (previousScene == "PauseMenu")
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
        
        SceneManager.LoadScene(previousScene);
    }

    #endregion
}

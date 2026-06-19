using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private bool isPaused = false;
    private static string pausedFromScene = string.Empty;

    void Start()
    {
        if (!PlayerPrefs.HasKey("LevelReached"))
        {
            PlayerPrefs.SetInt("LevelReached", 1);
            PlayerPrefs.Save();
        }

        if (IsGameplayScene(SceneManager.GetActiveScene().name) && UiManager.Instance == null)
        {
            UiManager.EnsureInstance();
        }
    }

    //void Start()
    //{
    //}
    //IEnumerator ApplyResolutionNextFrame()
    //{
    //    yield return null; // espera 1 frame

    //    ApplySavedResolution();
    //}
    //void ApplySavedResolution()
    //{
    //    int index = PlayerPrefs.GetInt("Resolution", -1);

    //    Resolution[] resolutions = Screen.resolutions;

    //    if (index >= 0 && index < resolutions.Length)
    //    {
    //        Resolution r = resolutions[index];
    //        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    //    }
    //    else
    //    {
    //        // primeira execu��o faz fallback seguro
    //        Screen.SetResolution(1920, 1080, true);
    //        PlayerPrefs.SetInt("Resolution", 0);
    //        PlayerPrefs.Save();
    //    }
    //}
    void Update()
    {
        if (!IsGameplayScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

        bool escapePressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        escapePressed = Input.GetKeyDown(KeyCode.Escape);
#endif
        if (!escapePressed && Keyboard.current != null)
        {
            escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        }

        if (escapePressed)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void LevelSelect()
    {
        UIAudioManager.Instance?.PlayClick();
        RestoreGlobalRuntimeState();
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadGame()
    {
        UIAudioManager.Instance?.PlayClick();
        RestoreGlobalRuntimeState();
        UiManager manager = UiManager.Instance ?? UiManager.EnsureInstance();
        manager.LoadSavedGame();
    }

    public void SaveGame()
    {
        UIAudioManager.Instance?.PlayClick();
        UiManager manager = UiManager.Instance ?? UiManager.EnsureInstance();
        manager.SaveGameNow();
    }

    public void SettingsGame()
    {
        UIAudioManager.Instance?.PlayClick();
        RestoreGlobalRuntimeState();
        
        // Save current scene so we can return to it from Settings
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("PreviousScene", currentScene);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Settings");
    }

    public static void ExitGame()
    {
        UIAudioManager.Instance?.PlayClick();
        Application.Quit();
    }

    public static void MainMenu()
    {
        UIAudioManager.Instance?.PlayClick();
        RestoreGlobalRuntimeState();
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        if (IsGameplayScene(activeScene))
        {
            pausedFromScene = activeScene;
        }

        isPaused = true;
        UIAudioManager.Instance?.PlayClick();
        UiManager manager = UiManager.Instance ?? UiManager.EnsureInstance();
        manager.SaveGameNow();
        SceneManager.LoadScene("PauseMenu");
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        UIAudioManager.Instance?.PlayClick();
        RestoreGlobalRuntimeState();

        UiManager manager = UiManager.Instance ?? UiManager.EnsureInstance();
        if (manager.LoadSavedGame())
        {
            return;
        }

        string sceneToResume = string.IsNullOrWhiteSpace(pausedFromScene) ? "LevelSelect" : pausedFromScene;
        SceneManager.LoadScene(sceneToResume);
    }

    public void RestartGame()
    {
        UIAudioManager.Instance?.PlayClick();
        RestoreGlobalRuntimeState();
        string sceneToRestart = string.IsNullOrWhiteSpace(pausedFromScene) ? "LevelSelect" : pausedFromScene;
        SceneManager.LoadScene(sceneToRestart);
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3";
    }

    private static void RestoreGlobalRuntimeState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

}
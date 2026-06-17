using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private bool gameStarted = false;
    private bool isPaused = false;

    //void Start()
    //{
    //    StartCoroutine(ApplyResolutionNextFrame());
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
    //        // primeira execução faz fallback seguro
    //        Screen.SetResolution(1920, 1080, true);
    //        PlayerPrefs.SetInt("Resolution", 0);
    //        PlayerPrefs.Save();
    //    }
    //}
    void Update()
    {
        if (!gameStarted) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void LevelSelect()
    {
        gameStarted = true;
        UIAudioManager.Instance.PlayClick();
        Time.timeScale = 1;
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadGame()
    {
        UIAudioManager.Instance.PlayClick();
        SceneManager.LoadScene("LoadGame");
    }

    public void SettingsGame()
    {
        UIAudioManager.Instance.PlayClick();
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
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        isPaused = true;
        UIAudioManager.Instance.PlayClick();
        SceneManager.LoadScene("PauseMenu");
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        UIAudioManager.Instance.PlayClick();
        SceneManager.LoadScene("LevelSelect");
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    public void RestartGame()
    {
        UIAudioManager.Instance.PlayClick();
        Time.timeScale = 1;
        SceneManager.LoadScene("LevelSelect");
    }

//    // Static methods for buttons to call without needing GameObject reference
//    public static void StaticQuitGame()
//    {
//        Debug.Log("Quit button pressed");
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//    }

//    public static void StaticRestartGame()
//    {
//        Time.timeScale = 1;
//        SceneManager.LoadScene("LevelSelect");
//    }

//    public static void StaticGoToMainMenu()
//    {
//        Time.timeScale = 1;
//        SceneManager.LoadScene("MainMenu");
//    }
}
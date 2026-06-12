using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool gameStarted = false;
    private bool isPaused = false;
    public AudioSource mainMenuAudioSource;

    void Awake()
    {
    
    }

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

    public void StartGame()
    {
        if (mainMenuAudioSource != null)
        {
            mainMenuAudioSource.Stop();
        }

        gameStarted = true;
        Time.timeScale = 1;
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("LoadGame");
    }

    public void SettingsGame()
    {
        SceneManager.LoadScene("Settings");
    }

    public static void ExitGame()
    {
        Application.Quit();
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        isPaused = true;
        SceneManager.LoadScene("PauseMenu");
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        SceneManager.LoadScene("LevelSelect");
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    public void RestartGame()
    {
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
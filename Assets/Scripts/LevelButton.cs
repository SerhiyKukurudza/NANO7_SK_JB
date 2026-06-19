using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] public int level;
    [SerializeField] private string levelSceneName;
    [SerializeField] private Button button;

    void Start()
    {
        Debug.Log($"LevelButton Start() chamado em {gameObject.name}");

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        // Check if this level is unlocked
        // Level 1 is always unlocked (LevelReached starts at 1)
        // Level 2 unlocks when player passes Level 1
        // Level 3 unlocks when player passes Level 2
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        if (level > levelReached)
        {
            // Level not reached yet - disable button
            if (button != null)
            {
                button.interactable = false;
            }
        }
        else
        {
            // Level is unlocked - enable button
            if (button != null)
            {
                button.interactable = true;
            }
        }

        // Add click listener if not already added
        // if (button != null && button.onClick.GetPersistentEventCount() == 0)
        // {
        //     Debug.Log("Persistent events: " + button.onClick.GetPersistentEventCount());

        //     button.onClick.AddListener(OnLevelSelected);
        // }
        if (button != null)
        {
            button.onClick.AddListener(OnLevelSelected);
            Debug.Log("Listener adicionado");
        }
    }

    private void OnLevelSelected()
    {
         Debug.Log("Botão clicado!");

        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        // Additional safety check - can't select levels not yet reached
        if (level > levelReached)
        {
            Debug.LogWarning($"Cannot select Level {level} - not yet reached!");
            return;
        }

        UIAudioManager.Instance?.PlayClick();

        // Determine scene name if not assigned
        string sceneToLoad = levelSceneName;
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            sceneToLoad = $"Level{level}";
        }

        // Load the level with loading screen
        LevelTransitionLoader.LoadLevelWithInterlude(sceneToLoad, level, "LoadingScene", 1.5f);
    }
}

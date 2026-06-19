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

        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        if (level > levelReached)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }
        else
        {
            if (button != null)
            {
                button.interactable = true;
            }
        }


        if (button != null)
        {
            button.onClick.AddListener(OnLevelSelected);
            Debug.Log("Listener adicionado");
        }
    }

    private void OnLevelSelected()
    {
         Debug.Log("BotÃ£o clicado!");

        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        if (level > levelReached)
        {
            Debug.LogWarning($"Cannot select Level {level} - not yet reached!");
            return;
        }

        UIAudioManager.Instance?.PlayClick();

        string sceneToLoad = levelSceneName;
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            sceneToLoad = $"Level{level}";
        }

        LevelTransitionLoader.LoadLevelWithInterlude(sceneToLoad, level, "LoadingScene", 1.5f);
    }
}

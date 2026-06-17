using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [Header("Systems")]
    public UIAudioManager audioManager;
    public Image brightnessOverlay;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (audioManager == null)
            audioManager = GetComponentInChildren<UIAudioManager>();
    }
}
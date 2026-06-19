using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource mainMenuAudioSource;
    [SerializeField] private AudioSource uiSfxAudioSource;

    [Header("Clips")]
    public AudioClip clickSound;
    public AudioClip mainMenuMusic;

    [Header("Volume Settings")]
    private float masterVolume = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mainMenuAudioSource == null)
        {
            Transform mainMenuSourceTransform = transform.Find("MainMenuAudioSource");
            if (mainMenuSourceTransform != null)
            {
                mainMenuAudioSource = mainMenuSourceTransform.GetComponent<AudioSource>();
            }
        }
        if (mainMenuAudioSource == null)
        {
            mainMenuAudioSource = GetComponent<AudioSource>();
        }
        if (mainMenuAudioSource == null)
        {
            mainMenuAudioSource = gameObject.AddComponent<AudioSource>();
        }
        mainMenuAudioSource.loop = true;
        mainMenuAudioSource.playOnAwake = false;
        mainMenuAudioSource.spatialBlend = 0f;

        if (uiSfxAudioSource == null)
        {
            uiSfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
        uiSfxAudioSource.playOnAwake = false;
        uiSfxAudioSource.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadVolumeSettings();
        PlayMainMenuMusic();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    void Start()
    {
        if (mainMenuAudioSource.clip == null)
            PlayMainMenuMusic();
    }

    void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        ApplyVolume();
    }

    void ApplyVolume()
    {
        if (mainMenuAudioSource != null)
        {
            mainMenuAudioSource.volume = 0.5f * masterVolume;
        }

        if (uiSfxAudioSource != null)
        {
            uiSfxAudioSource.volume = 1f * masterVolume;
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();

        ApplyVolume();
    }

    public void PlayClick()
    {
        if (clickSound != null)
            uiSfxAudioSource.PlayOneShot(clickSound);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadVolumeSettings();

        string name = scene.name;

        if (name == "Level1" || name == "Level2" || name == "Level3")
        {
            StopMusic();
            return;
        }
        EnsureMenuMusic();
    }

    private void EnsureMenuMusic()
    {
        if (mainMenuMusic == null)
        {
            return;
        }

        mainMenuAudioSource.clip = mainMenuMusic;

        if (!mainMenuAudioSource.isPlaying)
        {
            mainMenuAudioSource.Play();
        }
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic == null) return;

        mainMenuAudioSource.clip = mainMenuMusic;

        if (!mainMenuAudioSource.isPlaying)
            mainMenuAudioSource.Play();
    }

    public void StopMusic()
    {
        if (mainMenuAudioSource != null)
        {
            mainMenuAudioSource.Stop();
        }
    }
}

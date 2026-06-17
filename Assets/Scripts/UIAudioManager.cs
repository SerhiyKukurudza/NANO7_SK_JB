using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;

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
        DontDestroyOnLoad(gameObject);

        // MUSIC SOURCE
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f;

        // SFX SOURCE
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1f;

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log("Music volume: " + musicSource.volume);

        LoadVolumeSettings();
        PlayMainMenuMusic();
    }

    void Start()
    {
        if (musicSource.clip == null)
            PlayMainMenuMusic();
    }

    void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        ApplyVolume();
    }

    void ApplyVolume()
    {
        musicSource.volume = 0.5f * masterVolume;
        sfxSource.volume = 1f * masterVolume;
    }

    // CALLED FROM SETTINGS SLIDER
    public void SetMasterVolume(float value)
    {
        masterVolume = value;

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();

        ApplyVolume();
    }

    // UI CLICK
    public void PlayClick()
    {
        if (clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene carregada: " + scene.name);

        string name = scene.name;

        if (name == "Level1" || name == "Level2" || name == "Level3")
        {
            Debug.Log("Parar música");

            StopMusic();
            return;
        }
        Debug.Log("Garantir música");

        EnsureMenuMusic();
    }

    private void EnsureMenuMusic()
    {
        Debug.Log("EnsureMenuMusic");

        if (mainMenuMusic == null)
        {
            Debug.Log("mainMenuMusic é NULL");
            return;
        }

        Debug.Log("isPlaying = " + musicSource.isPlaying);

        musicSource.clip = mainMenuMusic;

        if (!musicSource.isPlaying)
        {
            Debug.Log("Play()");
            musicSource.Play();
        }
    }

    // MENU MUSIC
    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic == null) return;

        musicSource.clip = mainMenuMusic;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}

//using UnityEngine;
 //using UnityEngine.SceneManagement;

//public class UIAudioManager : MonoBehaviour
//{
//    public static UIAudioManager Instance;

//    [Header("Audio Sources")]
//    private AudioSource musicSource;
//    private AudioSource sfxSource;

//    [Header("Clips")]
//    public AudioClip clickSound;
//    public AudioClip mainMenuMusic;

//    void Awake()
//    {
//        if (Instance != null)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        // MUSIC SOURCE
//        musicSource = gameObject.AddComponent<AudioSource>();
//        musicSource.loop = true;
//        musicSource.playOnAwake = false;
//        musicSource.volume = 0.5f;

//        // SFX SOURCE
//        sfxSource = gameObject.AddComponent<AudioSource>();
//        sfxSource.playOnAwake = false;
//        sfxSource.spatialBlend = 0f;
//        sfxSource.volume = 1f;

//        SceneManager.sceneLoaded += OnSceneLoaded;

//        PlayMainMenuMusic();
//    }

//    // UI CLICK
//    public void PlayClick()
//    {
//        if (clickSound != null)
//            sfxSource.PlayOneShot(clickSound);
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        string name = scene.name;

//        // Gameplay scenes - parar música
//        if (name == "Level1" || name == "Level2" || name == "Level3")
//        {
//            StopMusic();
//            return;
//        }
//        if (!musicSource.isPlaying)
//        {
//            PlayMainMenuMusic();
//        }
//    }

//    // MENU MUSIC
//    public void PlayMainMenuMusic()
//    {
//        if (mainMenuMusic == null)
//            return;

//        if (musicSource.clip == mainMenuMusic && musicSource.isPlaying)
//            return;

//        musicSource.clip = mainMenuMusic;
//        musicSource.Play();
//    }

//    public void StopMusic()
//    {
//        musicSource.Stop();
//    }
//}
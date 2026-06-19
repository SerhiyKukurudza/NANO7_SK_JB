using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponSoundManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Weapon Firing Sounds")]
    [SerializeField] private AudioClip pistolFireSound;
    [SerializeField] private AudioClip rifleFireSound;
    [SerializeField] private AudioClip shotgunFireSound;

    [Header("Volume")]
    [SerializeField] private float fireVolume = 1f;

    private WeaponManager weaponManager;
    private static WeaponSoundManager instance;
    private float masterVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
        LoadMasterVolumeFromPrefs();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        LoadMasterVolumeFromPrefs();
    }

    public void PlayWeaponFireSound()
    {
        if (weaponManager == null)
        {
            weaponManager = FindFirstObjectByType<WeaponManager>();
        }

        if (weaponManager == null)
            return;

        int currentWeaponIndex = weaponManager.GetCurrentWeaponIndex();
        PlayWeaponFireSound(currentWeaponIndex);
    }

    public void PlayWeaponFireSound(int weaponIndex)
    {
        AudioClip clipToPlay = null;

        switch (weaponIndex)
        {
            case 0:
                clipToPlay = pistolFireSound;
                break;
            case 1:
                clipToPlay = rifleFireSound;
                break;
            case 2:
                clipToPlay = shotgunFireSound;
                break;
            default:
                return;
        }

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay, fireVolume * masterVolume);
        }
    }

    public static WeaponSoundManager GetInstance()
    {
        return instance;
    }

    public void SetFireVolume(float volume)
    {
        fireVolume = Mathf.Clamp01(volume);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadMasterVolumeFromPrefs();

        if (weaponManager == null)
        {
            weaponManager = FindFirstObjectByType<WeaponManager>();
        }
    }

    private void LoadMasterVolumeFromPrefs()
    {
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVolume", 1f));
    }
}

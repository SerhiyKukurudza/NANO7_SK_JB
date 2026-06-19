using UnityEngine;

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

    void Awake()
    {
        // Singleton pattern
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

        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound (not spatial)
    }

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
    }

    /// <summary>
    /// Plays the firing sound for the current weapon
    /// </summary>
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

    /// <summary>
    /// Plays the firing sound for a specific weapon by index
    /// </summary>
    public void PlayWeaponFireSound(int weaponIndex)
    {
        AudioClip clipToPlay = null;

        switch (weaponIndex)
        {
            case 0: // Pistol
                clipToPlay = pistolFireSound;
                break;
            case 1: // Rifle
                clipToPlay = rifleFireSound;
                break;
            case 2: // Shotgun
                clipToPlay = shotgunFireSound;
                break;
            default:
                return;
        }

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay, fireVolume);
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
}

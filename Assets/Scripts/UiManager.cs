using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    private static bool loadingFromSave;

    public static bool ConsumeLoadingFromSaveFlag()
    {
        if (!loadingFromSave)
        {
            return false;
        }

        loadingFromSave = false;
        return true;
    }

    public static UiManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject managerObject = new GameObject("UIManager");
        return managerObject.AddComponent<UiManager>();
    }

    [System.Serializable]
    private class SaveHistoryEntry
    {
        public string action;
        public string sceneName;
        public string utcTimestamp;
    }

    [System.Serializable]
    private class GameSaveData
    {
        public string sceneName;
        public int levelReached = 1;
        public int currentHealth = 100;
        public int maxHealth = 100;
        public int currentWeaponIndex = -1;
        public bool[] unlockedWeapons = new bool[3];
        public bool hasPlayerTransform;
        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;
        public int saveCount;
        public int loadCount;
        public string lastSaveUtc;
        public string lastLoadUtc;
        public List<SaveHistoryEntry> history = new List<SaveHistoryEntry>();
    }

    [Header("Systems")]
    public UIAudioManager audioManager;
    public Image brightnessOverlay;

    private const string SaveFileName = "savegame.json";
    private GameSaveData pendingRestoreData;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (audioManager == null)
            audioManager = GetComponentInChildren<UIAudioManager>();
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        bool isGameplayScene = sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3";

        if (!isGameplayScene)
        {
            return;
        }

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveGameNow();
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            LoadSavedGame();
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            HealPlayerToFull();
        }

        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
            UnlockAllLevels();
        }

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            UnlockAllWeapons();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    public bool HasSavedGame()
    {
        return File.Exists(GetSaveFilePath());
    }

    public bool SaveGameNow()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool isGameplayScene = sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3";

        if (!isGameplayScene)
        {
            return false;
        }

        GameSaveData data = LoadDataFromDisk();
        if (data == null)
        {
            data = new GameSaveData();
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        Transform playerTransform = ResolvePlayerTransform(playerHealth);

        data.sceneName = sceneName;
        data.levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        data.currentHealth = playerHealth != null ? playerHealth.GetCurrentHealth() : data.currentHealth;
        data.maxHealth = playerHealth != null ? playerHealth.GetMaxHealth() : data.maxHealth;
        data.currentWeaponIndex = weaponManager != null ? weaponManager.GetCurrentWeaponIndex() : data.currentWeaponIndex;
        data.unlockedWeapons = BuildUnlockedWeaponArray(weaponManager);
        data.hasPlayerTransform = playerTransform != null;
        if (playerTransform != null)
        {
            Vector3 position = playerTransform.position;
            data.playerPosX = position.x;
            data.playerPosY = position.y;
            data.playerPosZ = position.z;
        }
        data.saveCount += 1;
        data.lastSaveUtc = System.DateTime.UtcNow.ToString("o");
        AppendHistory(data, "save", sceneName);

        File.WriteAllText(GetSaveFilePath(), JsonUtility.ToJson(data, true));
        PlayerPrefs.Save();
        return true;
    }

    public bool LoadSavedGame()
    {
        GameSaveData data = LoadDataFromDisk();
        if (data == null || string.IsNullOrWhiteSpace(data.sceneName))
        {
            return false;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;

        data.loadCount += 1;
        data.lastLoadUtc = System.DateTime.UtcNow.ToString("o");
        AppendHistory(data, "load", data.sceneName);
        File.WriteAllText(GetSaveFilePath(), JsonUtility.ToJson(data, true));

        loadingFromSave = true;
        pendingRestoreData = data;
        SceneManager.LoadScene(data.sceneName);
        return true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pendingRestoreData == null || pendingRestoreData.sceneName != scene.name)
        {
            return;
        }

        StartCoroutine(ApplyPendingRestoreAfterSceneStart());
    }

    private IEnumerator ApplyPendingRestoreAfterSceneStart()
    {
        // Wait one frame so scene objects complete Start() and don't override restored state.
        yield return null;

        if (pendingRestoreData == null)
        {
            yield break;
        }

        PlayerPrefs.SetInt("LevelReached", pendingRestoreData.levelReached);
        PlayerPrefs.Save();

        PlayerHealth playerHealth = FindPlayerHealth();
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(pendingRestoreData.maxHealth, false);
            playerHealth.SetHealth(pendingRestoreData.currentHealth);
        }

        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.RestoreWeaponState(pendingRestoreData.unlockedWeapons, pendingRestoreData.currentWeaponIndex);
        }

        if (pendingRestoreData.hasPlayerTransform)
        {
            Transform playerTransform = ResolvePlayerTransform(playerHealth);
            if (playerTransform != null)
            {
                Vector3 savedPosition = new Vector3(
                    pendingRestoreData.playerPosX,
                    pendingRestoreData.playerPosY,
                    pendingRestoreData.playerPosZ);

                Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = playerTransform.GetComponentInChildren<Rigidbody2D>();
                }

                if (rb != null)
                {
                    rb.position = savedPosition;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                else
                {
                    playerTransform.position = savedPosition;
                }
            }
        }

        pendingRestoreData = null;
        loadingFromSave = false;
    }

    private string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private GameSaveData LoadDataFromDisk()
    {
        string path = GetSaveFilePath();
        if (!File.Exists(path))
        {
            return null;
        }

        return JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));
    }

    private bool[] BuildUnlockedWeaponArray(WeaponManager weaponManager)
    {
        bool[] unlocked = new bool[3];

        if (weaponManager == null)
        {
            return unlocked;
        }

        for (int i = 0; i < unlocked.Length; i++)
        {
            unlocked[i] = weaponManager.HasWeaponUnlocked(i);
        }

        return unlocked;
    }

    private void AppendHistory(GameSaveData data, string action, string sceneName)
    {
        if (data.history == null)
        {
            data.history = new List<SaveHistoryEntry>();
        }

        data.history.Add(new SaveHistoryEntry
        {
            action = action,
            sceneName = sceneName,
            utcTimestamp = System.DateTime.UtcNow.ToString("o")
        });
    }

    private void HealPlayerToFull()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.SetHealth(playerHealth.GetMaxHealth());
    }

    private void UnlockAllLevels()
    {
        PlayerPrefs.SetInt("LevelReached", 3);
        PlayerPrefs.Save();
    }

    private void UnlockAllWeapons()
    {
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager == null)
        {
            return;
        }

        weaponManager.RestoreWeaponState(new bool[] { true, true, true }, 0);
    }

    private Transform ResolvePlayerTransform(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
        {
            return null;
        }

        Transform transformToUse = playerHealth.transform;
        if (playerHealth.transform.root != null && playerHealth.transform.root.CompareTag("Player"))
        {
            transformToUse = playerHealth.transform.root;
        }

        return transformToUse;
    }

    private PlayerHealth FindPlayerHealth()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            return health;
        }

        GameObject taggedPlayer = GameObject.FindWithTag("Player");
        if (taggedPlayer == null)
        {
            return null;
        }

        health = taggedPlayer.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = taggedPlayer.GetComponentInChildren<PlayerHealth>();
        }

        return health;
    }

}
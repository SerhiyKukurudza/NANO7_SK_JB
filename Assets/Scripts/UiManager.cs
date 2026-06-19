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
    private static bool godModeEnabled;

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

        UiManager existing = FindFirstObjectByType<UiManager>();
        if (existing != null)
        {
            return existing;
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

    [Header("Gameplay HUD")]
    [SerializeField] private GameObject persistentGameplayHudRoot;

    [Header("Cheats")]
    [SerializeField] private bool cheatsEnabled = true;
    [SerializeField] private bool logCheatActions = true;
    [SerializeField] private bool showCheatOverlay = true;
    [SerializeField] private float cheatMessageDuration = 2.5f;

    private const string SaveFileName = "savegame.json";
    private GameSaveData pendingRestoreData;
    private string cheatOverlayMessage = string.Empty;
    private float cheatOverlayUntilTime;
    private GUIStyle cheatOverlayStyle;

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

        if (!cheatsEnabled)
        {
            cheatsEnabled = true;
        }

        EnsureGameplayHudForScene(SceneManager.GetActiveScene().name);
        RebindPlayerHealthToPersistentHud();
    }

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool isGameplayScene = sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3";

        if (!isGameplayScene)
        {
            return;
        }

        if (!cheatsEnabled)
        {
            return;
        }

        HandleCheatInputs();
    }

    private void HandleCheatInputs()
    {
        if (WasCheatKeyPressed(KeyCode.F1, Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame))
        {
            ToggleGodMode();
        }

        if (WasCheatKeyPressed(KeyCode.F5, Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame))
        {
            if (SaveGameNow())
            {
                LogCheat("F5: Save game");
            }
        }

        if (WasCheatKeyPressed(KeyCode.F9, Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame))
        {
            if (LoadSavedGame())
            {
                LogCheat("F9: Load game");
            }
        }

        if (WasCheatKeyPressed(KeyCode.F6, Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame))
        {
            HealPlayerToFull();
            LogCheat("F6: Heal to full");
        }

        if (WasCheatKeyPressed(KeyCode.F7, Keyboard.current != null && Keyboard.current.f7Key.wasPressedThisFrame))
        {
            UnlockAllLevels();
            LogCheat("F7: Unlock all levels");
        }

        if (WasCheatKeyPressed(KeyCode.F8, Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame))
        {
            UnlockAllWeapons();
            LogCheat("F8: Unlock all weapons");
        }

        if (WasCheatKeyPressed(KeyCode.F10, Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame))
        {
            LoadNextGameplayLevel();
        }

        if (WasCheatKeyPressed(KeyCode.F12, Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame))
        {
            ForcePlayerDeath();
            LogCheat("F12: Force player death");
        }
    }

    private bool WasCheatKeyPressed(KeyCode legacyKey, bool inputSystemPressed)
    {
        bool legacyPressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        legacyPressed = Input.GetKeyDown(legacyKey);
#endif
        return legacyPressed || inputSystemPressed;
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

        EnsureGameplayHudForScene(scene.name);
        RebindPlayerHealthToPersistentHud();

        if (pendingRestoreData == null || pendingRestoreData.sceneName != scene.name)
        {
            return;
        }

        StartCoroutine(ApplyPendingRestoreAfterSceneStart());
    }

    private IEnumerator ApplyPendingRestoreAfterSceneStart()
    {
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
            playerHealth.SetInvincible(godModeEnabled);
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
        PlayerHealth playerHealth = FindPlayerHealth();
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

    private void ToggleGodMode()
    {
        godModeEnabled = !godModeEnabled;
        ApplyGodModeToCurrentPlayer();
        LogCheat(godModeEnabled ? "F1: God Mode ON" : "F1: God Mode OFF");
    }

    private void ApplyGodModeToCurrentPlayer()
    {
        PlayerHealth playerHealth = FindPlayerHealth();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.SetInvincible(godModeEnabled);
    }

    private void LoadNextGameplayLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = null;

        if (currentScene == "Level1")
        {
            nextScene = "Level2";
            PlayerPrefs.SetInt("LevelReached", Mathf.Max(PlayerPrefs.GetInt("LevelReached", 1), 2));
        }
        else if (currentScene == "Level2")
        {
            nextScene = "Level3";
            PlayerPrefs.SetInt("LevelReached", Mathf.Max(PlayerPrefs.GetInt("LevelReached", 1), 3));
        }
        else if (currentScene == "Level3")
        {
            nextScene = "WinMenu";
        }

        if (string.IsNullOrWhiteSpace(nextScene))
        {
            return;
        }

        PlayerPrefs.Save();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nextScene);
        LogCheat($"F10: Jump to {nextScene}");
    }

    private void ForcePlayerDeath()
    {
        PlayerHealth playerHealth = FindPlayerHealth();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.ForceKill();
    }

    private void LogCheat(string message)
    {
        ShowCheatOverlayMessage(message);

        if (!logCheatActions)
        {
            return;
        }

        Debug.Log($"[CHEAT] {message}");
    }

    private void ShowCheatOverlayMessage(string message)
    {
        if (!showCheatOverlay)
        {
            return;
        }

        cheatOverlayMessage = message;
        cheatOverlayUntilTime = Time.unscaledTime + Mathf.Max(0.5f, cheatMessageDuration);
    }

    private void OnGUI()
    {
        if (!showCheatOverlay)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(cheatOverlayMessage) || Time.unscaledTime > cheatOverlayUntilTime)
        {
            return;
        }

        if (cheatOverlayStyle == null)
        {
            cheatOverlayStyle = new GUIStyle(GUI.skin.box);
            cheatOverlayStyle.fontSize = 18;
            cheatOverlayStyle.alignment = TextAnchor.MiddleCenter;
            cheatOverlayStyle.normal.textColor = Color.white;
            cheatOverlayStyle.padding = new RectOffset(12, 12, 8, 8);
        }

        float msgWidth = Mathf.Min(Screen.width - 40f, 520f);
        Rect area = new Rect((Screen.width - msgWidth) * 0.5f, 20f, msgWidth, 44f);
        GUI.Box(area, cheatOverlayMessage, cheatOverlayStyle);
    }

    private void EnsureGameplayHudForScene(string sceneName)
    {
        bool gameplayScene = IsGameplayScene(sceneName);

        if (!gameplayScene)
        {
            if (persistentGameplayHudRoot != null)
            {
                persistentGameplayHudRoot.SetActive(false);
            }
            return;
        }

        if (persistentGameplayHudRoot == null)
        {
            persistentGameplayHudRoot = FindGameplayHudRoot();
        }

        if (persistentGameplayHudRoot == null)
        {
            return;
        }

        GameObject hudRoot = persistentGameplayHudRoot.transform.root.gameObject;
        if (persistentGameplayHudRoot != hudRoot)
        {
            persistentGameplayHudRoot = hudRoot;
        }

        DontDestroyOnLoad(persistentGameplayHudRoot);

        persistentGameplayHudRoot.SetActive(true);

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        HashSet<GameObject> processedRoots = new HashSet<GameObject>();
        for (int i = 0; i < canvases.Length; i++)
        {
            GameObject candidateRoot = canvases[i].transform.root.gameObject;

            if (!processedRoots.Add(candidateRoot))
            {
                continue;
            }

            if (candidateRoot == persistentGameplayHudRoot)
            {
                continue;
            }

            if (candidateRoot.GetComponentInChildren<HotbarController>(true) != null)
            {
                Destroy(candidateRoot);
            }
        }
    }

    private GameObject FindGameplayHudRoot()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            GameObject candidateRoot = canvases[i].transform.root.gameObject;
            bool hasHotbar = candidateRoot.GetComponentInChildren<HotbarController>(true) != null;
            if (hasHotbar)
            {
                return candidateRoot;
            }
        }

        return null;
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3";
    }

    private void RebindPlayerHealthToPersistentHud()
    {
        if (persistentGameplayHudRoot == null)
        {
            return;
        }

        HealthBar hudHealthBar = persistentGameplayHudRoot.GetComponentInChildren<HealthBar>(true);
        if (hudHealthBar == null)
        {
            return;
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.BindHealthBar(hudHealthBar, true);
    }

}

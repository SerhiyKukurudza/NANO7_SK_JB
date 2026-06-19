using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionLoader : MonoBehaviour
{
    private static LevelTransitionLoader instance;

    private class TransitionState
    {
        public int maxHealth = 100;
        public int currentHealth = 100;
        public bool[] unlockedWeapons = new bool[3];
        public int selectedWeaponIndex = -1;
        public int sizeStateId = 1;
    }

    [SerializeField] private float defaultLoadingDuration = 1.5f;
    private bool transitionInProgress;

    public static void LoadLevelWithInterlude(string targetSceneName, int reachedLevelValue, string loadingSceneName, float loadingDuration = -1f)
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            return;
        }

        LevelTransitionLoader loader = EnsureInstance();
        if (loader.transitionInProgress)
        {
            return;
        }

        loader.StartCoroutine(loader.RunTransition(targetSceneName, reachedLevelValue, loadingSceneName, loadingDuration));
    }

    private static LevelTransitionLoader EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject loaderObject = new GameObject("LevelTransitionLoader");
        instance = loaderObject.AddComponent<LevelTransitionLoader>();
        DontDestroyOnLoad(loaderObject);
        return instance;
    }

    private IEnumerator RunTransition(string targetSceneName, int reachedLevelValue, string loadingSceneName, float loadingDuration)
    {
        transitionInProgress = true;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        UiManager.EnsureInstance();

        TransitionState transitionState = CaptureTransitionState();

        if (!IsSceneInBuild(targetSceneName))
        {
            transitionInProgress = false;
            yield break;
        }

        if (reachedLevelValue > 0)
        {
            int currentReached = PlayerPrefs.GetInt("LevelReached", 1);
            PlayerPrefs.SetInt("LevelReached", Mathf.Max(currentReached, reachedLevelValue));
            PlayerPrefs.Save();
        }

        if (!string.IsNullOrWhiteSpace(loadingSceneName) && IsSceneInBuild(loadingSceneName))
        {
            SceneManager.LoadScene(loadingSceneName);
            float waitTime = loadingDuration > 0f ? loadingDuration : defaultLoadingDuration;
            if (waitTime > 0f)
            {
                yield return new WaitForSecondsRealtime(waitTime);
            }
        }

        SceneManager.LoadScene(targetSceneName);
        yield return null;

        ApplyTransitionState(transitionState);
        transitionInProgress = false;
    }

    private TransitionState CaptureTransitionState()
    {
        TransitionState state = new TransitionState();

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            state.maxHealth = playerHealth.GetMaxHealth();
            state.currentHealth = playerHealth.GetCurrentHealth();
        }

        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            state.selectedWeaponIndex = weaponManager.GetCurrentWeaponIndex();
            for (int i = 0; i < state.unlockedWeapons.Length; i++)
            {
                state.unlockedWeapons[i] = weaponManager.HasWeaponUnlocked(i);
            }
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            state.sizeStateId = playerMovement.GetSizeStateId();
        }

        return state;
    }

    private void ApplyTransitionState(TransitionState state)
    {
        if (state == null)
        {
            return;
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(state.maxHealth, true);
            playerHealth.SetHealth(state.currentHealth);
        }

        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.RestoreWeaponState(state.unlockedWeapons, state.selectedWeaponIndex);
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetSizeStateId(state.sizeStateId);
        }
    }

    private bool IsSceneInBuild(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = Path.GetFileNameWithoutExtension(scenePath);
            if (sceneFileName == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}

using UnityEngine;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string nextLevelName;
    [SerializeField] private int nextLevelValue;
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float loadingDuration = 1.5f;
    [SerializeField] private bool requireFinalBarrierOpen = true;
    [SerializeField] private BarrierTrap finalBarrier;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        bool isPlayer = other.CompareTag("Player") || other.transform.root.CompareTag("Player");
        if (!isPlayer)
        {
            return;
        }

        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        if (requireFinalBarrierOpen && IsFinalBarrierStillClosed())
        {
            return;
        }

        LevelTransitionLoader.LoadLevelWithInterlude(nextLevelName, nextLevelValue, loadingSceneName, loadingDuration);
    }

    private bool IsFinalBarrierStillClosed()
    {
        BarrierTrap resolvedBarrier = ResolveFinalBarrier();
        if (resolvedBarrier != null)
        {
            if (!resolvedBarrier.gameObject.activeInHierarchy)
            {
                return false;
            }

            return !resolvedBarrier.IsOpen;
        }

        BarrierTrap[] barriers = FindObjectsByType<BarrierTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < barriers.Length; i++)
        {
            BarrierTrap barrier = barriers[i];
            if (barrier == null || !barrier.IsFinalBarrier)
            {
                continue;
            }

            if (barrier.gameObject.activeInHierarchy && !barrier.IsOpen)
            {
                return true;
            }
        }

        GameObject barrierObject = GameObject.Find("Final_Barrier");
        if (barrierObject == null || !barrierObject.activeInHierarchy)
        {
            return false;
        }

        BarrierTrap barrierTrap = barrierObject.GetComponent<BarrierTrap>();
        if (barrierTrap != null)
        {
            return barrierTrap.IsFinalBarrier && !barrierTrap.IsOpen;
        }

        return true;
    }

    private BarrierTrap ResolveFinalBarrier()
    {
        if (finalBarrier != null)
        {
            return finalBarrier;
        }

        BarrierTrap[] barriers = FindObjectsByType<BarrierTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < barriers.Length; i++)
        {
            if (barriers[i] != null && barriers[i].IsFinalBarrier)
            {
                finalBarrier = barriers[i];
                return finalBarrier;
            }
        }

        return null;
    }
}

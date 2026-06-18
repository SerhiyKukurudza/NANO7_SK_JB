using UnityEngine;

[DisallowMultipleComponent]
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private int weaponIndex; // 0 = Pistol, 1 = Rifle, 2 = Shotgun
    
    private WeaponManager weaponManager;
    private bool pickedUp = false;

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager not found in scene!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!pickedUp && collision.CompareTag("Player"))
        {
            PickupWeapon();
        }
    }

    private void PickupWeapon()
    {
        if (weaponManager == null) return;
        if (pickedUp) return; // Prevent double pickup
        
        pickedUp = true;
        Debug.Log($"[WeaponPickup] PickupWeapon triggered on {gameObject.name}, weaponIndex: {weaponIndex}");

        // Prevent additional trigger events immediately.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        Debug.Log($"[WeaponPickup] Disabled {colliders.Length} colliders");
        
        weaponManager.PickupWeapon(weaponIndex);

        // Notify displays in the local pickup group (parent + siblings) to fade out.
        Transform visualRoot = transform.parent != null ? transform.parent : transform;
        Debug.Log($"[WeaponPickup] Visual root: {visualRoot.gameObject.name}");
        WeaponPickupDisplay[] allDisplays = visualRoot.GetComponentsInChildren<WeaponPickupDisplay>(true);
        Debug.Log($"[WeaponPickup] Found {allDisplays.Length} displays in {visualRoot.gameObject.name}");
        bool hasAnyDisplay = false;

        for (int i = 0; i < allDisplays.Length; i++)
        {
            if (allDisplays[i] != null)
            {
                allDisplays[i].OnPickedUp();
                hasAnyDisplay = true;
                Debug.Log($"[WeaponPickup] Called OnPickedUp on display {i}: {allDisplays[i].gameObject.name}");
            }
        }

        if (!hasAnyDisplay)
        {
            WeaponPickupDisplay[] sceneDisplays = FindObjectsByType<WeaponPickupDisplay>(FindObjectsSortMode.None);
            Debug.Log($"[WeaponPickup] Searching scene for matching display, found {sceneDisplays.Length} total displays");
            for (int i = 0; i < sceneDisplays.Length; i++)
            {
                if (sceneDisplays[i] != null && sceneDisplays[i].GetWeaponIndex() == weaponIndex)
                {
                    Debug.Log($"[WeaponPickup] Matched scene display by weaponIndex on {sceneDisplays[i].gameObject.name}");
                    sceneDisplays[i].OnPickedUp();
                    hasAnyDisplay = true;
                }
            }
        }

        if (hasAnyDisplay)
        {
            // Safety fallback in case fade/destroy is interrupted.
            Debug.Log($"[WeaponPickup] Destroying visual root {visualRoot.gameObject.name} after 0.6s");
            Destroy(visualRoot.gameObject, 0.6f);
        }
        else
        {
            Debug.LogWarning($"[WeaponPickup] No displays found! Hiding renderers and destroying {visualRoot.gameObject.name}");
            // If no display, hide any renderer immediately and destroy.
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }

            Destroy(visualRoot.gameObject);
        }
        
        Debug.Log("Weapon picked up: " + weaponIndex);
    }

    public int GetWeaponIndex()
    {
        return weaponIndex;
    }
}

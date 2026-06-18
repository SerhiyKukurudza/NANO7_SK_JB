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
        // Prevent additional trigger events immediately.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        weaponManager.PickupWeapon(weaponIndex);

        // Notify displays in the local pickup group (parent + siblings) to fade out.
        Transform visualRoot = transform.parent != null ? transform.parent : transform;
        WeaponPickupDisplay[] allDisplays = visualRoot.GetComponentsInChildren<WeaponPickupDisplay>(true);
        bool hasAnyDisplay = false;

        for (int i = 0; i < allDisplays.Length; i++)
        {
            if (allDisplays[i] != null)
            {
                allDisplays[i].OnPickedUp();
                hasAnyDisplay = true;
            }
        }

        if (!hasAnyDisplay)
        {
            WeaponPickupDisplay[] sceneDisplays = FindObjectsByType<WeaponPickupDisplay>(FindObjectsSortMode.None);
            for (int i = 0; i < sceneDisplays.Length; i++)
            {
                if (sceneDisplays[i] != null && sceneDisplays[i].GetWeaponIndex() == weaponIndex)
                {
                    sceneDisplays[i].OnPickedUp();
                    hasAnyDisplay = true;
                }
            }
        }

        if (hasAnyDisplay)
        {
            // Safety fallback in case fade/destroy is interrupted.
            Destroy(visualRoot.gameObject, 0.6f);
        }
        else
        {
            // If no display, hide any renderer immediately and destroy.
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }

            Destroy(visualRoot.gameObject);
        }
    }

    public int GetWeaponIndex()
    {
        return weaponIndex;
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private int weaponIndex;
    
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
        if (pickedUp) return;
        
        pickedUp = true;
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        weaponManager.PickupWeapon(weaponIndex);

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
            Destroy(visualRoot.gameObject, 0.6f);
        }
        else
        {
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

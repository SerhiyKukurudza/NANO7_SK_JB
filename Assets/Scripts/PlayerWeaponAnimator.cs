using UnityEngine;

public class PlayerWeaponAnimator : MonoBehaviour
{
    private Animator animator;
    private WeaponManager weaponManager;
    
    // Animator parameter hashes
    private int hashHasWeapon = Animator.StringToHash("HasWeapon");
    private int hashHasPistol = Animator.StringToHash("HasPistol");
    private int hashHasRifle = Animator.StringToHash("HasRifle");
    private int hashHasShotgun = Animator.StringToHash("HasShotgun");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            SetWeaponFlags(-1);
        }
    }

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        
        if (animator == null)
        {
            Debug.LogError("Animator not found on Player!");
            return;
        }
        
        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager not found in scene!");
            return;
        }

        if (!HasBoolParameter("HasWeapon") || !HasBoolParameter("HasPistol") || !HasBoolParameter("HasRifle") || !HasBoolParameter("HasShotgun"))
        {
            Debug.LogError("Animator is missing one or more Bool parameters: HasWeapon, HasPistol, HasRifle, HasShotgun.");
        }
        
        // Initialize with no weapon selected.
        SetWeaponFlags(-1);
        
        // Subscribe to weapon change events
        weaponManager.OnWeaponChanged += OnWeaponChanged;
    }

    void Update()
    {
        if (animator == null || weaponManager == null)
        {
            return;
        }

        int currentIndex = weaponManager.GetCurrentWeaponIndex();
        bool hasWeapon = currentIndex >= 0 && weaponManager.HasWeaponUnlocked(currentIndex);

        SetWeaponFlags(hasWeapon ? currentIndex : -1);
    }

    private void OnWeaponChanged(WeaponData newWeapon, int slotIndex)
    {
        if (animator == null) return;
        
        SetWeaponFlags(slotIndex);
        Debug.Log("Player animation updated: weapon flags set for slot " + slotIndex + " (" + newWeapon.weaponName + ")");
    }

    private void SetWeaponFlags(int weaponIndex)
    {
        if (!HasBoolParameter("HasWeapon") || !HasBoolParameter("HasPistol") || !HasBoolParameter("HasRifle") || !HasBoolParameter("HasShotgun"))
        {
            return;
        }

        bool hasWeapon = weaponIndex >= 0;
        animator.SetBool(hashHasWeapon, hasWeapon);
        animator.SetBool(hashHasPistol, weaponIndex == 0);
        animator.SetBool(hashHasRifle, weaponIndex == 1);
        animator.SetBool(hashHasShotgun, weaponIndex == 2);
    }

    private bool HasBoolParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        return false;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
        }
    }
}

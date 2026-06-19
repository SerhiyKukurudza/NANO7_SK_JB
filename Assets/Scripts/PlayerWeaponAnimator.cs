using UnityEngine;

public class PlayerWeaponAnimator : MonoBehaviour
{
    private const string HasWeaponParameter = "HasWeapon";
    private const string HasPistolParameter = "HasPistol";
    private const string HasRifleParameter = "HasRifle";
    private const string HasShotgunParameter = "HasShotgun";
    private const string IsShootingParameter = "isShooting";
    private const string VelocityParameter = "Velocity";

    private Animator animator;
    private WeaponManager weaponManager;
    private Rigidbody2D rb;
    
    private int hashHasWeapon = Animator.StringToHash(HasWeaponParameter);
    private int hashHasPistol = Animator.StringToHash(HasPistolParameter);
    private int hashHasRifle = Animator.StringToHash(HasRifleParameter);
    private int hashHasShotgun = Animator.StringToHash(HasShotgunParameter);
    private int hashIsShooting = Animator.StringToHash(IsShootingParameter);
    private int hashVelocity = Animator.StringToHash(VelocityParameter);
    private int hashShootPistolState = Animator.StringToHash("Base Layer.Player_Shoot_Pistol");
    private int hashShootRifleState = Animator.StringToHash("Base Layer.Player_Shoot_Rifle");
    private int hashShootShotgunState = Animator.StringToHash("Base Layer.Player_Shoot_Shotgun");

    [SerializeField] private float shootHoldDuration = 0.18f;
    [SerializeField] private float idleShootVelocityThreshold = 0.05f;
    [SerializeField] private float shootCrossfadeDuration = 0.05f;

    private float shootUntilTime;
    private int activeShootStateHash;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (animator != null)
        {
            SetWeaponFlags(-1);
            SetShootState(false);
        }
    }

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        
        if (animator == null)
        {
            return;
        }
        
        if (weaponManager == null)
        {
            return;
        }

        if (!HasBoolParameter(HasWeaponParameter) || !HasBoolParameter(HasPistolParameter) || !HasBoolParameter(HasRifleParameter) || !HasBoolParameter(HasShotgunParameter))
        {
            return;
        }
        
        SetWeaponFlags(-1);
        
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

        if (shootUntilTime > 0f && Time.time >= shootUntilTime)
        {
            shootUntilTime = 0f;
            SetShootState(false);
            activeShootStateHash = 0;
        }
    }

    private void OnWeaponChanged(WeaponData newWeapon, int slotIndex)
    {
        if (animator == null) return;
        
        SetWeaponFlags(slotIndex);
    }

    private void SetWeaponFlags(int weaponIndex)
    {
        if (!HasBoolParameter(HasWeaponParameter) || !HasBoolParameter(HasPistolParameter) || !HasBoolParameter(HasRifleParameter) || !HasBoolParameter(HasShotgunParameter))
        {
            return;
        }

        bool hasWeapon = weaponIndex >= 0;
        animator.SetBool(hashHasWeapon, hasWeapon);
        animator.SetBool(hashHasPistol, weaponIndex == 0);
        animator.SetBool(hashHasRifle, weaponIndex == 1);
        animator.SetBool(hashHasShotgun, weaponIndex == 2);
    }

    public void TriggerShoot()
    {
        if (animator == null)
        {
            return;
        }

        if (!HasBoolParameter(IsShootingParameter))
        {
            return;
        }

        if (!IsIdleEnoughForShoot())
        {
            return;
        }

        int shootStateHash = GetShootStateHash();
        if (shootStateHash != 0 && HasState(shootStateHash))
        {
            animator.CrossFade(shootStateHash, shootCrossfadeDuration);
            activeShootStateHash = shootStateHash;
        }

        shootUntilTime = Time.time + Mathf.Max(0.05f, shootHoldDuration);
        SetShootState(true);
    }

    private int GetShootStateHash()
    {
        int currentIndex = weaponManager != null ? weaponManager.GetCurrentWeaponIndex() : -1;

        switch (currentIndex)
        {
            case 0:
                return hashShootPistolState;
            case 1:
                return hashShootRifleState;
            case 2:
                return hashShootShotgunState;
            default:
                return 0;
        }
    }

    private bool IsIdleEnoughForShoot()
    {
        if (rb != null)
        {
            return Mathf.Abs(rb.linearVelocity.x) <= idleShootVelocityThreshold;
        }

        if (HasFloatParameter(VelocityParameter))
        {
            return animator.GetFloat(hashVelocity) <= idleShootVelocityThreshold;
        }

        return true;
    }

    private void SetShootState(bool value)
    {
        if (HasBoolParameter(IsShootingParameter))
        {
            animator.SetBool(hashIsShooting, value);
        }
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

    private bool HasFloatParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Float)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasState(int stateHash)
    {
        return animator != null && animator.HasState(0, stateHash);
    }

    void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    private WeaponManager weaponManager;
    private SpriteRenderer spriteRenderer;
    private PlayerWeaponAnimator playerWeaponAnimator;
    private float nextFireTime;

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerWeaponAnimator = GetComponent<PlayerWeaponAnimator>();
    }

    void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        TryShoot();
    }

    private void TryShoot()
    {
        if (projectilePrefab == null) return;
        if (weaponManager == null) return;

        WeaponData weapon = weaponManager.GetCurrentWeapon();
        if (weapon == null) return;

        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + 1f / Mathf.Max(weapon.fireRate, 0.1f);
        playerWeaponAnimator?.TriggerShoot();

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        bool facingLeft = spriteRenderer != null && spriteRenderer.flipX;
        Vector2 direction = facingLeft ? Vector2.left : Vector2.right;

        GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = obj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, weapon.damage, weapon.range);
        }
    }
}

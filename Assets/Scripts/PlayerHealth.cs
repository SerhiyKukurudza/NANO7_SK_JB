using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Cheats")]
    [SerializeField] private bool invincible;

    [Header("Death")]
    [SerializeField] private bool loadGameOverOnDeath = true;
    [SerializeField] private float gameOverDelaySeconds = 1.2f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<int, int> onHealthChanged;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;

    private bool isDead;
    private int temporaryInvincibilityCounter;

    private void Awake()
    {
        EnsureHealthBarBinding();
    }

    private HealthBar FindPlayerHealthBar()
    {
        // Prefer a HealthBar inside the same HUD root that contains the HotbarController.
        HotbarController hotbar = FindFirstObjectByType<HotbarController>();
        if (hotbar != null)
        {
            HealthBar hudBar = hotbar.GetComponentInParent<Canvas>()?.GetComponentInChildren<HealthBar>(true);
            if (hudBar != null)
            {
                return hudBar;
            }
        }

        HealthBar[] all = FindObjectsByType<HealthBar>(FindObjectsSortMode.None);
        foreach (HealthBar bar in all)
        {
            Canvas canvas = bar.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return bar;
            }
        }
        return null;
    }

    private void EnsureHealthBarBinding()
    {
        if (healthBar == null)
        {
            healthBar = FindPlayerHealthBar();
        }
    }

    public void BindHealthBar(HealthBar bar, bool refreshNow = true)
    {
        if (bar == null)
        {
            return;
        }

        healthBar = bar;

        if (refreshNow)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        EnsureHealthBarBinding();

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (invincible || temporaryInvincibilityCounter > 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);

        EnsureHealthBarBinding();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        EnsureHealthBarBinding();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);

        EnsureHealthBarBinding();

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int value, bool keepCurrentHealth = true)
    {
        value = Mathf.Max(1, value);
        maxHealth = value;

        if (keepCurrentHealth)
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        else
        {
            currentHealth = maxHealth;
        }

        EnsureHealthBarBinding();

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetInvincible(bool value)
    {
        invincible = value;
    }

    public bool IsInvincible()
    {
        return invincible;
    }

    public void SetTemporaryInvincible(bool value)
    {
        if (value)
        {
            temporaryInvincibilityCounter++;
            return;
        }

        temporaryInvincibilityCounter = Mathf.Max(0, temporaryInvincibilityCounter - 1);
    }

    public void ForceKill()
    {
        if (isDead)
        {
            return;
        }

        currentHealth = 0;
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = GetComponentInChildren<PlayerMovement>();
        }
        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            playerMovement.Die();
        }

        onDeath?.Invoke();

        if (loadGameOverOnDeath)
        {
            StartCoroutine(LoadGameOverAfterDelay());
        }
    }

    private IEnumerator LoadGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelaySeconds);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("GameOverMenu");
    }

}

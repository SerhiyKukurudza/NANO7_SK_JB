using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Death")]
    [SerializeField] private bool loadGameOverOnDeath = true;
    [SerializeField] private float gameOverDelaySeconds = 1.2f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<int, int> onHealthChanged;

    private HealthBar healthBar;
    private bool isDead;

    private void Awake()
    {
        healthBar = FindFirstObjectByType<HealthBar>();
    }

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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

        currentHealth = Mathf.Max(currentHealth - amount, 0);

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

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);

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

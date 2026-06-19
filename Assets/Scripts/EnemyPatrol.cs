using UnityEngine;
using UnityEngine.UI;

public class EnemyPatrol : MonoBehaviour
{
    //public float testDamageInterval = 1f;
    //private float timer;

    public Transform[] patrolPoints;
    public float speed = 2f;
    private int i;
    private SpriteRenderer spriteRenderer;

    private HealthBar healthBar;
    private bool isDead;

    public int maxHealth = 100;
    private int currentHealth;

    public int damage = 1;
    [Header("Progression")]
    [SerializeField] private bool isBoss;
    [SerializeField] private string bossNameContains = "Boss";

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }
    void Update()
    {
        if (Vector2.Distance(transform.position, patrolPoints[i].position) < 0.25f)
        {
            i++;
            if (i == patrolPoints.Length)
            {
                i = 0;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, patrolPoints[i].position, speed * Time.deltaTime);
        spriteRenderer.flipX = (transform.position.x - patrolPoints[i].position.x) < 0f;

        //timer += Time.deltaTime;

        //if (timer >= testDamageInterval)
        //{
        //    TakeDamage(5);
        //    timer = 0f;
        //}
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth == 0)
        {
            isDead = true;
            HandleDeathSideEffects();
            Destroy(gameObject);
        }
    }

    private void HandleDeathSideEffects()
    {
        if (!IsBoss())
        {
            return;
        }

        BarrierTrap[] barriers = FindObjectsByType<BarrierTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < barriers.Length; i++)
        {
            if (barriers[i] != null && barriers[i].IsFinalBarrier)
            {
                barriers[i].OpenBarrier();
            }
        }

        GameObject finalBarrierObject = GameObject.Find("Final_Barrier");
        if (finalBarrierObject == null)
        {
            return;
        }

        BarrierTrap barrierTrap = finalBarrierObject.GetComponent<BarrierTrap>();
        if (barrierTrap != null && barrierTrap.IsFinalBarrier)
        {
            barrierTrap.OpenBarrier();
            return;
        }
    }

    private bool IsBoss()
    {
        if (isBoss)
        {
            return true;
        }

        if (CompareTag("Boss") || transform.root.CompareTag("Boss"))
        {
            return true;
        }

        string objectName = gameObject.name;
        if (!string.IsNullOrEmpty(objectName) && objectName.Contains(bossNameContains))
        {
            return true;
        }

        string rootName = transform.root != null ? transform.root.name : string.Empty;
        return !string.IsNullOrEmpty(rootName) && rootName.Contains(bossNameContains);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ApplyDamageIfPlayer(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ApplyDamageIfPlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyDamageIfPlayer(other.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ApplyDamageIfPlayer(collision.gameObject);
    }

    private void ApplyDamageIfPlayer(GameObject candidate)
    {
        if (candidate == null)
        {
            return;
        }

        if (!candidate.CompareTag("Player") && !candidate.transform.root.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = candidate.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = candidate.GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            playerHealth = candidate.GetComponentInChildren<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
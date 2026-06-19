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

    public int maxHealth = 100;
    private int currentHealth;

    public int damage = 1;

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
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth == 0)
        {
            Destroy(gameObject);
        }
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
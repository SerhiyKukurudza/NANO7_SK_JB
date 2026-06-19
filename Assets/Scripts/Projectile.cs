using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;

    private Vector2 direction;
    private int damage;
    private float range;
    private float distanceTraveled;

    public void Initialize(Vector2 dir, int dmg, float rng)
    {
        direction = dir.normalized;
        damage = dmg;
        range = rng;

        // Flip sprite if travelling left
        if (direction.x < 0f)
        {
            Vector3 s = transform.localScale;
            transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
        }
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.Translate(direction * step, Space.World);
        distanceTraveled += step;

        if (distanceTraveled >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the player's own collider
        if (other.CompareTag("Player")) return;

        // Try to apply damage to an enemy
        EnemyPatrol enemy = other.GetComponent<EnemyPatrol>();
        if (enemy == null)
            enemy = other.GetComponentInParent<EnemyPatrol>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Destroy on any solid (non-trigger) surface
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}

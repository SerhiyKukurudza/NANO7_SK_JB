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
        if (other.CompareTag("Player")) return;

        EnemyPatrol enemy = other.GetComponent<EnemyPatrol>();
        if (enemy == null)
            enemy = other.GetComponentInParent<EnemyPatrol>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 2f;
    private int i;
    private SpriteRenderer spriteRenderer;

    public int damage = 1;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Player player = other.GetComponent<Player>();
    //        if (player != null)
    //        {
    //            player.TakeDamage(GameManager.Instance.enemyDamage);
    //        }
    //    }
    //}
}
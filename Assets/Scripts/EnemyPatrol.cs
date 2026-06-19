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
    private Animator animator;
    private Rigidbody2D rb;

    private HealthBar healthBar;
    private bool isDead;

    public int maxHealth = 100;
    private int currentHealth;

    public int damage = 1;
    
    [Header("Attack Settings")]
    [SerializeField] private float detectionDistance = 5f;
    [SerializeField] private float attackPushForce = 10f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private bool limitAttackToFirstTwoPoints = true;
    
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private GameObject targetPlayer;
    
    [Header("Progression")]
    [SerializeField] private bool isBoss;
    [SerializeField] private string bossNameContains = "Boss";

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
        if (isAttacking)
        {
            // Para inimigos normais, teleporta se chegar muito perto
            if (!IsBoss())
            {
                // Verificar se colidiu com o player durante o ataque
                if (targetPlayer == null)
                {
                    targetPlayer = GameObject.FindGameObjectWithTag("Player");
                }
                
                if (targetPlayer != null)
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    // Se muito perto durante o ataque, teleporta
                    if (distanceToPlayer < 1f)
                    {
                        TeleportToRandomPoint();
                        return;
                    }
                }
                
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    isAttacking = false;
                    if (animator != null)
                    {
                        animator.SetBool("IsAttacking", false);
                    }
                    // Depois de atacar, volta ao ponto 0
                    i = 0;
                    
                    // Parar o movimento do Rigidbody
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                }
                return;
            }
            else
            {
                // Para BOSS: apenas contar o tempo de animação, continua patrulhando
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    isAttacking = false;
                    if (animator != null)
                    {
                        animator.SetBool("IsAttacking", false);
                    }
                }
                // Não return, continua para patrulhar
            }
        }

        // Detectar player
        DetectPlayer();

        // Se não está atacando, patrulhar normalmente (apenas entre pontos 0 e 1 se limitado)
        int maxPatrolIndex = (limitAttackToFirstTwoPoints && patrolPoints.Length > 1) ? 1 : patrolPoints.Length - 1;
        
        if (Vector2.Distance(transform.position, patrolPoints[i].position) < 0.25f)
        {
            i++;
            if (i > maxPatrolIndex)
            {
                i = 0;
            }
        }
        
        transform.position = Vector2.MoveTowards(transform.position, patrolPoints[i].position, speed * Time.deltaTime);
        spriteRenderer.flipX = (transform.position.x - patrolPoints[i].position.x) < 0f;
    }

    private void DetectPlayer()
    {
        // Encontrar o player se ainda não foi encontrado
        if (targetPlayer == null)
        {
            targetPlayer = GameObject.FindGameObjectWithTag("Player");
        }

        if (targetPlayer == null)
        {
            return;
        }

        // Verificar se o player está dentro da zona permitida (entre pontos 0 e 1)
        if (limitAttackToFirstTwoPoints && patrolPoints.Length > 1)
        {
            if (!IsPlayerInAllowedZone())
            {
                return;
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToPlayer < detectionDistance)
        {
            // Iniciar ataque para ambos (BOSS e inimigos normais)
            StartAttackAnimation(targetPlayer.transform.position);
        }
    }

    private void StartAttackAnimation(Vector3 playerPosition)
    {
        if (isAttacking)
        {
            return; // Já está atacando
        }

        isAttacking = true;
        attackTimer = attackDuration;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
        }

        // Determinar direção para o player
        Vector2 directionToPlayer = (playerPosition - transform.position).normalized;
        spriteRenderer.flipX = directionToPlayer.x < 0f;

        // Para inimigos normais, aplicar movimento de ataque
        if (!IsBoss())
        {
            ApplyAttackMovement(directionToPlayer);
        }
    }

    private void ApplyAttackMovement(Vector2 directionToPlayer)
    {
        if (rb == null)
        {
            return;
        }

        Vector2 attackVelocity = directionToPlayer * attackPushForce;
        
        // Limitar o movimento dentro da zona permitida
        if (limitAttackToFirstTwoPoints && patrolPoints.Length > 1)
        {
            float minX = Mathf.Min(patrolPoints[0].position.x, patrolPoints[1].position.x);
            float maxX = Mathf.Max(patrolPoints[0].position.x, patrolPoints[1].position.x);
            
            // Se tentar sair dos limites, reduza a velocidade
            if (transform.position.x + attackVelocity.x * Time.deltaTime < minX || 
                transform.position.x + attackVelocity.x * Time.deltaTime > maxX)
            {
                attackVelocity.x = 0;
            }
        }
        
        rb.linearVelocity = attackVelocity;
    }
    private bool IsPlayerInAllowedZone()
    {
        if (patrolPoints.Length < 2)
        {
            return true;
        }

        Vector3 playerPos = targetPlayer.transform.position;
        float minX = Mathf.Min(patrolPoints[0].position.x, patrolPoints[1].position.x);
        float maxX = Mathf.Max(patrolPoints[0].position.x, patrolPoints[1].position.x);

        return playerPos.x >= minX && playerPos.x <= maxX;
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
        // Se está atacando e colidiu com o player, teleporta aleatoriamente
        if (isAttacking && IsPlayerCollision(other.gameObject))
        {
            TeleportToRandomPoint();
            return;
        }
        
        ApplyDamageIfPlayer(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se está atacando e colidiu com o player, teleporta aleatoriamente
        if (isAttacking && IsPlayerCollision(collision.gameObject))
        {
            TeleportToRandomPoint();
            return;
        }
        
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

    private bool IsPlayerCollision(GameObject candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        return candidate.CompareTag("Player") || candidate.transform.root.CompareTag("Player");
    }

    private void TeleportToRandomPoint()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            return;
        }

        // Resetar a velocidade PRIMEIRO
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Escolher aleatoriamente entre ponto 0 e ponto 1
        int randomPoint = Random.Range(0, 2);
        transform.position = patrolPoints[randomPoint].position;
        i = randomPoint;

        // Resetar o ataque
        isAttacking = false;
        attackTimer = 0f;
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
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
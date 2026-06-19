using UnityEngine;
using System.Collections.Generic;

public class LaserSpikeTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float spikeActiveStart = 1.0833f;
    [SerializeField] private float spikeActiveEnd = 2.0f;
    [SerializeField] private float animationDuration = 3.5166667f;
    
    private Animator animator;
    private float lastDamageTime = -999f;
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();

    private HealthBar healthBar;

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("LaserSpikeTrap: No Animator found on this GameObject!");
        }   
    }

    private void Update()
    {
        foreach (GameObject player in playersInTrigger)
        {
            if (player == null)
                continue;

            if (IsSpikesActive())
            {
                ApplyDamageIfNeeded(player);
            }
        }
    }

    private bool IsSpikesActive()
    {
        if (animator == null)
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        float normalizedTime = stateInfo.normalizedTime % 1f;
        
        float currentTime = normalizedTime * animationDuration;
        
        return currentTime >= spikeActiveStart && currentTime <= spikeActiveEnd;
    }

    private void ApplyDamageIfNeeded(GameObject player)
    {
        if (Time.time - lastDamageTime < damageInterval)
            return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            playerHealth = player.GetComponentInChildren<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            playersInTrigger.Add(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            playersInTrigger.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            playersInTrigger.Remove(collision.gameObject);
        }
    }

    public void SetSpikeTiming(float activeStart, float activeEnd, float totalDuration)
    {
        spikeActiveStart = activeStart;
        spikeActiveEnd = activeEnd;
        animationDuration = totalDuration;
    }
}

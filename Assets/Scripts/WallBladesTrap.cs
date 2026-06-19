using UnityEngine;
using System.Collections.Generic;

public class WallBladesTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float bladesActiveStart = 2.3166667f;
    [SerializeField] private float bladesActiveEnd = 3.55f;
    [SerializeField] private float animationDuration = 3.85f;
    
    private Animator animator;
    private float lastDamageTime = -999f;
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("WallBladesTrap: No Animator found on this GameObject!");
        }
    }

    private void Update()
    {
        foreach (GameObject player in playersInTrigger)
        {
            if (player == null)
                continue;

            if (AreBaradesActive())
            {
                ApplyDamageIfNeeded(player);
            }
        }
    }

    private bool AreBaradesActive()
    {
        if (animator == null)
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        float normalizedTime = stateInfo.normalizedTime % 1f;
        
        float currentTime = normalizedTime * animationDuration;
        
        return currentTime >= bladesActiveStart && currentTime <= bladesActiveEnd;
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

    public void SetBladesTiming(float activeStart, float activeEnd, float totalDuration)
    {
        bladesActiveStart = activeStart;
        bladesActiveEnd = activeEnd;
        animationDuration = totalDuration;
    }
}

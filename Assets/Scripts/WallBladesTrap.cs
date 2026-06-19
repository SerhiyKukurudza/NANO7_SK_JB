using UnityEngine;
using System.Collections.Generic;

public class WallBladesTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f; // Delay between damage hits
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float bladesActiveStart = 2.3166667f; // 2:19 (frame 139 @ 60fps)
    [SerializeField] private float bladesActiveEnd = 3.55f;       // 3:33 (frame 213 @ 60fps)
    [SerializeField] private float animationDuration = 3.85f;     // Total animation length
    
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
        // Check each player currently in the trigger
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

        // Get the current animation state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Get the normalized time (0 to 1) of the current animation
        float normalizedTime = stateInfo.normalizedTime % 1f; // % 1 to handle looping
        
        // Convert normalized time to actual seconds
        float currentTime = normalizedTime * animationDuration;
        
        // Check if we're in the "blades active" window
        return currentTime >= bladesActiveStart && currentTime <= bladesActiveEnd;
    }

    private void ApplyDamageIfNeeded(GameObject player)
    {
        // Only apply damage if enough time has passed since last damage
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
        // Ensure the player is in our set (in case they were added late)
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

    // Method to update timings if needed (useful for testing or if animation changes)
    public void SetBladesTiming(float activeStart, float activeEnd, float totalDuration)
    {
        bladesActiveStart = activeStart;
        bladesActiveEnd = activeEnd;
        animationDuration = totalDuration;
    }
}

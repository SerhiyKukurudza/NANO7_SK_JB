using UnityEngine;
using System.Collections.Generic;

public class SawTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f; // Delay between damage hits
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float animationDuration = 0.76666665f; // Total animation length
    // Saw rotates during the entire animation cycle
    [SerializeField] private bool damageWholeAnimation = true;
    
    private Animator animator;
    private float lastDamageTime = -999f;
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("SawTrap: No Animator found on this GameObject!");
        }
    }

    private void Update()
    {
        // Check each player currently in the trigger
        foreach (GameObject player in playersInTrigger)
        {
            if (player == null)
                continue;

            // Saw always does damage during animation
            if (damageWholeAnimation)
            {
                ApplyDamageIfNeeded(player);
            }
        }
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
}

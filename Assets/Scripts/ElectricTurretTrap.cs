using UnityEngine;
using System.Collections.Generic;

public class ElectricTurretTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f; // Delay between damage hits
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float fireStart = 2.0f;           // 2:00
    [SerializeField] private float fireEnd = 2.55f;            // 2:33 (2 seconds + 33 frames @ 60fps)
    [SerializeField] private float animationDuration = 2.6833334f; // Total animation length
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private float soundVolume = 1f;
    
    private Animator animator;
    private AudioSource audioSource;
    private float lastDamageTime = -999f;
    private float lastFireSoundTime = -999f;
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("ElectricTurretTrap: No Animator found on this GameObject!");
        }
        
        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 2D sound
    }

    private void Update()
    {
        // Check each player currently in the trigger
        foreach (GameObject player in playersInTrigger)
        {
            if (player == null)
                continue;

            if (IsTurretFiring())
            {
                ApplyDamageIfNeeded(player);
            }
        }
    }

    private bool IsTurretFiring()
    {
        if (animator == null)
            return false;

        // Get the current animation state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Get the normalized time (0 to 1) of the current animation
        float normalizedTime = stateInfo.normalizedTime % 1f; // % 1 to handle looping
        
        // Convert normalized time to actual seconds
        float currentTime = normalizedTime * animationDuration;
        
        // Check if we're in the "firing" window
        bool isFiring = currentTime >= fireStart && currentTime <= fireEnd;
        
        // Play fire sound once per firing cycle
        if (isFiring && Time.time - lastFireSoundTime > 1f) // Ensure we don't play sound too frequently
        {
            PlayFireSound();
            lastFireSoundTime = Time.time;
        }
        
        return isFiring;
    }

    private void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound, soundVolume);
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

    // Method to update timings if needed (useful for testing or if animation changes)
    public void SetFireTiming(float start, float end, float totalDuration)
    {
        fireStart = start;
        fireEnd = end;
        animationDuration = totalDuration;
    }
}

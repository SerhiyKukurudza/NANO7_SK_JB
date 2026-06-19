using UnityEngine;
using System.Collections.Generic;

public class ElectricTurretTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float fireStart = 2.0f;
    [SerializeField] private float fireEnd = 2.55f;
    [SerializeField] private float animationDuration = 2.6833334f;
    
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void Update()
    {
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

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        float normalizedTime = stateInfo.normalizedTime % 1f;
        
        float currentTime = normalizedTime * animationDuration;
        
        bool isFiring = currentTime >= fireStart && currentTime <= fireEnd;
        
        if (isFiring && Time.time - lastFireSoundTime > 1f)
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

    public void SetFireTiming(float start, float end, float totalDuration)
    {
        fireStart = start;
        fireEnd = end;
        animationDuration = totalDuration;
    }
}

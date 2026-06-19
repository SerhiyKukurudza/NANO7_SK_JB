using UnityEngine;
using System.Collections.Generic;

public class SawTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float animationDuration = 0.76666665f;
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
        foreach (GameObject player in playersInTrigger)
        {
            if (player == null)
                continue;

            if (damageWholeAnimation)
            {
                ApplyDamageIfNeeded(player);
            }
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
}

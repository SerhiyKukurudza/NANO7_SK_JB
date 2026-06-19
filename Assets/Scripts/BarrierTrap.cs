using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BarrierTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f; // Delay between damage hits
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float lasersActiveStart = 0.1666667f; // 0:10 (10 frames at 60fps)
    [SerializeField] private float lasersActiveEnd = 0.5833333f;   // 0:35 (35 frames at 60fps)
    [SerializeField] private float animationDuration = 2.5166667f; // Total animation length

    [Header("Open Settings")]
    [SerializeField] private bool isFinalBarrier;
    [SerializeField] private string openTriggerParameter = "Open";
    [SerializeField] private string openBoolParameter = "IsOpen";
    [SerializeField] private float disableColliderDelay = 0.15f;
    [SerializeField] private bool deactivateAfterOpen = true;
    [SerializeField] private float deactivateDelay = 0.8f;
    
    private Animator animator;
    private Collider2D barrierCollider;
    private float lastDamageTime = -999f;
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();
    private bool isOpen;

    public bool IsFinalBarrier => isFinalBarrier;
    public bool IsOpen => isOpen;

    private void Start()
    {
        animator = GetComponent<Animator>();
        barrierCollider = GetComponent<Collider2D>();
        
        if (animator == null)
        {
            Debug.LogError("BarrierTrap: No Animator found on this GameObject!");
        }
    }

    private void Update()
    {
        if (isOpen)
        {
            return;
        }

        // Check each player currently in the trigger
        foreach (GameObject player in playersInTrigger)
        {
            if (player == null)
                continue;

            if (AreLasersActive())
            {
                ApplyDamageIfNeeded(player);
            }
        }
    }

    private bool AreLasersActive()
    {
        if (animator == null)
            return false;

        // Get the current animation state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Get the normalized time (0 to 1) of the current animation
        float normalizedTime = stateInfo.normalizedTime % 1f; // % 1 to handle looping
        
        // Convert normalized time to actual seconds
        float currentTime = normalizedTime * animationDuration;
        
        // Check if we're in the "lasers active" window
        return currentTime >= lasersActiveStart && currentTime <= lasersActiveEnd;
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
    public void SetLaserTiming(float activeStart, float activeEnd, float totalDuration)
    {
        lasersActiveStart = activeStart;
        lasersActiveEnd = activeEnd;
        animationDuration = totalDuration;
    }

    public void OpenBarrier()
    {
        if (!isFinalBarrier)
        {
            return;
        }

        if (isOpen)
        {
            return;
        }

        isOpen = true;
        playersInTrigger.Clear();

        if (animator != null)
        {
            if (HasTriggerParameter(openTriggerParameter))
            {
                animator.SetTrigger(openTriggerParameter);
            }

            if (HasBoolParameter(openBoolParameter))
            {
                animator.SetBool(openBoolParameter, true);
            }
        }

        StartCoroutine(DisableBarrierFlow());
    }

    private IEnumerator DisableBarrierFlow()
    {
        if (disableColliderDelay > 0f)
        {
            yield return new WaitForSeconds(disableColliderDelay);
        }

        if (barrierCollider != null)
        {
            barrierCollider.enabled = false;
        }

        enabled = false;

        if (!deactivateAfterOpen)
        {
            yield break;
        }

        if (deactivateDelay > 0f)
        {
            yield return new WaitForSeconds(deactivateDelay);
        }

        gameObject.SetActive(false);
    }

    private bool HasTriggerParameter(string parameterName)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBoolParameter(string parameterName)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        return false;
    }
}

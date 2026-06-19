using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BarrierTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;
    
    [Header("Animation Timings (in seconds)")]
    [SerializeField] private float lasersActiveStart = 0.1666667f;
    [SerializeField] private float lasersActiveEnd = 0.5833333f;
    [SerializeField] private float animationDuration = 2.5166667f;

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

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        float normalizedTime = stateInfo.normalizedTime % 1f;
        
        float currentTime = normalizedTime * animationDuration;
        
        return currentTime >= lasersActiveStart && currentTime <= lasersActiveEnd;
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

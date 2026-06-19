using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private enum PlayerSizeState
    {
        Small,
        Normal,
        Large
    }

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float customGravity = -9.81f;
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Dash")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashRecoveryCooldown = 0.5f;
    [SerializeField] private float dashDuration = 0.3f;

    [Header("Size")]
    [SerializeField] private Vector3 smallScale = new Vector3(4.5f, 4.5f, 4.5f);
    [SerializeField] private Vector3 normalScale = new Vector3(6f, 6f, 6f);
    [SerializeField] private Vector3 largeScale = new Vector3(7.5f, 7.5f, 7.5f);
    [SerializeField] private float smallSpeedMultiplier = 1.25f;
    [SerializeField] private float largeSpeedMultiplier = 0.8f;
    [SerializeField] private float smallJumpMultiplier = 1.2f;
    [SerializeField] private float largeJumpMultiplier = 0.8f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;
    
    private Vector2 velocity = Vector2.zero;
    private float movementInput = 0f;
    private bool isGrounded = false;
    private bool isDashing = false;
    private float nextDashCooldown = 0f;
    private float baseMovementSpeed;
    private float baseJumpForce;
    private PlayerSizeState currentSizeState = PlayerSizeState.Normal;
    
    private int hashVelocity = Animator.StringToHash("Velocity");
    private int hashJump = Animator.StringToHash("Jump");
    private int hashDash = Animator.StringToHash("Dash");
    private int hashDeath = Animator.StringToHash("Death");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        baseMovementSpeed = movementSpeed;
        baseJumpForce = jumpForce;
        normalScale = transform.localScale;
        ApplySizeState(currentSizeState);
        
        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundChecker.position, groundCheckRadius, groundLayer);
        
        movementInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            movementInput = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            movementInput = 1f;
        
        ControlMovement();
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }
        
        if (Keyboard.current.shiftKey.wasPressedThisFrame && nextDashCooldown <= 0 && !isDashing)
        {
            Dash();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            DecreaseSize();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            IncreaseSize();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetSize();
        }
        
        if (nextDashCooldown > 0)
        {
            nextDashCooldown -= Time.deltaTime;
        }
        
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        velocity = rb.linearVelocity;
        
        if (!isDashing)
        {
            velocity.x = movementInput * movementSpeed;
        }
        
        if (!isGrounded)
        {
            velocity.y += customGravity * Time.fixedDeltaTime;
        }
        else
        {
            if (velocity.y < 0)
            {
                velocity.y = 0f;
            }
        }
        
        rb.linearVelocity = velocity;
    }

    private void ControlMovement()
    {
        if (movementInput > 0.1f)
        {
            spriteRenderer.flipX = false;
        }
        else if (movementInput < -0.1f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void Jump()
    {
        animator.SetTrigger(hashJump);
        
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    private void Dash()
    {
        if (isDashing) return;
        
        isDashing = true;
        nextDashCooldown = dashRecoveryCooldown;
        playerHealth?.SetTemporaryInvincible(true);
        
        animator.SetTrigger(hashDash);
        
        float dashDirection = spriteRenderer.flipX ? -1f : 1f;
        velocity.x = dashDirection * dashForce;
        rb.linearVelocity = velocity;
        
        Invoke(nameof(StopDash), dashDuration);
    }

    private void StopDash()
    {
        isDashing = false;
        playerHealth?.SetTemporaryInvincible(false);
    }

    private void UpdateAnimations()
    {
        float absoluteVelocity = Mathf.Abs(velocity.x);
        animator.SetFloat(hashVelocity, absoluteVelocity);
    }

    public void Die()
    {
        animator.SetTrigger(hashDeath);
        rb.linearVelocity = Vector2.zero;
        enabled = false;
    }

    public void Reset()
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        nextDashCooldown = 0f;
        enabled = true;
    }

    private void DecreaseSize()
    {
        if (currentSizeState == PlayerSizeState.Normal)
        {
            ApplySizeState(PlayerSizeState.Small);
            return;
        }

        if (currentSizeState == PlayerSizeState.Large)
        {
            ApplySizeState(PlayerSizeState.Normal);
        }
    }

    private void IncreaseSize()
    {
        if (currentSizeState == PlayerSizeState.Small)
        {
            ApplySizeState(PlayerSizeState.Normal);
            return;
        }

        if (currentSizeState == PlayerSizeState.Normal)
        {
            ApplySizeState(PlayerSizeState.Large);
        }
    }

    private void ResetSize()
    {
        ApplySizeState(PlayerSizeState.Normal);
    }

    private void ApplySizeState(PlayerSizeState newState)
    {
        currentSizeState = newState;

        switch (currentSizeState)
        {
            case PlayerSizeState.Small:
                transform.localScale = smallScale;
                movementSpeed = baseMovementSpeed * smallSpeedMultiplier;
                jumpForce = baseJumpForce * smallJumpMultiplier;
                break;

            case PlayerSizeState.Large:
                transform.localScale = largeScale;
                movementSpeed = baseMovementSpeed * largeSpeedMultiplier;
                jumpForce = baseJumpForce * largeJumpMultiplier;
                break;

            default:
                transform.localScale = normalScale;
                movementSpeed = baseMovementSpeed;
                jumpForce = baseJumpForce;
                break;
        }
    }

    public int GetSizeStateId()
    {
        return (int)currentSizeState;
    }

    public void SetSizeStateId(int stateId)
    {
        if (stateId < (int)PlayerSizeState.Small || stateId > (int)PlayerSizeState.Large)
        {
            stateId = (int)PlayerSizeState.Normal;
        }

        ApplySizeState((PlayerSizeState)stateId);
    }

}

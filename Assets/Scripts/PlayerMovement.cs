using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
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
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private Vector2 velocity = Vector2.zero;
    private float movementInput = 0f;
    private bool isGrounded = false;
    private bool isDashing = false;
    private float nextDashCooldown = 0f;
    
    // Animator parameter hashes (for performance)
    private int hashVelocity = Animator.StringToHash("Velocity");
    private int hashJump = Animator.StringToHash("Jump");
    private int hashDash = Animator.StringToHash("Dash");
    private int hashDeath = Animator.StringToHash("Death");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        if (animator == null) Debug.LogError("Animator not found!");
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found!");
        if (groundChecker == null) Debug.LogError("Ground checker not assigned!");
        
        // Configure Rigidbody2D for platformer
        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        }
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundChecker.position, groundCheckRadius, groundLayer);
        
        // Get movement input (Keyboard)
        movementInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            movementInput = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            movementInput = 1f;
        
        // Control horizontal movement
        ControlMovement();
        
        // Control jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }
        
        // Control dash (Shift)
        if (Keyboard.current.shiftKey.wasPressedThisFrame && nextDashCooldown <= 0 && !isDashing)
        {
            Dash();
        }
        
        // Update dash cooldown timer
        if (nextDashCooldown > 0)
        {
            nextDashCooldown -= Time.deltaTime;
        }
        
        // Update animations
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // Get current velocity
        velocity = rb.linearVelocity;
        
        // Apply horizontal movement (only if not dashing)
        if (!isDashing)
        {
            velocity.x = movementInput * movementSpeed;
        }
        
        // Apply custom gravity
        if (!isGrounded)
        {
            // In air - apply gravity
            velocity.y += customGravity * Time.fixedDeltaTime;
        }
        else
        {
            // On ground - no falling velocity
            if (velocity.y < 0)
            {
                velocity.y = 0f;
            }
        }
        
        // Apply velocity to Rigidbody2D
        rb.linearVelocity = velocity;
    }

    private void ControlMovement()
    {
        // Flip sprite based on direction
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
        // Trigger animation FIRST
        animator.SetTrigger(hashJump);
        
        // Apply jump force
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    private void Dash()
    {
        if (isDashing) return;
        
        isDashing = true;
        nextDashCooldown = dashRecoveryCooldown;
        
        // Trigger animation FIRST
        animator.SetTrigger(hashDash);
        
        // Determine dash direction and apply movement
        float dashDirection = spriteRenderer.flipX ? -1f : 1f;
        velocity.x = dashDirection * dashForce;
        rb.linearVelocity = velocity;
        
        // Stop dash after duration
        Invoke(nameof(StopDash), dashDuration);
    }

    private void StopDash()
    {
        isDashing = false;
    }

    private void UpdateAnimations()
    {
        // Update velocity parameter (for idle/run)
        float absoluteVelocity = Mathf.Abs(velocity.x);
        animator.SetFloat(hashVelocity, absoluteVelocity);
    }

    // Public method to kill the player
    public void Die()
    {
        animator.SetTrigger(hashDeath);
        rb.linearVelocity = Vector2.zero;
        enabled = false;
    }

    // Method to reset the player (if necessary)
    public void Reset()
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        nextDashCooldown = 0f;
        enabled = true;
    }

}

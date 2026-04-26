using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NormalToyMovement : MonoBehaviour
{
    public ToyEnergy toyEnergy;

    public float moveSpeed = 8f;
    public float airSpeedMultiplier = 0.9f;  // Max horizontal speed in air (fraction of moveSpeed)
    public float acceleration = 13f;
    public float deceleration = 16f;
    public float airControlMultiplier = 0.75f; // How quickly you can accelerate/change dir in air
    
    public float jumpForce = 11f;
    public float fallGravityMultiplier = 2.5f;
    public float riseGravityMultiplier = 1.4f; // Gravity strength while rising (>1 = snappier/punchier rise)
    public float maxFallSpeed = 15f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
    public float turnSpeed = 20f;

    [Header("Walk Juice")]
    public float walkWobbleSpeed = 15f;
    public float walkWobbleAngle = 10f;
    public float walkWobbleReturnSpeed = 10f;

    [Header("Sound FX")]
    public AudioClip jumpSound;
    public AudioClip wooshSound;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public bool IsControlled { get; private set; } = false;
    // True when the toy has horizontal input OR is airborne (jumping/falling)
    public bool IsMoving => Mathf.Abs(horizontalInput) > 0.01f || !isGrounded;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float defaultGravity;
    private bool isGrounded;
    private bool isFacingRight = true;
    private float originalScaleX;
    private float targetScaleX;
    private float wobbleTimer;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        defaultGravity = rb.gravityScale;
        originalScaleX = Mathf.Abs(transform.localScale.x);
        targetScaleX = originalScaleX;

        // Automatically fix the "sticking to walls / can't jump while touching a wall" issue
        // by removing physics friction from the player so they slide smoothly against walls.
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
        noFriction.friction = 0f;
        noFriction.bounciness = 0f;
        
        rb.sharedMaterial = noFriction;
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.sharedMaterial = noFriction;
        }
    }

    public void SetControl(bool state)
    {
        IsControlled = state;
        if (state)
        {
            PlayWoosh();
        }
        else
        {
            horizontalInput = 0f;
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Mathf.Abs(transform.localScale.x - targetScaleX) > 0.01f)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Lerp(currentScale.x, targetScaleX, turnSpeed * Time.deltaTime);
            transform.localScale = currentScale;
        }

        if (!IsControlled || toyEnergy == null || !toyEnergy.HasEnergy)
        {
            horizontalInput = 0f;
            
            if (anim != null)
            {
                bool playJumpAnim = !isGrounded && (coyoteTimeCounter <= 0f || rb.linearVelocity.y > 1.5f);
                anim.SetBool("isJumping", playJumpAnim);
                anim.SetFloat("xVelocity", 0f);
                anim.SetFloat("yVelocity", rb.linearVelocity.y);
            }
            
            return; 
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0f && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0f && isFacingRight)
        {
            Flip();
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            PlayJump();
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
            // Apply fall gravity immediately so the character doesn't float after jump cut
            rb.gravityScale = defaultGravity * fallGravityMultiplier;
        }

        if (Mathf.Abs(horizontalInput) > 0.01f && isGrounded)
        {
            wobbleTimer += Time.deltaTime * walkWobbleSpeed;
            float zRotation = Mathf.Sin(wobbleTimer) * walkWobbleAngle;
            
            Vector3 euler = transform.localEulerAngles;
            euler.z = zRotation;
            transform.localEulerAngles = euler;
        }
        else
        {
            wobbleTimer = 0f;
            Vector3 euler = transform.localEulerAngles;
            euler.z = Mathf.LerpAngle(euler.z, 0f, Time.deltaTime * walkWobbleReturnSpeed);
            transform.localEulerAngles = euler;
        }

        if (anim != null)
        {
            // Prevent tiny bumps/wobbles from flickering the jump animation
            bool playJumpAnim = !isGrounded && (coyoteTimeCounter <= 0f || rb.linearVelocity.y > 1f);
            anim.SetBool("isJumping", playJumpAnim);
            anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        if (!IsControlled || toyEnergy == null || !toyEnergy.HasEnergy)
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.gravityScale = defaultGravity * fallGravityMultiplier;
            }
            else
            {
                rb.gravityScale = defaultGravity;
            }
            
            rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
            return; 
        }

        float currentSpeed = isGrounded ? moveSpeed : moveSpeed * airSpeedMultiplier;
        float targetSpeed = horizontalInput * currentSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        
        float baseAccelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float accelRate = isGrounded ? baseAccelRate : baseAccelRate * airControlMultiplier;
        
        rb.AddForce(accelRate * speedDiff * Vector2.right);

        if (rb.linearVelocity.y < 0)
        {
            // Falling: fast fall
            rb.gravityScale = defaultGravity * fallGravityMultiplier;
        }
        else if (isGrounded)
        {
            // On ground: reset gravity so next jump starts clean
            rb.gravityScale = defaultGravity;
        }
        else if (rb.linearVelocity.y > 0)
        {
            // Rising: slightly increased gravity for a punchy, fast-paced rise
            rb.gravityScale = defaultGravity * riseGravityMultiplier;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        targetScaleX = isFacingRight ? originalScaleX : -originalScaleX;
    }

    // ── Sound helpers ─────────────────────────────────────────────────

    private void PlayJump()
    {
        if (jumpSound == null || SoundFXManager.instance == null) return;
        SoundFXManager.instance.PlaySoundFXClip(jumpSound, transform, sfxVolume);
    }

    private void PlayWoosh()
    {
        if (wooshSound == null || SoundFXManager.instance == null) return;
        SoundFXManager.instance.PlaySoundFXClip(wooshSound, transform, sfxVolume);
    }
}
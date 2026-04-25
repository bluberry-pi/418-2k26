using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NormalToyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float airSpeedMultiplier = 0.8f;
    public float acceleration = 13f;
    public float deceleration = 16f;
    public float airControlMultiplier = 0.5f;
    
    [Header("Jumping")]
    public float jumpForce = 13f;
    public float fallGravityMultiplier = 2.5f;
    public float maxFallSpeed = 15f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    
    [Header("Environment")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    [Header("Visuals")]
    public float turnSpeed = 20f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float defaultGravity;
    private bool isGrounded;
    private bool isFacingRight = true;
    
    private float originalScaleX;
    private float targetScaleX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
        
        // Store the starting size so we know what to scale back to
        originalScaleX = Mathf.Abs(transform.localScale.x);
        targetScaleX = originalScaleX;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Input checks for flipping
        if (horizontalInput > 0f && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0f && isFacingRight)
        {
            Flip();
        }

        // Smoothly transition the scale to make the flip look natural
        if (Mathf.Abs(transform.localScale.x - targetScaleX) > 0.01f)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Lerp(currentScale.x, targetScaleX, turnSpeed * Time.deltaTime);
            transform.localScale = currentScale;
        }

        // Ground check
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffering
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Execute Jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Variable Jump Height (releasing jump early)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = isGrounded ? moveSpeed : moveSpeed * airSpeedMultiplier;
        float targetSpeed = horizontalInput * currentSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        
        float baseAccelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float accelRate = isGrounded ? baseAccelRate : baseAccelRate * airControlMultiplier;
        
        rb.AddForce(accelRate * speedDiff * Vector2.right);

        // Apply heavier gravity when falling
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravity * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }

        // Clamp fall speed
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        // Instead of instantly changing the scale, we set a target for the Update loop to smoothly blend towards
        targetScaleX = isFacingRight ? originalScaleX : -originalScaleX;
    }
}
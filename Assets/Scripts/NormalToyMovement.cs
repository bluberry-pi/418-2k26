using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NormalToyMovement : MonoBehaviour
{
    [Header("Game Flow")]
    [Tooltip("Drag the Key object here so the player knows when to start moving.")]
    public KeyStart startingKey;

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
        
        originalScaleX = Mathf.Abs(transform.localScale.x);
        targetScaleX = originalScaleX;
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

        if (startingKey != null && !startingKey.isPlayingNormally)
        {
            horizontalInput = 0f;
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
        }

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

        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravity * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        targetScaleX = isFacingRight ? originalScaleX : -originalScaleX;
    }
}
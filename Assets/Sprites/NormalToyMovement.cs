using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NormalToyMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float airSpeedMultiplier = 0.8f;
    
    public float acceleration = 13f;
    public float deceleration = 16f;
    public float airControlMultiplier = 0.5f;
    
    public float jumpForce = 13f;
    public float fallGravityMultiplier = 2.5f;
    public float maxFallSpeed = 15f;
    
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float defaultGravity;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
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
}
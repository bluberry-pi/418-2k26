using UnityEngine;

/// Attach to your airplane GameObject.
/// Requires a Rigidbody2D — set gravityScale to 0 in the Inspector.
/// Plug in a ToyEnergy component and this will drain the slider while flying.
/// Movement is only allowed when IsControlled = true (set by ToyManager via KeyStart).
[RequireComponent(typeof(Rigidbody2D))]
public class AeroplaneMovement : MonoBehaviour
{
    [Header("Energy")]
    public ToyEnergy toyEnergy;

    [Header("Speed")]
    public float thrustSpeed     = 6f;   // vertical speed on W
    public float horizontalSpeed = 5f;   // left/right speed
    public float diveSpeed       = 4f;   // downward speed on S

    [Header("Gravity")]
    public float fallAcceleration = 20f; // downward pull when no W
    public float maxFallSpeed     = 12f;

    [Header("Pitch Rotation")]
    public float pitchUpAngle     = 35f;  // nose-up degrees on W
    public float pitchDownAngle   = -30f; // nose-down degrees on S
    public float rotationSpeed    = 7f;

    [Header("Horizontal Turn")]
    public float flipSpeed = 14f;

    // ── Control state (set by ToyManager via KeyStart) ──────────
    public bool IsControlled { get; private set; } = false;

    // ToyManager uses this to know whether to drain energy
    public bool IsMoving => IsControlled &&
        (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
         Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S));

    // ── internals ────────────────────────────────────────────────
    private Rigidbody2D rb;
    private float       targetAngle   = 0f;
    private float       fallVelocity  = 0f;
    private bool        facingRight   = true;
    private float       originalScaleX;
    private float       targetScaleX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;   // manual gravity
        rb.freezeRotation = true;

        originalScaleX = Mathf.Abs(transform.localScale.x);
        targetScaleX   = originalScaleX;
    }

    /// Called by KeyStart to hand over / revoke control
    public void SetControl(bool state)
    {
        IsControlled = state;
        if (!state)
        {
            // Stop horizontal movement when control lost; let gravity still apply
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void Update()
    {
        // No control or no energy → drift with gravity, no input
        if (!IsControlled || toyEnergy == null || !toyEnergy.HasEnergy)
        {
            // level nose gently back to horizontal
            float currentZ = transform.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f;
            transform.localEulerAngles = new Vector3(0f, 0f,
                Mathf.LerpAngle(currentZ, 0f, rotationSpeed * Time.deltaTime));
            return;
        }

        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);
        bool a = Input.GetKey(KeyCode.A);
        bool d = Input.GetKey(KeyCode.D);

        // ── Target nose angle ────────────────────────────────────
        if (w)
            targetAngle = pitchUpAngle;
        else if (s)
            targetAngle = pitchDownAngle;
        else if (a || d)
            targetAngle = 0f;  // level out when flying left/right without W

        // ── Smooth rotation ──────────────────────────────────────
        float visualAngle = facingRight ? targetAngle : -targetAngle;
        float currentAngle = transform.localEulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;
        float newZ = Mathf.LerpAngle(currentAngle, visualAngle, rotationSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0f, 0f, newZ);

        // ── Facing direction ─────────────────────────────────────
        if (d) facingRight = true;
        if (a) facingRight = false;

        targetScaleX = facingRight ? originalScaleX : -originalScaleX;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Lerp(scale.x, targetScaleX, flipSpeed * Time.deltaTime);
        transform.localScale = scale;
    }

    void FixedUpdate()
    {
        if (!IsControlled || toyEnergy == null || !toyEnergy.HasEnergy)
        {
            // Only gravity, no horizontal
            fallVelocity -= fallAcceleration * Time.fixedDeltaTime;
            fallVelocity  = Mathf.Max(fallVelocity, -maxFallSpeed);
            rb.linearVelocity = new Vector2(0f, fallVelocity);
            return;
        }

        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);
        bool a = Input.GetKey(KeyCode.A);
        bool d = Input.GetKey(KeyCode.D);

        float hInput = (d ? 1f : 0f) - (a ? 1f : 0f);

        // ── Vertical ─────────────────────────────────────────────
        if (w)
            fallVelocity = thrustSpeed;
        else if (s)
            fallVelocity = -diveSpeed;
        else
        {
            fallVelocity -= fallAcceleration * Time.fixedDeltaTime;
            fallVelocity  = Mathf.Max(fallVelocity, -maxFallSpeed);
        }

        // ── Horizontal ───────────────────────────────────────────
        float hVelocity = Mathf.Abs(hInput) > 0.01f
            ? hInput * horizontalSpeed
            : Mathf.Lerp(rb.linearVelocity.x, 0f, 0.18f);

        rb.linearVelocity = new Vector2(hVelocity, fallVelocity);
    }
}

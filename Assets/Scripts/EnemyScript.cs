using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Wiggle Juice")]
    public float wiggleAngle = 15f;        // max rotation swing in degrees
    public float wiggleSpeed = 8f;         // how fast it wiggles
    public float wiggleScaleAmount = 0.08f; // subtle squish scale pulse

    [Header("Death Effect")]
    public GameObject deathParticlePrefab;
    public float deathExpandScale = 2.2f;
    public float deathDuration = 0.35f;

    // ── internals ──────────────────────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isActive = false;
    private bool isDying = false;
    private Vector3 originalScale;
    private float wiggleTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;

        // Enemies start fully static — activated later by EnemyManager
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }

    // Called by EnemyManager when the player enters the trigger zone
    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        rb.bodyType = RigidbodyType2D.Dynamic;  // drop from the air naturally
    }

    void Update()
    {
        if (!isActive || isDying) return;

        Transform target = GetTarget();
        if (target == null) return;

        // ── Horizontal chase ────────────────────────────────────
        float dir = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        // ── Flip sprite to face player ──────────────────────────
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
        transform.localScale = s;

        // ── Wiggle: rotation + scale pulse ──────────────────────
        wiggleTimer += Time.deltaTime * wiggleSpeed;

        float rot = Mathf.Sin(wiggleTimer) * wiggleAngle;
        transform.localEulerAngles = new Vector3(0f, 0f, rot);

        float scalePulse = 1f + Mathf.Abs(Mathf.Sin(wiggleTimer * 0.5f)) * wiggleScaleAmount;
        transform.localScale = new Vector3(
            s.x * scalePulse,
            Mathf.Abs(s.y) * (1f / scalePulse), // squash/stretch: opposite axis
            s.z
        );
    }

    // ── Called by HitBox when the punch lands ──────────────────
    public void Die()
    {
        if (isDying) return;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDying = true;

        // Freeze physics
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Flash white
        if (sr != null) sr.color = Color.white;

        // Stop wiggle rotation
        transform.localEulerAngles = Vector3.zero;

        float elapsed = 0f;
        Vector3 baseScale = originalScale;

        // Phase 1 — expand (first half)
        while (elapsed < deathDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (deathDuration * 0.5f);
            float scale = Mathf.Lerp(1f, deathExpandScale, t);
            transform.localScale = baseScale * scale;
            yield return null;
        }

        // Phase 2 — contract to zero (second half)
        elapsed = 0f;
        Vector3 bigScale = baseScale * deathExpandScale;
        while (elapsed < deathDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (deathDuration * 0.5f);
            transform.localScale = Vector3.Lerp(bigScale, Vector3.zero, t);
            yield return null;
        }

        // Spawn particles
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private Transform GetTarget()
    {
        if (ToyManager.Instance == null) return null;
        NormalToyMovement toy = ToyManager.Instance.CurrentToy;
        return toy != null ? toy.transform : null;
    }
}

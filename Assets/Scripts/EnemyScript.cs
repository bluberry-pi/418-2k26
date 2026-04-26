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

    [Header("Kill Effect")]
    [Tooltip("Particle prefab to spawn at the toy's position when the enemy kills it.")]
    public GameObject toyDeathParticlePrefab;

    // ── internals ──────────────────────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isActive  = false;
    private bool isDying   = false;
    private bool hasFallen = false;   // true once the enemy has actually started falling
    private bool hasLanded = false;   // true once the enemy has touched the ground
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

        // ── Wait until we have actually fallen and landed before chasing ───
        if (!hasLanded)
        {
            // Step 1: wait until the enemy is actually moving downward (has started falling)
            if (!hasFallen)
            {
                if (rb.linearVelocity.y < -1f)
                    hasFallen = true;
                return; // still at rest or just activated — keep waiting
            }

            // Step 2: once falling, wait for near-zero vertical velocity (landed)
            if (Mathf.Abs(rb.linearVelocity.y) < 0.5f)
                hasLanded = true;
            else
                return; // still falling — do nothing except let gravity work
        }

        Transform target = GetTarget();
        if (target == null) return;

        // ── Horizontal chase ────────────────────────────────────
        float dir = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        // ── Wiggle: rotation + scale pulse ──────────────────────
        wiggleTimer += Time.deltaTime * wiggleSpeed;

        float rot = Mathf.Sin(wiggleTimer) * wiggleAngle;
        transform.localEulerAngles = new Vector3(0f, 0f, rot);

        // Always derive scale from originalScale — never read back transform.localScale,
        // or the modifications compound every frame (X→Infinity, Y→0)
        float facing     = dir >= 0 ? 1f : -1f;
        float scalePulse = 1f + Mathf.Abs(Mathf.Sin(wiggleTimer * 0.5f)) * wiggleScaleAmount;
        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * facing * scalePulse,   // flip + squash X
            originalScale.y / scalePulse,                        // squash/stretch Y (inverse)
            originalScale.z
        );
    }

    // ── Kill the currently-controlled toy on contact ────────────
    private void OnCollisionEnter2D(Collision2D col)
    {
        TryKillToy(col.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKillToy(other.gameObject);
    }

    private void TryKillToy(GameObject other)
    {
        if (isDying) return;

        // Only react to objects on the Toy layer
        if (other.layer != LayerMask.NameToLayer("Toy")) return;

        // Only kill the *currently controlled* toy
        if (ToyManager.Instance == null) return;
        NormalToyMovement toy = ToyManager.Instance.CurrentToy;
        if (toy == null || other != toy.gameObject) return;

        // ── Juice ──────────────────────────────────────────────────
        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.ShakeDeath();
            GameJuice.Instance.Flash();
        }

        // ── Spawn death particle at toy position ───────────────────
        if (toyDeathParticlePrefab != null)
            Instantiate(toyDeathParticlePrefab, other.transform.position, Quaternion.identity);

        Destroy(other);
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

        // ── Juice ─────────────────────────────────────────────────
        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.HitFreeze();
            GameJuice.Instance.ShakeDeath();
        }

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

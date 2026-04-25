using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Pressure Plate Settings")]
    public float speed = 2f;
    public float targetY = -14.45f;

    [Header("Door Settings")]
    public Transform door;
    public float doorSpeed = 2f;
    public float doorTargetY = -5.19f;

    [Header("Detection Zone")]
    [Tooltip("Layer(s) that count as a toy for pressing this plate.")]
    public LayerMask toyLayer;
    [Tooltip("Width of the detection zone. Match your plate's visual width.")]
    public float detectionWidth = 1f;
    [Tooltip("Height of the detection zone above the plate surface.")]
    public float detectionHeight = 0.25f;
    [Tooltip("How high above the plate center to place the detection zone.")]
    public float detectionOffsetY = 0.6f;

    // ── internals ─────────────────────────────────────────────────
    private float initialY;
    private float doorInitialY;
    private bool isPressed = false;

    void Start()
    {
        initialY = transform.position.y;

        if (door != null)
            doorInitialY = door.position.y;
    }

    void Update()
    {
        // ── Detect toy ABOVE the plate every frame ────────────────
        // OverlapBox sits just above the plate surface so it reliably
        // catches any toy standing on top, even as the plate moves.
        Vector2 checkCenter = new Vector2(
            transform.position.x,
            transform.position.y + detectionOffsetY
        );
        isPressed = Physics2D.OverlapBox(checkCenter,
                                         new Vector2(detectionWidth, detectionHeight),
                                         0f,
                                         toyLayer) != null;

        // ── Move the pressure plate ───────────────────────────────
        float targetPosY = isPressed ? targetY : initialY;
        Vector3 plateTarget = new Vector3(transform.position.x, targetPosY, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, plateTarget, speed * Time.deltaTime);

        // ── Move the door ─────────────────────────────────────────
        if (door != null)
        {
            float dTargetY = isPressed ? doorTargetY : doorInitialY;
            Vector3 doorTarget = new Vector3(door.position.x, dTargetY, door.position.z);
            door.position = Vector3.MoveTowards(door.position, doorTarget, doorSpeed * Time.deltaTime);
        }
    }

    // Visualise the detection zone in the Scene view for easy tuning
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(transform.position.x,
                                     transform.position.y + detectionOffsetY,
                                     transform.position.z);
        Gizmos.DrawWireCube(center, new Vector3(detectionWidth, detectionHeight, 0.1f));
    }
}

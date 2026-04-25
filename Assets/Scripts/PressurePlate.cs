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

    [Header("Detection")]
    [Tooltip("Must match the layer name used on your toy objects.")]
    public string toyLayerName = "Toy";

    // ── internals ─────────────────────────────────────────────────
    private float initialY;
    private float doorInitialY;
    private bool isPressed = false;
    private Collider2D plateCollider;
    private ContactFilter2D toyFilter;
    private List<Collider2D> overlapResults = new List<Collider2D>();

    void Start()
    {
        initialY = transform.position.y;

        if (door != null)
            doorInitialY = door.position.y;

        plateCollider = GetComponent<Collider2D>();

        // Build a filter that only matches the Toy layer
        toyFilter = new ContactFilter2D();
        toyFilter.SetLayerMask(LayerMask.GetMask(toyLayerName));
        toyFilter.useLayerMask  = true;
        toyFilter.useTriggers   = true; // detect even if toy collider is a trigger
    }

    void Update()
    {
        // ── Detect toy on plate every frame (no collision events needed) ──
        isPressed = false;
        if (plateCollider != null)
        {
            int hits = plateCollider.Overlap(toyFilter, overlapResults);
            isPressed = hits > 0;
        }

        // ── Move the pressure plate ───────────────────────────────
        float targetPosY = isPressed ? targetY : initialY;
        Vector3 targetPosition = new Vector3(transform.position.x, targetPosY, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // ── Move the door ─────────────────────────────────────────
        if (door != null)
        {
            float doorTargetPosY = isPressed ? doorTargetY : doorInitialY;
            Vector3 doorTargetPosition = new Vector3(door.position.x, doorTargetPosY, door.position.z);
            door.position = Vector3.MoveTowards(door.position, doorTargetPosition, doorSpeed * Time.deltaTime);
        }
    }
}

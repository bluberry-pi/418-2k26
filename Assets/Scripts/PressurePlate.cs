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
    
    private float initialY;
    private float doorInitialY;
    private int toysOnPlate = 0;
    private bool isPressed = false;

    void Start()
    {
        initialY = transform.position.y;
        if (door != null)
        {
            doorInitialY = door.position.y;
        }
    }

    void Update()
    {
        // Move the pressure plate
        float target = isPressed ? targetY : initialY;
        Vector3 targetPosition = new Vector3(transform.position.x, target, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Move the door
        if (door != null)
        {
            float dTarget = isPressed ? doorTargetY : doorInitialY;
            Vector3 doorTargetPosition = new Vector3(door.position.x, dTarget, door.position.z);
            door.position = Vector3.MoveTowards(door.position, doorTargetPosition, doorSpeed * Time.deltaTime);
        }
    }

    // ── 2D Collision (solid colliders) ──────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Toy"))
        {
            toysOnPlate++;
            isPressed = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Toy"))
        {
            toysOnPlate--;
            if (toysOnPlate <= 0)
            {
                toysOnPlate = 0;
                isPressed = false;
            }
        }
    }

    // ── 2D Trigger (if plate or toy uses Is Trigger) ─────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Toy"))
        {
            toysOnPlate++;
            isPressed = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Toy"))
        {
            toysOnPlate--;
            if (toysOnPlate <= 0)
            {
                toysOnPlate = 0;
                isPressed = false;
            }
        }
    }
}


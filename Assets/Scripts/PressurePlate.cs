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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Toy"))
        {
            toysOnPlate++;
            isPressed = true;
        }
    }

    void OnCollisionExit(Collision collision)
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

    // Adding trigger support just in case the plate or toy uses 'Is Trigger'
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Toy"))
        {
            toysOnPlate++;
            isPressed = true;
        }
    }

    void OnTriggerExit(Collider other)
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

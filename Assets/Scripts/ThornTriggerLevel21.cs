using UnityEngine;

public class ThornTriggerLevel21 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float targetY = 0.02f;
    public float moveSpeed = 5f;

    private bool shouldMove = false;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    void Update()
    {
        if (shouldMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                shouldMove = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Toy"))
        {
            shouldMove = true;
        }
    }
}

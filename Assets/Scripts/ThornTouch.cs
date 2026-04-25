using UnityEngine;

public class ThornTouch : MonoBehaviour
{
    [Header("Effects")]
    [Tooltip("Particle prefab to spawn when the Toy is destroyed. Can be left null.")]
    public GameObject deathParticlePrefab;

    private void HandleCollision(GameObject otherObj)
    {
        GameObject toy = null;

        // Check if this object is Danger and it collided with a Toy
        if (gameObject.layer == LayerMask.NameToLayer("Danger") && otherObj.layer == LayerMask.NameToLayer("Toy"))
        {
            toy = otherObj;
        }
        // Check if this object is Toy and it collided with Danger
        else if (gameObject.layer == LayerMask.NameToLayer("Toy") && otherObj.layer == LayerMask.NameToLayer("Danger"))
        {
            toy = gameObject;
        }

        if (toy != null)
        {
            if (deathParticlePrefab != null)
            {
                Instantiate(deathParticlePrefab, toy.transform.position, Quaternion.identity);
            }
            Destroy(toy);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        HandleCollision(collider.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
}

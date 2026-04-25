using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public GameObject destructionEffect; // Assign your broken wall/door prefab here
    public Transform spawnLocation; // Drag an empty GameObject here to set the spawn location

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PhyDoor"))
        {
            // Instantiate the broken prefab instantly if one is assigned
            if (destructionEffect != null)
            {
                // Use spawnLocation if provided, otherwise default to the door's position
                Vector3 spawnPos = spawnLocation != null ? spawnLocation.position : other.transform.position;
                
                // Debug out where it's spawning
                if (spawnLocation != null)
                {
                    Debug.Log($"Using custom spawn location. Spawning prefab at: {spawnPos}");
                }
                else
                {
                    Debug.Log($"No custom spawn location set. Spawning prefab at door's position: {spawnPos}");
                }

                Instantiate(destructionEffect, spawnPos, Quaternion.identity);
            }

            // Destroy the original object instantly
            Destroy(other.gameObject);
        }
    }
}
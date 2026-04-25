using UnityEngine;

// Attach this to a GameObject with a Box Collider 2D (Is Trigger = ON).
// Drag your 2 enemy GameObjects into the enemies array in the Inspector.
public class EnemyManager : MonoBehaviour
{
    [Header("Enemies to Activate")]
    public EnemyScript[] enemies;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        // Only react to the currently controlled Toy
        if (other.gameObject.layer != LayerMask.NameToLayer("Toy")) return;
        if (ToyManager.Instance == null) return;

        NormalToyMovement toy = other.GetComponent<NormalToyMovement>();
        if (toy == null || toy != ToyManager.Instance.CurrentToy) return;

        triggered = true;

        foreach (EnemyScript enemy in enemies)
        {
            if (enemy != null)
                enemy.Activate();
        }
    }
}

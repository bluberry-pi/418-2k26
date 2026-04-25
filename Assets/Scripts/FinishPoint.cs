using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    private void OriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Toy"))
        {
            //go to next level
            SceneController.instance.NextLevel();
        }
    }
}

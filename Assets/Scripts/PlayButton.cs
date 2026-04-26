using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
public void OnPlayPressed()
{
    Debug.Log("Play pressed! Current index: " + SceneManager.GetActiveScene().buildIndex);
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
}
}

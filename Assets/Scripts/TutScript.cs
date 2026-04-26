using UnityEngine;

/// Destroys the tutorial Canvas the moment the linked KeyStart
/// finishes its 4-click wind-up and begins playing normally.
public class TutScript : MonoBehaviour
{
    [Header("Tutorial Canvas")]
    [Tooltip("Drag the tutorial Canvas GameObject here. It will be destroyed when the toy starts.")]
    public GameObject tutorialCanvas;

    [Header("Linked Key")]
    [Tooltip("Drag the KeyStart that controls this toy here.")]
    public KeyStart linkedKey;

    private bool dismissed = false;

    void Update()
    {
        if (dismissed) return;
        if (linkedKey == null || tutorialCanvas == null) return;

        if (linkedKey.isPlayingNormally)
        {
            dismissed = true;
            Destroy(tutorialCanvas);
        }
    }
}

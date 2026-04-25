using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// =====================================================================
/// AutoSortingManager — Attach to your GameJuice / Manager GameObject.
///
/// Fixes flickering/Z-fighting caused by sprites sharing the same
/// sorting order. Automatically assigns every SpriteRenderer a UNIQUE
/// sorting order by walking the scene Hierarchy top-to-bottom.
///
/// Objects HIGHER in the Hierarchy list → drawn BEHIND  (lower order)
/// Objects LOWER  in the Hierarchy list → drawn IN FRONT (higher order)
///
/// This matches Unity's default visual intent with zero manual work.
/// =====================================================================
[DefaultExecutionOrder(-200)]
public class AutoSortingManager : MonoBehaviour
{
    [Tooltip("Objects tagged 'NoAutoSort' will be skipped and keep their manual order.")]
    public bool respectNoAutoSortTag = true;

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start() => ApplySorting();

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplySorting();

    void ApplySorting()
    {
        // Walk the hierarchy depth-first, top to bottom
        // This gives each sprite a unique incrementing index
        int order = 0;

        // Get all root GameObjects
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            AssignOrdersRecursive(root.transform, ref order);
        }
    }

    void AssignOrdersRecursive(Transform t, ref int order)
    {
        // Check for sprite renderer on this object
        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bool skip = respectNoAutoSortTag && t.CompareTag("NoAutoSort");
            if (!skip)
            {
                sr.sortingOrder = order;
                order++;
            }
        }

        // Recurse into children (in sibling order)
        for (int i = 0; i < t.childCount; i++)
        {
            AssignOrdersRecursive(t.GetChild(i), ref order);
        }
    }
}

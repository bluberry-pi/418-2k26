using UnityEngine;
using System.Collections;

public class Punch : MonoBehaviour
{
    public Sprite retracted;
    public Sprite extended;
    public SpriteRenderer sr;

    public float punchDistance = 0.5f;
    public float punchSpeed = 0.02f;

    public GameObject hitbox;

    Vector3 startPos;
    private NormalToyMovement parentMovement;

    void Start()
    {
        startPos = transform.localPosition;
        sr.sprite = retracted;
        parentMovement = GetComponentInParent<NormalToyMovement>();

        hitbox.SetActive(false);
    }

    void Update()
    {
        if (parentMovement != null && !parentMovement.IsControlled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            StartCoroutine(PunchAnim());
        }
    }

    IEnumerator PunchAnim()
    {
        sr.sprite = extended;

        // turn on hitbox briefly
        hitbox.SetActive(true);

        Vector3 target = startPos + new Vector3(punchDistance, 0, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / punchSpeed;
            transform.localPosition = Vector3.Lerp(startPos, target, t);
            yield return null;
        }

        // disable hitbox after impact
        hitbox.SetActive(false);

        // return back
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / punchSpeed;
            transform.localPosition = Vector3.Lerp(target, startPos, t);
            yield return null;
        }

        sr.sprite = retracted;
    }
}
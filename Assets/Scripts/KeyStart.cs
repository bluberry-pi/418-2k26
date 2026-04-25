using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] 
public class KeyStart : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] frames;
    public float framesPerSecond = 11.25f;
    public int framesToSkipOnClick = 2; 

    [Header("Interaction Settings")]
    public int clicksToStartPlaying = 4;

    // CHANGED: This is now public so the player movement script can read it!
    public bool isPlayingNormally = false; 

    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex = 0;
    private int clickCount = 0;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition);

            if (hitCollider != null && hitCollider.gameObject == this.gameObject)
            {                
                if (!isPlayingNormally)
                {
                    HandleInteraction();
                }
            }
        }

        if (isPlayingNormally && frames.Length > 0)
        {
            timer += Time.deltaTime;
            
            if (timer >= 1f / framesPerSecond)
            {
                timer = 0f;
                currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
                spriteRenderer.sprite = frames[currentFrameIndex];
            }
        }
    }

    private void HandleInteraction()
    {
        clickCount++;
        if (clickCount >= clicksToStartPlaying)
        {
            isPlayingNormally = true;
            timer = 0f; 
        }
        else
        {
            if (frames.Length > 0)
            {
                int jumpAmount = framesToSkipOnClick + 1; 
                currentFrameIndex = (currentFrameIndex + jumpAmount) % frames.Length;
                spriteRenderer.sprite = frames[currentFrameIndex];
            }
        }
    }
}
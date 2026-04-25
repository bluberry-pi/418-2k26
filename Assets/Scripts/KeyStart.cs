using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] 
public class KeyStart : MonoBehaviour
{
    [Header("Connected Toy (use one or the other)")]
    public NormalToyMovement connectedToy;       // drag normal toy here
    public AeroplaneMovement connectedAeroplane; // OR drag aeroplane here
    public ToyEnergy connectedEnergy;

    public Sprite[] frames;
    public float framesPerSecond = 11.25f;
    public int framesToSkipOnClick = 2; 
    public int clicksToStartPlaying = 4;

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
                else
                {
                    // Re-click after started: switch control back to this toy
                    SwitchControl();
                }
            }
        }

        if (isPlayingNormally && frames.Length > 0)
        {
            if (connectedEnergy == null || connectedEnergy.HasEnergy)
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
    }

    private void HandleInteraction()
    {
        clickCount++;

        if (clickCount >= clicksToStartPlaying)
        {
            isPlayingNormally = true;
            timer = 0f; 
            
            if (connectedEnergy != null)
            {
                connectedEnergy.FillEnergy();
            }

            SwitchControl();
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

    private void SwitchControl()
    {
        if (ToyManager.Instance == null || connectedEnergy == null) return;

        if (connectedAeroplane != null)
        {
            // Aeroplane path — ToyManager doesn't know about AeroplaneMovement directly,
            // so we call SetControl manually and just update the slider via SwitchToy with null toy.
            // We pass null for the NormalToyMovement and handle aeroplane control ourselves.
            ToyManager.Instance.SwitchAeroplane(connectedAeroplane, connectedEnergy);
        }
        else if (connectedToy != null)
        {
            ToyManager.Instance.SwitchToy(connectedToy, connectedEnergy);
        }
    }
}
using System.Collections;
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

    [Header("Sound FX")]
    [Tooltip("Assign one or more click/wind-up sounds. A random one plays on each key click.")]
    public AudioClip[] clickSounds;
    public int clicksToStartPlaying = 4;

    [Header("Toy Music")]
    [Tooltip("Music that plays while the toy is running. Fades in on start, fades out on energy empty.")]
    public AudioClip toyMusic;
    [Range(0f, 1f)] public float musicVolume  = 0.8f;
    public float musicFadeInDuration  = 0.8f;
    public float musicFadeOutDuration = 2.0f;

    public bool isPlayingNormally = false; 

    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex = 0;
    private int clickCount = 0;
    private float timer = 0f;

    // ── Music internals ───────────────────────────────────────────────
    private AudioSource musicSource;
    private Coroutine   musicFadeCoroutine;
    private bool        wasMusicPlaying = false; // tracks energy→music state

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
        }

        // Create a dedicated AudioSource for this toy's music
        musicSource             = gameObject.AddComponent<AudioSource>();
        musicSource.clip        = toyMusic;
        musicSource.loop        = true;
        musicSource.volume      = 0f;
        musicSource.playOnAwake = false;
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
            bool hasEnergy = (connectedEnergy == null || connectedEnergy.HasEnergy);

            // ── Sprite animation (only while energy is available) ────
            if (hasEnergy)
            {
                timer += Time.deltaTime;
                
                if (timer >= 1f / framesPerSecond)
                {
                    timer = 0f;
                    currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
                    spriteRenderer.sprite = frames[currentFrameIndex];
                }
            }

            // ── Music fade-out when energy runs out ──────────────────
            if (!hasEnergy && wasMusicPlaying)
            {
                wasMusicPlaying = false;
                FadeMusic(0f, musicFadeOutDuration);
            }
        }
    }

    private void HandleInteraction()
    {
        clickCount++;

        // Play a random click sound on every key press
        if (clickSounds != null && clickSounds.Length > 0 && SoundFXManager.instance != null)
        {
            SoundFXManager.instance.PlayRandomSoundFXClip(clickSounds, transform, 1f);
        }

        if (clickCount >= clicksToStartPlaying)
        {
            isPlayingNormally = true;
            timer = 0f; 
            
            if (connectedEnergy != null)
            {
                connectedEnergy.FillEnergy();
            }

            SwitchControl();

            // ── Start the toy music with a fade-in ───────────────────
            if (toyMusic != null)
            {
                wasMusicPlaying = true;
                musicSource.clip = toyMusic;
                musicSource.volume = 0f;
                musicSource.Play();
                FadeMusic(musicVolume, musicFadeInDuration);
            }
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
            ToyManager.Instance.SwitchAeroplane(connectedAeroplane, connectedEnergy, this);
        }
        else if (connectedToy != null)
        {
            ToyManager.Instance.SwitchToy(connectedToy, connectedEnergy, this);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Music helpers
    // ─────────────────────────────────────────────────────────────────

    /// Called by ToyManager when another toy is selected — kills music instantly.
    public void StopMusicImmediate()
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = null;
        musicSource.volume = 0f;
        musicSource.Stop();
        wasMusicPlaying = false;
    }

    /// Smoothly fade the music AudioSource to targetVolume over duration seconds.
    private void FadeMusic(float targetVolume, float duration)
    {
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeMusicRoutine(targetVolume, duration));
    }

    private IEnumerator FadeMusicRoutine(float targetVolume, float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed     = 0f;

        while (elapsed < duration)
        {
            elapsed           += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        // Stop the source once fully faded out to free the audio thread
        if (targetVolume <= 0f)
            musicSource.Stop();
    }
}
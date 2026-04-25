using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// =====================================================================
/// GameJuice — Global juice singleton. Persists across ALL scenes.
/// 
/// Drop on a GameObject in your FIRST scene. It survives scene loads.
/// =====================================================================
public class GameJuice : MonoBehaviour
{
    public static GameJuice Instance { get; private set; }

    // ── Scene-Start Shake ─────────────────────────────────────────
    [Header("Scene Start Shake")]
    public float startShakeDuration  = 0.55f;
    public float startShakeAmplitude = 1.8f;

    // ── Per-event shake presets ───────────────────────────────────
    [Header("Hit / Death Shake Presets")]
    public float hitShakeDuration    = 0.18f;
    public float hitShakeAmplitude   = 2.5f;
    public float deathShakeDuration  = 0.32f;
    public float deathShakeAmplitude = 4.0f;

    // ── Hit Freeze ────────────────────────────────────────────────
    [Header("Hit Freeze")]
    [Range(0f, 0.25f)]
    public float hitFreezeDuration = 0.055f; // fraction of a second — very snappy

    // ── Screen Flash ──────────────────────────────────────────────
    [Header("Screen Flash")]
    [Tooltip("Assign the full-screen CanvasGroup (white Image, alpha 0). See setup notes.")]
    public CanvasGroup flashPanel;
    public float flashDuration = 0.18f;

    // ── internals ────────────────────────────────────────────────
    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeCoroutine;
    private Coroutine flashCoroutine;

    // ─────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        FindNoise();
        Shake(startShakeDuration, startShakeAmplitude);
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindNoise();
        Shake(startShakeDuration, startShakeAmplitude);
    }

    // Searches the scene for a CinemachineCamera with a noise behaviour
    void FindNoise()
    {
        noise = null;
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
            noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // ─────────────────────────────────────────────────────────────
    //  Public API — call these from any script
    // ─────────────────────────────────────────────────────────────

    /// Shake camera with custom duration and amplitude.
    public void Shake(float duration, float amplitude)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, amplitude));
    }

    /// Short snappy shake — call when punching something.
    public void ShakeHit()   => Shake(hitShakeDuration, hitShakeAmplitude);

    /// Big boom shake — call on toy or enemy death.
    public void ShakeDeath() => Shake(deathShakeDuration, deathShakeAmplitude);

    /// Freeze time for a tiny moment — very punchy feel on impact.
    public void HitFreeze()  => StartCoroutine(HitFreezeRoutine());

    /// Flash the screen white briefly.
    public void Flash()
    {
        if (flashPanel == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    // ─────────────────────────────────────────────────────────────
    //  Coroutines
    // ─────────────────────────────────────────────────────────────

    IEnumerator ShakeRoutine(float duration, float amplitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Damp amplitude as shake nears its end
            float damped = Mathf.Lerp(amplitude, 0f, elapsed / duration);
            if (noise != null) noise.AmplitudeGain = damped;

            elapsed += Time.unscaledDeltaTime; // unscaled: works during hit-freeze
            yield return null;
        }
        if (noise != null) noise.AmplitudeGain = 0f;
    }

    IEnumerator HitFreezeRoutine()
    {
        Time.timeScale = 0.04f;
        yield return new WaitForSecondsRealtime(hitFreezeDuration);
        Time.timeScale = 1f;
    }

    IEnumerator FlashRoutine()
    {
        flashPanel.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            flashPanel.alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        flashPanel.alpha = 0f;
    }
}

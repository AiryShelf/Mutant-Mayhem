using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BloodVignetteController : MonoBehaviour
{
    [Header("References")]

    [SerializeField] Image vignetteImage; // Assign your edge-blood PNG (transparent center)

    [Header("Resting Alpha (low health => more alpha)")]
    [Tooltip("Max resting alpha when health is 0%")]
    [Range(0f, 1f)] public float maxRestingAlpha = 0.7f;

    [Tooltip("Smooths resting alpha updates when health changes")]
    public float restingAlphaLerpSpeed = 6f;

    [Header("Damage Pulse")]

    [Tooltip("Curve input: damageRatio (0..1), output: pulse intensity scaler")]
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("How quickly the pulse rises to its peak")]
    public float pulseRiseTime = 0.07f;

    [Tooltip("Minimum fade-out time to add for small hits")]
    public float minPulseFadeTime = 1f;

    [Tooltip("Maximum fade-out time to add for huge hits")]
    public float maxPulseFadeTime = 5f;

    float targetRestingAlpha;          // driven by healthRatio
    float currentVisualAlpha;          // what we actually set to the Image
    Coroutine pulseRoutine;
    Color workingColor;
    Player player;
    float lastHealth;
    float cumulativeFadeTime = 0f;
    public float maxCumulativeFadeTime = 12f;

    void Awake()
    {
        vignetteImage = GetComponent<Image>();
        vignetteImage.raycastTarget = false;

        if (!vignetteImage) vignetteImage = GetComponent<Image>();
        var c = vignetteImage.color; c.a = 0f; vignetteImage.color = c;
        currentVisualAlpha = 0f;
        targetRestingAlpha = 0f;
    }

    void Start()
    {
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.stats.playerHealthScript.OnPlayerHealthChanged += OnHealthChanged;
            player.stats.playerHealthScript.OnPlayerMaxHealthChanged += OnHealthChanged;
        }
        lastHealth = player.stats.playerHealthScript.GetHealth();
    }
    
    void OnDestroy()
    {
        if (player != null)
        {
            player.stats.playerHealthScript.OnPlayerHealthChanged -= OnHealthChanged;
            player.stats.playerHealthScript.OnPlayerMaxHealthChanged -= OnHealthChanged;
        }
    }

    void Update()
    {
        if (pulseRoutine != null)
            return;
        // Smooth towards resting alpha unless a pulse is actively overriding
        workingColor = vignetteImage.color;
        currentVisualAlpha = Mathf.MoveTowards(currentVisualAlpha, targetRestingAlpha, restingAlphaLerpSpeed * Time.unscaledDeltaTime);
        workingColor.a = currentVisualAlpha;
        vignetteImage.color = workingColor;
    }

    void OnHealthChanged(float uselessNumber)
    {
        Debug.Log("Blood Vignette Health Changed");
        float currentHealth = player.stats.playerHealthScript.GetHealth();
        float maxHealth = player.stats.playerHealthScript.GetMaxHealth();

        UpdateRestingAlpha(currentHealth, maxHealth);

        if (currentHealth < lastHealth)
            PulseOnDamage((lastHealth - currentHealth) / currentHealth);

        lastHealth = currentHealth;
    }

    void UpdateRestingAlpha(float currentHealth, float maxHealth)
    {
        float healthRatio = Mathf.Clamp01(currentHealth / Mathf.Max(0.0001f, maxHealth));
        // More alpha as health goes lower:
        targetRestingAlpha = (1f - healthRatio) * maxRestingAlpha;
    }

    void PulseOnDamage(float damageRatio)
    {
        damageRatio = Mathf.Clamp01(damageRatio);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine(damageRatio));
    }

    IEnumerator PulseRoutine(float damageRatio)
    {
        // Peak alpha = resting + curve(dmg)*maxPulseAdd (clamped 0..1)
        float pulseAdd = pulseCurve.Evaluate(damageRatio);
        float peakAlpha = Mathf.Clamp01(currentVisualAlpha + pulseAdd);
        peakAlpha = Mathf.Min(peakAlpha, 1);

        // Rise quickly to peak
        float t = 0f;
        while (t < pulseRiseTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / pulseRiseTime);
            currentVisualAlpha = Mathf.Lerp(currentVisualAlpha, peakAlpha, k);
            var c = vignetteImage.color; c.a = currentVisualAlpha; vignetteImage.color = c;
            yield return null;
        }
        currentVisualAlpha = peakAlpha;
        var c2 = vignetteImage.color; c2.a = currentVisualAlpha; vignetteImage.color = c2;

        // ---- CUMULATIVE FADE ----
        // Add new fade time contributed by this hit.
        float added = Mathf.Lerp(minPulseFadeTime, maxPulseFadeTime, damageRatio);
        cumulativeFadeTime += Mathf.Max(0.0001f, added);
        cumulativeFadeTime = Mathf.Min(maxCumulativeFadeTime, cumulativeFadeTime);

        // We'll fade from the *current* alpha toward the resting alpha using a snapshot
        // of the remaining cumulative time. If more time is added mid‑fade (new damage),
        // we rebase from the current alpha and take a fresh snapshot so the fade slows
        // smoothly instead of "hanging then dropping".
        float snapshotTotal = cumulativeFadeTime;
        float prevCumulative = cumulativeFadeTime;
        float startAlpha = currentVisualAlpha;

        while (cumulativeFadeTime > 0f)
        {
            // Detect new damage adding time mid‑fade; rebase progress smoothly.
            if (cumulativeFadeTime > prevCumulative + 0.0001f)
            {
                startAlpha = currentVisualAlpha;   // continue from wherever we are
                snapshotTotal = cumulativeFadeTime; // new total to consume
            }
            prevCumulative = cumulativeFadeTime;

            float dt = Time.unscaledDeltaTime;
            cumulativeFadeTime = Mathf.Max(0f, cumulativeFadeTime - dt);

            // progress goes 0..1 over the *snapshot* duration
            float progress = 1f - (snapshotTotal <= 0f ? 0f : (cumulativeFadeTime / snapshotTotal));
            progress = Mathf.Clamp01(progress);

            currentVisualAlpha = Mathf.Lerp(startAlpha, targetRestingAlpha, progress);
            var c = vignetteImage.color; c.a = currentVisualAlpha; vignetteImage.color = c;

            yield return null;
        }

        currentVisualAlpha = targetRestingAlpha;
        var c3 = vignetteImage.color; c3.a = currentVisualAlpha; vignetteImage.color = c3;

        pulseRoutine = null;
    }
}
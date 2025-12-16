using TMPro;
using UnityEngine;
using System.Collections;

public class TextFly : MonoBehaviour
{
    [SerializeField] string objectPoolName;
    [SerializeField] bool isWorldSpace = false;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float moveSpeedVariation = 5f; 
    [SerializeField] float fadeDuration = 2f;
    [SerializeField] float fadeDurationMax = 3f;
    [SerializeField] float fadeAlphaStart = 1f;
    [SerializeField] float fadeAlphaEnd = 0f;

    [Header("Scale")]
    [Tooltip("Visual/resting scale used for animation (kept separate from the prefab/pool scale).")]
    [SerializeField] float workingScale = 1f;

    [Header("Text pop curve (around 1 stays ~1)")]
    [Tooltip("How much to amplify the pop effect")]
    [SerializeField] float popGain = 1f;
    [Tooltip("Exponent for pop amplification")]
    [SerializeField] float popExponent = 2f;  
    [Tooltip("Maximum scale for pop effect (0 = no cap)")]
    [SerializeField] float popMax = 6f;
    [Tooltip("Ratio of pulseMaxScale to add to fadeDuration for total duration")]
    [SerializeField] float popRatioDurationFactor = 0.5f; // Multiplies pulseMaxScale set in Initialize(), and adds to duration
    [SerializeField] float speedBoostFactor; // Multiplies pulseMaxScale to add to movement speed

    Vector3 initialWorldPosition;
    TextMeshPro tmpTextWorld;
    TextMeshProUGUI tmpTextUi;
    Color textColor;
    Vector2 flyDir;
    RectTransform rectTransform;

    float elapsedTime;

    // Apply animation outputs in LateUpdate so we win against any mid-frame overwrites.
    bool _applyThisFrame;
    Vector3 _desiredWorldPos;
    Vector2 _desiredAnchoredPos;
    Vector3 _desiredLocalScale;
    float _desiredAlpha;

    void Awake()
    {
        initialWorldPosition = transform.position;
        moveSpeed = Random.Range(Mathf.Max(0.01f, moveSpeed - moveSpeedVariation), moveSpeed + moveSpeedVariation);

        if (isWorldSpace)
        {
            tmpTextWorld = GetComponent<TextMeshPro>();
        }
        else
        {
            tmpTextUi = GetComponent<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public void Initialize(string text, Color color, float alphaMax, Vector2 dir, bool isWorldSpace, float pulseMaxScale)
    {
        StopAllCoroutines();

        this.isWorldSpace = isWorldSpace;
        flyDir = dir;
        textColor = color;

        // Base/resting scale for this animation
        Vector3 baseScale = Vector3.one * workingScale;
        transform.localScale = baseScale;
        _desiredLocalScale = baseScale;

        if (isWorldSpace)
        {
            if (!tmpTextWorld) tmpTextWorld = GetComponent<TextMeshPro>();
            tmpTextWorld.text = text;

            initialWorldPosition = transform.position;
            _desiredWorldPos = initialWorldPosition;

            tmpTextWorld.color = new Color(textColor.r, textColor.g, textColor.b, fadeAlphaStart);
        }
        else
        {
            if (!tmpTextUi) tmpTextUi = GetComponent<TextMeshProUGUI>();
            if (!rectTransform) rectTransform = GetComponent<RectTransform>();

            tmpTextUi.text = text;

            _desiredAnchoredPos = rectTransform.anchoredPosition;

            tmpTextUi.color = new Color(textColor.r, textColor.g, textColor.b, fadeAlphaStart);
        }

        _desiredAlpha = fadeAlphaStart;

        float duration = Mathf.Min(fadeDuration + (pulseMaxScale * popRatioDurationFactor), fadeDurationMax);

        // Nonlinear pop around 1: f(s)=1 + gain*(s)^exp, capped by popMax (pulseMaxScale is treated as additive over 1)
        float scaleInput = Mathf.Max(0f, pulseMaxScale);
        float amplifiedScale = 1f + popGain * Mathf.Pow(scaleInput, Mathf.Max(1f, popExponent));
        if (popMax > 0f) amplifiedScale = Mathf.Min(popMax, amplifiedScale);
        Vector3 scaleMax = new Vector3(amplifiedScale, amplifiedScale, amplifiedScale);

        float speedBoost = pulseMaxScale * speedBoostFactor;

        StartCoroutine(FadeAndMove(alphaMax, duration, speedBoost, baseScale, scaleMax));
    }

    private IEnumerator FadeAndMove(float alphaMax, float duration, float speedBoost, Vector3 baseScale, Vector3 scaleMax)
    {
        elapsedTime = 0f;
        Vector2 startAnchored = _desiredAnchoredPos;
        Vector3 startWorld = _desiredWorldPos;

        while (elapsedTime < duration)
        {
            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.0001f, duration));
            float easedT = 1f - Mathf.Pow(1f - t, 2f);

            Vector2 offset = flyDir * (moveSpeed + speedBoost) * easedT;

            _desiredAlpha = Mathf.Clamp(Mathf.Lerp(fadeAlphaStart, fadeAlphaEnd, t), 0, alphaMax);

            if (isWorldSpace)
                _desiredWorldPos = startWorld + (Vector3)offset;
            else
                _desiredAnchoredPos = startAnchored + offset;

            // Two-phase pulse: up fast, then down smooth. Peak early for a snappy pop.
            const float peakFraction = 0.25f;
            if (t <= peakFraction)
            {
                float upT = t / Mathf.Max(0.0001f, peakFraction);
                float easedUp = 1f - Mathf.Pow(1f - upT, 3f); // EaseOutCubic
                _desiredLocalScale = Vector3.LerpUnclamped(baseScale, scaleMax, easedUp);
            }
            else
            {
                float downT = (t - peakFraction) / Mathf.Max(0.0001f, 1f - peakFraction);
                float easedDown = Mathf.Pow(downT, 3f); // EaseInCubic
                _desiredLocalScale = Vector3.LerpUnclamped(scaleMax, baseScale, easedDown);
            }

            _applyThisFrame = true;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _applyThisFrame = false;
    }

    private void LateUpdate()
    {
        if (!_applyThisFrame)
            return;

        _applyThisFrame = false;

        // Apply scale
        transform.localScale = _desiredLocalScale;

        // Apply position + color
        if (isWorldSpace)
        {
            transform.position = _desiredWorldPos;
            if (tmpTextWorld)
                tmpTextWorld.color = new Color(textColor.r, textColor.g, textColor.b, _desiredAlpha);
        }
        else if (rectTransform)
        {
            rectTransform.anchoredPosition = _desiredAnchoredPos;
            if (tmpTextUi)
                tmpTextUi.color = new Color(textColor.r, textColor.g, textColor.b, _desiredAlpha);
        }
    }
}

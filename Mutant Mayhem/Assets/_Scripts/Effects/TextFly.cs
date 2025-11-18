using TMPro;
using UnityEngine;
using System.Collections;
using UnityEditor.EditorTools;

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

    Vector3 initialScale;
    TextMeshPro tmpTextWorld;
    TextMeshProUGUI tmpTextUi;
    Color textColor;
    Vector2 initialPosition;
    Vector2 flyDir;
    RectTransform rectTransform;
    private Coroutine pulseRoutine; // [ADDED] track external pulse coroutine

    float elapsedTime;
    float t;
    float easedT;
    Vector2 newPos;
    float alpha;

    void Awake()
    {
        initialScale = transform.localScale;
        moveSpeed = Random.Range(Mathf.Max(0.01f, moveSpeed - moveSpeedVariation), moveSpeed + moveSpeedVariation); // true centered random, clamped

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
        // [ADDED] ensure any prior animations are stopped so we don't fight old coroutines
        StopAllCoroutines();
        if (pulseRoutine != null)
        {
            GameTools.StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
        transform.localScale = initialScale;
        
        if (isWorldSpace)
        {
            tmpTextWorld.text = text;
            textColor = color;
            initialPosition = transform.position;
            tmpTextWorld.color = new Color(textColor.r, textColor.g, textColor.b, fadeAlphaStart);
        }
        else
        {
            tmpTextUi.text = text;
            textColor = color;
            initialPosition = rectTransform.anchoredPosition;
            tmpTextUi.color = new Color(textColor.r, textColor.g, textColor.b, fadeAlphaStart);
        }

        this.isWorldSpace = isWorldSpace;
        flyDir = dir;

        float newDuration = Mathf.Min(fadeDuration + (pulseMaxScale * popRatioDurationFactor), fadeDurationMax);
        
        StartCoroutine(FadeAndMove(alphaMax, newDuration, pulseMaxScale * speedBoostFactor));

        // Nonlinear pop around 1: f(1)=1, f(s)=1 + gain*(s-1)^exp, capped by popMax
        float scale = Mathf.Max(0f, pulseMaxScale);
        float amplifiedScale = 1f + popGain * Mathf.Pow(scale, Mathf.Max(1f, popExponent));
        if (popMax > 0f) amplifiedScale = Mathf.Min(popMax, amplifiedScale);
        Vector3 scaleMax = new Vector3(amplifiedScale, amplifiedScale, amplifiedScale);
        
        pulseRoutine = GameTools.StartCoroutine(GameTools.PulseScaleEffect(transform, newDuration, initialScale, scaleMax));
    }

    private IEnumerator FadeAndMove(float alphaMax, float duration, float speedBoost)
    {
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            t = elapsedTime / duration;
            easedT = 1f - Mathf.Pow(1f - t, 2f);
            newPos = initialPosition + flyDir * (moveSpeed + speedBoost) * easedT;
            alpha = Mathf.Lerp(fadeAlphaStart, fadeAlphaEnd, t);
            alpha = Mathf.Clamp(alpha, 0, alphaMax);

            if (isWorldSpace)
            {
                transform.position = newPos;
                tmpTextWorld.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            }
            else
            {
                rectTransform.anchoredPosition = newPos;
                tmpTextUi.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}

using TMPro;
using UnityEngine;
using System.Collections;
using UnityEditor.EditorTools;

public class TextFly : MonoBehaviour
{
    [SerializeField] string objectPoolName;
    bool isWorldSpace = false;
    public float moveSpeed = 8f;
    [SerializeField] float moveSpeedVariation = 5f; 
    public float fadeDuration = 2f;
    [SerializeField] float fadeAlphaStart = 1f;
    [SerializeField] float fadeAlphaEnd = 0f;
    public float alphaMax = 0.5f;

    [Header("Text pop curve (around 1 stays ~1)")]
    [Tooltip("How much to amplify the pop effect")]
    [SerializeField] float popGain = 1f;
    [Tooltip("Exponent for pop amplification")]
    [SerializeField] float popExponent = 2f;  
    [Tooltip("Maximum scale for pop effect (0 = no cap)")]
    [SerializeField] float popMax = 6f;

    Vector3 initialScale;
    TextMeshPro tmpTextWorld;
    TextMeshProUGUI tmpTextUi;
    Color textColor;
    Vector2 initialPosition;
    Vector2 flyDir;
    RectTransform rectTransform;

    void Awake()
    {
        initialScale = transform.localScale;
        moveSpeed += Random.Range(moveSpeed - moveSpeedVariation, moveSpeed + moveSpeedVariation);
    }

    public void Initialize(string text, Color color, float alphaMax, Vector2 dir, bool isWorldSpace, float pulseMaxScale)
    {
        if (isWorldSpace)
        {
            tmpTextWorld = GetComponent<TextMeshPro>();
            tmpTextWorld.text = text;
            textColor = tmpTextWorld.color;
            initialPosition = transform.position;
            if (color != null)
                textColor = color;
        }
        else
        {
            tmpTextUi = GetComponent<TextMeshProUGUI>();
            tmpTextUi.text = text;
            textColor = tmpTextUi.color;
            rectTransform = GetComponent<RectTransform>();
            initialPosition = rectTransform.anchoredPosition;
            if (color != null)
                textColor = color;
        }

        this.isWorldSpace = isWorldSpace;
        flyDir = dir;

        StartCoroutine(FadeAndMove(alphaMax));

        // Nonlinear pop around 1: f(1)=1, f(s)=1 + gain*(s-1)^exp, capped by popMax
        float delta = Mathf.Max(0f, pulseMaxScale - 1f);
        float amplifiedScale = 1f + popGain * Mathf.Pow(delta, Mathf.Max(1f, popExponent));
        if (popMax > 0f) amplifiedScale = Mathf.Min(popMax, amplifiedScale);
        Vector3 scaleMax = new Vector3(amplifiedScale, amplifiedScale, amplifiedScale);
        
        GameTools.StartCoroutine(GameTools.PulseScaleEffect(transform, fadeDuration, initialScale, scaleMax));
    }

    private IEnumerator FadeAndMove(float alphaMax)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            Vector2 newPos = initialPosition + flyDir * moveSpeed * easedT;
            float alpha = Mathf.Lerp(fadeAlphaStart, fadeAlphaEnd, t);
            alpha = Mathf.Clamp(alpha, 0, alphaMax);

            // Update based on space type
            if (isWorldSpace)
            {
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
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

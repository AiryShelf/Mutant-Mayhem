using TMPro;
using UnityEngine;
using System.Collections;

public class TextFly : MonoBehaviour
{
    [SerializeField] string objectPoolName;
    bool isWorldSpace = false;
    public float moveSpeed = 8f;
    [SerializeField] float moveSpeedVariation = 5f; 
    public float fadeDuration = 2f;
    [SerializeField] float fadeStart = 1f;
    [SerializeField] float fadeEnd = 0f;
    public float alphaMax = 0.5f;
    TextMeshPro tmpTextWorld;
    TextMeshProUGUI tmpTextUi;
    Color textColor;
    Vector2 initialPosition;
    Vector2 flyDir;
    RectTransform rectTransform;

    void Awake()
    {
        moveSpeed += Random.Range(moveSpeed - moveSpeedVariation, moveSpeed + moveSpeedVariation);
    }

    void OnEnable()
    {
        alphaMax = 0.5f;
    }

    public void Initialize(string text, Color color, float alphaMax, Vector2 dir, bool isWorldSpace)
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
        this.alphaMax = alphaMax;

        StartCoroutine(FadeAndMove());
    }

    private IEnumerator FadeAndMove()
    {
        
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 2f);

            // Update based on space type
            if (isWorldSpace)
            {
                // Convert world space to screen space and move the text
                Vector3 worldPos = initialPosition + flyDir * moveSpeed * easedT;
                transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
                float alpha = Mathf.Lerp(fadeStart, fadeEnd, t);
                alpha = Mathf.Clamp(alpha, 0, alphaMax);
                tmpTextWorld.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            }
            else
            {
                // Move in canvas
                Vector2 newPos = initialPosition + flyDir * moveSpeed * easedT;
                rectTransform.anchoredPosition = newPos;
                float alpha = Mathf.Lerp(fadeStart, fadeEnd, t);
                tmpTextUi.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}

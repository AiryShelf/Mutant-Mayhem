using TMPro;
using UnityEngine;
using System.Collections;

public class TextFly : MonoBehaviour
{
    bool isWorldSpace = false;
    public float moveSpeed = 8f;
    [SerializeField] float moveSpeedVariation = 5f; 
    public float fadeDuration = 2f;
    [SerializeField] float fadeStart = 1f;
    [SerializeField] float fadeEnd = 0f;
    TextMeshPro tmpTextWorld;
    TextMeshProUGUI tmpTextUi;
    Color textColor;
    Vector2 initialPosition;
    Vector2 flyDir;
    RectTransform rectTransform;

    void Start()
    {
        moveSpeed += Random.Range(moveSpeed - moveSpeedVariation, moveSpeed + moveSpeedVariation);
        StartCoroutine(FadeAndMove());
    }

    public void Initialize(string text, Vector2 dir, bool isWorldSpace)
    {
        if (isWorldSpace)
        {
            tmpTextWorld = GetComponent<TextMeshPro>();
            tmpTextWorld.text = text;
            textColor = tmpTextWorld.color;
            initialPosition = transform.position;
        }
        else
        {
            tmpTextUi = GetComponent<TextMeshProUGUI>();
            tmpTextUi.text = text;
            textColor = tmpTextUi.color;
            rectTransform = GetComponent<RectTransform>();
            initialPosition = rectTransform.anchoredPosition;
        }

        this.isWorldSpace = isWorldSpace;
        flyDir = dir;
    }

    private IEnumerator FadeAndMove()
    {
        
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Apply easing out effect: the speed decreases as the text moves up
            float t = elapsedTime / fadeDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 2f);

            // Update based on space type
            if (isWorldSpace)
            {
                // Convert world space to screen space and move the text
                Vector3 worldPos = initialPosition + flyDir * moveSpeed * easedT;
                transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
                float alpha = Mathf.Lerp(fadeStart, fadeEnd, t);
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

        Destroy(gameObject);  // Destroy the text after it fades out
    }
}

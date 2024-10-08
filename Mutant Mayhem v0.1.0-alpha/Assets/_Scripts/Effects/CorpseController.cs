using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseController : MonoBehaviour
{
    [SerializeField] Sprite[] corpseSprites;
    [SerializeField] float timeToStartFade;
    [SerializeField] float timeForFade;

    SpriteRenderer mySr;

    void Start()
    {
        // Select random corpseSprite
        mySr = GetComponent<SpriteRenderer>();
        int randIndex = Random.Range(0, corpseSprites.Length);
        mySr.sprite = corpseSprites[randIndex];

        // Random flip 
        int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        //Debug.Log(sign);
        mySr.transform.localScale = new Vector3 (
                                    mySr.transform.localScale.x, 
                                    mySr.transform.localScale.y * sign, 
                                    mySr.transform.localScale.z);

        // Reduce alpha and darken
        Color color = mySr.color;
        Color.RGBToHSV(color, out float h, out float s, out float v);
        v *= 0.9f;
        
        Color newColor = Color.HSVToRGB(h, s, v);
        newColor.a = 0.9f;

        mySr.color = newColor;
        
        StartCoroutine(WaitToFade());
    }

    IEnumerator WaitToFade()
    {
        yield return new WaitForSeconds(timeToStartFade);
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        Color startColor = mySr.color;
        float timeElapsed = 0;

        while (timeElapsed < timeForFade)
        {
            yield return new WaitForFixedUpdate();
            timeElapsed += Time.fixedDeltaTime;

            Color newColor = Color.Lerp(startColor, 
                             new Color(0,0,0,0), timeElapsed / timeForFade);
            
            mySr.color = newColor;
        }
        Destroy(gameObject);
    }
}

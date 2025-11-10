using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCorpseController : CorpseController
{
    [Header("Set at runtime by EnemyHealth or DroneHealth")]
    public Sprite[] corpseSprites;
    [Header("Corpse Settings")]
    [SerializeField] string corpseExplosionPoolName = "Explosion_Corpse_Red";

    SpriteRenderer mySr;


    void Awake()
    {
        mySr = GetComponent<SpriteRenderer>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        mySr.color = startColor;
    }

    public void SetSpriteAndDie(Color color, float scale, float corpseExplosionScaleFactor)
    {
        // Call explosion
        GameObject explosion = PoolManager.Instance.GetFromPool(corpseExplosionPoolName);
        explosion.transform.position = transform.position;

        // Set explosion scale based on sprite pixel size and scale
        Vector2 newScale = new Vector2(
            mySr.sprite.rect.width / 480f * scale,
            mySr.sprite.rect.height / 480f * scale);
        explosion.transform.localScale = new Vector3(newScale.x, newScale.y, 1);
        explosion.transform.localScale *= corpseExplosionScaleFactor;

        // Set sprite scale
        mySr.transform.localScale = Vector3.one * scale * 0.9f; // Scale down a bit

        // Select random corpseSprite
        int randIndex = Random.Range(0, corpseSprites.Length - 1);
        mySr.sprite = corpseSprites[randIndex];

        // Random flip 
        int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        mySr.transform.localScale = new Vector3(
                                    mySr.transform.localScale.x,
                                    mySr.transform.localScale.y * sign,
                                    mySr.transform.localScale.z);

        // Reduce alpha and darken
        Color.RGBToHSV(color, out float h, out float s, out float v);
        v *= 0.9f;
        Color newColor = Color.HSVToRGB(h, s, v);
        newColor.a = 0.9f;
        mySr.color = newColor;

        StartCoroutine(WaitToFade());
    }

    protected override IEnumerator FadeOut()
    {
        startColor = mySr.color;
        float timeElapsed = 0;

        while (timeElapsed < timeForFade)
        {
            yield return new WaitForSeconds(0.25f);
            timeElapsed += 0.25f;

            Color newColor = Color.Lerp(startColor, 
                             new Color(startColor.r, startColor.g, startColor.b, 0), timeElapsed / timeForFade);
            
            mySr.color = newColor;
        }

        PoolManager.Instance.ReturnToPool(corpsePoolName, gameObject);
    }
}

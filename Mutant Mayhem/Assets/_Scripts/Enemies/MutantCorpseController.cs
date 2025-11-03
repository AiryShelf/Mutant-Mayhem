using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantCorpseController : CorpseController
{
    [Header("Mutant Corpse")]
    public SpriteRenderer bodySR;
    public SpriteRenderer headSR;
    public SpriteRenderer leftLegSR;
    public SpriteRenderer rightLegSR;

    List<SpriteRenderer> allSpriteRenderers = new List<SpriteRenderer>();

    void Awake()
    {
        allSpriteRenderers.Add(bodySR);
        allSpriteRenderers.Add(headSR);
        allSpriteRenderers.Add(leftLegSR);
        allSpriteRenderers.Add(rightLegSR);
    }

    void OnEnable()
    {
        StartCoroutine(WaitToFade());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    /// <summary>
    /// Applies the given genome to the corpse prefab
    /// </summary>
    /// <param name="g"></param>
    public void ApplyGenome(Genome g)
    {
        // Set anchors
        headSR.transform.localPosition = g.bodyGene.headAnchorOffset * g.headGene.scale;
        leftLegSR.transform.localPosition = g.bodyGene.leftLegAnchorOffset * g.legGene.scale;
        rightLegSR.transform.localPosition = g.bodyGene.rightLegAnchorOffset * g.legGene.scale;

        // Set sprites and colors
        bodySR.sprite = g.bodyGene.corpseSprites[Random.Range(0, g.bodyGene.corpseSprites.Count)];
        Color workingColor = g.bodyGene.color;
        workingColor.a = 0.8f;
        bodySR.color = g.bodyGene.color;
        headSR.sprite = g.headGene.corpseSprites[Random.Range(0, g.headGene.corpseSprites.Count)];
        headSR.color = g.headGene.color;
        leftLegSR.sprite = g.legGene.leftLegCorpseSprites[Random.Range(0, g.legGene.leftLegCorpseSprites.Count)];
        leftLegSR.color = g.legGene.color;
        rightLegSR.sprite = g.legGene.leftLegCorpseSprites[Random.Range(0, g.legGene.rightLegCorpseSprites.Count)];
        rightLegSR.color = g.legGene.color;

        // Apply scales
        bodySR.transform.localScale = Vector3.one * g.bodyGene.scale;
        headSR.transform.localScale = Vector3.one * g.headGene.scale;
        leftLegSR.transform.localScale = Vector3.one * g.legGene.scale;
        rightLegSR.transform.localScale = Vector3.one * g.legGene.scale;

        SetAlphaAndDarkenAllSprites(0.9f, 0.9f);
    }

    protected override IEnumerator FadeOut()
    {
        startColor = bodySR.color; // Used for alpha reset on disable

        // Reduce the alpha of each SR over time
        float elapsedTime = 0f;
        float startAlpha = bodySR.color.a;
        while (elapsedTime < timeForFade)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeForFade;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, t);
            foreach (SpriteRenderer sr in allSpriteRenderers)
            {
                Color color = sr.color;
                color.a = newAlpha;
                sr.color = color;
            }

            yield return null;
        }

        PoolManager.Instance.ReturnToPool(corpsePoolName, gameObject);
    }

    void SetAlphaAndDarkenAllSprites(float alpha = 0.9f, float darkenFactor = 0.9f)
    {
        foreach (SpriteRenderer sr in allSpriteRenderers)
        {
            var color = sr.color;
            color.a = alpha;
            sr.color = color;

            Color.RGBToHSV(color, out float h, out float s, out float v);
            v *= darkenFactor;
            Color newColor = Color.HSVToRGB(h, s, v);
            newColor.a = alpha;
            sr.color = newColor;
        }
        
    }
}

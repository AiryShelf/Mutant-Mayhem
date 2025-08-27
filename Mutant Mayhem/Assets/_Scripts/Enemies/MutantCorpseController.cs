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

    void OnEnable()
    {
        SpriteRenderer[] spriteRenderers = new SpriteRenderer[] { bodySR, headSR, leftLegSR, rightLegSR };
        foreach (SpriteRenderer sr in spriteRenderers)
        { 
            var color = sr.color;
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v *= 0.9f;
            Color newColor = Color.HSVToRGB(h, s, v);
            newColor.a = 0.9f;
            sr.color = newColor;
        }

        StartCoroutine(WaitToFade());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bodySR.color = startColor;
        headSR.color = startColor;
        leftLegSR.color = startColor;
        rightLegSR.color = startColor;
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

            bodySR.color = new Color(bodySR.color.r, bodySR.color.g, bodySR.color.b, newAlpha);
            headSR.color = new Color(headSR.color.r, headSR.color.g, headSR.color.b, newAlpha);
            leftLegSR.color = new Color(leftLegSR.color.r, leftLegSR.color.g, leftLegSR.color.b, newAlpha);
            rightLegSR.color = new Color(rightLegSR.color.r, rightLegSR.color.g, rightLegSR.color.b, newAlpha);

            yield return null;
        }

        PoolManager.Instance.ReturnToPool(corpsePoolName, gameObject);
    }
}

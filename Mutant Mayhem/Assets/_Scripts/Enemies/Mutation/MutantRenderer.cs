using System.Collections.Generic;
using UnityEngine;

public class MutantRenderer : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] Transform headAnchor;
    [SerializeField] Transform leftLegAnchor;
    [SerializeField] Transform rightLegAnchor;

    [Header("Sprite Renderers")]
    //[SerializeField] Transform enemyBaseTransform;
    [SerializeField] SpriteRenderer bodySR;
    [SerializeField] SpriteRenderer headSR;
    [SerializeField] AnimationControllerMutant animationControllerMutant;
    [SerializeField] float minColorBrightness = 0.75f;

    SpriteRenderer leftLegSR;
    SpriteRenderer rightLegSR;

    void Awake()
    {
        // Get the SpriteRenderers from the AnimationControllerMutant
        leftLegSR = animationControllerMutant.leftLegAnimator.GetComponent<SpriteRenderer>();
        rightLegSR = animationControllerMutant.rightLegAnimator.GetComponent<SpriteRenderer>();
    }

    public void ApplyGenome(Genome g)
    {
        GeneDatabase.InitialiseIfNeeded();

        // Look‑ups
        var bodyGene = GeneDatabase.Body(g.bodyGene.id);
        var headGene = GeneDatabase.Head(g.headGene.id);
        var legGene = GeneDatabase.Leg(g.legGene.id);

        /* 1️⃣  Set sprites and animators */
        bodySR.sprite = bodyGene.sprite;
        headSR.sprite = headGene.sprite;
        animationControllerMutant.leftLegAnimator.runtimeAnimatorController = legGene.leftLegAnimatorController;
        animationControllerMutant.rightLegAnimator.runtimeAnimatorController = legGene.rightLegAnimatorController;
        animationControllerMutant.animSpeedFactor = legGene.animSpeedFactor;
        animationControllerMutant.switchToRunBuffer = legGene.switchToRunBuffer;
        animationControllerMutant.maxAnimSpeed = legGene.maxAnimSpeed;

        /* 2️⃣  ✏  CHANGED  — Apply scales */
        //enemyBaseTransform.localScale = Vector3.one * g.bodyScale;  // Scale the whole enemy base
        bodySR.transform.localScale = Vector3.one * g.bodyGene.scale;
        headAnchor.localScale = Vector3.one * g.headGene.scale;
        leftLegAnchor.localScale = Vector3.one * g.legGene.scale;
        rightLegAnchor.localScale = Vector3.one * g.legGene.scale;

        headAnchor.localPosition = bodyGene.headAnchorOffset * g.bodyGene.scale;
        leftLegAnchor.localPosition = bodyGene.leftLegAnchorOffset * g.bodyGene.scale;
        rightLegAnchor.localPosition = bodyGene.rightLegAnchorOffset * g.bodyGene.scale;
    }

    public void RandomizeColor(float randColorRange)
    {
        SpriteRenderer[] renderers = new SpriteRenderer[]
        {
            bodySR,
            headSR,
            leftLegSR,
            rightLegSR
        };

        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            float red = color.r + Random.Range(-randColorRange, randColorRange);
            float green = color.g + Random.Range(-randColorRange, randColorRange);
            float blue = color.b + Random.Range(-randColorRange, randColorRange);

            // Clamp color values to stay within range
            Mathf.Clamp01(red);
            Mathf.Clamp01(green);
            Mathf.Clamp01(blue);

            // prevent division by zero, ensure color is bright enough
            if (red + green + blue == 0f)
            {
                red = minColorBrightness / 3;
                green = minColorBrightness / 3;
                blue = minColorBrightness / 3;
            }
            else
            if (red + green + blue < minColorBrightness)
            {
                var factor = minColorBrightness / (red + green + blue);
                red *= factor;
                green *= factor;
                blue *= factor;
            }

            red = Mathf.Clamp01(red);
            green = Mathf.Clamp01(green);
            blue = Mathf.Clamp01(blue);

            renderer.color = new Color(red, green, blue, color.a);
        }
    }

    public Color RandomizePartColor(Color color, float randColorRange)
    {
        float red = color.r + Random.Range(-randColorRange, randColorRange);
        float green = color.g + Random.Range(-randColorRange, randColorRange);
        float blue = color.b + Random.Range(-randColorRange, randColorRange);

        // Clamp color values to stay within range
        red = Mathf.Clamp01(red);
        green = Mathf.Clamp01(green);
        blue = Mathf.Clamp01(blue);

        // prevent division by zero, ensure color is bright enough
        if (red + green + blue == 0f)
        {
            red = minColorBrightness / 3;
            green = minColorBrightness / 3;
            blue = minColorBrightness / 3;
        }
        else
        if (red + green + blue < minColorBrightness)
        {
            var factor = minColorBrightness / (red + green + blue);
            red *= factor;
            green *= factor;
            blue *= factor;
        }

        return new Color(red, green, blue, color.a);
    }
}
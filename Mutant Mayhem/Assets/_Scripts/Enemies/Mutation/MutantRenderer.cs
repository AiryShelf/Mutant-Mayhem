using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    public AnimationControllerMutant animationControllerMutant;
    [SerializeField] float minColorBrightness = 0.75f;

    SpriteRenderer leftLegSR;
    SpriteRenderer rightLegSR;

    ShadowCaster2D shadowCaster2D;

    void Awake()
    {
        // Get the SpriteRenderers from the AnimationControllerMutant
        leftLegSR = animationControllerMutant.leftLegAnimator.GetComponent<SpriteRenderer>();
        rightLegSR = animationControllerMutant.rightLegAnimator.GetComponent<SpriteRenderer>();
        shadowCaster2D = GetComponent<ShadowCaster2D>();
    }

    public void ApplyGenome(Genome g)
    {
        GeneDatabase.InitialiseIfNeeded();

        // Look‑ups
        var bodyGeneBase = GeneDatabase.Body(g.bodyGene.id);
        var headGeneBase = GeneDatabase.Head(g.headGene.id);
        var legGeneBase = GeneDatabase.Leg(g.legGene.id);

        // Set sprites/animators and colors
        bodySR.sprite = bodyGeneBase.sprite;
        bodySR.color = g.bodyGene.color;
        headSR.sprite = headGeneBase.sprite;
        headSR.color = g.headGene.color;
        animationControllerMutant.leftLegAnimator.runtimeAnimatorController = legGeneBase.leftLegAnimatorController;
        leftLegSR.color = g.legGene.color;
        animationControllerMutant.rightLegAnimator.runtimeAnimatorController = legGeneBase.rightLegAnimatorController;
        rightLegSR.color = g.legGene.color;

        // Set sprite sorting layers
        if (legGeneBase.isFlying)
        {
            bodySR.sortingLayerName = "FlyingEnemies";
            headSR.sortingLayerName = "FlyingEnemies";
            leftLegSR.sortingLayerName = "FlyingEnemies";
            rightLegSR.sortingLayerName = "FlyingEnemies";
        }
        else
        {
            bodySR.sortingLayerName = "Enemies";
            headSR.sortingLayerName = "Enemies";
            leftLegSR.sortingLayerName = "Enemies";
            rightLegSR.sortingLayerName = "Enemies";
        }

        // Scale anim speeds with leg size with inverse relation, with a base of 8 scale
        animationControllerMutant.animSpeedFactor = legGeneBase.animSpeedFactor * (8 / g.legGene.scale);
        animationControllerMutant.switchToRunBuffer = legGeneBase.switchToRunBuffer * (8 / g.legGene.scale);
        animationControllerMutant.maxAnimSpeed = legGeneBase.maxAnimSpeed;

        // Apply scales
        //enemyBaseTransform.localScale = Vector3.one * g.bodyScale;  // Scale the whole enemy base
        bodySR.transform.localScale = Vector3.one * g.bodyGene.scale * WaveController.Instance.sizeMultiplier;
        headAnchor.localScale = Vector3.one * g.headGene.scale * WaveController.Instance.sizeMultiplier;
        leftLegAnchor.localScale = Vector3.one * g.legGene.scale * WaveController.Instance.sizeMultiplier;
        rightLegAnchor.localScale = Vector3.one * g.legGene.scale * WaveController.Instance.sizeMultiplier;

        headAnchor.localPosition = bodyGeneBase.headAnchorOffset * g.bodyGene.scale * WaveController.Instance.sizeMultiplier;
        leftLegAnchor.localPosition = bodyGeneBase.leftLegAnchorOffset * g.bodyGene.scale * WaveController.Instance.sizeMultiplier;
        rightLegAnchor.localPosition = bodyGeneBase.rightLegAnchorOffset * g.bodyGene.scale * WaveController.Instance.sizeMultiplier;

        if (!g.legGene.bodyCastsShadows)
        {
            shadowCaster2D.enabled = false;
            return;
        }
        else
        {
            // Update shadow shape
            shadowCaster2D.enabled = true;

            var path = bodyGeneBase.shadowShapePoints;
            Vector3[] vertices = new Vector3[path.Length];

            for (int i = 0; i < path.Length; i++)
            {
                Vector2 p = path[i];
                p.x *= g.bodyGene.scale;
                p.y *= g.bodyGene.scale;
                vertices[i] = new Vector3(p.x, p.y, 0);
            }

            ShadowCaster2DHelper.SetShapePath(shadowCaster2D, vertices);
        }
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
}
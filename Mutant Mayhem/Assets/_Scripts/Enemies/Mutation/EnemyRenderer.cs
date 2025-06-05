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
}
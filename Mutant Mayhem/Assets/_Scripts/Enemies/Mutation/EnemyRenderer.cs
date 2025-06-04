using UnityEngine;

public class EnemyRenderer : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] Transform headAnchor;
    [SerializeField] Transform leftLegAnchor;
    [SerializeField] Transform rightLegAnchor;

    [Header("Sprite Renderers")]
    [SerializeField] Transform enemyBaseTransform;
    [SerializeField] SpriteRenderer bodySR;
    [SerializeField] SpriteRenderer headSR;
    [SerializeField] AnimationControllerMutant animationControllerMutant;

    public void ApplyGenome(Genome g)
    {
        GeneDatabase.InitialiseIfNeeded();

        // Look‑ups
        var bodyGene = GeneDatabase.Body(g.bodyId);
        var headGene = GeneDatabase.Head(g.headId);
        var legGene = GeneDatabase.Leg(g.legId);

        /* 1️⃣  Set sprites and animators */
        bodySR.sprite = bodyGene.sprite;
        headSR.sprite = headGene.sprite;
        animationControllerMutant.leftLegAnimator.runtimeAnimatorController = legGene.leftLegAnimatorController;
        animationControllerMutant.rightLegAnimator.runtimeAnimatorController = legGene.rightLegAnimatorController;
        animationControllerMutant.animSpeedFactor = legGene.animSpeedFactor;
        animationControllerMutant.switchToRunBuffer = legGene.switchToRunBuffer;
        animationControllerMutant.maxAnimSpeed = legGene.maxAnimSpeed;

        /* 2️⃣  ✏  CHANGED  — Apply scales */
        enemyBaseTransform.localScale = Vector3.one * g.bodyScale;  // Scale the whole enemy base
        bodySR.transform.localScale = Vector3.one * g.bodyScale;
        headAnchor.localScale = Vector3.one * g.headScale;
        leftLegAnchor.localScale = Vector3.one * g.legScale;
        rightLegAnchor.localScale = Vector3.one * g.legScale;

        headAnchor.localPosition = bodyGene.headAnchorOffset * g.bodyScale;
        leftLegAnchor.localPosition = bodyGene.leftLegAnchorOffset * g.bodyScale;
        rightLegAnchor.localPosition = bodyGene.rightLegAnchorOffset * g.bodyScale;
    }
}
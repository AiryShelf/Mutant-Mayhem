using UnityEngine;

public class EnemyRenderer : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] private Transform headAnchor;
    [SerializeField] private Transform leftLegAnchor;
    [SerializeField] private Transform rightLegAnchor;

    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer bodySR;
    [SerializeField] private SpriteRenderer headSR;
    [SerializeField] private SpriteRenderer leftLegSR;
    [SerializeField] private SpriteRenderer rightLegSR;

    public void ApplyGenome(Genome g)
    {
        GeneDatabase.InitialiseIfNeeded();

        // Look‑ups
        var bodyGene = GeneDatabase.Body(g.bodyId);
        var headGene = GeneDatabase.Head(g.headId);
        var lLegGene = GeneDatabase.Leg(g.leftLegId);
        var rLegGene = GeneDatabase.Leg(g.rightLegId);

        /* 1️⃣  Set sprites (unchanged) */
        bodySR.sprite = bodyGene.sprite;
        headSR.sprite = headGene.sprite;
        leftLegSR.sprite = lLegGene.lSprite;
        rightLegSR.sprite = rLegGene.rSprite;

        /* 2️⃣  ✏  CHANGED  — Apply scales */
        bodySR.transform.localScale = Vector3.one * g.bodyScale;
        headSR.transform.localScale = Vector3.one * g.headScale;
        leftLegSR.transform.localScale = Vector3.one * g.leftLegScale;
        rightLegSR.transform.localScale = Vector3.one * g.rightLegScale;

        /* 3️⃣  ✏  CHANGED  — Anchor positions scale with BODY */
        headAnchor.localPosition = bodyGene.headAnchorOffset * g.bodyScale;
        leftLegAnchor.localPosition = bodyGene.leftLegAnchorOffset * g.bodyScale;
        rightLegAnchor.localPosition = bodyGene.rightLegAnchorOffset * g.bodyScale;
    }
    
#if UNITY_EDITOR
    // 🔸 Convenience button in Inspector for quick preview
    [ContextMenu("Apply Random Genome")]
    private void ApplyRandom()
    {
        var g = new Genome(
            GetRandom<BodyGeneSO>().id,
            GetRandom<HeadGeneSO>().id,
            GetRandom<LegGeneSO>().id,
            GetRandom<LegGeneSO>().id,
            Random.Range(0.7f, 1.3f),  // body scale
            Random.Range(0.7f, 1.3f),  // head scale
            Random.Range(0.7f, 1.3f),  // left leg scale
            Random.Range(0.7f, 1.3f)   // right leg scale
        );

        ApplyGenome(g);
    }
    private T GetRandom<T>() where T : UnityEngine.Object
    {
        var all = Resources.FindObjectsOfTypeAll(typeof(T));
        if (all.Length == 0) return null;

        return (T)all[Random.Range(0, all.Length)];
    }
#endif
}
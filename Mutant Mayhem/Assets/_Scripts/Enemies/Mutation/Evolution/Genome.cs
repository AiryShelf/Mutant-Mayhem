using UnityEngine;

[System.Serializable]
public class Genome
{
    [Header("Genes")]
    public BodyGeneSO bodyGene;
    public HeadGeneSO headGene;
    public LegGeneSO legGene;

    [Header("Body Parts")]
    public string bodyId, headId, legId;

    public float bodyScale = 1f;
    public float headScale = 1f;
    public float legScale = 1f;

    public int numberOfGenes => 3; // body, head, legs

    [Header("Behavior")]
    public EnemyIdleSOBase idleSOBase;
    public EnemyChaseSOBase chaseSOBase;
    public EnemyShootSOBase shootSOBase;

    [Header("Other")]
    public Vector2 bodyColliderOffset = new Vector2(0, 0);
    public Vector2 bodyColliderSize = new Vector2(1, 1);

    public Genome(
        string body, string head,
        string leg,
        float bScale = 1, float hScale = 1,
        float lScale = 1,
        EnemyIdleSOBase idle = null,
        EnemyChaseSOBase chase = null)
    {
        bodyId = body; headId = head; legId = leg;
        bodyScale = bScale; headScale = hScale; legScale = lScale;
        idleSOBase = idle; chaseSOBase = chase;
    }
    
    public Genome(BodyGeneSO body, HeadGeneSO head, LegGeneSO leg)
    {
        bodyGene = body;
        headGene = head;
        legGene = leg;
    }
}
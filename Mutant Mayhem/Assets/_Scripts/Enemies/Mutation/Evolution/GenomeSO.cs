using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Genome",
                 menuName = "EnemyEvolution/GenomeSO")]
public class GenomeSO : ScriptableObject
{
    [Header("Body Parts")]
    public string bodyId, headId, legId;

    // 🔸 NEW – scale factors (1 = default size)
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

    public Genome ToGenome()
    {
        return new Genome(
            bodyId, headId, legId,
            bodyScale, headScale, legScale,
            idleSOBase, chaseSOBase);
    }
}

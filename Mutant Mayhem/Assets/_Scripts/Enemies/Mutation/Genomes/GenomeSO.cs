using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Genome",
                 menuName = "EnemyEvolution/GenomeSO")]
public class GenomeSO : ScriptableObject
{
    [Header("Genes")]
    public BodyGeneSO bodyGene;
    public HeadGeneSO headGene;
    public LegGeneSO  legGene;

    public Genome ToGenome()
    {
        return new Genome(bodyGene, headGene, legGene)
        {
            bodyGene = bodyGene,
            headGene = headGene,
            legGene =  legGene         
        };
    }
}

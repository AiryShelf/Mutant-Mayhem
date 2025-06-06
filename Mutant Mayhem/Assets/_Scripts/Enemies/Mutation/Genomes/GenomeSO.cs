using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Genome",
                 menuName = "EnemyEvolution/GenomeSO")]
public class GenomeSO : ScriptableObject
{
    [Header("Genes")]
    public BodyGeneSO bodyGeneSO;
    public HeadGeneSO headGeneSO;
    public LegGeneSO  legGeneSO;

    public Genome ToGenome()
    {
        return new Genome(bodyGeneSO, headGeneSO, legGeneSO)
        {
            bodyGene = Instantiate(bodyGeneSO),
            headGene = Instantiate(headGeneSO),
            legGene = Instantiate(legGeneSO)      
        };
    }
}

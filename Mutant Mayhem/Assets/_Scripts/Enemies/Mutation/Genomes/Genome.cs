using UnityEngine;

[System.Serializable]
public class Genome
{
    [Header("Genes")]
    public BodyGeneSO bodyGene;
    public HeadGeneSO headGene;
    public LegGeneSO  legGene;

    public int numberOfGenes => 3; // body, head, legs
    
    public Genome(BodyGeneSO body, HeadGeneSO head, LegGeneSO leg)
    {
        bodyGene = body;
        headGene = head;
        legGene =  leg;
    }
}
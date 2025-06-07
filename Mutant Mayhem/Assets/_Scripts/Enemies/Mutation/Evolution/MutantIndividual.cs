using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MutantVariant { Runner, Chaser, Siege }

public class MutantIndividual
{
    public Genome genome;
    public MutantVariant variant;
    public float fitness;

    public MutantIndividual(Genome g, MutantVariant v)
    {
        genome = g; variant = v;
    }

    public void AddFitness(float amount)
    {
        fitness += amount;
    }

    public MutantIndividual CloneBare() => new MutantIndividual(new Genome(genome.bodyGene, genome.headGene, genome.legGene), variant);
}

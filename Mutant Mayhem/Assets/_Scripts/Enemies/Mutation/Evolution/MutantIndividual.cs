using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MutantVariant { Fighter, Fodder }

public class MutantIndividual
{
    public Genome genome;
    public MutantVariant variant;
    public int lifetimes;
    float cumulativeFitness;

    public float AverageFitness => cumulativeFitness / Mathf.Max(1, lifetimes);

    public MutantIndividual(Genome g, MutantVariant v)
    {
        genome = g; variant = v;
    }

    public void AddFitness(float amount)
    {
        cumulativeFitness += amount;
        lifetimes++;
    }
}

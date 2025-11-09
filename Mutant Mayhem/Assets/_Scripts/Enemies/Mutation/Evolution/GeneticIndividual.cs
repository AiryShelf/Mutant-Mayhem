using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PopulationVariantType { Fighter, Fodder }

public class GeneticIndividual
{
    public Genome genome;
    public PopulationVariantType variant;
    public int lifetimeCount;
    public float fitnessCount;

    public float AverageFitness => fitnessCount / Mathf.Max(1, lifetimeCount);

    public GeneticIndividual(Genome g, PopulationVariantType v)
    {
        genome = g; variant = v;
    }

    public void AddFitness(float amount)
    {
        fitnessCount += amount;
        lifetimeCount++;
    }
}

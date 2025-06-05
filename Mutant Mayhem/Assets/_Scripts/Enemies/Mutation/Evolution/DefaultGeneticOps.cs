using UnityEngine;
using System.Collections.Generic;

public class DefaultGeneticOps
{
    private BodyGeneSO[] bodies;
    private HeadGeneSO[] heads;
    private LegGeneSO[] legs;

    public DefaultGeneticOps()
    {
        GeneDatabase.InitialiseIfNeeded();
        bodies = Resources.LoadAll<BodyGeneSO>("");
        heads = Resources.LoadAll<HeadGeneSO>("");
        legs = Resources.LoadAll<LegGeneSO>("");
    }

    public Genome Crossover(Genome a, Genome b)
    {
        Debug.Log("Crossover between genomes: " + a + " and " + b);

        if (a == null || b == null)
        {
            Debug.LogError("Crossover failed: One or both genomes are null.");
            return null;
        }
        if (a.numberOfGenes != b.numberOfGenes)
        {
            Debug.LogError($"Crossover failed: Number of genes mismatch. a: {a.numberOfGenes}, b: {b.numberOfGenes}");
            return null;
        }

        // Start with a copy of A
        string bodyId = a.bodyGene.id;
        string headId = a.headGene.id;
        string legId = a.legGene.id;
        float bodyScale = a.bodyGene.scale;
        float headScale = a.headGene.scale;
        float legScale = a.legGene.scale;
        EnemyIdleSOBase idleSOBase = a.legGene.idleSOBase;
        EnemyChaseSOBase chaseSOBase = a.legGene.chaseSOBase;

        // Decide how many parts to exchange (min 1, max all but one)
        int partsToSwap = Random.Range(1, a.numberOfGenes);
        var partIndices = new System.Collections.Generic.HashSet<int>();
        while (partIndices.Count < partsToSwap)
            partIndices.Add(Random.Range(0, a.numberOfGenes));

        foreach (int index in partIndices)
        {
            switch (index)
            {
                case 0: // body
                    bodyId = b.bodyGene.id;
                    bodyScale = Mathf.Lerp(a.bodyGene.scale, b.bodyGene.scale, Random.value);
                    Debug.Log("Swapped to bodyId: " + bodyId + ", bodyScale: " + bodyScale);
                    break;
                case 1: // head
                    headId = b.headGene.id;
                    headScale = Mathf.Lerp(a.headGene.scale, b.headGene.scale, Random.value);
                    Debug.Log("Swapped to headId: " + headId + ", headScale: " + headScale);
                    break;
                case 2: // legs
                    legId = b.legGene.id;
                    legScale = Mathf.Lerp(a.legGene.scale, b.legGene.scale, Random.value);
                    idleSOBase = b.idleSOBase;
                    chaseSOBase = b.chaseSOBase;
                    Debug.Log("Swapped to legId: " + legId + ", legScale: " + legScale);
                    Debug.Log("Swapped idleSOBase: " + (idleSOBase != null ? idleSOBase.name : "null") +
                              ", chaseSOBase: " + (chaseSOBase != null ? chaseSOBase.name : "null"));
                    break;
                default:
                    Debug.LogError($"Invalid index {index} in crossover operation. numberOfGenes is out of range. Expected 0, 1, or 2.");
                    break;
            }
        }

        return new Genome(bodyId, headId, legId, bodyScale, headScale, legScale,
                          idleSOBase, chaseSOBase);
    }

public void Mutate(Genome genome, float mutationRate, float difficultyScaleTotal)
{
    bool mutatedPart = false;

    var population = EvolutionManager.Instance.GetPopulation();
    Debug.Log("Population variant count: " + population.Count);

    if (population.Count > 0)
        {
            //
            var allIndividuals = new List<EnemyIndividual>();
            foreach (var list in population.Values)
                allIndividuals.AddRange(list);
            Debug.Log("All individuals count: " + allIndividuals.Count);

            if (allIndividuals.Count > 0)
            {
                if (Random.value < mutationRate && !mutatedPart)
                {
                    genome.bodyId = RandomChoice(allIndividuals).genome.bodyId;
                    mutatedPart = true;
                }
                if (Random.value < mutationRate && !mutatedPart)
                {
                    genome.headId = RandomChoice(allIndividuals).genome.headId;
                    mutatedPart = true;
                }
                if (Random.value < mutationRate && !mutatedPart)
                {
                    var individual = RandomChoice(allIndividuals);
                    genome.legId = individual.genome.legId;
                    genome.idleSOBase = individual.genome.idleSOBase;
                    genome.chaseSOBase = individual.genome.chaseSOBase;
                    mutatedPart = true;
                }
            }
        }

    // 🔸 mutate scales
    float delta = 0.2f * (1 + EvolutionManager.Instance._currentWave);
    Debug.Log("EvolutionManager mutate delta: " + delta);

    if (Random.value < mutationRate)
    {
        genome.bodyGene.scale += Random.Range(-delta, delta);
        Debug.Log("Mutated bodyScale: " + genome.bodyGene.scale);
    }
    if (Random.value < mutationRate) 
    {
        genome.headGene.scale += Random.Range(-delta, delta);
        Debug.Log("Mutated headScale: " + genome.headGene.scale);
    }
    if (Random.value < mutationRate)
    {
        genome.legGene.scale += Random.Range(-delta, delta);
        Debug.Log("Mutated legScale: " + genome.legGene.scale);
    }

    ClampAndNormalize(ref genome, difficultyScaleTotal);
}

    public void ClampAndNormalize(ref Genome genome, float maxTotal)
    {
        // Keep parts from getting too small
        float minPerGene = (maxTotal / genome.numberOfGenes) * 0.5f;
        genome.bodyGene.scale = Mathf.Max(genome.bodyGene.scale, minPerGene);
        genome.headGene.scale = Mathf.Max(genome.headGene.scale, minPerGene);
        genome.legGene.scale  = Mathf.Max(genome.legGene.scale, minPerGene);

        // Normalize scales to fit within maxTotal
        float total = genome.bodyGene.scale + genome.headGene.scale + genome.legGene.scale;
        if (total > maxTotal)
        {
            float factor = maxTotal / total;
            genome.bodyGene.scale *= factor;
            genome.headGene.scale *= factor;
            genome.legGene.scale *= factor;
        }
    }

    private T RandomChoice<T>(T[] arr) => arr[Random.Range(0, arr.Length)];
    private T RandomChoice<T>(List<T> list) => list[Random.Range(0, list.Count)];
}
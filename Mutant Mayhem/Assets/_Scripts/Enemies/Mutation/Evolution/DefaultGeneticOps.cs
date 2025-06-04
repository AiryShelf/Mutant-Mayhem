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
        string bodyId = a.bodyId;
        string headId = a.headId;
        string legId = a.legId;
        float bodyScale = a.bodyScale;
        float headScale = a.headScale;
        float legScale = a.legScale;
        EnemyIdleSOBase idleSOBase = a.idleSOBase;
        EnemyChaseSOBase chaseSOBase = a.chaseSOBase;

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
                    bodyId = b.bodyId;
                    bodyScale = Mathf.Lerp(a.bodyScale, b.bodyScale, Random.value);
                    break;
                case 1: // head
                    headId = b.headId;
                    headScale = Mathf.Lerp(a.headScale, b.headScale, Random.value);
                    break;
                case 2: // legs
                    legId = b.legId;
                    legScale = Mathf.Lerp(a.legScale, b.legScale, Random.value);
                    idleSOBase = b.idleSOBase;
                    chaseSOBase = b.chaseSOBase;
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

    if (population.Count > 0)
    {
        // Flatten population to a single list
        var allIndividuals = new List<EnemyIndividual>();
        foreach (var list in population.Values)
            allIndividuals.AddRange(list);

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
    float delta = 0.2f * (1 + EvolutionManager.Instance._currentWave) * (1 + EvolutionManager.Instance.difficultyScalePerWave);
    Debug.Log("EvolutionManager delta: " + delta);

    if (Random.value < mutationRate) genome.bodyScale += Random.Range(-delta, delta);
    if (Random.value < mutationRate) genome.headScale += Random.Range(-delta, delta);
    if (Random.value < mutationRate) genome.legScale += Random.Range(-delta, delta);

    ClampAndNormalize(ref genome, difficultyScaleTotal);
}

    public void ClampAndNormalize(ref Genome genome, float maxTotal)
    {
        // Keep parts from getting too small
        float minPerGene = (maxTotal / genome.numberOfGenes) * 0.5f;
        genome.bodyScale = Mathf.Max(genome.bodyScale, minPerGene);
        genome.headScale = Mathf.Max(genome.headScale, minPerGene);
        genome.legScale  = Mathf.Max(genome.legScale, minPerGene);

        // Normalize scales to fit within maxTotal
        float total = genome.bodyScale + genome.headScale + genome.legScale;
        if (total > maxTotal)
        {
            float factor = maxTotal / total;
            genome.bodyScale *= factor;
            genome.headScale *= factor;
            genome.legScale *= factor;
        }
    }

    private T RandomChoice<T>(T[] arr) => arr[Random.Range(0, arr.Length)];
    private T RandomChoice<T>(List<T> list) => list[Random.Range(0, list.Count)];
}
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
        // Validate parents
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

        // Start with all genes from parent A
        BodyGeneSO bodyGene = a.bodyGene;
        HeadGeneSO headGene = a.headGene;
        LegGeneSO  legGene  = a.legGene;

        // Decide how many parts to swap (min 1, max 2)
        int partsToSwap = Random.Range(1, a.numberOfGenes);
        var partIndices = new HashSet<int>();
        while (partIndices.Count < partsToSwap)
            partIndices.Add(Random.Range(0, a.numberOfGenes));

        foreach (int index in partIndices)
        {
            switch (index)
            {
                case 0: // body
                    bodyGene = b.bodyGene;
                    bodyGene.scale = Mathf.Lerp(a.bodyGene.scale, b.bodyGene.scale, Random.Range(0f, 1f));
                    bodyGene.color = Color.Lerp(a.bodyGene.color, b.bodyGene.color, Random.Range(0f, 1f));
                    Debug.Log($"[Crossover] Swapped body gene to “{bodyGene.id}”");
                    break;
                case 1: // head
                    headGene = b.headGene;
                    headGene.scale = Mathf.Lerp(a.headGene.scale, b.headGene.scale, Random.Range(0f, 1f));
                    headGene.color = Color.Lerp(a.headGene.color, b.headGene.color, Random.Range(0f, 1f));
                    Debug.Log($"[Crossover] Swapped head gene to “{headGene.id}”");
                    break;
                case 2: // legs
                    legGene = b.legGene;
                    legGene.scale = Mathf.Lerp(a.legGene.scale, b.legGene.scale, Random.Range(0f, 1f));
                    legGene.color = Color.Lerp(a.legGene.color, b.legGene.color, Random.Range(0f, 1f));
                    Debug.Log($"[Crossover] Swapped leg gene to “{legGene.id}”");
                    break;
            }
        }

        return new Genome(bodyGene, headGene, legGene);
    }

public void Mutate(Genome genome, float mutationRate, float difficultyScaleTotal)
{
    bool mutatedPart = false;

    var population = EvolutionManager.Instance.GetPopulation();
    Debug.Log("[Mutate] Population variant count: " + population.Count);

    if (population.Count > 0)
    {
        var allIndividuals = new List<EnemyIndividual>();
        foreach (var list in population.Values)
            allIndividuals.AddRange(list);
        Debug.Log("[Mutate] All individuals count: " + allIndividuals.Count);

        if (allIndividuals.Count > 0)
        {
            if (Random.value < mutationRate)
            {
                BodyGeneSO newGene = RandomChoice(allIndividuals).genome.bodyGene;
                Color newColor = Color.Lerp(genome.bodyGene.color, newGene.color, Random.Range(0f, 1f));
                newGene.color = newColor;
                newGene.scale = genome.bodyGene.scale;
                Debug.Log($"[Mutate] Mutated body gene {genome.bodyGene.id} to: {newGene.id}");

                genome.bodyGene = newGene;
                mutatedPart = true;
            }
            if (Random.value < mutationRate && !mutatedPart)
            {
                HeadGeneSO newGene = RandomChoice(allIndividuals).genome.headGene;
                Color newColor = Color.Lerp(genome.headGene.color, newGene.color, Random.Range(0f, 1f));
                newGene.color = newColor;
                newGene.scale = genome.headGene.scale;
                Debug.Log($"[Mutate] Mutated head gene {genome.headGene.id} to: {newGene.id}");

                genome.headGene = newGene;
                mutatedPart = true;
            }
            if (Random.value < mutationRate && !mutatedPart)
            {
                LegGeneSO newGene = RandomChoice(allIndividuals).genome.legGene;
                Color newColor = Color.Lerp(genome.legGene.color, newGene.color, Random.Range(0f, 1f));
                newGene.color = newColor;
                newGene.scale = genome.legGene.scale;
                Debug.Log($"[Mutate] Mutated leg gene {genome.legGene.id} to: {newGene.id}");

                genome.legGene = newGene;
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
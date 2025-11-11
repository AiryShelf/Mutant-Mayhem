using UnityEngine;
using System.Collections.Generic;

public class DefaultGeneticOps
{
    #region Crossover ---------------------------------------------------------
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
        LegGeneSO legGene = a.legGene;

        // Decide how many parts to swap
        int partsToSwap = Random.Range(1, a.numberOfGenes);
        var partIndices = new HashSet<int>();
        while (partIndices.Count < partsToSwap)
            partIndices.Add(Random.Range(0, a.numberOfGenes));

        foreach (int index in partIndices)
        {
            switch (index)
            {
                case 0: // Body
                    bodyGene = b.bodyGene;
                    bodyGene.scale = Mathf.Lerp(a.bodyGene.scale, b.bodyGene.scale, Random.Range(0f, 1f));
                    bodyGene.color = Color.Lerp(a.bodyGene.color, b.bodyGene.color, Random.Range(0f, 1f));
                    Debug.Log($"[Crossover] Swapped body gene to “{bodyGene.id}”");
                    break;
                case 1: // Head
                    headGene = b.headGene;
                    headGene.scale = Mathf.Lerp(a.headGene.scale, b.headGene.scale, Random.Range(0f, 1f));
                    headGene.color = Color.Lerp(a.headGene.color, b.headGene.color, Random.Range(0f, 1f));
                    Debug.Log($"[Crossover] Swapped head gene to “{headGene.id}”");
                    break;
                case 2: // Legs
                    legGene = b.legGene;
                    legGene.scale = Mathf.Lerp(a.legGene.scale, b.legGene.scale, Random.Range(0f, 1f));
                    legGene.color = Color.Lerp(a.legGene.color, b.legGene.color, Random.Range(0f, 1f));
                    Debug.Log($"[Crossover] Swapped leg gene to “{legGene.id}”");
                    break;
            }
        }

        return new Genome(bodyGene, headGene, legGene);
    }

    #endregion
    
    #region Mutate ----------------------------------------------------------

    public void Mutate(Genome genome, float difficultyScaleTotal)
    {
        float mutationChance = PlanetManager.Instance.currentPlanet.mutationChance
                             + (WaveControllerRandom.Instance.currentWaveIndex
                             * PlanetManager.Instance.currentPlanet.addMutationChancePerWave);
        mutationChance = Mathf.Clamp(mutationChance, 0f, PlanetManager.Instance.currentPlanet.mutationChanceMax);
        Debug.Log("[Mutate] Mutation chance: " + mutationChance);

        var population = EvolutionManager.Instance.GetPopulation();
        Debug.Log("[Mutate] Population variant count: " + population.Count);

        if (population.Count > 0)
        {
            var allIndividuals = new List<GeneticIndividual>();
            foreach (var list in population.Values)
                allIndividuals.AddRange(list);
            Debug.Log("[Mutate] All individuals count: " + allIndividuals.Count);

            if (allIndividuals.Count > 0)
            {
                // Mutate only one part per mutation call
                bool partWasMutated = false;
                genome.bodyGene = MutateGene(genome, g => g.bodyGene, allIndividuals, 
                                             ref partWasMutated, mutationChance, "body");
                genome.headGene = MutateGene(genome, g => g.headGene, allIndividuals, 
                                             ref partWasMutated, mutationChance, "head");
                genome.legGene = MutateGene(genome, g => g.legGene, allIndividuals, 
                                             ref partWasMutated, mutationChance, "leg");
            }
        }

        // 🔸 Mutate scales
        float delta = PlanetManager.Instance.currentPlanet.mutationIntensity
                    + (WaveControllerRandom.Instance.currentWaveIndex
                    * PlanetManager.Instance.currentPlanet.addMutationIntensityPerWave);
        float deltaDown = -delta / 2;
        Debug.Log("[GeneticOps] Mutate scale between: delta up " + delta + ", delta down " + deltaDown);

        if (Random.value < mutationChance)
        {
            genome.bodyGene.scale += Random.Range(deltaDown, delta);
            genome.RandomizePartColor(genome.bodyGene, genome.bodyGene.color, delta);
            Debug.Log("Mutated bodyScale: " + genome.bodyGene.scale);
        }
        if (Random.value < mutationChance)
        {
            genome.headGene.scale += Random.Range(deltaDown, delta);
            genome.RandomizePartColor(genome.headGene, genome.headGene.color, delta);
            Debug.Log("Mutated headScale: " + genome.headGene.scale);
        }
        if (Random.value < mutationChance)
        {
            genome.legGene.scale += Random.Range(deltaDown, delta);
            genome.RandomizePartColor(genome.legGene, genome.legGene.color, delta);
            Debug.Log("Mutated legScale: " + genome.legGene.scale);
        }

        ClampAndNormalize(ref genome, difficultyScaleTotal);
    }

    #endregion

    #region  Helpers ---------------------------------------------------------

    public void ClampAndNormalize(ref Genome genome, float maxTotal)
    {
        // Keep parts from getting too small
        float minPerGene = (maxTotal / genome.numberOfGenes) * 0.6f;
        genome.bodyGene.scale = Mathf.Max(genome.bodyGene.scale, minPerGene);
        genome.headGene.scale = Mathf.Max(genome.headGene.scale, minPerGene);
        genome.legGene.scale = Mathf.Max(genome.legGene.scale, minPerGene);

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

    private T MutateGene<T>(Genome genome, System.Func<Genome, T> geneSelector, List<GeneticIndividual> allIndividuals, ref bool mutatedPart, float mutationChance, string partName) where T : GeneSOBase
    {
        if (Random.value < mutationChance && !mutatedPart)
        {
            T currentGene = geneSelector(genome);
            T newGene = geneSelector(RandomChoice(allIndividuals).genome);
            Color newColor = Color.Lerp(currentGene.color, newGene.color, Random.Range(0f, 1f));
            newGene.color = newColor;
            newGene.scale = currentGene.scale;
            Debug.Log($"[Mutate] Mutated {partName} gene {currentGene.id} to: {newGene.id}");

            mutatedPart = true;
            return newGene;
        }
        return geneSelector(genome);
    }
    
    #endregion
}
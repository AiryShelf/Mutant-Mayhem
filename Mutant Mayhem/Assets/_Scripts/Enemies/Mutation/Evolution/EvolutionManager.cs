using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Spawns a population of modular enemies, evaluates their fitness at wave‑end,
/// then breeds the next generation using crossover + mutation.
/// </summary>
public class EvolutionManager : MonoBehaviour
{
    // ───────────────────────────────────────────────── Inspector ──────────────────────────────────────────
    [Header("Setup")]
    [SerializeField] GameObject enemyPrefab;  // the MutantShell prefab

    [Tooltip("Difficulty adds to the allowed total scale of parts for each generation.")]
    public float difficultyScaleTotal = 6;  // Increases with difficulty and is the total sum of body+head+leg scales allowed
    public float difficultyScalePerWave = 0.2f;
    public int minLifetimesToEvolve = 10;
    public int minPopSizeToEvolve = 5;  // Minimum population population size for each variant

    // ───────────────────────────────────────────────── Internals ─────────────────────────────────────────
    readonly Dictionary<PopulationVariantType, List<GeneticIndividual>> _population = new();
    public Dictionary<PopulationVariantType, List<GeneticIndividual>> GetPopulation()
    {
        return _population;
    }
    DefaultGeneticOps _ops;

    [Header("Runtime State")]
    public int _currentSubwaveCount = 0;
    public int populationCount = 0;
    int _previousMaxIndex = 0;
    bool _genZeroBuilt = false;
    WaveSpawnerRandom _waveSpawner;
    int _spawnIndex = 0;
    int _lastSpawnCycleIndex = 0;

    public static EvolutionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;

        _ops = new DefaultGeneticOps();
        
    }

    void Start()
    {
        if (!WaveControllerRandom.Instance)
        {
            Debug.LogWarning("EvolutionManager: WaveControllerRandom instance not found!");
            return;
        }
        _waveSpawner = WaveControllerRandom.Instance.waveSpawner;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void BuildGenerationZero()
    {
        Debug.Log("EvolutionManager: Building generation 0 for all variants.");
        // build generation 0 for each variant
        foreach (PopulationVariantType v in System.Enum.GetValues(typeof(PopulationVariantType)))
        {
            var startingPop = GetStartingPopulation(v);
            if (startingPop.Count > 0)
            {
                _population[v] = startingPop;
            }
            else
            {
                _population[v] = new List<GeneticIndividual>();
            }
        }
        _genZeroBuilt = true;

        populationCount = _population.Sum(kvp => kvp.Value.Count);
    }

    #region Public API -------------------------------------------------------------

    /// <summary>Call at the beginning of a wave.</summary>
    public void SpawnWaveFull()
    {
        if (!_genZeroBuilt)
        {
            BuildGenerationZero();
        }

        foreach (var kvp in _population)
        {
            var individuals = kvp.Value;

            foreach (var ind in individuals)
            {
                var enemy = PoolManager.Instance.GetFromPool("Mutant");
                var pos = GetRandomSpawnPosition();
                enemy.transform.position = pos;
                enemy.transform.rotation = Quaternion.identity;
                EnemyCounter.EnemyCount++;

                var enemyMutant = enemy.GetComponent<EnemyMutant>();
                if (enemyMutant == null)
                {
                    Debug.LogError("EvolutionManager: EnemyMutant prefab must have an EnemyMutant component!");
                    continue;
                }

                enemyMutant.InitializeMutant(ind);
            }
        }
    }

    public void SpawnWaveCycle()
    {
        if (populationCount == 0)
        {
            Debug.LogWarning("EvolutionManager: Population is empty, cannot spawn wave.");
            return;
        }
        
        var kvp = _population.ElementAt(_currentSubwaveCount % _population.Count);
        var individuals = kvp.Value;

        int totalToSpawn = Mathf.CeilToInt(individuals.Count * PlanetManager.Instance.currentPlanet.totalSpawnFactor);

        for (int i = 0; i < totalToSpawn; i++)
        {
            int index = (_lastSpawnCycleIndex + i + 1) % individuals.Count;

            var individual = individuals[index];
            var enemy = PoolManager.Instance.GetFromPool("Mutant");
            var pos = _waveSpawner.GetPointOnSquareBoundary(0, Mathf.PI, true);
            enemy.transform.position = pos;
            enemy.transform.rotation = Quaternion.identity;
            EnemyCounter.EnemyCount++;

            var enemyMutant = enemy.GetComponent<EnemyMutant>();
            if (enemyMutant == null)
            {
                Debug.LogError("EvolutionManager: EnemyMutant prefab must have an EnemyMutant component!");
                continue;
            }

            enemyMutant.InitializeMutant(individual);
            Debug.Log($"EvolutionManager: Spawned individual {index} of variant {individual.variant}." +
            $"Lifetime count = {individual.lifetimeCount}, Fitness = {individual.fitnessCount}" +
            $"\nGenome: Body {individual.genome.bodyGene.id}, Head {individual.genome.headGene.id}, Legs {individual.genome.legGene.id}" +
            $"\nScale: Body {individual.genome.bodyGene.scale}, Head {individual.genome.headGene.scale}, Legs {individual.genome.legGene.scale}");
        }

        _lastSpawnCycleIndex = (_lastSpawnCycleIndex + totalToSpawn) % individuals.Count;
    }

    public void CrossoverAndMutate()
    {
        if (!_genZeroBuilt)
        {
            BuildGenerationZero();
        }

        Debug.Log($"EvolutionManager: Ending subwave {_currentSubwaveCount} and attempting to evolve population of {populationCount}.");

        IncrementDifficultyScale();
        AddNewGenesForWave();

        foreach (var variant in _population.Keys.ToArray())
        {
            Crossover(variant);

            // Apply mutation to the variant's population
            foreach (var ind in _population[variant])
            {
                _ops.Mutate(ind.genome, difficultyScaleTotal);
            }
        }
    }

    public void MutateAndSpawn()
    {
        if (!_genZeroBuilt)
        {
            BuildGenerationZero();
        }

        _currentSubwaveCount++;
        var kvp = _population.ElementAt(_currentSubwaveCount % _population.Count);
        var variant = kvp.Key;

        // Apply mutation to the variant's population
        foreach (var ind in _population[variant])
        {
            _ops.Mutate(ind.genome, difficultyScaleTotal);
        }

        SpawnWaveCycle();
    }
    
    #endregion

    #region Helpers ---------------------------------------------------------------

    List<GeneticIndividual> GetStartingPopulation(PopulationVariantType variantType)
    {
        int maxIndex = _waveSpawner.maxIndex;
        Debug.Log("EvolutionManager: Adding starting population of variant " + variantType + " at maxIndex " + maxIndex);

        var startingPopulation = new List<GeneticIndividual>();

        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        if (currentPlanet == null)
        {
            Debug.LogError("Current planet is null! Cannot get starting population.");
            return startingPopulation;
        }
        
        for (int i = 0; i <= maxIndex; i++)
        {
            foreach (var g in currentPlanet.waveSOBase.subWaves[i].genomeList)
            {
                Debug.Log($"EvolutionManager: Adding genome to starting population of variant {variantType}: {g}, from index {i}");
                var genome = g.ToGenome();

                _ops.ClampAndNormalize(ref genome, difficultyScaleTotal);
                startingPopulation.Add(new GeneticIndividual(genome, variantType));
            }
        }

        _previousMaxIndex = maxIndex;

        Debug.Log("EvolutionManager: Starting population build complete for variant " + variantType + ", with " + startingPopulation.Count + " individuals.");
        return startingPopulation;
    }

    void Crossover(PopulationVariantType variant)
    {
        // Only Crossover matured individuals
        var individuals = _population[variant];
        var matureIndividuals = individuals.Where(ind => ind.lifetimeCount > minLifetimesToEvolve).ToList();

        if (matureIndividuals.Count < minPopSizeToEvolve)
        {
            //Debug.Log($"EvolutionManager: Not enough mature individuals have lived {minLifetimesToEvolve} lifetimes " +
                //$"(Individual Count: {matureIndividuals.Count}, requires {minPopSizeToEvolve}) for population variant {variant} to evolve, skipping crossover.");
            return;
        }

        matureIndividuals.Sort((a, b) => b.AverageFitness.CompareTo(a.AverageFitness));

        int numParents = Mathf.RoundToInt(matureIndividuals.Count * 0.7f);
        int numChildren = Mathf.RoundToInt(matureIndividuals.Count * 0.3f);
        // Ensure same count
        if (numParents + numChildren > matureIndividuals.Count)
            numChildren = matureIndividuals.Count - numParents;

        var nextGen = new List<GeneticIndividual>();

        // Keep top N parents (clone bare)
        for (int i = 0; i < numParents && i < matureIndividuals.Count; i++)
            nextGen.Add(matureIndividuals[i]);

        // Generate children from top parents
        for (int i = 0; i < numChildren; i++)
        {
            var parentA = matureIndividuals[Random.Range(0, numParents)];
            var parentB = matureIndividuals[Random.Range(0, numParents)];
            var childGenome = _ops.Crossover(parentA.genome, parentB.genome);
            nextGen.Add(new GeneticIndividual(childGenome, variant));
        }

        // Add back young individuals that did not participate in evolution
        var youngIndividuals = individuals.Where(ind => ind.lifetimeCount <= minLifetimesToEvolve).ToList();
        nextGen.AddRange(youngIndividuals);

        _population[variant] = nextGen;

        Debug.Log("EvolutionManager: Crossover complete for variant " + variant +
                  ". Variant population size after crossover: " + _population[variant].Count +
                  $", Parents: {numParents}, New Children: {numChildren}, Old Children (Not part of crossover): {youngIndividuals.Count}");
    }

    void AddNewGenesForWave()
    {
        int currentMaxIndex = _waveSpawner.maxIndex;
        if (currentMaxIndex <= _previousMaxIndex) return;

        Debug.Log($"EvolutionManager: Adding new genes for wave {_currentSubwaveCount} with previousIndex {_previousMaxIndex} and maxIndex {currentMaxIndex}.");

        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        if (currentPlanet == null) return;

        int popIncreasePerVariant = currentPlanet.waveSOBase.subWaves[currentMaxIndex].popIncreasePerVariant;
        for (int i = _previousMaxIndex + 1; i <= currentMaxIndex; i++)
        {
            // Add new genomes from the current wave
            foreach (var g in currentPlanet.waveSOBase.subWaves[i].genomeList)
            {
                Debug.Log($"EvolutionManager: Adding new genome from subwave index {i}: {g} {g.bodyGeneSO.id} {g.headGeneSO.id} {g.legGeneSO.id}");
                var genome = g.ToGenome();

                // For each variant, add new individuals with gene scales randomized within population bounds
                foreach (var variant in _population.Keys.ToArray())
                {
                    // Get scale bounds from existing population
                    float minBody = float.MaxValue, maxBody = float.MinValue;
                    float minHead = float.MaxValue, maxHead = float.MinValue;
                    float minLeg = float.MaxValue, maxLeg = float.MinValue;

                    foreach (var individual in _population[variant])
                    {
                        minBody = Mathf.Min(minBody, individual.genome.bodyGene.scale);
                        maxBody = Mathf.Max(maxBody, individual.genome.bodyGene.scale);
                        minHead = Mathf.Min(minHead, individual.genome.headGene.scale);
                        maxHead = Mathf.Max(maxHead, individual.genome.headGene.scale);
                        minLeg = Mathf.Min(minLeg, individual.genome.legGene.scale);
                        maxLeg = Mathf.Max(maxLeg, individual.genome.legGene.scale);
                    }

                    for (int j = 0; j < popIncreasePerVariant; j++)
                    {
                        // Randomize gene scales within current population bounds for this variant
                        genome.bodyGene.scale = Random.Range(minBody, maxBody);
                        genome.headGene.scale = Random.Range(minHead, maxHead);
                        genome.legGene.scale = Random.Range(minLeg, maxLeg);

                        _ops.ClampAndNormalize(ref genome, difficultyScaleTotal);
                        _population[variant].Add(new GeneticIndividual(genome, variant));
                    }
                }
            }
        }
        Debug.Log($"EvolutionManager: Population increased by {popIncreasePerVariant * _population.Count} for wave _{_currentSubwaveCount}");
        _previousMaxIndex = currentMaxIndex;
        populationCount = _population.Sum(kvp => kvp.Value.Count);
    }

    Vector2 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float distance = Random.Range(20f, 30f); // between min and max radius
        return dir * distance;
    }

    public void IncrementDifficultyScale()
    {
        difficultyScaleTotal += difficultyScalePerWave;
        Debug.Log($"Difficulty scale increased to {difficultyScaleTotal}");
    }
    #endregion
}
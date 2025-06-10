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
    [SerializeField] GameObject enemyPrefab;  // the EnemyShell prefab
    [SerializeField] float mutationRate = 0.2f;

    [Tooltip("Difficulty adds to the allowed total scale of parts for each generation.")]
    public float difficultyScaleTotal = 6;  // Increases with difficulty
    public float difficultyScalePerWave = 0.2f;
    public int minLifetimesToEvolve = 1;

    // ───────────────────────────────────────────────── Internals ─────────────────────────────────────────
    readonly Dictionary<MutantVariant, List<MutantIndividual>> _population = new();
    public Dictionary<MutantVariant, List<MutantIndividual>> GetPopulation()
    {
        return _population;
    }
    DefaultGeneticOps _ops;

    [Header("Runtime State")]
    public int _currentWave = 0;
    public int populationCount = 0;
    int _previousMaxIndex = 0;
    bool _genZeroBuilt = false;
    WaveSpawnerRandom _waveSpawner;

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
        foreach (MutantVariant v in System.Enum.GetValues(typeof(MutantVariant)))
        {
            var startingPop = GetStartingPopulation(v);
            if (startingPop.Count > 0)
            {
                _population[v] = startingPop;
            }
            else
            {
                _population[v] = CreateRandomPopulation(v);
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

    void SpawnWaveCycle()
    {
        var kvp = _population.ElementAt(_currentWave % _population.Count);
        var individuals = kvp.Value;

        foreach (var individual in individuals)
        {
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
        }
    }

    public void EvolveAndSpawn()
    {
        if (!_genZeroBuilt)
        {
            BuildGenerationZero();
        }

        Debug.Log("EvolutionManager: Ending wave " + _currentWave + " and evolving population.");
        _currentWave++;
        IncrementDifficultyScale();

        var kvp = _population.ElementAt(_currentWave % _population.Count);
        var variant = kvp.Key;

        // Only Crossover matured individuals
        var individuals = _population[variant];
        var matureIndividuals = individuals.Where(ind => ind.lifetimes > minLifetimesToEvolve).ToList();
        matureIndividuals.Sort((a, b) => b.AverageFitness.CompareTo(a.AverageFitness));

        int numParents = Mathf.CeilToInt(matureIndividuals.Count * 0.8f);
        int numChildren = Mathf.FloorToInt(matureIndividuals.Count * 0.2f);
        var nextGen = new List<MutantIndividual>();

        // Keep top N parents (clone bare)
        for (int i = 0; i < numParents && i < matureIndividuals.Count; i++)
            nextGen.Add(matureIndividuals[i]);

        // Generate children from top parents
        for (int i = 0; i < numChildren; i++)
        {
            var parentA = matureIndividuals[Random.Range(0, numParents)];
            var parentB = matureIndividuals[Random.Range(0, numParents)];
            var childGenome = _ops.Crossover(parentA.genome, parentB.genome);
            nextGen.Add(new MutantIndividual(childGenome, variant));
        }

        // Add back young individuals that did not participate in evolution
        var youngIndividuals = individuals.Where(ind => ind.lifetimes <= minLifetimesToEvolve).ToList();
        nextGen.AddRange(youngIndividuals);

        _population[variant] = nextGen;

        // Apply mutation to the entire population
        foreach (var ind in _population[variant])
        {
            _ops.Mutate(ind.genome, mutationRate, difficultyScaleTotal);
        }

        AddNewUnlocksForWave();

        SpawnWaveCycle();
    }
    
    #endregion

    #region Helpers ---------------------------------------------------------------

    List<MutantIndividual> GetStartingPopulation(MutantVariant v)
    {
        int maxIndex = _waveSpawner.maxIndex;
        Debug.Log("EvolutionManager: Adding starting population for variant " + v + " at maxIndex " + maxIndex);

        var list = new List<MutantIndividual>();

        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        if (currentPlanet == null)
        {
            Debug.LogError("Current planet is null! Cannot get starting population.");
            return list;
        }
        
        for (int i = 0; i <= maxIndex; i++)
        {
            foreach (var g in currentPlanet.waveSOBase.subWaves[i].genomeList)
            {
                Debug.Log($"EvolutionManager: Adding genome to starting population for variant {v}: {g}, from index {i}");
                var genome = g.ToGenome();

                _ops.ClampAndNormalize(ref genome, difficultyScaleTotal);
                list.Add(new MutantIndividual(genome, v));
            }
        }

        _previousMaxIndex = maxIndex;

        Debug.Log("EvolutionManager: Starting population for variant " + v + " has " + list.Count + " individuals.");
        return list;
    }
    void AddNewUnlocksForWave()
    {
        int currentMaxIndex = _waveSpawner.maxIndex;
        if (currentMaxIndex <= _previousMaxIndex) return;

        Debug.Log($"EvolutionManager: Adding new unlocks for wave {_currentWave} with previousIndex {_previousMaxIndex} and maxIndex {currentMaxIndex}.");

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

                _ops.ClampAndNormalize(ref genome, difficultyScaleTotal);

                foreach (var variant in _population.Keys.ToArray())
                {
                    for (int j = 0; j < popIncreasePerVariant; j++)
                    {
                        _population[variant].Add(new MutantIndividual(genome, variant));
                    }
                }
            }
        }
        Debug.Log($"EvolutionManager: Population increased by {popIncreasePerVariant * _population.Count} for wave _{_currentWave}");
        _previousMaxIndex = currentMaxIndex;
        populationCount = _population.Sum(kvp => kvp.Value.Count);
    }

    List<MutantIndividual> CreateRandomPopulation(MutantVariant v)
    {
        Debug.Log("EvolutionManager: Creating random population for variant " + v);

        var list = new List<MutantIndividual>();
        for (int i = 0; i < 10; i++)
        {
            var g = new Genome(
                GetRandom<BodyGeneSO>(),
                GetRandom<HeadGeneSO>(),
                GetRandom<LegGeneSO>()
            );
            g.bodyGene.scale = Random.Range(5f, 10f);
            g.headGene.scale = Random.Range(5f, 10f);
            g.legGene.scale = Random.Range(5f, 10f);

            _ops.ClampAndNormalize(ref g, difficultyScaleTotal);

            list.Add(new MutantIndividual(g, v));
        }
        return list;
    }

    Vector2 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float distance = Random.Range(20f, 30f); // between min and max radius
        return dir * distance;
    }

    T GetRandom<T>() where T : UnityEngine.Object
    {
        var all = Resources.FindObjectsOfTypeAll(typeof(T));
        if (all.Length == 0)
        {
            Debug.LogError($"GetRandom<{typeof(T).Name}> found no assets! Are your gene assets in a Resources folder?");
            return null;
        }

        return (T)all[Random.Range(0, all.Length)];
    }

    public void IncrementDifficultyScale()
    {
        difficultyScaleTotal += difficultyScalePerWave;
        Debug.Log($"Difficulty scale increased to {difficultyScaleTotal}");
    }
    #endregion
}
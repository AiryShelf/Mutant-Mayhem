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
    [SerializeField] int populationPerVariant = 10;
    [SerializeField] float mutationRate = 0.08f;

    [Tooltip("Difficulty adds to the allowed total scale each generation.")]
    public float difficultyScaleTotal = 6;  // Increases with difficulty
    public float difficultyScalePerWave = 0.2f;

    // ───────────────────────────────────────────────── Internals ─────────────────────────────────────────
    readonly Dictionary<MutantVariant, List<MutantIndividual>> _population = new();
    public Dictionary<MutantVariant, List<MutantIndividual>> GetPopulation()
    {
        return _population;
    }
    readonly Dictionary<MutantVariant, List<MutantIndividual>> _liveThisWave = new();

    DefaultGeneticOps _ops;
    public int _currentWave = 0;
    int _previousMaxIndex = 0;
    bool _genZeroBuilt = false;

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
            Debug.LogError("EvolutionManager: WaveControllerRandom instance not found!");
            return;
        }
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
                populationPerVariant = startingPop.Count;
            }
            else
            {
                _population[v] = CreateRandomPopulation(v);
            }
        }
        _genZeroBuilt = true;
    }

    #region Public API -------------------------------------------------------------

    /// <summary>Call at the beginning of a wave.</summary>
    public void SpawnWave()
    {
        if (!_genZeroBuilt)
        {
            BuildGenerationZero();
        }

        foreach (var kvp in _population)
        {
            var individuals = kvp.Value;
            _liveThisWave[kvp.Key] = new List<MutantIndividual>(individuals.Count);

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

                _liveThisWave[kvp.Key].Add(ind);
            }
        }
    }

    /// <summary>Call once when the wave ends (all enemies dead or player triggered).</summary>
    public void EndWaveAndEvolve()
    {
        Debug.Log("EvolutionManager: Ending wave " + _currentWave + " and evolving population.");
        _currentWave++;
        IncrementDifficultyScale();

        // 1) Evaluate done → Breed next generation, retain top parents
        foreach (var variant in _population.Keys.ToArray())
        {
            var individuals = _population[variant];
            individuals.Sort((a, b) => b.fitness.CompareTo(a.fitness));

            int numParents = Mathf.CeilToInt(populationPerVariant / 2f);
            int numChildren = Mathf.FloorToInt(populationPerVariant / 2f);
            int eliteCount = Mathf.Max(1, numParents);

            var nextGen = new List<MutantIndividual>();

            // Keep top N parents (clone bare)
            for (int i = 0; i < numParents && i < individuals.Count; i++)
                nextGen.Add(individuals[i].CloneBare());

            // Generate children to fill up to populationPerVariant
            for (int i = 0; i < numChildren; i++)
            {
                var parentA = individuals[Random.Range(0, eliteCount)];
                var parentB = individuals[Random.Range(0, eliteCount)];
                var childGenome = _ops.Crossover(parentA.genome, parentB.genome);
                _ops.Mutate(childGenome, mutationRate, difficultyScaleTotal);
                nextGen.Add(new MutantIndividual(childGenome, variant));
            }

            _population[variant] = nextGen;
        }

        // Add new unlocks for this wave
        AddNewUnlocksForWave();

        // 2) Cleanup old live references
        _liveThisWave.Clear();
    }
    #endregion

    #region Helpers ---------------------------------------------------------------

    List<MutantIndividual> GetStartingPopulation(MutantVariant v)
    {
        int maxIndex = WaveControllerRandom.Instance.waveSpawner.maxIndex;
        Debug.Log("EvolutionManager: Getting starting population for variant " + v + " at maxIndex " + maxIndex);

        var list = new List<MutantIndividual>(populationPerVariant);

        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        if (currentPlanet == null)
        {
            Debug.LogError("Current planet is null! Cannot get starting population.");
            return list;
        }

        
        for (int i = 0; i < maxIndex; i++)
        {
            foreach (var g in currentPlanet.waveSOBase.subWaves[i].genomeList)
            {
                Debug.Log("EvolutionManager: Adding genome to starting population for variant " + v + ": " + g);
                var genome = g.ToGenome();

                _ops.ClampAndNormalize(ref genome, difficultyScaleTotal);
                list.Add(new MutantIndividual(genome, v));
            }
        }

        Debug.Log("EvolutionManager: Starting population for variant " + v + " has " + list.Count + " individuals.");
        return list;
    }
    void AddNewUnlocksForWave()
    {
        int currentMaxIndex = WaveControllerRandom.Instance.waveSpawner.maxIndex;
        if (currentMaxIndex <= _previousMaxIndex) return;

        Debug.Log($"EvolutionManager: Adding new unlocks for wave {_currentWave} with previousIndex {_previousMaxIndex} and maxIndex {currentMaxIndex}.");

        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        if (currentPlanet == null) return;

        int popIncreasePerVariant = currentPlanet.waveSOBase.subWaves[currentMaxIndex].popIncreasePerVariant;
        for (int i = _previousMaxIndex; i <= currentMaxIndex; i++)
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
                populationPerVariant += popIncreasePerVariant;
            }
        }
        Debug.Log($"EvolutionManager: Population increased by {popIncreasePerVariant * _population.Count} for wave _{_currentWave}");
        _previousMaxIndex = currentMaxIndex;
    }

    List<MutantIndividual> CreateRandomPopulation(MutantVariant v)
    {
        Debug.Log("EvolutionManager: Creating random population for variant " + v);

        var list = new List<MutantIndividual>(populationPerVariant);
        for (int i = 0; i < populationPerVariant; i++)
        {
            var g = new Genome(
                GetRandom<BodyGeneSO>(),
                GetRandom<HeadGeneSO>(),
                GetRandom<LegGeneSO>()
            );
            g.bodyGene.scale = Random.Range(2f, 4f);
            g.headGene.scale = Random.Range(2f, 4f);
            g.legGene.scale = Random.Range(2f, 4f);

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
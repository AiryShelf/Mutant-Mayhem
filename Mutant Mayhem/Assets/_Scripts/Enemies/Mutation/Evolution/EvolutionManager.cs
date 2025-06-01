using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a population of modular enemies, evaluates their fitness at wave‑end,
/// then breeds the next generation using crossover + mutation.
/// </summary>
public class EvolutionManager : MonoBehaviour
{
    // ───────────────────────────────────────────────── Inspector ──────────────────────────────────────────
    [Header("Setup")]
    [SerializeField] EnemyRenderer enemyPrefab;  // the EnemyShell prefab
    [SerializeField] int populationPerVariant = 10;
    [SerializeField] float mutationRate = 0.08f;

    [Tooltip("Difficulty bonus adds to the allowed total scale each generation.")]
    [SerializeField] float difficultyScalePerWave = 0.2f;

    // ───────────────────────────────────────────────── Internals ─────────────────────────────────────────
    readonly Dictionary<EnemyVariant, List<EnemyIndividual>> _population = new();
    readonly Dictionary<EnemyVariant, List<EnemyIndividual>> _liveThisWave = new();

    DefaultGeneticOps _ops;
    int _currentWave = 0;

    public static EvolutionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _ops = new DefaultGeneticOps();

        // build generation 0 for each variant
        foreach (EnemyVariant v in System.Enum.GetValues(typeof(EnemyVariant)))
            _population[v] = CreateRandomPopulation(v);
    }

    #region Public API -------------------------------------------------------------

    /// <summary>Call at the beginning of a wave.</summary>
    public void SpawnWave()
    {
        _currentWave++;
        foreach (var kvp in _population)
        {
            var list = kvp.Value;
            _liveThisWave[kvp.Key] = new List<EnemyIndividual>(list.Count);

            foreach (var ind in list)
            {
                var pos = GetRandomSpawnPosition();
                var enemy  = Instantiate(enemyPrefab, pos, Quaternion.identity);
                EnemyCounter.EnemyCount++;
                var enemyMutant = enemy.GetComponent<MutatedEnemy>();
                if (enemyMutant == null)
                {
                    Debug.LogError("EnemyRenderer prefab must have an EnemyBase component!");
                    continue;
                }

                enemy.ApplyGenome(ind.genome);

                ind.runtimeRenderer = enemy;
                ind.fitness = 0f;

                enemyMutant.AssignIndividual(ind); // assign reference for direct access
                _liveThisWave[kvp.Key].Add(ind);
            }
        }
    }

    /// <summary>Call directly from EnemyBase when the enemy dies.</summary>
    public void AddFitness(EnemyIndividual individual, float delta)
    {
        if (individual == null) return;
        individual.AddFitness(delta);
    }

    /// <summary>Call once when the wave ends (all enemies dead or player triggered).</summary>
    public void EndWaveAndEvolve()
    {
        // 1) Evaluate done → Breed next generation
        foreach (var kvp in _population)
        {
            var list = kvp.Value;

            // Sort by fitness descending
            list.Sort((a, b) => b.fitness.CompareTo(a.fitness));

            int eliteCount = Mathf.Max(1, list.Count / 4);
            var nextGen = new List<EnemyIndividual>();

            // Keep elites
            for (int i = 0; i < eliteCount; i++)
                nextGen.Add(list[i].CloneBare());              // copy genome only

            // Fill rest
            while (nextGen.Count < list.Count)
            {
                var parentA = list[Random.Range(0, eliteCount)];
                var parentB = list[Random.Range(0, eliteCount)];

                var childGenome = _ops.Crossover(parentA.genome, parentB.genome);
                _ops.Mutate(childGenome, mutationRate, _currentWave * difficultyScalePerWave);

                nextGen.Add(new EnemyIndividual(childGenome, kvp.Key));
            }

            _population[kvp.Key] = nextGen;
        }

        // 2) Cleanup old live references
        _liveThisWave.Clear();
    }
    #endregion

    #region Helpers ---------------------------------------------------------------
    List<EnemyIndividual> CreateRandomPopulation(EnemyVariant v)
    {
        var list = new List<EnemyIndividual>(populationPerVariant);
        for (int i = 0; i < populationPerVariant; i++)
        {
            var g = new Genome(
                GetRandom<BodyGeneSO>().id,
                GetRandom<HeadGeneSO>().id,
                GetRandom<LegGeneSO>().id,
                GetRandom<LegGeneSO>().id,
                Random.Range(0.8f, 1.2f),
                Random.Range(0.8f, 1.2f),
                Random.Range(0.8f, 1.2f),
                Random.Range(0.8f, 1.2f)
            );

            list.Add(new EnemyIndividual(g, v));
        }
        return list;
    }

    Vector2 GetRandomSpawnPosition() =>
        Random.insideUnitCircle * 6f;   // quick stub – replace with your spawn system

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
    #endregion
}

// ────────────────────────────────────────────────── Helper classes ───────────────
public enum EnemyVariant { Runner, Chaser, Siege }

public class EnemyIndividual
{
    public Genome genome;
    public EnemyVariant variant;
    public float fitness;
    public EnemyRenderer runtimeRenderer;   // filled during wave

    public EnemyIndividual(Genome g, EnemyVariant v)
    {
        genome = g; variant = v;
    }

    public void AddFitness(float amount)
    {
        fitness += amount;
    }

    public EnemyIndividual CloneBare() => new EnemyIndividual(new Genome(
        genome.bodyId, genome.headId, genome.leftLegId, genome.rightLegId,
        genome.bodyScale, genome.headScale, genome.leftLegScale, genome.rightLegScale), variant);
}
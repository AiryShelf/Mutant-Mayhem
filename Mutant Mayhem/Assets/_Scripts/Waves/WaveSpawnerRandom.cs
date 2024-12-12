using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveSpawnerRandom : MonoBehaviour
{
    public WaveControllerRandom waveController;
    [SerializeField] TileManager tileManager;
    [SerializeField] Tilemap structureTilemap;
    [SerializeField] LayerMask checkClearLayers;
    public Transform qCubeTrans;
    public WaveSOBase masterWave;
    [SerializeField] int maxIndexToSelectAtStart;
    [SerializeField] WaveSOBase currentWave;
    [SerializeField] int numSubwavesAtStart;
    [SerializeField] int timeToNextSubWave;
    [SerializeField] int minTimeToNextSubWave;

    public int currentSubWaveIndex;
    public int currentConstantWaveIndex;
    public bool waveSpawning;
    public int numberOfSubwaves;
    public bool waveComplete;

    Vector2 centerPoint;
    int waveSeconds;
    Coroutine waveTimer;

    void Start()
    {
        centerPoint = qCubeTrans.position;

        // Copy the master wave
        currentWave = Instantiate(masterWave, transform);       
    }

    #region Build Wave

    public void StartWave()
    {       
        if (waveTimer != null)
        {
            StopCoroutine(waveTimer);
        }

        waveTimer = StartCoroutine(WaveTimer());
    }

    void BuildWave()
    {
        currentWave.subWaves.Clear();
        currentWave.subWaveMultipliers.Clear();
        currentWave.subWaveStyles.Clear();
        currentWave.timesToTriggerSubwaves.Clear();

        // Add more subwaves over time, apply difficulty
        numberOfSubwaves = Mathf.CeilToInt(numSubwavesAtStart + waveController.currentWaveIndex * 
                         SettingsManager.Instance.SubwaveListGrowthFactor);

        // Build Wave
        int prevSubwaveIndex = 0;
        int timeInSequence = 0;
        for (int i = 0; i < numberOfSubwaves; i++)
        {
            // Find max index to select based on current wave, plus starting max index
            int maxIndex = Mathf.FloorToInt(waveController.currentWaveIndex / waveController.wavesTillAddBase) 
                           + maxIndexToSelectAtStart - SettingsManager.Instance.WavesTillAddWaveBase;
            maxIndex = Mathf.Clamp(maxIndex, 0, masterWave.subWaves.Count - 1);
            Debug.Log("maxIndex for wave " + waveController.currentWaveIndex + ": " + maxIndex);

            // Select random index, wave, and style. Prevent doubles
            int subWaveIndex = Random.Range(0, maxIndex + 1);
            if (maxIndex > 0)
            {
                while (subWaveIndex == prevSubwaveIndex)
                {
                    subWaveIndex = Random.Range(0, maxIndex + 1);
                }
            }
            SubWaveSO waveToAdd = masterWave.subWaves[subWaveIndex];

            // Allow for extra waveSyles to select from
            int styleIndex = Random.Range(0, maxIndex + 1);
            styleIndex = Mathf.Clamp(styleIndex, 0, masterWave.subWaveStyles.Count - 1);
            SubWaveStyleSO styleToAdd = masterWave.subWaveStyles[styleIndex];

            // Add selection to list
            currentWave.subWaves.Add(waveToAdd);
            currentWave.subWaveMultipliers.Add(masterWave.subWaveMultipliers[subWaveIndex]);
            //Debug.Log("Added SubWave: " + waveToAdd.name);
            currentWave.subWaveStyles.Add(styleToAdd);
            //Debug.Log("Added SubWave Style: " + styleToAdd.name);
            currentWave.timesToTriggerSubwaves.Add(timeInSequence);

            // Find next time slot
            int delay = Mathf.CeilToInt(timeToNextSubWave);
            delay = Mathf.Clamp(delay, minTimeToNextSubWave, int.MaxValue);
            timeInSequence += delay;

            prevSubwaveIndex = subWaveIndex;
        }
        Debug.Log("Number of SubWaves for wave " + waveController.currentWaveIndex + 
                  ": " + currentWave.subWaves.Count);
    }

    IEnumerator WaveTimer()
    {
        Debug.Log("Start Wave " + waveController.currentWaveIndex);

        // Initialize
        currentSubWaveIndex = 0;
        currentConstantWaveIndex = 0;
        waveComplete = false;
        waveSpawning = true;
        waveSeconds = 0;

        BuildWave();

        List<int> _timesToTriggerSubwaves = ApplySubWaveTimeMultipliers();
        List<int> _timesToTriggerConstantWaves = ApplyConstantWaveTimeMultipliers();     

        // Find max length of time, add 5 seconds
        int maxTime = _timesToTriggerSubwaves.Max() + 5;
        Debug.Log("MaxTime for wave " + waveController.currentWaveIndex + ": " + maxTime);

        while (waveSpawning)
        {
            // Check if current wave seconds has a subWave to trigger
            if (_timesToTriggerSubwaves.Contains(waveSeconds))
            {
                currentSubWaveIndex = 
                    _timesToTriggerSubwaves.IndexOf(waveSeconds);
                StartCoroutine(SpawnSubWave(currentSubWaveIndex));
                Debug.Log($"Subwave {currentSubWaveIndex} started at {waveSeconds} seconds");
            }
            // If current wave seconds has constantWave to trigger
            if (_timesToTriggerConstantWaves.Contains(waveSeconds))
            {
                currentConstantWaveIndex = 
                    _timesToTriggerConstantWaves.IndexOf(waveSeconds);
                StartCoroutine(SpawnConstantWave(currentConstantWaveIndex));
                //Debug.Log("Start ConstantWave");
            }

            yield return new WaitForSeconds(1);
            waveSeconds++;
            
            if (waveSeconds > maxTime)
            {
                // End the checks for new waves
                waveSpawning = false;

                // Start checking for no enemies
                while (EnemyCounter.EnemyCount > 0)
                {
                    yield return new WaitForSeconds(1);
                    waveSeconds++;
                }

                // End the wave
                waveComplete = true;
                Debug.Log("Wave " + waveController.currentWaveIndex + " Complete");
            }
        }
    }

    #endregion

    #region Multipliers

    List<int> ApplySubWaveTimeMultipliers()
    {
        // Apply timing multipliers to Subwaves
        int prevTime = -1;
        List<int> _timesToTriggerSubWaves = new List<int>();
        foreach (int timeToTrigger in currentWave.timesToTriggerSubwaves)
        {
            // This gives each wave its own time-slot to spawn in, since doubles dont work.
            int time = (int)Mathf.Floor(timeToTrigger * waveController.subwaveDelayMult * 
                                        SettingsManager.Instance.SubwaveDelayMult);
            while (time <= prevTime)
                time++;
            prevTime = time;

            _timesToTriggerSubWaves.Add(time);
        }

        return _timesToTriggerSubWaves;
    }

    List<int> ApplyConstantWaveTimeMultipliers()
    {
        List<int> _timesToTriggerConstantWaves = new List<int>();
        int prevTime = -1;
        // Apply timing multipliers to ConstantWaves
        foreach (int timeToTrigger in currentWave.timesToTriggerConstantWaves)
        {
            // This gives each wave its own time-slot to spawn in, doubles dont work.
            int time = (int)Mathf.Floor(timeToTrigger * waveController.subwaveDelayMult * 
                                        SettingsManager.Instance.SubwaveDelayMult);
            if (time == prevTime)
                time++;
            _timesToTriggerConstantWaves.Add(time);
        } 

        return _timesToTriggerConstantWaves;
    }

    float GetBatchMultiplier(int subWaveIndex, bool isSubWave)
    {
        float batchMult;
        if (isSubWave)
        {
            batchMult = currentWave.subWaveMultipliers[subWaveIndex] * 
                        waveController.batchMultiplier * SettingsManager.Instance.BatchSpawnMult;
        }
        else
        {
            batchMult = currentWave.constantWaveMultipliers[subWaveIndex] * 
                        waveController.batchMultiplier * SettingsManager.Instance.BatchSpawnMult;
        }

        return batchMult;
    }

    #endregion

    #region Spawn Subwave

    IEnumerator SpawnSubWave(int subWaveIndex)
    {
        //Debug.Log("Start spawning SubWave index: " + subWaveIndex);
        // Set up for subwave or constant wave;        
        SubWaveSO subWave = currentWave.subWaves[subWaveIndex];
        SubWaveStyleSO subWaveStyle = currentWave.subWaveStyles[subWaveIndex];
        
        // Make copy of wave to be created
        List<GameObject> _enemyPrefabList = new List<GameObject>(subWave.enemyPrefabList);
        List<int> _numberToSpawn = new List<int>(subWave.numberToSpawn);

        // Get batchMultiplier
        float batchMult = GetBatchMultiplier(subWaveIndex, true);

        // Apply batchMult to SubWave numbers to spawn
        for (int i = 0; i < _numberToSpawn.Count; i++)
        {
            _numberToSpawn[i] = Mathf.FloorToInt(_numberToSpawn[i] * batchMult);
            //Debug.Log("Number to spawn of index " + i + ": " + _numberToSpawn[i]);
        }

        // Get starting point, angle, radius
        float spawnRadius = CalculateSpawnRadiusAndCenter();
        Vector2 spawnPos = GetPointOnCircumference(spawnRadius, 0, 1, true);
        float spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

        int listIndex = 0;
        while (_enemyPrefabList.Count > 0)
        {
            // Next new batch point 
            spawnPos = GetPointOnCircumference(
                        spawnRadius, spawnAngle, subWaveStyle.spreadForNextBatch, 
                        subWaveStyle.randomizeNextBatchSpread);
            spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, spawnPos - (Vector2)transform.position);

            // Lock batch to local point
            float batchAngle = spawnAngle;
            
            // Style's batchAmount start value is increasing with waveController Mult
            int batchSize = Mathf.FloorToInt(subWaveStyle.batchAmount * batchMult / 
                                             waveController.batchMultStart);

            // Spawn batch
            for (int i = 0; i < batchSize; i++)
            {    
                //Debug.Log("Start spawning batch of size: " + batchSize);

                // Clear empty list items
                while (_numberToSpawn.Count > 0 && _numberToSpawn[listIndex] <= 0)
                {
                    //Debug.Log("Removed List Items");
                    _numberToSpawn.RemoveAt(listIndex);
                    _enemyPrefabList.RemoveAt(listIndex);

                    // Exit if all empty
                    if (_numberToSpawn.Count <= 0)
                    {
                        //Debug.Log("Finished SubWave");
                        yield break;
                    }
                    // Check index reset
                    if (listIndex > _numberToSpawn.Count - 1)
                        listIndex = 0;
                }

                // If maxEnemies, wait to continue
                while (EnemyCounter.EnemyCount >= EnemyCounter.MaxEnemies)
                {
                    yield return new WaitForSeconds(1);
                }

                // Spawn
                StartCoroutine(TryToSpawn(spawnPos, subWave, listIndex, spawnRadius, spawnAngle));
                _numberToSpawn[listIndex]--;

                // Apply selection type
                if (subWaveStyle.selectionType == SelectionType.OneOfEach)
                {
                    listIndex++;
                }

                // Check index reset
                if (listIndex > _enemyPrefabList.Count - 1)
                    listIndex = 0;

                // Get next spawn around batchAngle
                spawnPos = GetPointOnCircumference(spawnRadius, batchAngle, 
                                                   subWaveStyle.spreadForBatch, true);
                spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

                yield return new WaitForSeconds(subWaveStyle.timeToNextSpawn);
            }

            yield return new WaitForSeconds(subWaveStyle.timeToNextBatch);
        }
        
        //Debug.Log("Finished SubWave");
    }

    IEnumerator SpawnConstantWave(int constantWaveIndex)
    {
        //  *****  NOT IN USE  *****
        SubWaveSO subWave = currentWave.constantWaves[constantWaveIndex];
        SubWaveStyleSO subWaveStyle = currentWave.constantWaveStyles[constantWaveIndex];
        yield return null;
    }

    IEnumerator TryToSpawn(Vector2 spawnPos, SubWaveSO subWave, int index, float radius, float angle)
    {
        bool spawned = false;
        float radSpread = 0;
        while (!spawned)
        {
            spawned = SpawnEnemy(spawnPos, subWave, index);
            yield return null;
            if (!spawned)
            {
                spawnPos = GetPointOnCircumference(radius, angle, radSpread, true);
                radSpread += 0.02f;
            }
        }
    }

    bool SpawnEnemy(Vector2 spawnPos, SubWaveSO subWave, int index)
    {
        Vector3Int gridPos = structureTilemap.WorldToCell(spawnPos);
        if (tileManager.CheckGridIsClear(gridPos, checkClearLayers, true))
        {
            EnemyCounter.EnemyCount++;

            EnemyBase enemyBase = subWave.enemyPrefabList[index].GetComponent<EnemyBase>();
            if (enemyBase == null)
            {
                Debug.LogError("Could not find EnemyBase in prefab when spawning enemy");
                return false;
            }

            GameObject enemyObj = PoolManager.Instance.GetFromPool(enemyBase.objectPoolName);
            //enemyObj.GetComponent<EnemyBase>().RandomizeStats();
            enemyObj.transform.position = spawnPos;
            
            return true;
        }
        else
        {
            Debug.Log("Wave Spawner CheckGridIsClear failed");
            return false;
        }
    }

    float CalculateSpawnRadiusAndCenter()
    {
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        List<Vector3Int> allPositions = tileManager.GetAllStructurePositions();
        allPositions.Add(new Vector3Int((int)qCubeTrans.position.x, (int)qCubeTrans.position.y, (int)qCubeTrans.position.z));

        if (allPositions == null || allPositions.Count == 0)
        {
            Debug.LogWarning("No structure positions found. Using default spawn radius.");
            centerPoint = qCubeTrans.position;
            return waveController.spawnRadiusBuffer;
        }

        foreach (Vector3 structurePosition in allPositions)
        {
            minBounds = Vector3.Min(minBounds, structurePosition);
            maxBounds = Vector3.Max(maxBounds, structurePosition);
        }

        centerPoint = (minBounds + maxBounds) / 2;
        float maxDistance = Mathf.Max(
            Mathf.Abs(centerPoint.x - minBounds.x),
            Mathf.Abs(centerPoint.y - minBounds.y),
            Mathf.Abs(centerPoint.x - maxBounds.x),
            Mathf.Abs(centerPoint.y - maxBounds.y)
        );

        // Add a buffer to ensure enemies spawn outside the base
        float spawnRadius = maxDistance + waveController.spawnRadiusBuffer;
        Debug.Log("CalculatedSpawnRadius: " + spawnRadius);
        return spawnRadius;
    }

    Vector2 GetPointOnCircumference(float radius, float startRadAngle, float radSpread, bool randomize)
    {
        if (randomize)
            startRadAngle += Random.Range(-radSpread * Mathf.PI, radSpread * Mathf.PI);
        else
            startRadAngle += radSpread * Mathf.PI;

        float x = centerPoint.x + radius * Mathf.Cos(startRadAngle);
        float y = centerPoint.y + radius * Mathf.Sin(startRadAngle);
    
        return new Vector2(x, y);
    }

    #endregion
}

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
    public WaveSOBase waveBase;
    [SerializeField] WaveSOBase currentWave;
    [SerializeField] int numSubwavesAtStart;
    [SerializeField] int timeToNextSubWave;
    [SerializeField] int minTimeToNextSubWave;

    public int currentSubWaveIndex;
    public int currentConstantWaveIndex;
    public bool waveSpawning;
    public int numberOfSubwaves;
    public bool waveComplete;
    public int maxIndex = 0;

    Vector2 centerPoint;
    int waveSeconds;
    Coroutine waveTimer;
    float halfWidth, halfHeight;

    void Start()
    {
        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        maxIndex = Mathf.FloorToInt(waveController.currentWaveIndex /
                                    waveController.wavesTillAddIndex) +
                                    currentPlanet.maxIndexToSelectAtStart + 
                                    SettingsManager.Instance.WavesTillAddWaveBaseDifficultyAdjust;

        centerPoint = qCubeTrans.position;  
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
        // Get the Master Wave, create an empty wave to build
        PlanetSO currentPlanet = PlanetManager.Instance.currentPlanet;
        if (currentPlanet == null)
        {
            Debug.LogError("PlanetManager.Instance.currentPlanet is null in BuildWave");
            return;
        }
        waveBase = currentPlanet.waveSOBase;
        currentWave = ScriptableObject.CreateInstance<WaveSOBase>();
        currentWave.subWaves = new List<SubWaveSO>();
        currentWave.subWaveMultipliers = new List<float>();
        currentWave.constantWaveMultipliers = new List<float>();
        currentWave.subWaveStyles = new List<SubWaveStyleSO>();
        currentWave.timesToTriggerSubwaves = new List<int>();
        currentWave.timesToTriggerConstantWaves = new List<int>();

        // Add more subwaves over time, apply difficulty
        numberOfSubwaves = Mathf.CeilToInt(numSubwavesAtStart + waveController.currentWaveIndex * 
                           SettingsManager.Instance.SubwaveListGrowthFactor);
        
        // Find max index to select based on current wave, plus starting max index
        maxIndex = Mathf.FloorToInt(waveController.currentWaveIndex /
                                    waveController.wavesTillAddIndex) +
                                    currentPlanet.maxIndexToSelectAtStart + 
                                    SettingsManager.Instance.WavesTillAddWaveBaseDifficultyAdjust;
        maxIndex = Mathf.Clamp(maxIndex, 0, waveBase.subWaves.Count - 1);
        Debug.Log("MaxIndex for wave " + waveController.currentWaveIndex + ": " + maxIndex);

        int prevSubwaveIndex = 0;
        int timeInSequence = 0;
        for (int i = 0; i < numberOfSubwaves; i++)
        {
            // Select random index, and subwave
            int subWaveIndex = Random.Range(0, maxIndex + 1);
            if (maxIndex > 0)
            {
                // Prevent doubles
                while (subWaveIndex == prevSubwaveIndex)
                {
                    subWaveIndex = Random.Range(0, maxIndex + 1);
                }
            }
            SubWaveSO subwaveToAdd = waveBase.subWaves[subWaveIndex];

            // Allow for extra waveSyles to select from
            int styleIndex = Random.Range(0, maxIndex + 1);
            styleIndex = Mathf.Clamp(styleIndex, 0, waveBase.subWaveStyles.Count - 1);
            SubWaveStyleSO styleToAdd = waveBase.subWaveStyles[styleIndex];

            // Add selections to lists
            currentWave.subWaves.Add(subwaveToAdd);
            currentWave.subWaveMultipliers.Add(waveBase.subWaveMultipliers[subWaveIndex]);
            currentWave.subWaveStyles.Add(styleToAdd);
            currentWave.timesToTriggerSubwaves.Add(timeInSequence);
            //Debug.Log("Added SubWave: " + waveToAdd.name);
            //Debug.Log("Added SubWave Style: " + styleToAdd.name);

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
        List<float> _numberToSpawn = new List<float>(subWave.numberToSpawn);

        // Get batchMultiplier
        float batchMult = GetBatchMultiplier(subWaveIndex, true);

        // Apply batchMult to SubWave numbers to spawn
        for (int i = 0; i < _numberToSpawn.Count; i++)
        {
            _numberToSpawn[i] = Mathf.FloorToInt(_numberToSpawn[i] * batchMult);
            //Debug.Log("Number to spawn of index " + i + ": " + _numberToSpawn[i]);
        }

        // Get starting point, angle, radius
        (halfWidth, halfHeight) = CalculateSquareBounds();
        Vector2 spawnPos;
        float spawnAngle = 0;

        int listIndex = 0;
        while (_enemyPrefabList.Count > 0)
        {
            // Next new batch point 
            spawnPos = GetPointOnSquareBoundary(spawnAngle, subWaveStyle.spreadForNextBatch, 
                       subWaveStyle.randomizeNextBatchSpread, halfWidth, halfHeight);
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
                StartCoroutine(TryToSpawn(spawnPos, spawnAngle, subWave, subWaveStyle, listIndex));
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
                spawnPos = GetPointOnSquareBoundary(spawnAngle, subWaveStyle.spreadForNextBatch, 
                           subWaveStyle.randomizeNextBatchSpread, halfWidth, halfHeight);;
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

    #endregion

    #region Spawn Enemies

    IEnumerator TryToSpawn(Vector2 spawnPos, float spawnAngle, SubWaveSO subWave, SubWaveStyleSO style, int index)
    {
        bool spawned = false;
        float radSpread = 0;
        while (!spawned)
        {
            spawned = SpawnEnemy(spawnPos, subWave, index);
            yield return null;
            if (!spawned)
            {
                spawnPos = GetPointOnSquareBoundary(spawnAngle, style.spreadForNextBatch, 
                       style.randomizeNextBatchSpread, halfWidth, halfHeight);
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
            enemyObj.GetComponent<EnemyBase>().ResetStats();
            enemyObj.transform.position = spawnPos;
            
            return true;
        }
        else
        {
            Debug.Log("Wave Spawner CheckGridIsClear failed");
            return false;
        }
    }

    #endregion

    #region Spawn Helpers

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

    // Deptricated ***
    Vector2 GetPointOnCircumference(float radius, float startRadAngle, float radSpread, bool randomize)
    {
        // Depricated ***
        if (randomize)
            startRadAngle += Random.Range(-radSpread * Mathf.PI, radSpread * Mathf.PI);
        else
            startRadAngle += radSpread * Mathf.PI;

        float x = centerPoint.x + radius * Mathf.Cos(startRadAngle);
        float y = centerPoint.y + radius * Mathf.Sin(startRadAngle);
    
        return new Vector2(x, y);
    }

    // Call this once after you have minBounds, maxBounds, etc.
    (float, float) CalculateSquareBounds()
    {
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        List<Vector3Int> allPositions = tileManager.GetAllStructurePositions();
        allPositions.Add(new Vector3Int((int)qCubeTrans.position.x, (int)qCubeTrans.position.y, (int)qCubeTrans.position.z));

        if (allPositions == null || allPositions.Count == 0)
        {
            Debug.LogWarning("No structure positions found. Using default spawn radius.");
            centerPoint = qCubeTrans.position;
            return (0, 0);
        }

        foreach (Vector3 structurePosition in allPositions)
        {
            minBounds = Vector3.Min(minBounds, structurePosition);
            maxBounds = Vector3.Max(maxBounds, structurePosition);
        }

        centerPoint = (minBounds + maxBounds) / 2;

        // Then we do something like:
        float width  = maxBounds.x - minBounds.x;
        float height = maxBounds.y - minBounds.y;
        
        // centerPoint is midpoint
        centerPoint = (minBounds + maxBounds) * 0.5f;

        // Add your spawnRadiusBuffer to each dimension
        // e.g. halfWidth = (width / 2) + spawnRadiusBuffer
        float halfWidth  = (width  * 0.5f) + waveController.spawnRadiusBuffer;
        float halfHeight = (height * 0.5f) + waveController.spawnRadiusBuffer;

        return (halfWidth, halfHeight);
    }

    Vector2 GetPointOnSquareBoundary(float startRadAngle, float radSpread, bool randomize, float halfWidth, float halfHeight)
    {
        // 1) Add random offset if needed
        if (randomize)
            startRadAngle += Random.Range(-radSpread * Mathf.PI, radSpread * Mathf.PI);
        else
            startRadAngle += radSpread * Mathf.PI;

        // 2) Convert angle to direction
        float dirX = Mathf.Cos(startRadAngle);
        float dirY = Mathf.Sin(startRadAngle);

        // 3) Solve intersection with boundary
        float tX = (Mathf.Abs(dirX) > 0.0001f)
            ? (halfWidth / Mathf.Abs(dirX))
            : float.MaxValue;
        float tY = (Mathf.Abs(dirY) > 0.0001f)
            ? (halfHeight / Mathf.Abs(dirY))
            : float.MaxValue;

        float t = Mathf.Min(tX, tY);

        // 4) Final spawn position
        float spawnX = centerPoint.x + t * dirX;
        float spawnY = centerPoint.y + t * dirY;

        return new Vector2(spawnX, spawnY);
    }

    #endregion
}

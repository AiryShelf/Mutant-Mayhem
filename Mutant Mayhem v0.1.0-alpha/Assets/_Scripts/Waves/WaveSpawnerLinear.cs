using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveSpawnerLinear : MonoBehaviour
{
    public Transform qCubeTrans;
    public WaveSOBase currentWave;
    public WaveControllerRandom waveController;
    public int currentSubWaveIndex;
    public int currentConstantWaveIndex;

    public bool waveSpawning;
    public bool waveComplete;

    Vector2 centerPoint;
    int waveSeconds;
    Coroutine waveTimer;

    void Start()
    {
        centerPoint = qCubeTrans.position;
    }

    public void StartWave()
    {       
        if (waveTimer != null)
        {
            StopCoroutine(waveTimer);
        }

        waveTimer = StartCoroutine(WaveTimer());
    }

    IEnumerator WaveTimer()
    {
        Debug.Log("Start Wave");

        // Initialize
        currentSubWaveIndex = 0;
        currentConstantWaveIndex = 0;
        waveComplete = false;
        waveSpawning = true;
        waveSeconds = 0;

        // Apply timing multipliers to Subwaves
        List<int> _timesToTriggerSubWaves = new List<int>();
        List<int> _timesToTriggerConstantWaves = new List<int>();
        int prevTime = -1;
        foreach (int timeToTrigger in currentWave.timesToTriggerSubwaves)
        {
            // This gives each wave its own time-slot to spawn in, doubles dont work.
            int time = (int)Mathf.Floor(timeToTrigger * waveController.spawnSpeedMult);
            while (time <= prevTime)
                time++;
            prevTime = time;

            _timesToTriggerSubWaves.Add(time);
        }
        // Apply timing multipliers to ConstantWaves
        foreach (int timeToTrigger in currentWave.timesToTriggerConstantWaves)
        {
            // This gives each wave its own time-slot to spawn in, doubles dont work.
            int time = (int)Mathf.Floor(timeToTrigger * waveController.spawnSpeedMult);
            if (time == prevTime)
                time++;
            _timesToTriggerConstantWaves.Add(time);
        }           

        // Find max length of time, add 5 seconds
        int maxTime1 = currentWave.timesToTriggerSubwaves.Max();
        int maxTime2 = 1;
        if (currentWave.constantWaves.Count > 0)
        {
            maxTime2 = currentWave.timesToTriggerConstantWaves.Max();
        }
        int maxTime = Mathf.Max(maxTime1, maxTime2) + 5;
        //Debug.Log("MaxTime for wave: " + maxTime);

        while (waveSpawning)
        {
            // Check if current wave seconds has a subWave to trigger
            if (currentWave.timesToTriggerSubwaves.Contains(waveSeconds))
            {
                currentSubWaveIndex = 
                    currentWave.timesToTriggerSubwaves.IndexOf(waveSeconds);
                StartCoroutine(SpawnSubWave(currentSubWaveIndex, true));
                //Debug.Log("Start SubWave");
            }
            // If current wave seconds has constantWave trigger
            if (currentWave.timesToTriggerConstantWaves.Contains(waveSeconds))
            {
                currentConstantWaveIndex = 
                    currentWave.timesToTriggerConstantWaves.IndexOf(waveSeconds);
                StartCoroutine(SpawnSubWave(currentConstantWaveIndex, false));
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
                Debug.Log("Wave Complete");
            }
        }
    }

    IEnumerator SpawnSubWave(int subWaveIndex, bool isSubWave)
    {
        //Debug.Log("Spawn SubWave Started");
        // Set up for subwave or constant wave
        SubWaveSO subWave;
        SubWaveStyleSO subWaveStyle;
        if (isSubWave)
        {
            subWave = currentWave.subWaves[subWaveIndex];
            subWaveStyle = currentWave.subWaveStyles[subWaveIndex];
        }
        else
        {
            subWave = currentWave.constantWaves[subWaveIndex];
            subWaveStyle = currentWave.constantWaveStyles[subWaveIndex];
        }
    
        // Make copy of wave to be created
        List<GameObject> _enemyPrefabList = 
            new List<GameObject>(subWave.enemyPrefabList);
        List<int> _numberToSpawn = 
            new List<int>(subWave.numberToSpawn);

        float batchMult = FindBatchMultiplier(subWaveIndex, isSubWave);

        // Apply batch multipliers
        for (int i = 0; i < _numberToSpawn.Count; i++)
        {
            _numberToSpawn[i] *= Mathf.FloorToInt(batchMult * waveController.batchMultiplier);
        }

        // Get starting point, angle, radius
        float spawnRadius = waveController.spawnRadius;
        Vector2 spawnPos = GetPointOnCircumference(spawnRadius, 0, 1, true);
        float spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

        int listIndex = 0;
        while (_enemyPrefabList.Count > 0)
        {
            //Debug.Log("Start spawning batch");

            // Next new batch point 
            spawnPos = GetPointOnCircumference(
                spawnRadius, spawnAngle, subWaveStyle.spreadForNextBatch, 
                subWaveStyle.randomizeNextBatchSpread);
            spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

            // Lock batch to local point
            float batchAngle = spawnAngle;
            
            // Spawn batch
            for (int i = 0; i < subWaveStyle.batchAmount * batchMult; i++)
            {    
                // Clear empty items
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

                _numberToSpawn[listIndex]--;
                SpawnEnemy(spawnPos, subWave, listIndex);

                if (subWaveStyle.selectionType == SelectionType.OneOfEach)
                {
                    listIndex++;
                }

                // Check index reset
                if (listIndex > _enemyPrefabList.Count - 1)
                    listIndex = 0;

                // Get next spawn around batchAngle
                spawnPos = GetPointOnCircumference(
                    spawnRadius, batchAngle, subWaveStyle.spreadForBatch, false);
                spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

                yield return new WaitForSeconds(subWaveStyle.timeToNextSpawn * 
                                                waveController.spawnSpeedMult);
            }

            yield return new WaitForSeconds(subWaveStyle.timeToNextBatch * 
                                            waveController.spawnSpeedMult);
        }
        
        //Debug.Log("Finished SubWave");
    }

    void SpawnEnemy(Vector2 spawnPos, SubWaveSO subWave, int index)
    {
        EnemyCounter.EnemyCount++;
        Instantiate(subWave.enemyPrefabList[index], spawnPos, Quaternion.identity);
    }

    float FindBatchMultiplier(int subWaveIndex, bool isSubWave)
    {
        float batchMult;
        if (isSubWave)
        {
            batchMult = currentWave.subWaveMultipliers[subWaveIndex];
        }
        else
        {
            batchMult = currentWave.constantWaveMultipliers[subWaveIndex];
        }

        return batchMult;
    }

    Vector2 GetPointOnCircumference(float radius, float startRadAngle, 
                                    float radSpread, bool randomize)
    {
        if (randomize)
            startRadAngle += Random.Range(-radSpread * Mathf.PI, radSpread * Mathf.PI);
        else
            startRadAngle += radSpread * Mathf.PI;

        float x = centerPoint.x + radius * Mathf.Cos(startRadAngle);
        float y = centerPoint.y + radius * Mathf.Sin(startRadAngle);
    
        return new Vector2(x, y);
    }
}

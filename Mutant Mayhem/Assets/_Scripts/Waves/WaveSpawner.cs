using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public Transform qCubeTrans;
    public WaveSOBase currentWaveSOBase;
    public WaveController waveController;

    public static int GLOBAL_enemyCount;

    Vector2 centerPoint;
    int waveSeconds;
    Coroutine waveTimer;

    void Start()
    {
        centerPoint = qCubeTrans.position;
        TriggerWaveBase();
    }

    void OnDisable()
    {

    }

    public void TriggerWaveBase()
    {
        waveSeconds = 0;
        if (waveTimer != null)
        {
            StopCoroutine(waveTimer);
        }
        waveTimer = StartCoroutine(WaveTimer());
    }

    IEnumerator WaveTimer()
    {
        while (true)
        {
            if (currentWaveSOBase.secondsToTriggerSubWavesUnique.Contains(waveSeconds))
            {
                int index = currentWaveSOBase.secondsToTriggerSubWavesUnique.IndexOf(waveSeconds);
                StartCoroutine(SpawnSubWave(currentWaveSOBase.subWaves[index]));
            }

            yield return new WaitForSeconds(1);
            waveSeconds++;            
        }
    }

    IEnumerator SpawnSubWave(SubWaveSO subWave)
    {
        Debug.Log("Start SubWave");

        // Make copy of wave to be created
        List<GameObject> _enemyPrefabList = new List<GameObject>(subWave.enemyPrefabList);
        List<int> _numberToSpawn = new List<int>(subWave.numberToSpawn);

        // Get starting point, angle, radius
        float spawnRadius = waveController.spawnRadius;
        Vector2 spawnPos = GetPointOnCircumference(spawnRadius, 0, 1);
        float spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

        if (subWave.selectionType == SelectionType.OneOfEach)
        {
            int index = 0;
            while (_enemyPrefabList.Count > 0)
            {
                Debug.Log("Start spawning batch");
                // Next new batch point 
                spawnPos = GetPointOnCircumference(
                    spawnRadius, spawnAngle, subWave.spreadForNextBatch);
                spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

                // Lock batch to local point
                float batchAngle = spawnAngle;
                
                // Spawn batch
                for (int i = 0; i < subWave.batchAmount; i++)
                {    
                    // Clear empty items
                    while (_numberToSpawn.Count > 0 && _numberToSpawn[index] <= 0)
                    {
                        _numberToSpawn.RemoveAt(index);
                        _enemyPrefabList.RemoveAt(index);

                        // Exit if all empty
                        if (_numberToSpawn.Count <= 0)
                            yield break;
                        // Check index reset
                        if (index > _enemyPrefabList.Count - 1)
                            index = 0;
                    }

                    _numberToSpawn[index]--;
                    SpawnEnemy(spawnPos, subWave, index);
                    index++;

                    // Check index reset
                    if (index > _enemyPrefabList.Count - 1)
                        index = 0;

                    // Next spawn around batchAngle
                    spawnPos = GetPointOnCircumference(
                        spawnRadius, batchAngle, subWave.spreadForBatch);
                    spawnAngle = Mathf.Atan2(spawnPos.y, spawnPos.x);

                    yield return new WaitForSeconds(subWave.timeToNextSpawn);
                }

                yield return new WaitForSeconds(subWave.timeToNextBatch);
            }
        }

        Debug.Log("Finished SubWave");
    }

    void SpawnEnemy(Vector2 spawnPos, SubWaveSO subWave, int index)
    {
        GLOBAL_enemyCount++;
        GameObject enemy = Instantiate(subWave.enemyPrefabList[index], spawnPos, Quaternion.identity);
    }

    Vector2 GetPointOnCircumference(float radius, float startAngle, float radSpread)
    {
        startAngle += Random.Range(-radSpread * Mathf.PI, radSpread * Mathf.PI);

        float x = centerPoint.x + radius * Mathf.Cos(startAngle);
        float y = centerPoint.y + radius * Mathf.Sin(startAngle);
    
        return new Vector2(x, y);
    }
}

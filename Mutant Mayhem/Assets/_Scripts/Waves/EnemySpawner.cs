using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{  
    public List<GameObject> enemyList;
    [SerializeField] int[] enemySpawnChance;
    [SerializeField] float spawnDelay;
    [SerializeField] int spawnAmount;
    [SerializeField] int maxEnemies;
    [SerializeField] LayerMask layersForSpawnCollision;
    [SerializeField] float radiusForCollisionCheck = 0.25f;
    [SerializeField] float maxTimeToTryToSpawn = 10;
    [SerializeField] bool hide = true;

    public static int GLOBAL_enemyCount;
    SpriteRenderer SR;

    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        StartCoroutine(SpawnCycle());
    }

    void FixedUpdate()
    {
        // Keep outside the screen, center right.
        if (hide)
        {
            SR.enabled = false;
            Vector3 position = Camera.main.ViewportToWorldPoint(new Vector2(1.1f, 0.5f));
            position.z = 1;
            transform.position = position;
        }
        else
        {
            SR.enabled = true;
            Vector3 position = Camera.main.ViewportToWorldPoint(new Vector2(0.7f, 0.5f));
            position.z = 1;
            transform.position = position;
        }
    }

    IEnumerator SpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);
            yield return new WaitForEndOfFrame();
            if (GLOBAL_enemyCount < maxEnemies)
                StartCoroutine(SpawnEnemy());
        }
    }

    IEnumerator SpawnEnemy()
    {
        Bounds bounds = GetComponent<SpriteRenderer>().bounds;
        
        for (int i = 0; i < spawnAmount; i++)
        {
            float timeElapsed = 0;
            bool spawned = false;
            while (!spawned)
            {
                // Random spot within bounds
                timeElapsed += Time.deltaTime; 
                Vector2 spawnPos = new Vector2(Random.Range(bounds.min.x, bounds.max.x), 
                                            Random.Range(bounds.min.y, bounds.max.y));

                Collider2D hit = Physics2D.OverlapCircle(spawnPos, radiusForCollisionCheck, layersForSpawnCollision);
                if (!hit)
                {
                    // Random enemy Prefab from list
                    GameObject enemyPrefab = enemyList[Random.Range(0, enemyList.Count)];
                    Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    GLOBAL_enemyCount++;
                    spawned = true;
                }
                else 
                    Debug.Log ("overlap detected by spawner");

                if (timeElapsed > maxTimeToTryToSpawn)
                {
                    spawned = true;
                    Debug.Log("Could not spawn an enemy, no clear area");
                }

                yield return new WaitForEndOfFrame();   
            }  
        }
    }

    Vector2 GetPointOnCircumference(float radius, float radAngle, float radSpread)
    {
        radAngle += Random.Range(-radSpread * Mathf.PI, radSpread * Mathf.PI);
        Vector2 centerPoint = new Vector2(0, 0);

        float x = centerPoint.x + radius * Mathf.Cos(radAngle);
        float y = centerPoint.y + radius * Mathf.Sin(radAngle);
    
        return new Vector2(x, y);
    }
}

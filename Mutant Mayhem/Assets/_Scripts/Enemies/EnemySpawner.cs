using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
    public List<GameObject> enemyList;
    [SerializeField] int[] enemySpawnChance;
    [SerializeField] float spawnTime;
    [SerializeField] int spawnAmount;
    [SerializeField] bool hide = true;
    [SerializeField] int maxEnemies;

    public int GLOBAL_enemyCount;


    void Start()
    {
        StartCoroutine(SpawnCycle());
    }

    void FixedUpdate()
    {
        // Keep outside the screen, center right.
        if (hide)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            transform.position = Camera.main.ViewportToWorldPoint(new Vector2(1.1f, 0.5f));
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = true;
            transform.position = Camera.main.ViewportToWorldPoint(new Vector2(0.7f, 0.5f));
        }
    }

    IEnumerator SpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTime);
            yield return new WaitForEndOfFrame();
            if (GLOBAL_enemyCount < maxEnemies)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        
        for (int i = 0; i < spawnAmount; i++)
        {
            // Random spot within bounds
            Bounds bounds = GetComponent<SpriteRenderer>().bounds;
            Vector2 spawnPos = new Vector2(Random.Range(bounds.min.x, bounds.max.x), 
                                           Random.Range(bounds.min.y, bounds.max.y));
        
            // Random enemy Prefab from list
            GameObject enemyPrefab = enemyList[Random.Range(0, enemyList.Count)];
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            GLOBAL_enemyCount++;  
        }
    }
}

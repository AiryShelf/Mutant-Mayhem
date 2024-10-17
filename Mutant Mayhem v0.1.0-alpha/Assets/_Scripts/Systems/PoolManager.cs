using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    
    // Dictionary to hold multiple pools, keyed by a unique string identifier
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    
    public Transform poolParent;

    [SerializeField] int amountToAddWhenEmpty = 3;

    [Header("Starting Pools")]
    [SerializeField] List<GameObject> poolPrefabs = new List<GameObject>();
    [SerializeField] List<string> poolNames = new List<string>();
    [SerializeField] List<int> poolCounts = new List<int>();

    [Header("Dynamic For Debug")]
    [SerializeField] Dictionary<string, int> totalsCreated = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ClearPools()
    {
        foreach (KeyValuePair<string, Queue<GameObject>> kvp in pools)
        {
            Queue<GameObject> poolQueue = kvp.Value;

            while (poolQueue.Count > 0)
            {
                GameObject obj = poolQueue.Dequeue();
                Destroy(obj);  // Destroy each object in the pool
            }
        }

        pools.Clear();
        totalsCreated.Clear();
    }

    void InitializePools()
    {
        for (int i = 0; i < poolNames.Count; i++)
        {
            CreatePool(poolNames[i], poolPrefabs[i], poolCounts[i]);
        }
    }

    public void ResetAllPools()
    {
        ClearPools();
        InitializePools();
    }

    public void CreatePool(string poolKey, GameObject prefab, int initialSize)
    {
        if (!pools.ContainsKey(poolKey))
        {
            pools[poolKey] = new Queue<GameObject>();

            for (int i = 0; i < initialSize; i++)
            {
                AddToPool(poolKey, prefab);
            }
        }
    }

    void AddToPool(string poolKey, GameObject prefab)
    {
        if (!pools.ContainsKey(poolKey))
        {
            Debug.LogError("No pool found with poolKey: " + poolKey + " when trying to add to pool");
            return;
        }

        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        if (poolParent != null)
            obj.transform.parent = poolParent;

        pools[poolKey].Enqueue(obj);

        // For debug, track totals created
        if (!totalsCreated.ContainsKey(poolKey))
            totalsCreated[poolKey] = 0;

        totalsCreated[poolKey]++;
    }

    public GameObject GetFromPool(string poolKey)
    {
        if (pools.ContainsKey(poolKey) && pools[poolKey].Count > 0)
        {
            // If the pool is nearly empty, instantiate new objects
            if (pools[poolKey].Count == 1)
            {
                // Create new copies
                GameObject copyObj = pools[poolKey].Peek();
                for (int i = 0; i < amountToAddWhenEmpty; i++)
                {
                    AddToPool(poolKey, copyObj);
                }

                Debug.Log($"Added {amountToAddWhenEmpty} new objects to {poolKey}.  " +
                          "It was nearly empty and now totals " +
                          $"{totalsCreated[poolKey]} objects created.");
            }
            
            // If there are objects in the pool, dequeue one and activate it
            GameObject obj = pools[poolKey].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogError($"Pool with key {poolKey} does not exist or is empty.");
        return null;
    }

    public void ReturnToPool(string poolKey, GameObject obj)
    {
        if (pools.ContainsKey(poolKey))
        {
            obj.transform.SetParent(poolParent);
            obj.SetActive(false);
            pools[poolKey].Enqueue(obj);
            //Debug.Log($"Successfully returned {obj.name} back to {poolKey} pool");
        }
        else
        {
            Debug.LogError($"{obj.name} tried to go back into the {poolKey} pool, but not pool was found");
            Destroy(obj);
        }
    }
}

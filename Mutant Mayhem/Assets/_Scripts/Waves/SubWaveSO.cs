using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SubWave", 
                 menuName = "SubWaveSO")]
public class SubWaveSO : ScriptableObject
{
    public List<GameObject> enemyPrefabList;
    public List<int> numberToSpawn;
    public SelectionType selectionType;
    public float batchAmount = 1;
    public float timeToNextSpawn = 0.1f;
    public float spreadForBatch = 0.05f;
    // Convert spreads to full circle *2pie
    public float timeToNextBatch = 3;
    public float spreadForNextBatch = 1;
}

public enum SelectionType
{
    OneOfEach,
    AllOfOne
}

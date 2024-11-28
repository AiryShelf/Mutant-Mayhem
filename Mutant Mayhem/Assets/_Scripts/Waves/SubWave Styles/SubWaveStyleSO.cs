using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SubWave Style", 
                 menuName = "Game/Waves/SubWaveStyleSO")]
public class SubWaveStyleSO : ScriptableObject
{
    public SelectionType selectionType;
    public int batchAmount = 1;
    public float timeToNextSpawn = 0.1f;
    public float spreadForBatch = 0.05f;
    // Spread of 1 is half circle, which is full circle when randomizing
    public float timeToNextBatch = 3;
    public float spreadForNextBatch = 1;
    public bool randomizeNextBatchSpread = true;
}

public enum SelectionType
{
    OneOfEach,
    AllOfOne
}
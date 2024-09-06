using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SubWave", 
                 menuName = "Waves/SubWaveSO")]
public class SubWaveSO : ScriptableObject
{
    public List<GameObject> enemyPrefabList;
    public List<int> numberToSpawn;
}

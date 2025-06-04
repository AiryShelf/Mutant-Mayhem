using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SubWave",
                 menuName = "Game/Waves/SubWaveSO")]
public class SubWaveSO : ScriptableObject
{
    public List<GameObject> enemyPrefabList;
    public List<float> numberToSpawn;
    public List<Genome> genomeList;
    public int popIncreasePerVariant = 1;
}

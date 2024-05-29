using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", 
                 menuName = "WaveSOBase")]
public class WaveSOBase : ScriptableObject
{
    [Header("Sub Waves")]
    public List<SubWaveSO> subWaves;
    public List<int> subWaveMultipliers;
    public List<int> secondsToTriggerSubWavesUnique;

    [Header("Constant Waves")]
    public List<SubWaveSO> constantWaves;
    public List<int> constantWaveMultipliers;
    public List<float> timesToTriggerConstantWaves;

    [Header("Other Variables")]
    public int minEnemiesToNextWave;
}

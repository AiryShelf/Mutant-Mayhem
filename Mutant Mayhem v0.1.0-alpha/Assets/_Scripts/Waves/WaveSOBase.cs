using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", 
                 menuName = "Waves/WaveSOBase")]
[System.Serializable]
public class WaveSOBase : ScriptableObject
{
    //[Header("Sub Waves")]
    public List<SubWaveSO> subWaves;
    public List<SubWaveStyleSO> subWaveStyles;
    public List<float> subWaveMultipliers;
    public List<int> timesToTriggerSubwaves;

    //[Header("Constant Waves")]
    public List<SubWaveSO> constantWaves;
    public List<SubWaveStyleSO> constantWaveStyles;
    public List<float> constantWaveMultipliers;
    public List<int> timesToTriggerConstantWaves;

    //[Header("Other Variables")]
    public int minEnemiesToNextWave;
}

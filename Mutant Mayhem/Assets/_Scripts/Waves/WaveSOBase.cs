using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", 
                 menuName = "Waves/WaveSOBase")]
public class WaveSOBase : ScriptableObject
{
    //[Header("Sub Waves")]
    public List<SubWaveSO> subWaves;
    public List<SubWaveStyleSO> subWaveStyles;
    public List<int> subWaveMultipliers;
    public List<int> timesToTriggerSubWaves;

    //[Header("Constant Waves")]
    public List<SubWaveSO> constantWaves;
    public List<SubWaveStyleSO> constantWaveStyles;
    public List<int> constantWaveMultipliers;
    public List<int> timesToTriggerConstantWaves;

    //[Header("Other Variables")]
    public int minEnemiesToNextWave;
}

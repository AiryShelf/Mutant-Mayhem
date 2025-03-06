using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: The display of this class in the inspector is controlled by an Editor script

[CreateAssetMenu(fileName = "New Wave", 
                 menuName = "Game/Waves/WaveSOBase")]
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
}

using UnityEngine;

public class WaveDebug : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(10,10,120,40), "Spawn Wave"))
            EvolutionManager.Instance.SpawnWaveFull();

        if (GUI.Button(new Rect(10,60,120,40), "End Wave"))
            EvolutionManager.Instance.EvolveAndSpawn();
    }
}
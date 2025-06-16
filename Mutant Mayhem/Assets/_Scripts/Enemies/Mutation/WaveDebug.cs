using UnityEngine;

public class WaveDebug : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(10,10,120,40), "Spawn Wave"))
            EvolutionManager.Instance.MutateAndSpawn();

        if (GUI.Button(new Rect(10,60,120,40), "Evolve All"))
            EvolutionManager.Instance.CrossoverAndMutate();
    }
}
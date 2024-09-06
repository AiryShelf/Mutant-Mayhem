using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    [Header("Tutorial Tracking")]
    public static bool TutorialDisabled = false;
    public static bool tutorialShowedBuild = false;
    public static bool tutorialShowedUpgrade = false;

    WaveControllerRandom waveController;  

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize();
    }

    public static void Initialize()
    {
        // Reset tutorial
        tutorialShowedBuild = false;
        tutorialShowedUpgrade = false;
    }
}
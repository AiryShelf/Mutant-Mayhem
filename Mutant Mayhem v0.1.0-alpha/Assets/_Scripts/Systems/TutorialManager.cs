using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    public static bool TutorialDisabled {  get; private set; }

    [Header("Tutorial Tracking")]
    public static bool tutorialShowedBuild = false;
    public static bool tutorialShowedUpgrade = false; 

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

        SetTutorialStateAndProfile(ProfileManager.Instance.currentProfile.isTutorialEnabled);
    }

    public static void SetTutorialStateAndProfile(bool isOn)
    {
        tutorialShowedBuild = !isOn;
        tutorialShowedUpgrade = !isOn;
        TutorialDisabled = !isOn;
        ProfileManager.Instance.currentProfile.isTutorialEnabled = isOn;
        ProfileManager.Instance.SaveCurrentProfile();
    }

    public static void ResetShownPanels()
    {
        tutorialShowedBuild = false;
        tutorialShowedUpgrade = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    public static bool TutorialDisabled {  get; private set; }

    [Header("Tutorial Tracking")]
    public static bool TutorialShowedBuild = false;
    public static bool TutorialShowedUpgrade = false; 
    public static int NumTutorialsOpen = 0;

    [Header("Tutorial Control")]
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] float escKeyCooldown = 0.1f;
    public static bool escCooling;
    InputAction escapeAction;
    

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

        InputActionMap uIActionMap = inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");
        escapeAction.started += OnEscapePressed;
    }

    void Start()
    {
        SetTutorialStateAndProfile(ProfileManager.Instance.currentProfile.isTutorialEnabled);
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        StartCoroutine(EscapeKeyCooldown());
    }

    public static void SetTutorialStateAndProfile(bool isOn)
    {
        TutorialShowedBuild = !isOn;
        TutorialShowedUpgrade = !isOn;
        TutorialDisabled = !isOn;
        ProfileManager.Instance.currentProfile.isTutorialEnabled = isOn;
        ProfileManager.Instance.SaveCurrentProfile();
    }

    public static void ResetShownPanels()
    {
        TutorialShowedBuild = false;
        TutorialShowedUpgrade = false;
    }

    IEnumerator EscapeKeyCooldown()
    {
        yield return new WaitForSecondsRealtime(escKeyCooldown);
        escCooling = false;
    }
}
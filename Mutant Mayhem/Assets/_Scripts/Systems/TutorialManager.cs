using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public static bool IsTutorialDisabled { get; private set; }

    public Mission tutorialMission;

    [Header("Tutorial Tracking")]
    public static int NumTutorialsOpen = 0;

    [Header("Tutorial Control")]
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] float escKeyCooldown = 0.1f;
    [HideInInspector] public static bool escIsCooling;
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

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += SyncTutorialToProfile;
    }

    void OnDisable()
    {
        ProfileManager.OnProfileIsSet -= SyncTutorialToProfile;
    }

    void Start()
    {
        //SetTutorialState(ProfileManager.Instance.currentProfile.isTutorialEnabled);
        SyncTutorialToProfile(ProfileManager.Instance.currentProfile);
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        StartCoroutine(EscapeKeyCooldown());
    }

    void SyncTutorialToProfile(PlayerProfile playerProfile)
    {
        IsTutorialDisabled = !playerProfile.isTutorialEnabled;
    }

    public static void SetTutorialState(bool isOn)
    {
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        if (currentProfile == null)
        {
            Debug.LogError("No current profile to save tutorial setting.");
            return;
        }

        //Instance.SyncTutorialToProfile(currentProfile);

        Instance.ToggleTutorial(isOn);
    }

    void ToggleTutorial(bool isOn)
    {
        IsTutorialDisabled = !isOn;
        // Change profile settings
        ProfileManager.Instance.currentProfile.isTutorialEnabled = isOn;
        ProfileManager.Instance.SaveCurrentProfile(); // Save the profile with updated tutorial state

        Debug.Log("Tutorial Enabled: " + isOn);

        UI_MissionPanelController missionPanelController = FindObjectOfType<UI_MissionPanelController>();
        if (missionPanelController == null)
        {
            Debug.LogWarning("TutorialManager: missionPanelController is null");
            return;
        }

        if (isOn && !missionPanelController.missions.Contains(tutorialMission))
        {
            missionPanelController.AddMission(tutorialMission, true);
            missionPanelController.StartMission();
        }
        else if (!isOn && missionPanelController.missions.Contains(tutorialMission))
            missionPanelController.EndMission();
    }

    IEnumerator EscapeKeyCooldown()
    {
        yield return new WaitForSecondsRealtime(escKeyCooldown);
        escIsCooling = false;
    }
}
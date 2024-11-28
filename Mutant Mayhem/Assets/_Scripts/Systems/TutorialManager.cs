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
    static UI_MissionPanelController missionPanelController;

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
        SetTutorialState(ProfileManager.Instance.currentProfile.isTutorialEnabled);
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
        if (currentProfile != null)
            Instance.SyncTutorialToProfile(currentProfile);
    }

    public static void ResetTutorialPanel()
    {
        missionPanelController = FindObjectOfType<UI_MissionPanelController>();
        if (missionPanelController == null)
        {
            Debug.LogError("TutorialManager could not find MissionPanel");
            return;
        }

        if (!IsTutorialDisabled)
        {
            missionPanelController.gameObject.SetActive(true);
            missionPanelController.StartMission(Instance.tutorialMission);
        }
        else
        {
            missionPanelController.StartPlanetMission();
        }
    }

    IEnumerator EscapeKeyCooldown()
    {
        yield return new WaitForSecondsRealtime(escKeyCooldown);
        escIsCooling = false;
    }
}
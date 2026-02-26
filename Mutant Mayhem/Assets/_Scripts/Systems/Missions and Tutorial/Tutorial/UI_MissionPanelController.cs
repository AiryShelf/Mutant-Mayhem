using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MissionPanelController : MonoBehaviour
{
    public static UI_MissionPanelController Instance;

    public List<MissionSO> missions = new List<MissionSO>();
    [SerializeField] MissionSO currentMission;
    List<Task> _tasks = new List<Task>();
    [SerializeField] TextMeshProUGUI missionTitle;
    [SerializeField] TextMeshProUGUI objectiveTitle;
    [SerializeField] Transform objectivesGrid;
    [SerializeField] CanvasGroup completedStamp;
    [SerializeField] RectTransform backPanel;
    [SerializeField] ObjectiveInfoPanel objectiveInfoPanel;
    public int currentObjectiveIndex;

    [Header("Panel Open Effect")]
    [SerializeField] CanvasGroup missionPanelGroup;
    [SerializeField] Button missionPanelOpenButton;
    [SerializeField] float delayOpenTime = 8;
    [SerializeField] Image panelOutline;
    [SerializeField] Image backPanelImage;
    [SerializeField] float panelFlashTime = 4;
    [SerializeField] float panelFlashDelay = 0.25f;
    [SerializeField] Color flashColorOutline;
    Color startColorOutline;
    Color startColorBackPanel;
    bool isHidden = false;
    Coroutine panelOpenEffectCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        // Clear mission list and tasks at start, in case there are any set in inspector
        missions.Clear();
        foreach (Transform child in objectivesGrid)
        {
            _tasks.Add(child.GetComponent<Task>());
            //Debug.Log("Added task at start");
        }
        ClearTasksGrid();
    }

    void Start()
    {
        ShowMissionPanel(false);
        missionPanelOpenButton.gameObject.SetActive(false);
        completedStamp.alpha = 0;
        startColorOutline = panelOutline.color;
        startColorBackPanel = backPanelImage.color;
    }

    #region Panel Control

    public void StartPanelOpenEffect()
    {
        EnableMissionPanel(true);
        missionPanelGroup.alpha = 0;
        missionPanelOpenButton.gameObject.SetActive(false);

        if (panelOpenEffectCoroutine != null)
            StopCoroutine(panelOpenEffectCoroutine);

        panelOpenEffectCoroutine = StartCoroutine(PanelOpenEffect());
    }

    IEnumerator PanelOpenEffect()
    {
        yield return new WaitForSeconds(delayOpenTime);

        Initialize();
        Debug.Log("Mission panel initialized.");
        StartCoroutine(GameTools.FlashImage(backPanelImage, panelFlashTime, panelFlashDelay, flashColorOutline, startColorBackPanel));
        StartCoroutine(GameTools.FlashImage(panelOutline, panelFlashTime, panelFlashDelay, flashColorOutline, startColorOutline));  
    }

    void Initialize()
    {
        AddMission(PlanetManager.Instance.currentPlanet.mission, false);

        missionPanelGroup.alpha = 1;
        ShowMissionPanel(true);
        StartMission();
    }

    public void EnableMissionPanel(bool enable)
    {
        if (enable)
        {
            ShowMissionPanel(!isHidden);
            missionPanelOpenButton.gameObject.SetActive(isHidden);
            missionPanelGroup.alpha = 1;
        }
        else
        {
            missionPanelGroup.alpha = 0;
            ShowMissionPanel(false);
            missionPanelOpenButton.gameObject.SetActive(false);
        }

        completedStamp.alpha = 0;
    }

    public void ShowMissionPanel(bool show)
    {
        if (show)
        {
            missionPanelGroup.alpha = 1;
            missionPanelGroup.blocksRaycasts = true;
            missionPanelOpenButton.gameObject.SetActive(false);
            isHidden = false;
        }
        else
        {
            missionPanelGroup.alpha = 0;
            missionPanelGroup.blocksRaycasts = false;
            missionPanelOpenButton.gameObject.SetActive(true);
            isHidden = true;
        }
    }

    public void StopMissionPanel()
    {
        if (panelOpenEffectCoroutine != null)
            StopCoroutine(panelOpenEffectCoroutine);

        panelOutline.color = startColorOutline;
        backPanelImage.color = startColorBackPanel;

        EndMission(false, "", 0);
    }

    public void OnShowInfoClicked()
    {
        ShowObjectiveInfoPanel(true);
    }

    IEnumerator DelayedShowObjectiveInfoPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowObjectiveInfoPanel(true);
    }


    public void ShowObjectiveInfoPanel(bool show)
    {
        objectiveInfoPanel.gameObject.SetActive(show);
        TouchManager.Instance.ShowVirtualJoysticks(!show);

        // Set image
        Sprite infoSprite = currentMission.objectives[currentObjectiveIndex].infoImageSprite;
        Sprite infoSprite2 = currentMission.objectives[currentObjectiveIndex].inforImageSprite2;
        objectiveInfoPanel.infoImage.sprite = infoSprite;
        objectiveInfoPanel.infoImage2.sprite = infoSprite2;
        if (objectiveInfoPanel.infoImage.sprite == null)
            objectiveInfoPanel.infoImage.gameObject.SetActive(false);
        else
            objectiveInfoPanel.infoImage.gameObject.SetActive(true);

        if (objectiveInfoPanel.infoImage2.sprite == null)
            objectiveInfoPanel.infoImage2.gameObject.SetActive(false);
        else
            objectiveInfoPanel.infoImage2.gameObject.SetActive(true);

        if (infoSprite == null && infoSprite2 == null)
        {
            objectiveInfoPanel.imagesLayoutGroup.gameObject.SetActive(false);
        }
        else
        {
            objectiveInfoPanel.imagesLayoutGroup.gameObject.SetActive(true);
        }

        // Set text based on input device
        objectiveInfoPanel.titleText.text = currentMission.objectives[currentObjectiveIndex].objectiveTitle;
        if (InputManager.LastUsedDevice == Touchscreen.current)
            objectiveInfoPanel.descriptionText.text = currentMission.objectives[currentObjectiveIndex].objectiveDescriptionMobile;
        else if (InputManager.LastUsedDevice == Gamepad.current)
            objectiveInfoPanel.descriptionText.text = currentMission.objectives[currentObjectiveIndex].objectiveDescriptionGamepad;
        else
            objectiveInfoPanel.descriptionText.text = currentMission.objectives[currentObjectiveIndex].objectiveDescription;
    }

    #endregion

    #region Missions

    public void AddMission(MissionSO mission, bool setAsCurrentMission)
    {
        isHidden = false;
        if (setAsCurrentMission)
            missions.Insert(0, mission);
        else
            missions.Add(mission);

        Debug.Log("Added mission: " + mission);
    }

    public void StartMission()
    {
        //ClearTasksGrid();
        currentMission = missions[0];
        DisplayObjective(0);
    }

    public void EndMission(bool showTextFly, string planetName, int researchPointsGained)
    {
        if (currentMission == null)
            return;

        if (missions.Count == 0)
            return;

        if (showTextFly)
            ShowMissionCompletedTextFly(planetName, researchPointsGained);
        //ClearTasksGrid();

        missions.RemoveAt(0);
        ClearTasksGrid();

        if (missions.Count == 0)
        {
            EnableMissionPanel(false);
            return;
        }
        
        StartMission();
    }

    /// <summary>
    /// Checks if the current mission is completed and if it's the first time completion.
    /// </summary>
    /// <returns>(missionCompleted, firstTimeCompleted)</returns>
    (bool, bool) CheckMissionCompletionStatus()
    {
        if (currentObjectiveIndex + 1 >= currentMission.objectives.Count)
        {
            PlayerProfile profile = ProfileManager.Instance.currentProfile;
            if (profile.AddCompletedMission(PlanetManager.Instance.currentPlanet.bodyName, 
                                            currentMission.missionTitle, 
                                            currentMission.researchPointsReward))
            {
                if (missions.Count == 1) // Last mission on planet
                {
                    ProfileManager.Instance.SetPlanetCompleted(PlanetManager.Instance.currentPlanet.bodyName);
                }
                return (true, true);
            }
            else if (PlanetManager.Instance.currentPlanet.isTutorialPlanet)
            {
                MessageBanner.PulseMessage($"Tutorial mission completed!\n\nYou're ready for the real thing!", Color.cyan);
                return (true, false);
            }
            else
            {
                Debug.Log("Mission already completed previously, no research points awarded.");
                MessageBanner.PulseMessage($"Mission was previously completed! No research points are awarded.\n\n" +
                                            "Try another planet for new missions.", Color.yellow);
                return (true, false);
            }
        }
        return (false, false);
    }

    void ShowMissionCompletedTextFly(string planetName, int researchPointsGained)
    {
        // No text fly or RP for tutorial planet
        if (PlanetManager.Instance.currentPlanet.isTutorialPlanet)
            return;

        GameObject textFlyObj = PoolManager.Instance.GetFromPool(WaveController.Instance.textFlyPoolName);
        TextFly textFly = textFlyObj.GetComponent<TextFly>();
        textFly.transform.SetParent(WaveController.Instance.gameplayCanvas.transform, false);
        RectTransform rectTransform = textFly.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        string rpGainedCommas = researchPointsGained.ToString("N0");
        textFly.Initialize($"Planet {planetName} completed! +{rpGainedCommas} RP gained!\n\nNew Planet(s) unlocked!", 
                            Color.cyan, 1, Vector2.down, false, WaveController.Instance.textFlyMaxScale);
        
        MessageBanner.PulseMessage($"Planet {planetName} completed!\n\nYou earned {researchPointsGained} research points and unlocked new planet(s)!", Color.green);
    }

    #endregion

    #region Objectives

    void DisplayObjective(int index)
    {
        MessageManager.Instance.PlayConversation(currentMission.objectives[index].startConversation);

        completedStamp.alpha = 0;
        currentObjectiveIndex = index;

        objectiveTitle.text = currentMission.objectives[index].objectiveTitle;
        missionTitle.text = currentMission.missionTitle;

        ClearTasksGrid();

        // Set back panel size
        foreach (GameObject obj in currentMission.objectives[index].taskPrefabs)
        {
            Task task = Instantiate(obj, objectivesGrid).GetComponent<Task>();
            if (task != null)
            {
                _tasks.Add(task);
                task.missionPanelController = this;
                //Debug.Log("Added a task");
                
                // Adjust panel size
                RectTransform rect = obj.GetComponent<RectTransform>();
                if (rect == null)
                {
                    Debug.LogError("Could not find rect transform on task prefab!");
                    continue;
                }

                float newHeight = backPanel.sizeDelta.y + rect.sizeDelta.y;
                backPanel.sizeDelta = new Vector2(backPanel.sizeDelta.x, newHeight);
            }
        }
        ShowMissionPanel(true);

        if (currentMission.objectives[index].autoShowInfoPanel)
            StartCoroutine(DelayedShowObjectiveInfoPanel(3f));
    }

    void ObjectiveComplete()
    {
        MessageManager.Instance.PlayConversation(currentMission.objectives[currentObjectiveIndex].endConversation);

        StartCoroutine(DisplayCompletedForTime());
    }

    

    IEnumerator DisplayCompletedForTime()
    {
        completedStamp.alpha = 1;
        ShowMissionPanel(true);

        // Check mission completion status
        (bool missionCompleted, bool firstTimeCompleted) = CheckMissionCompletionStatus();
        
        yield return new WaitForSeconds(currentMission.objectives[currentObjectiveIndex].timeToShowCompletionUI);

        // Show next objective or end mission
        int nextObjectiveIndex = currentObjectiveIndex + 1;
        if (nextObjectiveIndex < currentMission.objectives.Count)
            DisplayObjective(nextObjectiveIndex);
        else 
        {
            completedStamp.alpha = 0;
            EndMission(firstTimeCompleted, 
                        PlanetManager.Instance.currentPlanet.bodyName, 
                        currentMission.researchPointsReward);
        }
    }

    void ClearTasksGrid()
    {
        foreach (Task task in _tasks)
        {
            RectTransform rect = task.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("Could not find rect transform on objective!");
                return;
            }

            float newHeight = backPanel.sizeDelta.y - rect.sizeDelta.y;
            backPanel.sizeDelta = new Vector2(backPanel.sizeDelta.x, newHeight);

            Destroy(task.gameObject);
            //Debug.Log("Removed a task");
        }

        _tasks.Clear();
    }

    public void CheckIfObjectiveComplete()
    {
        bool allComplete = true;
        // Loop through list to check if all complete
        foreach (Task objective in _tasks)
        {
            if (!objective.isComplete)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            ObjectiveComplete();
        }
    }

    #endregion
}

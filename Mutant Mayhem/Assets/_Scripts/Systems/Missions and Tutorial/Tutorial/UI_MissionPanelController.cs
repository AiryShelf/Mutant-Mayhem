using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionPanelController : MonoBehaviour
{
    public List<MissionSO> missions;
    [SerializeField] MissionSO currentMission;
    List<Task> _tasks = new List<Task>();
    [SerializeField] TextMeshProUGUI missionTitle;
    [SerializeField] TextMeshProUGUI objectiveTitle;
    [SerializeField] Transform objectivesGrid;
    [SerializeField] CanvasGroup completedStamp;
    [SerializeField] RectTransform backPanel;
    [SerializeField] protected float timeToShowCompleted = 2f;
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
    bool isPanelOpen = false;

    void Start()
    {
        ShowMissionPanel(false);
        completedStamp.alpha = 0;
        startColorOutline = panelOutline.color;
        startColorBackPanel = backPanelImage.color;
        StartCoroutine(PanelOpenEffect());
    }

    void Initialize()
    {
        foreach (Transform child in objectivesGrid)
        {
            _tasks.Add(child.GetComponent<Task>());
            //Debug.Log("Added task at start");
        }

        AddMission(PlanetManager.Instance.currentPlanet.mission, false);

        ShowMissionPanel(true);
        StartMission();
    }

    public void ShowMissionPanel(bool show)
    {
        isPanelOpen = show;

        if (show)
        {
            missionPanelGroup.alpha = 1;
            missionPanelGroup.blocksRaycasts = true;
            missionPanelOpenButton.gameObject.SetActive(false);
        }
        else
        {
            missionPanelGroup.alpha = 0;
            missionPanelGroup.blocksRaycasts = false;
            missionPanelOpenButton.gameObject.SetActive(true);
        }
    }

    IEnumerator PanelOpenEffect()
    {
        yield return new WaitForSecondsRealtime(delayOpenTime);

        Initialize();
        StartCoroutine(GameTools.FlashImage(backPanelImage, panelFlashTime, panelFlashDelay, flashColorOutline, startColorBackPanel));
        StartCoroutine(GameTools.FlashImage(panelOutline, panelFlashTime, panelFlashDelay, flashColorOutline, startColorOutline));  
    }

    public void AddMission(MissionSO mission, bool setAsCurrentMission)
    {
        if (setAsCurrentMission)
            missions.Insert(0, mission);
        else
            missions.Add(mission);

        Debug.Log("Added mission: " + mission);
    }

    public void OnShowInfoClicked()
    {
        ShowObjectiveInfoPanel(true);
    }

    public void StartMission()
    {
        //ClearTasksGrid();
        currentMission = missions[0];
        DisplayObjective(0);
    }

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
        ShowMissionPanel(isPanelOpen);
    }

    void ObjectiveComplete()
    {
        MessageManager.Instance.PlayConversation(currentMission.objectives[currentObjectiveIndex].endConversation);

        StartCoroutine(DisplayCompletedForTime());
    }

    public void EndMission()
    {
        // Should display a "Mission Completed" effect here, maybe with title text?
        //ClearTasksGrid();
        Debug.Log("Ending Mission: " + missions[0]);

        missions.RemoveAt(0);
        ClearTasksGrid();

        if (missions.Count == 0)
        {
            ProfileManager.Instance.SetPlanetCompleted(PlanetManager.Instance.currentPlanet.bodyName);
            gameObject.SetActive(false);
            return;
        }
        
        StartMission();
    }

    IEnumerator DisplayCompletedForTime()
    {
        completedStamp.alpha = 1;
        ShowMissionPanel(true);
        yield return new WaitForSeconds(timeToShowCompleted);

        int nextObjectiveIndex = currentObjectiveIndex + 1;

        if (nextObjectiveIndex < currentMission.objectives.Count)
            DisplayObjective(nextObjectiveIndex);
        else 
        {
            completedStamp.alpha = 0;
            EndMission();
        }
    }

    public void ShowObjectiveInfoPanel(bool show)
    {
        objectiveInfoPanel.gameObject.SetActive(show);
        objectiveInfoPanel.titleText.text = currentMission.objectives[currentObjectiveIndex].objectiveTitle;
        objectiveInfoPanel.descriptionText.text = currentMission.objectives[currentObjectiveIndex].objectiveDescription;
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
}

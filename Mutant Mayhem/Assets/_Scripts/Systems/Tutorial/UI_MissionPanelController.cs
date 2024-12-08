using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionPanelController : MonoBehaviour
{
    public List<Mission> missions;
    [SerializeField] Mission currentMission;
    List<Task> _tasks = new List<Task>();
    [SerializeField] TextMeshProUGUI missionTitle;
    [SerializeField] TextMeshProUGUI objectiveTitle;
    [SerializeField] Transform objectivesGrid;
    [SerializeField] CanvasGroup completedStamp;
    [SerializeField] RectTransform backPanel;
    [SerializeField] protected float timeToShowCompleted = 2f;
    [SerializeField] ObjectiveInfoPanel objectiveInfoPanel;

    public int currentObjectiveIndex;

    void Start()
    {
        foreach (Transform child in objectivesGrid)
        {
            _tasks.Add(child.GetComponent<Task>());
            Debug.Log("Added task at start");
        }

        AddMission(PlanetManager.Instance.currentPlanet.planetMission, false);

        if (!TutorialManager.IsTutorialDisabled && !missions.Contains(TutorialManager.Instance.tutorialMission))
        {
            AddMission(TutorialManager.Instance.tutorialMission, true);
        }

        StartMission();
    }

    public void AddMission(Mission mission, bool setAsCurrentMission)
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

    public void OnDisableTutorialClicked()
    {
        TutorialManager.SetTutorialState(false);
        if (currentMission.isTutorial)
            EndMission();
    }

    public void StartMission()
    {
        //ClearTasksGrid();
        currentMission = missions[0];
        DisplayObjective(0);
    }

    void DisplayObjective(int index)
    {
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
                Debug.Log("Added a task");
                
                // Adjust panel size
                RectTransform rect = obj.GetComponent<RectTransform>();
                if (rect == null)
                {
                    Debug.LogError("Could not find rect transform on objective!");
                    return;
                }

                float newHeight = backPanel.sizeDelta.y + rect.sizeDelta.y;
                backPanel.sizeDelta = new Vector2(backPanel.sizeDelta.x, newHeight);
            }
        }
    }

    void ObjectiveComplete()
    {
        completedStamp.alpha = 1;
        StartCoroutine(DisplayCompletedForTime());
    }

    public void EndMission()
    {
        // Should display a "Mission Completed" effect here, maybe with title text?
        //ClearTasksGrid();
        Debug.Log("Ending Mission: " + missions[0]);

        missions.RemoveAt(0);
        if (missions.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }
        
        StartMission();
    }

    IEnumerator DisplayCompletedForTime()
    {
        yield return new WaitForSeconds(timeToShowCompleted);

        int nextObjectiveIndex = currentObjectiveIndex + 1;

        if (nextObjectiveIndex < currentMission.objectives.Count)
            DisplayObjective(nextObjectiveIndex);
        else 
        {
            EndMission();
        }
    }

    public void ShowObjectiveInfoPanel(bool show)
    {
        objectiveInfoPanel.gameObject.SetActive(show);
        objectiveInfoPanel.titleText.text = currentMission.objectives[currentObjectiveIndex].objectiveTitle;
        objectiveInfoPanel.descriptionText.text = currentMission.objectives[currentObjectiveIndex].objectiveDescription;

        // Handle 'Disable Tutorial' button
        if (currentMission.missionTitle == "Tutorial")
            objectiveInfoPanel.disableTutorialButton.gameObject.SetActive(true);
        else
            objectiveInfoPanel.disableTutorialButton.gameObject.SetActive(false);
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
            Debug.Log("Removed a task");
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

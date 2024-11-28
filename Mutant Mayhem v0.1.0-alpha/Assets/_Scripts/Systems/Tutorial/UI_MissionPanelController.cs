using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionPanelController : MonoBehaviour
{
    public Mission currentMission;
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
        
    }

    public void OnShowInfoClicked()
    {
        ShowObjectiveInfoPanel(true);
    }

    public void OnDisableTutorialClicked()
    {
        TutorialManager.SetTutorialState(false);
        EndMission();
    }

    public void ObjectiveComplete()
    {
        completedStamp.alpha = 1;
        StartCoroutine(DisplayCompletedForTime());
    }

    public void EndMission()
    {
        // Should display a "Mission Completed" effect here
        gameObject.SetActive(false);
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

    public void StartMission(Mission mission)
    {
        currentMission = mission;
        DisplayObjective(0);
    }

    public void StartPlanetMission()
    {
        
    }

    public void DisplayObjective(int index)
    {
        completedStamp.alpha = 0;
        currentObjectiveIndex = index;

        objectiveTitle.text = currentMission.objectives[index].objectiveTitle;
        missionTitle.text = currentMission.missionTitle;

        ClearTasksGrid();

        // Set back panel size
        foreach (GameObject obj in currentMission.objectives[index].taskPrefabs)
        {
            Task objective = Instantiate(obj, objectivesGrid).GetComponent<Task>();
            if (objective != null)
            {
                _tasks.Add(objective);
                objective.tutorialPanelController = this;
                
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
        _tasks.Clear();

        foreach (Transform child in objectivesGrid)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("Could not find rect transform on objective!");
                return;
            }

            float newHeight = backPanel.sizeDelta.y - rect.sizeDelta.y;
            backPanel.sizeDelta = new Vector2(backPanel.sizeDelta.x, newHeight);

            Destroy(child.gameObject);
        }
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

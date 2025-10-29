using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI taskText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Image checkmarkImage;

    [Header("Dynamic vars")]
    public bool isComplete = false;
    public float progress = 0; // 0 to 1 value
    public UI_MissionPanelController missionPanelController;

    public void OnTaskComplete(InputAction.CallbackContext context)
    {
        //SetTaskComplete();
    }

    public void SetTaskComplete()
    {
        if (isComplete)
            return;

        progress = 1;
        isComplete = true;

        // Show checkmark, grey out text
        checkmarkImage.enabled = true;
        taskText.color = Color.grey;
        progressText.color = Color.white;

        missionPanelController.CheckIfObjectiveComplete();

        Debug.Log("Task completed");
    }

    public void SetTaskIncomplete()
    {
        progress = 0;
        isComplete = false;

        // Hide checkmark, reset text color
        checkmarkImage.enabled = false;
        taskText.color = Color.white;
        progressText.color = Color.grey;

        Debug.Log("Task set to incomplete");
    }

    protected void UpdateProgressText()
    {
        string percent = GameTools.FactorToPercent(1 + progress);
        progressText.text = percent;
    }
}

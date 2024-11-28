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
    public UI_MissionPanelController tutorialPanelController;

    public void OnTaskComplete(InputAction.CallbackContext context)
    {
        //SetTaskComplete();
    }

    public void SetTaskComplete()
    {
        progress = 1;
        isComplete = true;

        // Show checkmark, grey out text
        checkmarkImage.enabled = true;
        taskText.color = Color.grey;
        progressText.color = Color.white;

        tutorialPanelController.CheckIfObjectiveComplete();

        Debug.Log("Task completed");
    }

    protected void UpdateProgressText()
    {
        string percent = GameTools.FactorToPercent(1 + progress);
        progressText.text = percent;
    }
}

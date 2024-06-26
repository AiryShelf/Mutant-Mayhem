using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] string hideControlsStr;
    [SerializeField] string showControlsStr;
    [SerializeField] TextMeshProUGUI showHideText;
    [SerializeField] List<string> controlStrings;
    [SerializeField] GameObject controlsTextPrefab;
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsWave;
    [SerializeField] Player player;
    InputActionMap playerActionMap;
    InputAction helpAction;

    void Awake()
    {
        playerActionMap = player.inputAsset.FindActionMap("Player");
        helpAction = playerActionMap.FindAction("Help");

        foreach (string str in controlStrings)
        {
            GameObject obj = Instantiate(controlsTextPrefab, gridLayoutGroup.transform);
            obj.GetComponent<TextMeshProUGUI>().text = str;
            fadeCanvasGroupsWave.individualElements.Add(obj.GetComponent<CanvasGroup>());
        }

        showHideText.text = showControlsStr;
    }

    void OnEnable()
    {
        helpAction.performed += OnHelpPressed;
    }

    void Start()
    {
        if (!SettingsManager.TutorialDisabled)
            TogglePanel();
    }

    void OnDisable()
    {
        helpAction.performed -= OnHelpPressed;
    } 

    void OnHelpPressed(InputAction.CallbackContext context)
    {
        TogglePanel();   
    }

    void TogglePanel()
    {
        if (!player.isDead)
        {
            // Show/Hide Panel
            if (fadeCanvasGroupsWave.isTriggered)
            {
                fadeCanvasGroupsWave.isTriggered = false;
                showHideText.text = showControlsStr;
            }
            else
            {
                fadeCanvasGroupsWave.isTriggered = true;
                showHideText.text = hideControlsStr;
            }
        }
    }
    
}

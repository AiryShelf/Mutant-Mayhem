using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] string hideControlsStr;
    [SerializeField] string unhideControlsStr;
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
        helpAction.performed += OnHelpPressed;

        foreach (string str in controlStrings)
        {
            GameObject obj = Instantiate(controlsTextPrefab, gridLayoutGroup.transform);
            obj.GetComponent<TextMeshProUGUI>().text = str;
            fadeCanvasGroupsWave.individualGroups.Add(obj.GetComponent<CanvasGroup>());
        }
    }

    void OnDisable()
    {
        helpAction.performed -= OnHelpPressed;
    }

    void OnHelpPressed(InputAction.CallbackContext context)
    {
        if (!player.isDead)
        {
            // Show/Hide Panel
            if (fadeCanvasGroupsWave.isTriggered)
            {
                fadeCanvasGroupsWave.isTriggered = false;
                showHideText.text = unhideControlsStr;
            }
            else
            {
                fadeCanvasGroupsWave.isTriggered = true;
                showHideText.text = hideControlsStr;
            }
        }
    }
    
}

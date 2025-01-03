using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsPanel : MonoBehaviour
{
    public bool isOpen = true;
    [SerializeField] string hideControlsStr;
    [SerializeField] string showControlsStr;
    [SerializeField] TextMeshProUGUI header;
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
            obj.GetComponentInChildren<TextMeshProUGUI>().text = str;
            fadeCanvasGroupsWave.individualElements.Add(obj.GetComponent<CanvasGroup>());
        }

        header.text = showControlsStr;
    }

    void OnEnable()
    {
        helpAction.performed += OnHelpPressed;
    }

    void Start()
    {
        if (!TutorialManager.IsTutorialDisabled)
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

    public void TogglePanel()
    {
        if (!player.IsDead)
        {
            // Show/Hide Panel
            if (fadeCanvasGroupsWave.isTriggered)
            {
                isOpen = false;
                fadeCanvasGroupsWave.isTriggered = false;
                header.text = showControlsStr;
            }
            else
            {
                isOpen = true;
                fadeCanvasGroupsWave.isTriggered = true;
                header.text = hideControlsStr;
            }
        }
    }
    
}

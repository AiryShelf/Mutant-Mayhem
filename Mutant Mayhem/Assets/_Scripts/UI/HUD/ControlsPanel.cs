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
    [SerializeField] DeviceTextSwitcher headerTextSwitcher;
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] List<GameObject> controlsToFade;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsWave;
    [SerializeField] Player player;
    InputActionMap playerActionMap;
    InputAction helpAction;

    void Awake()
    {
        playerActionMap = player.inputAsset.FindActionMap("Player");
        helpAction = playerActionMap.FindAction("Help");

        header.text = showControlsStr;

        foreach (var obj in controlsToFade)
        {
            CanvasGroup group = obj.GetComponent<CanvasGroup>();
            fadeCanvasGroupsWave.individualElements.Add(group);
        }
    }

    void OnEnable()
    {
        helpAction.performed += OnHelpPressed;
    }

    void Start()
    {
        if (!PlanetManager.Instance.currentPlanet.mission.isTutorial)
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

            headerTextSwitcher.ResetAndUpdate();
        }
    }
    
}

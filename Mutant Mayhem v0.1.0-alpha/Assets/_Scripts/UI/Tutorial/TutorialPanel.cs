using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TutorialPanel : MonoBehaviour
{
    public bool pauseOnOpen;
    Player player;
    protected InputActionMap playerActionMap;
    protected InputAction playerFireAction;
    ControlsPanel controlsPanel;
    GameObject prevUiSelection;

    void Start()
    {
        player = FindObjectOfType<Player>();
        controlsPanel = FindObjectOfType<ControlsPanel>();

        if (TutorialManager.TutorialDisabled == true)
        {
            Destroy(gameObject);
            return;
        }
        
        if (pauseOnOpen)
            Time.timeScale = 0;

        if (player != null)
        {
            playerActionMap = player.inputAsset.FindActionMap("Player");
            playerFireAction = playerActionMap.FindAction("Fire");
            playerActionMap.Disable();
        }
        
        
        StartCoroutine(WaitToStoreSelection());
    }

    void OnDestroy() 
    {
        // Destroy parent
        GameObject parentObject = transform.parent.gameObject;
        Destroy(parentObject);
    }

    public virtual void OnOKButtonClick()
    {
        StartCoroutine(HandleOnOKButtonClick());
    }

    public virtual void OnDisableButtonClick()
    {
        StartCoroutine(HandleOnDisableButtonClick());
    }

    IEnumerator HandleOnOKButtonClick()
    {
        // Block input briefly to avoid unintended clicks
        yield return new WaitForSecondsRealtime(0.1f);

        RestorePreviousSelection();

        if (pauseOnOpen)
            Time.timeScale = 1;

        if (player != null)
        {
            playerActionMap.Enable();
        }
        
        Destroy(gameObject);
    }

    IEnumerator HandleOnDisableButtonClick()
    {
        // Block input briefly to avoid unintended clicks
        //EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSecondsRealtime(0.1f);

        RestorePreviousSelection();

        if (controlsPanel != null && controlsPanel.isOpen)
            controlsPanel.TogglePanel();

        if (pauseOnOpen)
            Time.timeScale = 1;

        TutorialManager.SetTutorialStateAndProfile(false);
        if (player != null)
        {
            playerActionMap.Enable();
        }
        
        Destroy(gameObject);
    }

    IEnumerator WaitToStoreSelection()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        // Store previously selected object
        //Debug.Log("Stored previous selection");
        prevUiSelection = EventSystem.current.currentSelectedGameObject;
    }

    protected void RestorePreviousSelection()
    {
        if (prevUiSelection != null)
        {
            Debug.Log("Restoring previous selection: " + prevUiSelection.name);
            EventSystem.current.SetSelectedGameObject(prevUiSelection);
        }
        else
        {
            Debug.Log("No previous selection to restore");
        }
    }
}

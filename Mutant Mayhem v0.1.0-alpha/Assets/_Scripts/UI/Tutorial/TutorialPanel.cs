using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialPanel : MonoBehaviour
{
    Player player;
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
        
        Time.timeScale = 0;
        player.inputAsset.FindActionMap("Player").Disable();
        
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
        //EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSecondsRealtime(0.1f);

        RestorePreviousSelection();

        Time.timeScale = 1;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }

    IEnumerator HandleOnDisableButtonClick()
    {
        // Block input briefly to avoid unintended clicks
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSecondsRealtime(0.1f);

        RestorePreviousSelection();

        if (controlsPanel.isOpen)
            controlsPanel.TogglePanel();

        Time.timeScale = 1;
        TutorialManager.TutorialDisabled = true;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }

    IEnumerator WaitToStoreSelection()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        // Store previously selected object
        //Debug.Log("Stored previous selection");
        prevUiSelection = EventSystem.current.currentSelectedGameObject;
    }

    void RestorePreviousSelection()
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

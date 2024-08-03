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

    public virtual void OnOKButtonClick()
    {
        // Re-select the previously selected GameObject
        if (prevUiSelection != null)
        {
            EventSystem.current.SetSelectedGameObject(prevUiSelection);
        }

        Time.timeScale = 1;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }

    public virtual void OnDisableButtonClick()
    {
        // Re-select the previously selected GameObject
        if (prevUiSelection != null)
        {
            EventSystem.current.SetSelectedGameObject(prevUiSelection);
        }
        // Hide controls help panel
        if (controlsPanel.isOpen)
            controlsPanel.TogglePanel();
        
        Time.timeScale = 1;
        TutorialManager.TutorialDisabled = true;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }

    IEnumerator WaitToStoreSelection()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        // Store previously selected object
        //Debug.Log("Stored previous selection");
        prevUiSelection = EventSystem.current.currentSelectedGameObject;
    }
}

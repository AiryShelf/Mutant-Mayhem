using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] InputActionAsset inputAsset;
    public bool pauseOnOpen;
    Player player;
    protected InputActionMap playerActionMap;
    protected InputAction playerFireAction;
    GameObject prevUiSelection;
    InputAction escapeAction;


    void Start()
    {
        TutorialManager.NumTutorialsOpen++;

        if (TutorialManager.TutorialDisabled == true)
        {
            Destroy(gameObject);
            return;
        }
        
        if (pauseOnOpen)
            TimeControl.Instance.PauseGame(true);
        
        InputActionMap uIActionMap = inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");
        escapeAction.started += OnEscapePressed;
        
        StartCoroutine(WaitToStoreSelection());
    }

    void OnDestroy() 
    {
        // Destroys parent **
        GameObject parentObject = transform.parent.gameObject;
        TutorialManager.NumTutorialsOpen--;

        if (escapeAction != null)
            escapeAction.started -= OnEscapePressed;

        Destroy(parentObject);
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (TutorialManager.escCooling)
            return;

        TutorialManager.escCooling = true;
        OnOKButtonClick();
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
            TimeControl.Instance.PauseGame(false);
        
        Destroy(gameObject);
    }

    IEnumerator HandleOnDisableButtonClick()
    {
        // Block input briefly to avoid unintended clicks
        //EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSecondsRealtime(0.1f);

        RestorePreviousSelection();
        TutorialManager.SetProfileAndTutorialState(false);

        if (pauseOnOpen)
            TimeControl.Instance.PauseGame(false);
        
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

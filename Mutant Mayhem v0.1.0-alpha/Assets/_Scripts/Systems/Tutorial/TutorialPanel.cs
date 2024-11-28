using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button disableTutorialButton;
    [SerializeField] InputActionAsset inputAsset;
    public bool pauseOnOpen;
    protected InputActionMap playerActionMap;
    protected InputAction playerFireAction;
    GameObject prevUiSelection;
    InputAction escapeAction;

    void Awake()
    {
        InputActionMap uIActionMap = inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");
    }

    void OnEnable()
    {
        escapeAction.started += OnEscapePressed;

        TutorialManager.NumTutorialsOpen++;

        if (TutorialManager.IsTutorialDisabled == true)
        {
            gameObject.SetActive(false);
            return;
        }
        
        if (pauseOnOpen)
            TimeControl.Instance.PauseGame(true);
        
        StartCoroutine(WaitToStoreSelection());
    }

    void OnDisable()
    {
        TutorialManager.NumTutorialsOpen--;

        if (escapeAction != null)
            escapeAction.started -= OnEscapePressed;
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (TutorialManager.escIsCooling)
            return;

        TutorialManager.escIsCooling = true;
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
        
        gameObject.SetActive(false);
    }

    IEnumerator HandleOnDisableButtonClick()
    {
        // Block input briefly to avoid unintended clicks
        //EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSecondsRealtime(0.1f);

        RestorePreviousSelection();
        TutorialManager.SetTutorialState(false);

        if (pauseOnOpen)
            TimeControl.Instance.PauseGame(false);
        
        gameObject.SetActive(false);
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

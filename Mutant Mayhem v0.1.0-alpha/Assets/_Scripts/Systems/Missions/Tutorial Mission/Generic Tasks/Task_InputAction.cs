using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_InputAction : Task
{
    [Header("Input Action")]
    [SerializeField] float timeToPerformAction = 2;
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] string actionMapName;
    [SerializeField] string actionName;
    [Header("Optional")]
    [SerializeField] string secondActionName;

    InputAction inputAction;
    InputAction secondInputAction;
    float actionTime = 0;
    bool isPerforming = false;
    bool isPerformingSecond = false;

    void OnEnable()
    {
        // Subscribe to objective's event(s)
        InputActionMap actionMap = inputAsset.FindActionMap(actionMapName);

        inputAction = actionMap.FindAction(actionName);
        inputAction.started += OnActionStarted;
        inputAction.canceled += OnActionCanceled;

        if (!string.IsNullOrEmpty(secondActionName))
        {
            secondInputAction = actionMap.FindAction(secondActionName);
            secondInputAction.started += OnSecondActionStarted;
            secondInputAction.canceled += OnSecondActionCanceled;
        }
    }

    void OnDisable()
    {
        inputAction.started -= OnActionStarted;
        inputAction.canceled -= OnActionCanceled;

        if (!string.IsNullOrEmpty(secondActionName))
        {
            secondInputAction.started -= OnSecondActionStarted;
            secondInputAction.canceled -= OnSecondActionCanceled;
        }
    }

    void Start()
    {
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (secondInputAction != null && !isPerformingSecond)
            return;

        if (isPerforming)
        {
            actionTime += Time.fixedDeltaTime;
            if (actionTime > timeToPerformAction)
                SetTaskComplete();

            progress = Mathf.Clamp(actionTime / timeToPerformAction, 0, 1);
            UpdateProgressText();
        }
    }

    

    void OnActionStarted(InputAction.CallbackContext _)
    {
        isPerforming = true;
    }

    void OnActionCanceled(InputAction.CallbackContext _)
    {
        isPerforming = false;
    }

    void OnSecondActionStarted(InputAction.CallbackContext _)
    {
        isPerformingSecond = true;
    }

    void OnSecondActionCanceled(InputAction.CallbackContext _)
    {
        isPerformingSecond = false;
    }
}

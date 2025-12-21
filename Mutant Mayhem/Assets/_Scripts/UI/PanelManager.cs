using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum UpgradePanelType
{
    None,
    Consumables,
    Exosuit,
    Lasers,
    Bullets,
    Structures,
    Repair,
    Explosives,
    Drones,
}

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [Header("Panel Tracking")]
    public static int NumPanelsOpen = 0;

    [Header("Panel Escape Control")]
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] float escKeyCooldown = 0.1f;
    [HideInInspector] public static bool escIsCooling;
    InputAction escapeAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InputActionMap uIActionMap = inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");
        escapeAction.started += OnEscapePressed;
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        StartCoroutine(EscapeKeyCooldown());
    }

    IEnumerator EscapeKeyCooldown()
    {
        yield return new WaitForSecondsRealtime(escKeyCooldown);
        escIsCooling = false;
    }
}
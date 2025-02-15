using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLooker : MonoBehaviour
{
    [SerializeField] float distDivisor;
    [SerializeField] Player player;
    public Transform playerTrans;
    public bool deathTriggered;
    public bool lockedToPlayer;
    Vector3 mousePos;
    InputController inputController;
    InputDevice currentDevice;

    void Start()
    {
        inputController = InputController.Instance;
        inputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;

        OnLastUsedDeviceChanged(InputController.LastUsedDevice);

        gameObject.transform.parent = null;
    }

    void FixedUpdate()
    {
        if (deathTriggered)
        {
            return;
        }

        if (lockedToPlayer)
        {
            transform.position = player.transform.position;
        }

        if (currentDevice == Keyboard.current)
            FollowMouse();
        else if (currentDevice == Touchscreen.current || currentDevice == Gamepad.current)
        {
            FollowCustomCursor();
        }
        
    }

    void FollowMouse()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 difference = mousePos - playerTrans.position;
        difference /= distDivisor;
        Vector3 newPos = playerTrans.position + difference;
        transform.position = newPos;
    }

    void FollowCustomCursor()
    {
        transform.position = CursorManager.Instance.GetCustomCursorWorldPos();
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        currentDevice = device;
    }
}

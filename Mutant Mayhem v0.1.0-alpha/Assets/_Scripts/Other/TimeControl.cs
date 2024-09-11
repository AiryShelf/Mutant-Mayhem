using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeControl : MonoBehaviour
{
    Player player;
    InputActionMap playerActionMap;
    InputAction timeControlAction;

    public float gameSpeed = 1;

    const float epsilon = 0.0001f;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerActionMap = player.inputAsset.FindActionMap("Player");
        timeControlAction = playerActionMap.FindAction("TimeControl");
        timeControlAction.performed += OnTimeControl;
    }

    void OnTimeControl(InputAction.CallbackContext context)
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            Time.timeScale += 0.1f;
        if (Input.GetKeyDown(KeyCode.KeypadMinus) && Time.timeScale - 0.01f > epsilon)
            Time.timeScale -= 0.1f;
        Debug.Log("timeScale changed to: " + Time.timeScale.ToString("#0.0"));
        MessagePanel.ShowMessage("For Debug: Timescale changed to: " + 
                                 Time.timeScale.ToString("#0.0"), Color.red);
    }


}

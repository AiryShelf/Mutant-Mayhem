using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeControl : MonoBehaviour
{
    public static TimeControl Instance;

    public static bool isPaused;

    float previousTimeScale = 1;
    const float epsilon = 0.0001f;

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
    }

    public void SubscribePlayerTimeControl(Player player)
    {
        InputActionMap playerActionMap = player.inputAsset.FindActionMap("Player");
        InputAction timeControlAction = playerActionMap.FindAction("TimeControl");
        timeControlAction.performed += OnTimeControl;
    }

    public void UnsubscribePlayerTimeControl(Player player)
    {
        InputActionMap playerActionMap = player.inputAsset.FindActionMap("Player");
        InputAction timeControlAction = playerActionMap.FindAction("TimeControl");
        timeControlAction.performed -= OnTimeControl;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1;
        previousTimeScale = 1;
    }

    void OnTimeControl(InputAction.CallbackContext context)
    {
        if (Keyboard.current.numpadPlusKey.isPressed)
            Time.timeScale += 0.1f;
        if (Keyboard.current.numpadMinusKey.isPressed && Time.timeScale - 0.01f > epsilon)
            if (Time.timeScale > 0.1f)
                Time.timeScale -= 0.1f;
        //Debug.Log("timeScale changed to: " + Time.timeScale.ToString("#0.0"));
        MessagePanel.PulseMessage("Timescale changed to: " + 
                                  Time.timeScale.ToString("#0.0"), Color.red);

        previousTimeScale = Time.timeScale;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0;
            isPaused = true;
            Debug.Log("Game Paused");
        }
        else
        {
            Time.timeScale = previousTimeScale;
            isPaused = false;
            Debug.Log("Game Un-Paused");
        }
    }
}

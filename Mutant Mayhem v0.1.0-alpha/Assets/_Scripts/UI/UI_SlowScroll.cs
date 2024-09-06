using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_SlowScroll : MonoBehaviour
{
    [SerializeField] float scrollDelay = 0.1f;
    Player player;
    InputActionMap uiActionMap;
    InputAction scrollAction;

    Coroutine slowScroll;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        uiActionMap = player.inputAsset.FindActionMap("UI");
        scrollAction = uiActionMap.FindAction("ScrollWheel");
    }

    void OnEnable()
    {
        scrollAction.Enable();
        scrollAction.performed += OnScrollPerformed;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        scrollAction.performed -= OnScrollPerformed;
        scrollAction.Disable();
    }

    private void OnScrollPerformed(InputAction.CallbackContext context)
    {
        // Included this for mac users, it slows the mouse-controlled scrolling
        if (slowScroll == null)
        {
            if (Keyboard.current[Key.LeftCtrl].isPressed || 
                Keyboard.current[Key.RightCtrl].isPressed ||
                Keyboard.current[Key.LeftCommand].isPressed || 
                Keyboard.current[Key.RightCommand].isPressed)
            {
                slowScroll = StartCoroutine(SlowScroll());    
            }
        }
    }

    IEnumerator SlowScroll()
    {
        yield return null;
        scrollAction.Disable();
        yield return new WaitForSeconds(scrollDelay);
        scrollAction.Enable();
        slowScroll = null;
    }
}

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
        #if UNITY_STANDALONE_OSX
            player = FindObjectOfType<Player>();
            uiActionMap = player.inputAsset.FindActionMap("UI");
            scrollAction = uiActionMap.FindAction("ScrollWheel");
        #endif
    }

    void OnEnable()
    {
        #if UNITY_STANDALONE_OSX
            scrollAction.Enable();
            scrollAction.performed += OnScrollPerformed;
        #endif
    }

    void OnDisable()
    {
        #if UNITY_STANDALONE_OSX
            StopAllCoroutines();
            scrollAction.performed -= OnScrollPerformed;
            scrollAction.Disable();
        #endif
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

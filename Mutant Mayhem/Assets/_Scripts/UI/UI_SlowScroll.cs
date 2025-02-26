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
        // Removed platform-specific compilation so it works on all platforms
        player = FindObjectOfType<Player>(); // <<-- Changed: Removed #if UNITY_STANDALONE_OSX block
        uiActionMap = player.inputAsset.FindActionMap("UI");
        scrollAction = uiActionMap.FindAction("Scroll");
    }

    void OnEnable()
    {
        scrollAction.Enable(); // <<-- Changed: Removed #if UNITY_STANDALONE_OSX block
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
        // Check if we are not already running the slow scroll coroutine
        if (slowScroll == null)
        {
            bool useSlowScroll = false;
            
            // Original check for Ctrl/Command keys
            if (Keyboard.current != null)
            {
                useSlowScroll = Keyboard.current[Key.LeftCtrl].isPressed ||
                                Keyboard.current[Key.RightCtrl].isPressed ||
                                Keyboard.current[Key.LeftCommand].isPressed ||
                                Keyboard.current[Key.RightCommand].isPressed;
            }
            
            // <<-- Added: Also check if the input came from a touchscreen device
            if (context.control.device is Touchscreen)
            {
                //useSlowScroll = true;
            }
            
            if (useSlowScroll)
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

/*
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
            scrollAction = uiActionMap.FindAction("Scroll");
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

*/

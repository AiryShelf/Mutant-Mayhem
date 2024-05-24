using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolbarSelector : MonoBehaviour
{
    [SerializeField] List<Image> boxImages;
    Image currentBox;
    Player player;
    Color unselectedColor;
    [SerializeField] Color selectedColor;
    InputActionMap playerActionMap;
    InputAction toolbarAction;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        currentBox = boxImages[0];
        unselectedColor = currentBox.color;
        SwitchBoxes(0);
    }

    void OnEnable()
    {
        // Subscribe to toolbar action event
        //player.inputAsset.actionMaps[0].actions[6].performed += OnToolbar;
        playerActionMap = player.inputAsset.FindActionMap("Player");
        toolbarAction = playerActionMap.FindAction("Toolbar");

        // Add my method to the action event
        toolbarAction.performed += OnToolbar;
    }

    void OnDisable()
    {
        // Unsubscribe to toolbar action event
        toolbarAction.performed -= OnToolbar;
    }

    // No context used below, but is necessary to form the method
    void OnToolbar(InputAction.CallbackContext context)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchBoxes(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchBoxes(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchBoxes(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchBoxes(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwitchBoxes(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SwitchBoxes(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SwitchBoxes(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SwitchBoxes(7);
        }
    }

    void SwitchBoxes(int i)
    {
        currentBox.color = unselectedColor;
        currentBox = boxImages[i];
        currentBox.color = selectedColor;

    }
}

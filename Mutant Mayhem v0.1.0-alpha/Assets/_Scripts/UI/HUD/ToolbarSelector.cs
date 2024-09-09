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

    void Start()
    {
        player = FindObjectOfType<Player>();
        currentBox = boxImages[0];
        unselectedColor = currentBox.color;
        SwitchBoxes(0);
    }

    public void SwitchBoxes(int i)
    {
        if (player.playerShooter.gunList[i] != null)
        {
            if (player.playerShooter.gunsUnlocked[i])
            {
                currentBox.color = unselectedColor;
                currentBox = boxImages[i];
                currentBox.color = selectedColor;
            }
        }
    }
}

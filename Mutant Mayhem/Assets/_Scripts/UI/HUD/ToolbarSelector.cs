using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolbarSelector : MonoBehaviour
{
    [SerializeField] List<Image> boxImages;
    [SerializeField] List<Image> gunImages;
    Image currentBox;
    Player player;
    Image startImage;
    Color unselectedColor;
    [SerializeField] Color selectedColor;

    void Start()
    {
        player = FindObjectOfType<Player>();

        currentBox = boxImages[0];
        unselectedColor = currentBox.color;
        SwitchBoxes(0);

        player.playerShooter.onPlayerGunSwitched += SwitchBoxes;
    }

    void OnDestroy()
    {
        player.playerShooter.onPlayerGunSwitched -= SwitchBoxes;
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

    public void UnlockBoxImage(int i)
    {
        Image image = gunImages[i];
        image.color = new Color(1,1,1,1);
        //Debug.Log("Toolbarselector played upgEffect");
        UpgradeManager.Instance.upgradeEffects.ToolbarUpgradeEffect((Vector2)image.transform.position);
    }

    public void LockBoxImage(int i)
    {
        Image image = gunImages[i];
        image.color = new Color(0, 0, 0, 0.4f);
        //Debug.Log("Toolbarselector played upgEffect");
        //UpgradeManager.Instance.upgradeEffects.ToolbarUpgradeEffect((Vector2)image.transform.position);
    }
}

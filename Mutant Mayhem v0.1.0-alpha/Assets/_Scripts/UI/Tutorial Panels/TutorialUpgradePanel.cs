using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUpgradePanel : TutorialPanel
{
    void Awake()
    {
        if (TutorialManager.TutorialShowedUpgrade || TutorialManager.TutorialDisabled)
        {
            Destroy(gameObject);
            return;
        }

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            playerActionMap = player.inputAsset.FindActionMap("Player");
            playerFireAction = playerActionMap.FindAction("Fire");
            playerActionMap.Disable();
        }
    }

    public override void OnOKButtonClick()
    {
        TutorialManager.TutorialShowedUpgrade = true;

        base.OnOKButtonClick();

        playerActionMap.Enable();
        playerFireAction.Disable();
    }

    public override void OnDisableButtonClick()
    {
        base.OnDisableButtonClick();

        playerActionMap.Enable();
        playerFireAction.Disable();
    }
}

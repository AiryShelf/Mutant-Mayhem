using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBuildPanel : TutorialPanel
{
    void Awake()
    {
        if (TutorialManager.TutorialShowedBuild)
        {
            Destroy(gameObject);
            //return;
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
        TutorialManager.TutorialShowedBuild = true;
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

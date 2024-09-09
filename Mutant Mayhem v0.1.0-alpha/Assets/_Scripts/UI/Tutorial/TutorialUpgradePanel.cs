using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUpgradePanel : TutorialPanel
{
    void Awake()
    {
        if (TutorialManager.tutorialShowedUpgrade)
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void OnOKButtonClick()
    {
        TutorialManager.tutorialShowedUpgrade = true;

        playerActionMap.Enable();
        playerFireAction.Disable();

        base.OnOKButtonClick();
    }

    public override void OnDisableButtonClick()
    {
        playerActionMap.Enable();
        playerFireAction.Disable();
        
        base.OnDisableButtonClick();
    }
}

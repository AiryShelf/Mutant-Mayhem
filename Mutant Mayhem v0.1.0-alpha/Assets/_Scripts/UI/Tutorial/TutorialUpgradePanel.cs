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

        RestorePreviousSelection();

        Time.timeScale = 1;
        playerActionMap.Enable();
        playerFireAction.Disable();

        Destroy(gameObject);
    }

    public override void OnDisableButtonClick()
    {
        TutorialManager.TutorialDisabled = true;

        RestorePreviousSelection();

        Time.timeScale = 1;
        playerActionMap.Enable();
        playerFireAction.Disable();

        Destroy(gameObject);
    }
}

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
        }
    }

    public override void OnOKButtonClick()
    {
        TutorialManager.tutorialShowedUpgrade = true;
        base.OnOKButtonClick();
    }
}

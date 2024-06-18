using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUpgradePanel : TutorialPanel
{
    void Awake()
    {
        if (SettingsManager.tutorialShowedUpgrade)
        {
            Destroy(gameObject);
        }
    }

    public override void OnOKButtonClick()
    {
        SettingsManager.tutorialShowedUpgrade = true;
        base.OnOKButtonClick();
    }
}
